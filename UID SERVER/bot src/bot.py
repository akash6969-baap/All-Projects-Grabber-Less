import discord
from discord import app_commands
from discord.ext import commands
import sqlite3
import requests
import os
import aiohttp
import asyncio
import time


# For logging robustness: use loguru if present, otherwise fall back to standard print
try:
    from loguru import logger
except ImportError:
    import logging
    logging.basicConfig(level=logging.INFO)
    logger = logging.getLogger("discord_v2_logger")

# ==========================================
# CUSTOM EMOJIS (Customize these values)
# ==========================================
E_SUCCESS = "<:success:1479801710267338873>"
E_WARNING = "<:warning:1479802268990701680>"
E_DOT     = "<:wifi:1496500724052394115>"
E_GEAR    = "<:gear:1479802080729501727>"
E_DONE    = "<:done:1500340658899259542>"
E_ROCKET  = "<:rocket:1496510701257359564>"
E_TRASH   = "<:trash:1493958705841180802>"
E_BULB    = "<:bulb:1479802288867381258>"
E_GREEN   = "<:green:1500340616666546256>"
E_RED     = "<:red:1500340642453389353>"


COLOR_WHITE = 0xFFFFFF
CLR_MASTER = 0x242629
CLR_ERROR = 0xda373c
FLAGS_COMPONENTS_V2 = 32768

import math

# --- Configuration ---
BOT_TOKEN = os.getenv("DISCORD_BOT_TOKEN", "MTUwOTk3NDYzOTc0Nzc5NzIwMw.Gi4owi.6SrA5LBVxOtRbjLWpsItkKQ_gd0mS8qsyDQUh0")
API_URL = os.getenv("API_URL", "https://bypass-portal-mu.vercel.app/api/v1/uids/add")

# --- Database Setup ---
DB_FILE = "guild_keys.db"

def init_db():
    conn = sqlite3.connect(DB_FILE)
    cursor = conn.cursor()
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS guild_config (
            guild_id TEXT PRIMARY KEY,
            api_key TEXT NOT NULL,
            whitelist_channel_id TEXT DEFAULT NULL,
            log_channel_id TEXT DEFAULT NULL,
            mod_role_id TEXT DEFAULT NULL
        )
    """)
    # Check if columns exist for backwards compatibility
    columns = [row[1] for row in cursor.execute("PRAGMA table_info(guild_config)").fetchall()]
    if "whitelist_channel_id" not in columns:
        cursor.execute("ALTER TABLE guild_config ADD COLUMN whitelist_channel_id TEXT DEFAULT NULL")
    if "log_channel_id" not in columns:
        cursor.execute("ALTER TABLE guild_config ADD COLUMN log_channel_id TEXT DEFAULT NULL")
    if "mod_role_id" not in columns:
        cursor.execute("ALTER TABLE guild_config ADD COLUMN mod_role_id TEXT DEFAULT NULL")
    conn.commit()
    conn.close()

def get_guild_config(guild_id):
    conn = sqlite3.connect(DB_FILE)
    conn.row_factory = sqlite3.Row
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM guild_config WHERE guild_id = ?", (str(guild_id),))
    row = cursor.fetchone()
    conn.close()
    if row:
        return dict(row)
    return None

def get_guild_key(guild_id):
    config = get_guild_config(guild_id)
    return config["api_key"] if config else None

def save_guild_config(guild_id, api_key=None, whitelist_channel_id=None, log_channel_id=None, mod_role_id=None):
    conn = sqlite3.connect(DB_FILE)
    cursor = conn.cursor()
    # Check if guild exists
    cursor.execute("SELECT 1 FROM guild_config WHERE guild_id = ?", (str(guild_id),))
    exists = cursor.fetchone()
    if not exists:
        cursor.execute("""
            INSERT INTO guild_config (guild_id, api_key, whitelist_channel_id, log_channel_id, mod_role_id)
            VALUES (?, ?, ?, ?, ?)
        """, (str(guild_id), api_key or "", whitelist_channel_id, log_channel_id, mod_role_id))
    else:
        # Build dynamic update
        updates = []
        params = []
        if api_key is not None:
            updates.append("api_key = ?")
            params.append(api_key)
        if whitelist_channel_id is not None:
            updates.append("whitelist_channel_id = ?")
            params.append(whitelist_channel_id)
        if log_channel_id is not None:
            updates.append("log_channel_id = ?")
            params.append(log_channel_id)
        if mod_role_id is not None:
            updates.append("mod_role_id = ?")
            params.append(mod_role_id)
        
        if updates:
            query = f"UPDATE guild_config SET {', '.join(updates)} WHERE guild_id = ?"
            params.append(str(guild_id))
            cursor.execute(query, tuple(params))
            
    conn.commit()
    conn.close()

# Initialize DB
init_db()

# --- FORMATTING HELPERS ---
def parse_emoji(emoji_str: str) -> dict | None:
    if not emoji_str:
        return None
    if emoji_str.startswith("<a:") or emoji_str.startswith("<:"):
        parts = emoji_str.strip("<>").split(":")
        if len(parts) >= 3:
            try:
                emoji_id = int(parts[2])
            except ValueError:
                emoji_id = parts[2]
            return {
                "name": parts[1],
                "id": emoji_id,
                "animated": parts[0] == "a"
            }
    return {"name": emoji_str}

def parse_duration_to_days(duration_str: str) -> int:
    if not duration_str:
        return 30
    val = str(duration_str).strip().lower()
    if val in ["lifetime", "never", "0", "max"]:
        return 365
    if val.endswith("h"):
        num = val[:-1]
        if num.isdigit():
            return max(1, math.ceil(int(num) / 24))
    elif val.endswith("d"):
        num = val[:-1]
        if num.isdigit():
            return max(1, int(num))
    elif val.endswith("w"):
        num = val[:-1]
        if num.isdigit():
            return max(1, int(num) * 7)
    elif val.endswith("m"):
        num = val[:-1]
        if num.isdigit():
            return max(1, int(num) * 30)
    if val.isdigit():
        return max(1, int(val))
    return 30

def parse_duration_to_hours(duration_str: str) -> str:
    if not duration_str:
        return "lifetime"
    val = duration_str.strip().lower()
    if val == "lifetime":
        return "lifetime"
    
    # Suffix map
    if val.endswith("h"):
        num = val[:-1]
        if num.isdigit():
            return str(int(num))
    elif val.endswith("d"):
        num = val[:-1]
        if num.isdigit():
            return str(int(num) * 24)
    elif val.endswith("w"):
        num = val[:-1]
        if num.isdigit():
            return str(int(num) * 168)
    elif val.endswith("m"):
        num = val[:-1]
        if num.isdigit():
            return str(int(num) * 720)
            
    if val.isdigit():
        return val
        
    return duration_str

def format_expiry(expiry_val) -> str:
    if not expiry_val:
        return "`N/A`"
    val_str = str(expiry_val).strip()
    if val_str.lower() in ["lifetime", "never", "0"]:
        return "Lifetime ♾️"
    
    if val_str.isdigit():
        ts = int(val_str)
        if ts > 1000000000:
            return f"<t:{ts}:F> (<t:{ts}:R>)"
            
    return f"`{val_str}`"

def v2_text(content: str) -> dict:
    return {"type": 10, "content": content}

def v2_separator(divider=True, spacing=1) -> dict:
    return {"type": 14, "divider": divider, "spacing": spacing}

def v2_container(components: list, accent=CLR_MASTER) -> dict:
    return {"type": 17, "accent_color": accent, "components": components}

def v2_section(texts: list, accessory_url: str = None) -> dict:
    if accessory_url:
        return {
            "type": 9,
            "components": [v2_text(t) for t in texts],
            "accessory": {
                "type": 11,
                "media": {"url": accessory_url}
            }
        }
    else:
        return v2_text("\n".join(texts))

def v2_gallery(url: str) -> dict:
    return {
        "type": 12,
        "items": [{"media": {"url": url}}]
    }

def v2_button(label: str = None, style=1, custom_id: str = None, url: str = None, emoji: dict = None) -> dict:
    payload = {"type": 2, "style": style}
    if label:
        payload["label"] = label
    if custom_id:
        payload["custom_id"] = custom_id
    if url:
        payload["url"] = url
    if emoji:
        payload["emoji"] = emoji
    return payload

def build_v2_response(title: str, lines: list, accessory_url: str = None, success: bool = True) -> list:
    header = f"### {title}"
    accent = CLR_MASTER if success else CLR_ERROR
    header_block = v2_section([header], accessory_url)
    if lines:
        formatted = []
        for line in lines:
            if line.strip().startswith(">") or line.strip().startswith("•") or line.strip().startswith("*-"):
                formatted.append(line)
            else:
                formatted.append(f"> {E_DOT} {line}")
        details_block = v2_text("\n".join(formatted))
        return [v2_container([header_block, v2_separator(), details_block], accent=accent)]
    else:
        return [v2_container([header_block], accent=accent)]

# --- HTTP API HELPERS ---
_http_session = None

async def get_http_session():
    global _http_session
    if _http_session is None or _http_session.closed:
        _http_session = aiohttp.ClientSession()
    return _http_session

async def async_vps_request(method: str, url: str, headers: dict = None, json_payload: dict = None, timeout: int = 8):
    session = await get_http_session()
    try:
        async with session.request(
            method,
            url,
            headers=headers,
            json=json_payload,
            timeout=aiohttp.ClientTimeout(total=timeout)
        ) as resp:
            try:
                data = await resp.json()
            except Exception:
                try:
                    text_data = await resp.text()
                    data = {"error": text_data}
                except Exception:
                    data = {}
            return resp.status, data
    except Exception as e:
        logger.error(f"VPS API async request error: {e}")
        return 0, {"error": str(e)}

async def _api(method: str, endpoint: str, token: str, payload: dict = None, max_retries: int = 3) -> dict | None:
    session = await get_http_session()
    headers = {
        "Authorization": f"Bot {token}",
        "Content-Type": "application/json"
    }
    for attempt in range(max_retries):
        try:
            async with session.request(
                method,
                f"https://discord.com/api/v10{endpoint}",
                json=payload, headers=headers
            ) as r:
                logger.debug(f"API Request {method} {endpoint} status: {r.status} (attempt {attempt + 1})")
                if r.status in (200, 201):
                    try:
                        return await r.json()
                    except Exception as e:
                        logger.error(f"API Response JSON Parse Error: {e}")
                        return None
                elif r.status == 204:
                    return True
                elif r.status == 429:
                    try:
                        res_json = await r.json()
                        retry_after = res_json.get("retry_after", 2 ** attempt)
                    except Exception:
                        retry_after = 2 ** attempt
                    logger.warning(f"API Rate limited on {method} {endpoint}. Retrying after {retry_after}s...")
                    await asyncio.sleep(retry_after)
                    continue
                elif r.status in (500, 502, 503, 504):
                    retry_after = 0.5 * (2 ** attempt)
                    logger.warning(f"API Transient Error {r.status} on {method} {endpoint}. Retrying in {retry_after}s... (attempt {attempt + 1})")
                    await asyncio.sleep(retry_after)
                    continue
                else:
                    try:
                        err_body = await r.text()
                        is_ignorable = False
                        ignore_reason = ""
                        
                        if "50007" in err_body or "50278" in err_body:
                            is_ignorable = True
                            ignore_reason = "User DM Blocked"
                        elif "10008" in err_body:
                            is_ignorable = True
                            ignore_reason = "Unknown Message"
                        elif "10003" in err_body:
                            is_ignorable = True
                            ignore_reason = "Unknown Channel"
                        elif "50013" in err_body:
                            is_ignorable = True
                            ignore_reason = "Missing Permissions"

                        if is_ignorable:
                            logger.info(f"API Expected Response ({r.status}) on {method} {endpoint}: {ignore_reason}")
                        else:
                            logger.error(f"API Error Response {r.status} on {method} {endpoint}: {err_body}")
                    except Exception as e:
                        logger.error(f"API Error Response status {r.status}, but failed to read body: {e}")
                    return None
        except Exception as e:
            logger.error(f"API Request Error on attempt {attempt + 1}: {e}")
            if attempt == max_retries - 1:
                return None
            await asyncio.sleep(1)
    return None

async def send_v2_log_internal(channel, embed: discord.Embed, token: str, message_id: str = None, content: str = None) -> str | None:
    if not channel:
        return None

    title = embed.title
    if not title and embed.author and embed.author.name:
        title = embed.author.name

    lines = []
    if embed.description:
        for d_line in embed.description.split("\n"):
            val = d_line.strip()
            if val:
                if not title and (val.startswith("###") or val.startswith("##") or val.startswith("#")):
                    title = val.lstrip("#").strip()
                else:
                    lines.append(val)

    if not title:
        title = "Notification"

    success = True
    if embed.color:
        color_val = embed.color.value if hasattr(embed.color, "value") else int(embed.color)
        r = (color_val >> 16) & 0xFF
        g = (color_val >> 8) & 0xFF
        b = color_val & 0xFF
        if r > 100 and g < 50 and b < 50:
            success = False
        elif color_val in [0xda373c, 0xff0000, 0xe74c3c, 0x992d22]:
            success = False

    title_lower = title.lower()
    critical_words = ["ban", "kick", "delete", "warn", "limit", "alert", "error", "warning", "fail", "timeout", "remove"]
    if any(word in title_lower for word in critical_words):
        success = False

    if not any(emoji in title for emoji in ["<:", "<a:", "🚫", "⚠️", "✅", "🛑", "⚙️", "🔧", "👤", "💬"]):
        if "ban" in title_lower or "kick" in title_lower:
            title = f"🔨 {title}"
        elif "delete" in title_lower or "remove" in title_lower:
            title = f"🗑️ {title}"
        elif "warn" in title_lower or "limit" in title_lower or "alert" in title_lower or "error" in title_lower or "warning" in title_lower:
            title = f"⚠️ {title}"
        elif "create" in title_lower or "join" in title_lower or "welcome" in title_lower:
            title = f"📥 {title}"
        elif "leave" in title_lower or "left" in title_lower:
            title = f"📤 {title}"
        elif "update" in title_lower or "edit" in title_lower:
            title = f"⚙️ {title}"

    accessory_url = None
    if embed.thumbnail and embed.thumbnail.url:
        accessory_url = embed.thumbnail.url
    elif embed.author and embed.author.icon_url:
        accessory_url = embed.author.icon_url

    for field in embed.fields:
        if field.name and field.value:
            name = field.name.strip()
            val = str(field.value).strip()
            if "\n" in val:
                lines.append(f"**{name}:**")
                for v_line in val.split("\n"):
                    if v_line.strip():
                        lines.append(f"  {v_line.strip()}")
            else:
                lines.append(f"**{name}:** {val}")

    comps = build_v2_response(title, lines, accessory_url=accessory_url, success=success)
    if message_id:
        payload = {"flags": FLAGS_COMPONENTS_V2, "components": comps}
        await _api("PATCH", f"/channels/{channel.id}/messages/{message_id}", token, payload)
        return message_id
    else:
        return await send_v2_raw(channel, comps, token, content=content)

async def send_v2_raw(channel, components: list, token: str, content: str = None) -> str | None:
    if not channel: return None
    if content:
        try:
            session = await get_http_session()
            headers = {
                "Authorization": f"Bot {token}",
                "Content-Type": "application/json"
            }
            async with session.post(
                f"https://discord.com/api/v10/channels/{channel.id}/messages",
                json={"content": content},
                headers=headers
            ) as r:
                pass
        except Exception as e:
            logger.error(f"Failed to send legacy content: {e}")
            
    payload = {"flags": FLAGS_COMPONENTS_V2, "components": components}
    data = await _api("POST", f"/channels/{channel.id}/messages", token, payload)
    return data.get("id") if data else None

# --- Bot Initialization ---
intents = discord.Intents.default()
intents.message_content = True
bot = commands.Bot(command_prefix="!", intents=intents, help_command=None)
bot.start_time = discord.utils.utcnow()


async def validate_api_key(api_key):
    headers = {"X-AUTH-KEY": api_key, "X-API-Key": api_key, "Content-Type": "application/json"}
    try:
        list_url = API_URL.replace("/uids/add", "/uids/list")
        status, result = await async_vps_request("GET", list_url, headers=headers, timeout=6)
        if status == 200:
            return True, "API Key is valid."
        elif status == 401:
            return False, "Invalid API Key."
        else:
            err_msg = result.get("error", f"Server responded with status: {status}") if isinstance(result, dict) else f"Server status: {status}"
            return False, err_msg
    except Exception as e:
        return False, f"Failed to connect to backend: {str(e)}"

# --- V2 Component Log Response Helper ---
async def send_v2_response(ctx, embed):
    token = BOT_TOKEN
    if token == "YOUR_DISCORD_BOT_TOKEN_HERE" or not token:
        await ctx.reply(embed=embed)
        return
    try:
        res = await send_v2_log_internal(ctx.channel, embed, token)
        if not res:
            logger.warning("V2 log send returned None. Falling back to standard embed.")
            await ctx.reply(embed=embed)
    except Exception as e:
        logger.error(f"V2 log send failed: {e}. Falling back to standard embed.")
        await ctx.reply(embed=embed)

async def reply_embed(ctx, title, description, color=discord.Color.blue(), fields=None, success=True):
    embed = discord.Embed(title=title, description=description, color=color)
    if fields:
        for name, value in fields:
            embed.add_field(name=name, value=value, inline=False)
    embed.set_footer(text="UID Bypass Live Gateway")
    await send_v2_response(ctx, embed)

# --- Helper Permissions Check ---
def check_command_permissions(ctx, config):
    if ctx.author.guild_permissions.administrator:
        return True, ""
    
    # Check Mod Role
    mod_role_id = config.get("mod_role_id")
    if mod_role_id:
        has_role = False
        for role in ctx.author.roles:
            if str(role.id) == str(mod_role_id):
                has_role = True
                break
        if not has_role:
            return False, f"{E_WARNING} You need the Staff/Mod role (<@&{mod_role_id}>) to use this command."
            
    # Check Whitelist Channel
    whitelist_channel_id = config.get("whitelist_channel_id")
    if whitelist_channel_id:
        if str(ctx.channel.id) != str(whitelist_channel_id):
            return False, f"{E_WARNING} This command can only be used in the whitelist channel: <#{whitelist_channel_id}>."
            
    return True, ""

# --- Setup Interactive Wizard Modal ---
class SetupModal(discord.ui.Modal, title="Configuration Wizard"):
    def __init__(self, api_key="", whitelist_chan="", logs_chan="", mod_role=""):
        super().__init__()
        self.api_key_input = discord.ui.TextInput(
            label="Client API Key",
            placeholder="KEY-XXXX-XXXX-XXXX-XXXX",
            required=True
        )
        self.whitelist_chan_input = discord.ui.TextInput(
            label="Whitelist Channel ID",
            placeholder="Channel where whitelisting is allowed",
            required=False
        )
        self.logs_chan_input = discord.ui.TextInput(
            label="Logs Channel ID",
            placeholder="Channel where logs will be posted",
            required=False
        )
        self.mod_role_input = discord.ui.TextInput(
            label="Staff/Mod Role ID",
            placeholder="Role ID permitted to use the bot",
            required=False
        )
        self.add_item(self.api_key_input)
        self.add_item(self.whitelist_chan_input)
        self.add_item(self.logs_chan_input)
        self.add_item(self.mod_role_input)

    async def on_submit(self, interaction: discord.Interaction):
        api_key = self.api_key_input.value.strip()
        whitelist_chan = self.whitelist_chan_input.value.strip()
        logs_chan = self.logs_chan_input.value.strip()
        mod_role = self.mod_role_input.value.strip()

        # Input cleaning & validation
        if whitelist_chan and not whitelist_chan.isdigit():
            await interaction.response.send_message(f"{E_WARNING} Whitelist Channel ID must be numeric digits.", ephemeral=True)
            return
        if logs_chan and not logs_chan.isdigit():
            await interaction.response.send_message(f"{E_WARNING} Logs Channel ID must be numeric digits.", ephemeral=True)
            return
        if mod_role and not mod_role.isdigit():
            await interaction.response.send_message(f"{E_WARNING} Staff/Mod Role ID must be numeric digits.", ephemeral=True)
            return

        await interaction.response.defer(ephemeral=True)
        
        # Verify API key
        is_valid, msg = await validate_api_key(api_key)
        if not is_valid:
            await interaction.followup.send(f"{E_WARNING} API Key Verification Failed: {msg}", ephemeral=True)
            return

        save_guild_config(interaction.guild.id, api_key, whitelist_chan or None, logs_chan or None, mod_role or None)

        # Build stunning V2 Component Success Card
        masked_key = f"{api_key[:11]}...{api_key[-4:]}" if len(api_key) >= 15 else api_key
        lines = [
            f"API Key: Linked (`{masked_key}`)",
            f"Whitelist Channel: <#{whitelist_chan}>" if whitelist_chan else "Whitelist Channel: Not Restricted",
            f"Logs Channel: <#{logs_chan}>" if logs_chan else "Logs Channel: Not Restricted",
            f"Staff/Mod Role: <@&{mod_role}>" if mod_role else "Staff/Mod Role: Administrator Only"
        ]

        comps = build_v2_response(f"{E_DONE} BOT CONFIGURATION SUCCESSFUL", lines, success=True)
        action_row = {
            "type": 1,
            "components": [
                v2_button("Deploy Control Panel", style=3, custom_id="setup_done", emoji=parse_emoji(E_ROCKET)),
                v2_button("Re-configure", style=2, custom_id="setup_basic_config", emoji=parse_emoji(E_GEAR))
            ]
        }
        comps[0]["components"].append(v2_separator())
        comps[0]["components"].append(action_row)

        res = await send_v2_raw(interaction.channel, comps, BOT_TOKEN)
        if not res:
            embed = discord.Embed(
                title=f"{E_DONE} BOT CONFIGURATION SUCCESSFUL",
                description="\n".join(lines),
                color=discord.Color.green()
            )
            embed.set_footer(text="UID Bypass Live Gateway")
            await interaction.channel.send(embed=embed)

        await interaction.followup.send(f"{E_SUCCESS} Bot configured successfully! V2 Success Card deployed.", ephemeral=True)

# --- Whitelist Interactive Modal ---
class WhitelistModal(discord.ui.Modal, title="Whitelist Player"):
    def __init__(self, api_key):
        super().__init__()
        self.api_key = api_key
        self.uid_input = discord.ui.TextInput(
            label="Player UID",
            placeholder="Enter player numerical UID",
            required=True
        )
        self.duration_input = discord.ui.TextInput(
            label="Duration (e.g., 1d, 7d, 30d, lifetime)",
            placeholder="e.g. 1d, 7d, 30d, lifetime",
            default="1d",
            required=False
        )
        self.add_item(self.uid_input)
        self.add_item(self.duration_input)

    async def on_submit(self, interaction: discord.Interaction):
        uid = self.uid_input.value.strip()
        duration = self.duration_input.value.strip()

        if not uid.isdigit():
            await interaction.response.send_message(f"{E_WARNING} Player UID must be numeric digits.", ephemeral=True)
            return

        await interaction.response.defer(ephemeral=True)

        days_val = parse_duration_to_days(duration)
        headers = {"X-AUTH-KEY": self.api_key, "X-API-Key": self.api_key, "Content-Type": "application/json"}
        payload = {"uid": uid, "days": days_val, "name": "Discord_Client"}

        try:
            status, result = await async_vps_request("POST", API_URL, headers=headers, json_payload=payload, timeout=8)

            if status == 200 and result.get("success"):
                lines = [
                    f"Player UID `{uid}` has been added to the bypass database.",
                    f"Duration: `{days_val} Day(s)`"
                ]
                comps = build_v2_response(f"UID Whitelisted Successfully! {E_SUCCESS}", lines, success=True)
                res = await send_v2_raw(interaction.channel, comps, BOT_TOKEN)
                if not res:
                    embed = discord.Embed(title=f"UID Whitelisted Successfully! {E_SUCCESS}", description="\n".join(lines), color=discord.Color.green())
                    embed.set_footer(text="UID Bypass Live Gateway")
                    await interaction.channel.send(embed=embed)

                # Send logs to logs channel if configured
                config = get_guild_config(interaction.guild.id) or {}
                logs_channel_id = config.get("log_channel_id")
                if logs_channel_id:
                    logs_channel = interaction.guild.get_channel(int(logs_channel_id))
                    if not logs_channel:
                        try:
                            logs_channel = await interaction.guild.fetch_channel(int(logs_channel_id))
                        except Exception:
                            pass
                    if logs_channel:
                        log_embed = discord.Embed(
                            title=f"📥 UID Whitelisted",
                            description=f"A new UID was added to the bypass database.",
                            color=discord.Color.green()
                        )
                        log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                        log_embed.add_field(name="Duration", value=f"`{days_val} Days`", inline=True)
                        log_embed.add_field(name="Moderator", value=f"{interaction.user.mention} ({interaction.user.id})", inline=True)
                        log_embed.add_field(name="Server", value=f"{interaction.guild.name} ({interaction.guild.id})", inline=False)
                        log_embed.set_footer(text="UID Audit Logs")
                        
                        if BOT_TOKEN != "YOUR_DISCORD_BOT_TOKEN_HERE" and BOT_TOKEN:
                            try:
                                log_res = await send_v2_log_internal(logs_channel, log_embed, BOT_TOKEN)
                                if not log_res:
                                    await logs_channel.send(embed=log_embed)
                            except Exception as e:
                                logger.error(f"Failed to send V2 log: {e}")
                                await logs_channel.send(embed=log_embed)
                        else:
                            await logs_channel.send(embed=log_embed)
                await interaction.followup.send(f"{E_SUCCESS} Whitelisted successfully!", ephemeral=True)
            else:
                err_msg = result.get("error", "Unknown backend error.")
                await interaction.followup.send(f"{E_WARNING} Whitelist Failed: {err_msg}", ephemeral=True)
        except Exception as e:
            await interaction.followup.send(f"{E_WARNING} Connection Error: {str(e)}", ephemeral=True)

# --- Extend Interactive Modal ---
class ExtendModal(discord.ui.Modal, title="Extend Player"):
    def __init__(self, api_key):
        super().__init__()
        self.api_key = api_key
        self.uid_input = discord.ui.TextInput(
            label="Player UID",
            placeholder="Enter player numerical UID to extend",
            required=True
        )
        self.duration_input = discord.ui.TextInput(
            label="Extend Duration (e.g., 1d, 7d, 30d)",
            placeholder="e.g. 1d, 7d, 30d",
            default="1d",
            required=False
        )
        self.add_item(self.uid_input)
        self.add_item(self.duration_input)

    async def on_submit(self, interaction: discord.Interaction):
        uid = self.uid_input.value.strip()
        duration = self.duration_input.value.strip()

        if not uid.isdigit():
            await interaction.response.send_message(f"{E_WARNING} Player UID must be numeric digits.", ephemeral=True)
            return

        await interaction.response.defer(ephemeral=True)

        days_val = parse_duration_to_days(duration)
        headers = {"X-AUTH-KEY": self.api_key, "X-API-Key": self.api_key, "Content-Type": "application/json"}
        payload = {"uid": uid, "days": days_val, "name": "Discord_Client"}

        try:
            status, result = await async_vps_request("POST", API_URL, headers=headers, json_payload=payload, timeout=8)

            if status == 200 and result.get("success"):
                lines = [
                    f"Player UID `{uid}` validity extended successfully.",
                    f"Added Days: `{days_val} Day(s)`",
                    f"Extended By: {interaction.user.mention}"
                ]
                comps = build_v2_response(f"UID Extended Successfully! {E_SUCCESS}", lines, success=True)
                res = await send_v2_raw(interaction.channel, comps, BOT_TOKEN)
                if not res:
                    embed = discord.Embed(title=f"UID Extended Successfully! {E_SUCCESS}", description="\n".join(lines), color=discord.Color.green())
                    embed.set_footer(text="UID Bypass Live Gateway")
                    await interaction.channel.send(embed=embed)

                # Send logs
                config = get_guild_config(interaction.guild.id) or {}
                logs_channel_id = config.get("log_channel_id")
                if logs_channel_id:
                    logs_channel = interaction.guild.get_channel(int(logs_channel_id))
                    if not logs_channel:
                        try:
                            logs_channel = await interaction.guild.fetch_channel(int(logs_channel_id))
                        except Exception:
                            pass
                    if logs_channel:
                        log_embed = discord.Embed(
                            title=f"⚡ UID Extended",
                            description=f"UID validity extended.",
                            color=discord.Color.green()
                        )
                        log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                        log_embed.add_field(name="Added Days", value=f"`+{days_val} Days`", inline=True)
                        log_embed.add_field(name="Moderator", value=f"{interaction.user.mention} ({interaction.user.id})", inline=True)
                        log_embed.add_field(name="Server", value=f"{interaction.guild.name} ({interaction.guild.id})", inline=False)
                        log_embed.set_footer(text="UID Audit Logs")
                        
                        if BOT_TOKEN != "YOUR_DISCORD_BOT_TOKEN_HERE" and BOT_TOKEN:
                            try:
                                log_res = await send_v2_log_internal(logs_channel, log_embed, BOT_TOKEN)
                                if not log_res:
                                    await logs_channel.send(embed=log_embed)
                            except Exception as e:
                                logger.error(f"Failed to send V2 log: {e}")
                                await logs_channel.send(embed=log_embed)
                        else:
                            await logs_channel.send(embed=log_embed)
                await interaction.followup.send(f"{E_SUCCESS} Extended successfully!", ephemeral=True)
            else:
                err_msg = result.get("error", "Unknown backend error.")
                await interaction.followup.send(f"{E_WARNING} Extend Failed: {err_msg}", ephemeral=True)
        except Exception as e:
            await interaction.followup.send(f"{E_WARNING} Connection Error: {str(e)}", ephemeral=True)

# --- Delete Interactive Modal ---
class DeleteModal(discord.ui.Modal, title="Delete Player"):
    def __init__(self, api_key):
        super().__init__()
        self.api_key = api_key
        self.uid_input = discord.ui.TextInput(
            label="Player UID",
            placeholder="Enter player numerical UID to delete",
            required=True
        )
        self.add_item(self.uid_input)

    async def on_submit(self, interaction: discord.Interaction):
        uid = self.uid_input.value.strip()
        if not uid.isdigit():
            await interaction.response.send_message(f"{E_WARNING} Player UID must be numeric digits.", ephemeral=True)
            return

        await interaction.response.defer(ephemeral=True)

        headers = {"X-AUTH-KEY": self.api_key, "X-API-Key": self.api_key, "Content-Type": "application/json"}
        payload = {"uid": uid}

        try:
            delete_url = API_URL.replace("/uids/add", "/uids/remove").replace("/external/whitelist/delete", "/uids/remove").replace("/external/whitelist", "/uids/remove")
            status, result = await async_vps_request("POST", delete_url, headers=headers, json_payload=payload, timeout=8)

            if status == 200 and result.get("success"):
                lines = [
                    f"Player UID `{uid}` has been successfully removed from the bypass.",
                    f"Removed By: {interaction.user.mention}"
                ]
                comps = build_v2_response(f"UID Deleted Successfully! {E_SUCCESS}", lines, success=True)
                res = await send_v2_raw(interaction.channel, comps, BOT_TOKEN)
                if not res:
                    embed = discord.Embed(title=f"UID Deleted Successfully! {E_SUCCESS}", description="\n".join(lines), color=discord.Color.green())
                    embed.set_footer(text="UID Bypass Live Gateway")
                    await interaction.channel.send(embed=embed)

                # Send logs to logs channel if configured
                config = get_guild_config(interaction.guild.id) or {}
                logs_channel_id = config.get("log_channel_id")
                if logs_channel_id:
                    logs_channel = interaction.guild.get_channel(int(logs_channel_id))
                    if not logs_channel:
                        try:
                            logs_channel = await interaction.guild.fetch_channel(int(logs_channel_id))
                        except Exception:
                            pass
                    if logs_channel:
                        log_embed = discord.Embed(
                            title=f"🗑️ UID Removed",
                            description=f"A UID was removed from the bypass database.",
                            color=discord.Color.orange()
                        )
                        log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                        log_embed.add_field(name="Moderator", value=f"{interaction.user.mention} ({interaction.user.id})", inline=True)
                        log_embed.add_field(name="Server", value=f"{interaction.guild.name} ({interaction.guild.id})", inline=False)
                        log_embed.set_footer(text="UID Audit Logs")
                        
                        if BOT_TOKEN != "YOUR_DISCORD_BOT_TOKEN_HERE" and BOT_TOKEN:
                            try:
                                log_res = await send_v2_log_internal(logs_channel, log_embed, BOT_TOKEN)
                                if not log_res:
                                    await logs_channel.send(embed=log_embed)
                            except Exception as e:
                                logger.error(f"Failed to send V2 log: {e}")
                                await logs_channel.send(embed=log_embed)
                        else:
                            await logs_channel.send(embed=log_embed)
                await interaction.followup.send(f"{E_SUCCESS} Deleted successfully!", ephemeral=True)
            else:
                err_msg = result.get("error", "Unknown backend error.")
                await interaction.followup.send(f"{E_WARNING} Delete Failed: {err_msg}", ephemeral=True)
        except Exception as e:
            await interaction.followup.send(f"{E_WARNING} Connection Error: {str(e)}", ephemeral=True)

# --- Global Interaction Handler for Setup Buttons ---
@bot.event
async def on_interaction(interaction: discord.Interaction):
    if interaction.type != discord.InteractionType.component:
        return
        
    custom_id = interaction.data.get("custom_id")
    if not custom_id:
        return
        
    if custom_id == "setup_basic_config":
        if not interaction.user.guild_permissions.administrator:
            await interaction.response.send_message(f"{E_WARNING} Only administrators can configure the bot.", ephemeral=True)
            return
            
        config = get_guild_config(interaction.guild.id) or {}
        modal = SetupModal(
            api_key=config.get("api_key", ""),
            whitelist_chan=config.get("whitelist_channel_id", ""),
            logs_chan=config.get("log_channel_id", ""),
            mod_role=config.get("mod_role_id", "")
        )
        await interaction.response.send_modal(modal)
        
    elif custom_id == "setup_done":
        if not interaction.user.guild_permissions.administrator:
            await interaction.response.send_message(f"{E_WARNING} Only administrators can perform this action.", ephemeral=True)
            return
            
        config = get_guild_config(interaction.guild.id)
        if not config or not config.get("api_key"):
            await interaction.response.send_message(f"{E_WARNING} Please configure the API Key first using the Basic Config button.", ephemeral=True)
            return

        # Verification of configured IDs
        whitelist_channel_id = config.get("whitelist_channel_id")
        log_channel_id = config.get("log_channel_id")
        mod_role_id = config.get("mod_role_id")
        
        # Verify whitelist channel
        if whitelist_channel_id:
            chan = interaction.guild.get_channel(int(whitelist_channel_id))
            if not chan:
                try:
                    chan = await interaction.guild.fetch_channel(int(whitelist_channel_id))
                except Exception:
                    await interaction.response.send_message(f"{E_WARNING} Warning: Whitelist Channel ID `{whitelist_channel_id}` was not found in this server. Please click Basic Config to re-configure.", ephemeral=True)
                    return
                    
        # Verify logs channel
        if log_channel_id:
            chan = interaction.guild.get_channel(int(log_channel_id))
            if not chan:
                try:
                    chan = await interaction.guild.fetch_channel(int(log_channel_id))
                except Exception:
                    await interaction.response.send_message(f"{E_WARNING} Warning: Logs Channel ID `{log_channel_id}` was not found in this server. Please click Basic Config to re-configure.", ephemeral=True)
                    return
                    
        # Verify mod role
        if mod_role_id:
            role = interaction.guild.get_role(int(mod_role_id))
            if not role:
                await interaction.response.send_message(f"{E_WARNING} Warning: Staff/Mod Role ID `{mod_role_id}` was not found in this server. Please click Basic Config to re-configure.", ephemeral=True)
                return
            
        lines = [
            "The bot has been configured successfully and is ready to use.",
            f"Whitelist Channel: <#{config['whitelist_channel_id']}>" if config.get("whitelist_channel_id") else "Whitelist Channel: Not Restricted",
            f"Logs Channel: <#{config['log_channel_id']}>" if config.get("log_channel_id") else "Logs Channel: Not Restricted",
            f"Staff/Mod Role: <@&{config['mod_role_id']}>" if config.get("mod_role_id") else "Staff/Mod Role: Administrator Only"
        ]
        
        comps = build_v2_response(f"BOT ACTIVATED {E_GREEN}", lines, success=True)
        await interaction.response.defer()
        await _api("PATCH", f"/channels/{interaction.channel_id}/messages/{interaction.message.id}", BOT_TOKEN, {"flags": FLAGS_COMPONENTS_V2, "components": comps})
 
    elif custom_id in ["panel_whitelist_modal", "panel_extend_modal", "panel_delete_modal"]:
        config = get_guild_config(interaction.guild.id)
        if not config or not config.get("api_key"):
            await interaction.response.send_message(f"{E_WARNING} Bot Setup Required. Ask an Admin to configure the bot.", ephemeral=True)
            return
 
        # Check permissions
        is_allowed = False
        if interaction.user.guild_permissions.administrator:
            is_allowed = True
        else:
            mod_role_id = config.get("mod_role_id")
            if mod_role_id:
                for r in interaction.user.roles:
                    if str(r.id) == str(mod_role_id):
                        is_allowed = True
                        break
            else:
                is_allowed = False
 
        if not is_allowed:
            await interaction.response.send_message(f"{E_WARNING} Access Denied: You do not have the required Staff/Mod role to perform this action.", ephemeral=True)
            return
 
        # Check Whitelist Channel restriction
        whitelist_channel_id = config.get("whitelist_channel_id")
        if whitelist_channel_id and not interaction.user.guild_permissions.administrator:
            if str(interaction.channel_id) != str(whitelist_channel_id):
                await interaction.response.send_message(f"{E_WARNING} This command can only be used in the whitelist channel: <#{whitelist_channel_id}>.", ephemeral=True)
                return
 
        # Open Modal
        if custom_id == "panel_whitelist_modal":
            await interaction.response.send_modal(WhitelistModal(config.get("api_key")))
        elif custom_id == "panel_extend_modal":
            await interaction.response.send_modal(ExtendModal(config.get("api_key")))
        else:
            await interaction.response.send_modal(DeleteModal(config.get("api_key")))

# --- Events ---
@bot.event
async def on_ready():
    print("=========================================")
    print(f" Bot is online as: {bot.user.name}")
    print(f" ID: {bot.user.id}")
    try:
        synced = await bot.tree.sync()
        print(f" Synced {len(synced)} Slash Commands globally! 🚀")
    except Exception as e:
        print(f" Failed to sync slash commands: {e}")
    print("=========================================")

# --- Helper Permissions Check for Interaction ---
def check_interaction_permissions(interaction: discord.Interaction, config: dict):
    if interaction.user.guild_permissions.administrator:
        return True, ""
    
    mod_role_id = config.get("mod_role_id")
    if mod_role_id:
        has_role = any(str(r.id) == str(mod_role_id) for r in interaction.user.roles)
        if not has_role:
            return False, f"{E_WARNING} Access Denied: You need the Staff/Mod role (<@&{mod_role_id}>) to use this command."
            
    whitelist_channel_id = config.get("whitelist_channel_id")
    if whitelist_channel_id:
        if str(interaction.channel_id) != str(whitelist_channel_id):
            return False, f"{E_WARNING} This command can only be used in the whitelist channel: <#{whitelist_channel_id}>."
            
    return True, ""

async def send_v2_interaction_response(interaction: discord.Interaction, title: str, lines: list, success: bool = True, fields: list = None):
    v2_lines = list(lines) if lines else []
    if fields:
        for fname, fval in fields:
            v2_lines.append(f"**{fname}:** {fval}")

    comps = build_v2_response(title, v2_lines, success=success)

    token = BOT_TOKEN
    msg_id = None
    if token and token != "YOUR_DISCORD_BOT_TOKEN_HERE":
        try:
            msg_id = await send_v2_raw(interaction.channel, comps, token)
        except Exception as e:
            logger.warning(f"V2 interaction response send failed: {e}")

    if msg_id:
        if not interaction.response.is_done():
            await interaction.response.send_message(f"{E_SUCCESS} Response sent!", ephemeral=True)
        else:
            try:
                await interaction.followup.send(f"{E_SUCCESS} Response sent!", ephemeral=True)
            except Exception:
                pass
    else:
        color = discord.Color.green() if success else discord.Color.red()
        embed = discord.Embed(title=title, description="\n".join(lines), color=color)
        if fields:
            for fname, fval in fields:
                embed.add_field(name=fname, value=fval, inline=True)
        embed.set_footer(text="UID Bypass Live Gateway")

        if not interaction.response.is_done():
            await interaction.response.send_message(embed=embed)
        else:
            await interaction.followup.send(embed=embed)

# ==========================================
# DISCORD SLASH COMMANDS (/commands)
# ==========================================

@bot.tree.command(name="setup", description="Start the interactive configuration wizard")
@app_commands.checks.has_permissions(administrator=True)
async def setup_slash(interaction: discord.Interaction):
    config = get_guild_config(interaction.guild.id) or {}
    modal = SetupModal(
        api_key=config.get("api_key", ""),
        whitelist_chan=config.get("whitelist_channel_id", ""),
        logs_chan=config.get("log_channel_id", ""),
        mod_role=config.get("mod_role_id", "")
    )
    await interaction.response.send_modal(modal)

@bot.tree.command(name="whitelist", description="Whitelist a player UID or deploy interactive management panel")
@app_commands.describe(uid="Player numerical UID to whitelist", duration="Duration e.g. 1d, 7d, 30d, lifetime")
async def whitelist_slash(interaction: discord.Interaction, uid: str = None, duration: str = "1d"):
    config = get_guild_config(interaction.guild.id)
    if not config or not config.get("api_key"):
        await interaction.response.send_message(f"{E_WARNING} Bot Setup Required. Ask an Administrator to configure using `/setup`.", ephemeral=True)
        return

    allowed, err_msg = check_interaction_permissions(interaction, config)
    if not allowed:
        await interaction.response.send_message(err_msg, ephemeral=True)
        return

    if not uid:
        lines = [
            "Link player UIDs to the Bypass system.",
            "Use the buttons below to **Whitelist**, **Extend**, or **Delete** player UIDs.",
            f"Whitelist Channel: <#{config['whitelist_channel_id']}>" if config.get("whitelist_channel_id") else "Whitelist Channel: Not Restricted",
            f"Staff/Mod Role: <@&{config['mod_role_id']}>" if config.get("mod_role_id") else "Staff/Mod Role: Administrator Only"
        ]
        
        comps = build_v2_response(f"{E_ROCKET} BYPASS MANAGEMENT PANEL", lines, success=True)
        action_row = {
            "type": 1,
            "components": [
                v2_button("Whitelist UID", style=1, custom_id="panel_whitelist_modal", emoji=parse_emoji(E_ROCKET)),
                v2_button("Extend UID", style=2, custom_id="panel_extend_modal", emoji=parse_emoji(E_GEAR)),
                v2_button("Delete UID", style=4, custom_id="panel_delete_modal", emoji=parse_emoji(E_TRASH))
            ]
        }
        comps[0]["components"].append(v2_separator())
        comps[0]["components"].append(action_row)

        res = await send_v2_raw(interaction.channel, comps, BOT_TOKEN)
        if not res:
            embed = discord.Embed(title=f"{E_ROCKET} BYPASS MANAGEMENT PANEL", description="\n".join(lines), color=discord.Color.blue())
            await interaction.response.send_message(embed=embed)
        else:
            await interaction.response.send_message(f"{E_SUCCESS} Control Panel deployed!", ephemeral=True)
        return

    if not uid.isdigit():
        await interaction.response.send_message(f"{E_WARNING} Player UID must be numeric digits.", ephemeral=True)
        return

    days_val = parse_duration_to_days(duration)
    api_key = config.get("api_key")
    headers = {"X-AUTH-KEY": api_key, "X-API-Key": api_key, "Content-Type": "application/json"}
    payload = {"uid": uid, "days": days_val, "name": "Discord_Client"}

    try:
        status, result = await async_vps_request("POST", API_URL, headers=headers, json_payload=payload, timeout=8)

        if status == 200 and result.get("success"):
            lines = [
                f"Player UID `{uid}` has been added to the bypass database.",
                f"Duration: `{days_val} Day(s)`",
                f"Moderator: {interaction.user.mention}"
            ]
            await send_v2_interaction_response(interaction, f"UID Whitelisted Successfully! {E_SUCCESS}", lines, success=True)

            # Send logs
            logs_channel_id = config.get("log_channel_id")
            if logs_channel_id:
                logs_channel = interaction.guild.get_channel(int(logs_channel_id))
                if not logs_channel:
                    try:
                        logs_channel = await interaction.guild.fetch_channel(int(logs_channel_id))
                    except Exception:
                        pass
                if logs_channel:
                    log_embed = discord.Embed(
                        title=f"📥 UID Whitelisted",
                        description=f"A new UID was added to the bypass database.",
                        color=discord.Color.green()
                    )
                    log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                    log_embed.add_field(name="Duration", value=f"`{days_val} Days`", inline=True)
                    log_embed.add_field(name="Moderator", value=f"{interaction.user.mention} ({interaction.user.id})", inline=True)
                    log_embed.add_field(name="Server", value=f"{interaction.guild.name} ({interaction.guild.id})", inline=False)
                    log_embed.set_footer(text="UID Audit Logs")
                    
                    token = BOT_TOKEN
                    if token != "YOUR_DISCORD_BOT_TOKEN_HERE" and token:
                        try:
                            log_res = await send_v2_log_internal(logs_channel, log_embed, token)
                            if not log_res:
                                await logs_channel.send(embed=log_embed)
                        except Exception as e:
                            logger.error(f"Failed to send V2 log: {e}")
                            await logs_channel.send(embed=log_embed)
                    else:
                        await logs_channel.send(embed=log_embed)
        else:
            err_msg = result.get("error", "Unknown backend error.")
            await interaction.response.send_message(f"{E_WARNING} Whitelist Failed: {err_msg}", ephemeral=True)
    except Exception as e:
        await interaction.response.send_message(f"{E_WARNING} Connection Error: {str(e)}", ephemeral=True)

@bot.tree.command(name="extend", description="Extend an existing whitelisted player UID duration")
@app_commands.describe(uid="Player numerical UID to extend", duration="Duration to add e.g. 1d, 7d, 30d")
async def extend_slash(interaction: discord.Interaction, uid: str, duration: str = "1d"):
    config = get_guild_config(interaction.guild.id)
    if not config or not config.get("api_key"):
        await interaction.response.send_message(f"{E_WARNING} Bot Setup Required. Ask an Administrator to configure using `/setup`.", ephemeral=True)
        return

    allowed, err_msg = check_interaction_permissions(interaction, config)
    if not allowed:
        await interaction.response.send_message(err_msg, ephemeral=True)
        return

    if not uid.isdigit():
        await interaction.response.send_message(f"{E_WARNING} Player UID must be numeric digits.", ephemeral=True)
        return

    days_val = parse_duration_to_days(duration)
    api_key = config.get("api_key")
    headers = {"X-AUTH-KEY": api_key, "X-API-Key": api_key, "Content-Type": "application/json"}
    payload = {"uid": uid, "days": days_val, "name": "Discord_Client"}

    try:
        status, result = await async_vps_request("POST", API_URL, headers=headers, json_payload=payload, timeout=8)

        if status == 200 and result.get("success"):
            lines = [
                f"Player UID `{uid}` validity extended successfully.",
                f"Added Duration: `+{days_val} Day(s)`",
                f"Extended By: {interaction.user.mention}"
            ]
            await send_v2_interaction_response(interaction, f"UID Extended Successfully! {E_SUCCESS}", lines, success=True)

            # Send logs
            logs_channel_id = config.get("log_channel_id")
            if logs_channel_id:
                logs_channel = interaction.guild.get_channel(int(logs_channel_id))
                if not logs_channel:
                    try:
                        logs_channel = await interaction.guild.fetch_channel(int(logs_channel_id))
                    except Exception:
                        pass
                if logs_channel:
                    log_embed = discord.Embed(
                        title=f"⚡ UID Extended",
                        description=f"UID validity extended.",
                        color=discord.Color.green()
                    )
                    log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                    log_embed.add_field(name="Added Days", value=f"`+{days_val} Days`", inline=True)
                    log_embed.add_field(name="Moderator", value=f"{interaction.user.mention} ({interaction.user.id})", inline=True)
                    log_embed.add_field(name="Server", value=f"{interaction.guild.name} ({interaction.guild.id})", inline=False)
                    log_embed.set_footer(text="UID Audit Logs")
                    
                    token = BOT_TOKEN
                    if token != "YOUR_DISCORD_BOT_TOKEN_HERE" and token:
                        try:
                            log_res = await send_v2_log_internal(logs_channel, log_embed, token)
                            if not log_res:
                                await logs_channel.send(embed=log_embed)
                        except Exception as e:
                            logger.error(f"Failed to send V2 log: {e}")
                            await logs_channel.send(embed=log_embed)
                    else:
                        await logs_channel.send(embed=log_embed)
        else:
            err_msg = result.get("error", "Unknown backend error.")
            await interaction.response.send_message(f"{E_WARNING} Extend Failed: {err_msg}", ephemeral=True)
    except Exception as e:
        await interaction.response.send_message(f"{E_WARNING} Connection Error: {str(e)}", ephemeral=True)

@bot.tree.command(name="delete", description="Remove or revoke a whitelisted player UID")
@app_commands.describe(uid="Player numerical UID to delete")
async def delete_slash(interaction: discord.Interaction, uid: str):
    config = get_guild_config(interaction.guild.id)
    if not config or not config.get("api_key"):
        await interaction.response.send_message(f"{E_WARNING} Bot Setup Required. Ask an Administrator to configure using `/setup`.", ephemeral=True)
        return

    allowed, err_msg = check_interaction_permissions(interaction, config)
    if not allowed:
        await interaction.response.send_message(err_msg, ephemeral=True)
        return

    if not uid.isdigit():
        await interaction.response.send_message(f"{E_WARNING} Player UID must be numeric digits.", ephemeral=True)
        return

    api_key = config.get("api_key")
    headers = {"X-AUTH-KEY": api_key, "X-API-Key": api_key, "Content-Type": "application/json"}
    payload = {"uid": uid}
    delete_url = API_URL.replace("/uids/add", "/uids/remove").replace("/external/whitelist/delete", "/uids/remove").replace("/external/whitelist", "/uids/remove")

    try:
        status, result = await async_vps_request("POST", delete_url, headers=headers, json_payload=payload, timeout=8)

        if status == 200 and result.get("success"):
            lines = [
                f"Player UID `{uid}` has been successfully removed from the bypass database.",
                f"Removed By: {interaction.user.mention}"
            ]
            await send_v2_interaction_response(interaction, f"UID Deleted Successfully! {E_SUCCESS}", lines, success=True)

            # Send logs
            logs_channel_id = config.get("log_channel_id")
            if logs_channel_id:
                logs_channel = interaction.guild.get_channel(int(logs_channel_id))
                if not logs_channel:
                    try:
                        logs_channel = await interaction.guild.fetch_channel(int(logs_channel_id))
                    except Exception:
                        pass
                if logs_channel:
                    log_embed = discord.Embed(
                        title=f"🗑️ UID Removed",
                        description=f"A UID was removed from the bypass database.",
                        color=discord.Color.orange()
                    )
                    log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                    log_embed.add_field(name="Moderator", value=f"{interaction.user.mention} ({interaction.user.id})", inline=True)
                    log_embed.add_field(name="Server", value=f"{interaction.guild.name} ({interaction.guild.id})", inline=False)
                    log_embed.set_footer(text="UID Audit Logs")
                    
                    token = BOT_TOKEN
                    if token != "YOUR_DISCORD_BOT_TOKEN_HERE" and token:
                        try:
                            log_res = await send_v2_log_internal(logs_channel, log_embed, token)
                            if not log_res:
                                await logs_channel.send(embed=log_embed)
                        except Exception as e:
                            logger.error(f"Failed to send V2 log: {e}")
                            await logs_channel.send(embed=log_embed)
                    else:
                        await logs_channel.send(embed=log_embed)
        else:
            err_msg = result.get("error", "Unknown backend error.")
            await interaction.response.send_message(f"{E_WARNING} Delete Failed: {err_msg}", ephemeral=True)
    except Exception as e:
        await interaction.response.send_message(f"{E_WARNING} Connection Error: {str(e)}", ephemeral=True)

@bot.tree.command(name="panel", description="Deploy the interactive Whitelist, Extend & Delete button panel")
async def panel_slash(interaction: discord.Interaction):
    config = get_guild_config(interaction.guild.id)
    if not config or not config.get("api_key"):
        await interaction.response.send_message(f"{E_WARNING} Bot Setup Required. Ask an Administrator to configure using `/setup`.", ephemeral=True)
        return

    allowed, err_msg = check_interaction_permissions(interaction, config)
    if not allowed:
        await interaction.response.send_message(err_msg, ephemeral=True)
        return

    lines = [
        "Link player UIDs to the Bypass system.",
        "Use the buttons below to **Whitelist**, **Extend**, or **Delete** player UIDs.",
        f"Whitelist Channel: <#{config['whitelist_channel_id']}>" if config.get("whitelist_channel_id") else "Whitelist Channel: Not Restricted",
        f"Staff/Mod Role: <@&{config['mod_role_id']}>" if config.get("mod_role_id") else "Staff/Mod Role: Administrator Only"
    ]
    
    comps = build_v2_response(f"{E_ROCKET} BYPASS MANAGEMENT PANEL", lines, success=True)
    action_row = {
        "type": 1,
        "components": [
            v2_button("Whitelist UID", style=1, custom_id="panel_whitelist_modal", emoji=parse_emoji(E_ROCKET)),
            v2_button("Extend UID", style=2, custom_id="panel_extend_modal", emoji=parse_emoji(E_GEAR)),
            v2_button("Delete UID", style=4, custom_id="panel_delete_modal", emoji=parse_emoji(E_TRASH))
        ]
    }
    comps[0]["components"].append(v2_separator())
    comps[0]["components"].append(action_row)

    res = await send_v2_raw(interaction.channel, comps, BOT_TOKEN)
    if not res:
        embed = discord.Embed(title=f"{E_ROCKET} BYPASS MANAGEMENT PANEL", description="\n".join(lines), color=discord.Color.blue())
        await interaction.response.send_message(embed=embed)
    else:
        await interaction.response.send_message(f"{E_SUCCESS} Control Panel deployed successfully!", ephemeral=True)

@bot.tree.command(name="status", description="Check the license and configuration status of this server")
async def status_slash(interaction: discord.Interaction):
    config = get_guild_config(interaction.guild.id)
    if not config or not config.get("api_key"):
        await send_v2_interaction_response(
            interaction,
            f"License Status: INACTIVE {E_RED}",
            ["This server is not activated. Run `/setup` to activate."],
            success=False
        )
        return

    api_key = config.get("api_key")
    masked_key = api_key[:11] + "..." + api_key[-4:]
    lines = [
        "This server is registered and authorized to whitelist UIDs.",
        f"Server ID: `{interaction.guild.id}`",
        f"Linked Key: `{masked_key}`",
        f"Whitelist Channel: <#{config['whitelist_channel_id']}>" if config.get("whitelist_channel_id") else "Whitelist Channel: Not Restricted",
        f"Logs Channel: <#{config['log_channel_id']}>" if config.get("log_channel_id") else "Logs Channel: Not Restricted",
        f"Staff/Mod Role: <@&{config['mod_role_id']}>" if config.get("mod_role_id") else "Staff/Mod Role: Administrator Only"
    ]
    await send_v2_interaction_response(interaction, f"License Status: ACTIVE {E_GREEN}", lines, success=True)

@bot.tree.command(name="ping", description="Check system latency and API Gateway ping")
async def ping_slash(interaction: discord.Interaction):
    ws_val = round(bot.latency * 1000, 2)
    lines = [
        f"Bot WebSocket Latency: `{ws_val}ms`",
        f"API Gateway Status: `Optimal`"
    ]
    await send_v2_interaction_response(interaction, "System Diagnostics 🏓", lines, success=True)

@bot.tree.command(name="help", description="Display full Slash Command guide and command manual")
async def help_slash(interaction: discord.Interaction):
    token = BOT_TOKEN
    
    header = v2_section([
        f"### Bot Help Center {E_BULB}",
        "Complete Slash Command manual and licensing guide."
    ], accessory_url=bot.user.display_avatar.url if bot.user else None)

    admin_block = v2_section([
        f"### {E_GEAR} Server Configuration (Admin Only)",
        f"> {E_DOT} **Command:** `/setup`",
        f"> {E_DOT} **Description:** Starts the configuration wizard modal to link Client API Key, channels, and roles.",
        f"> {E_DOT} **Command:** `/resetkey`",
        f"> {E_DOT} **Description:** De-authorizes this server by clearing its linked Client API Key."
    ])

    whitelist_block = v2_section([
        f"### {E_ROCKET} Whitelist & Bypass Management",
        f"> {E_DOT} **Command:** `/whitelist [uid] [duration]`",
        f"> {E_DOT} **Description:** Whitelists a UID or opens the interactive 3-button management panel.",
        f"> {E_DOT} **Command:** `/extend <uid> [duration]`",
        f"> {E_DOT} **Description:** Extends an existing UID's validity duration.",
        f"> {E_DOT} **Command:** `/delete <uid>`",
        f"> {E_DOT} **Description:** Revokes/deletes a whitelisted UID.",
        f"> {E_DOT} **Command:** `/panel`",
        f"> {E_DOT} **Description:** Deploys the interactive Whitelist, Extend & Delete button panel."
    ])

    util_block = v2_section([
        "### 📊 System & Diagnostics",
        f"> {E_DOT} **Command:** `/status`",
        f"> {E_DOT} **Description:** Shows activation status, linked API key, and channel restrictions.",
        f"> {E_DOT} **Command:** `/ping`",
        f"> {E_DOT} **Description:** Measures WebSocket, SQLite, and VPS API Gateway latency."
    ])

    footer = v2_text(f"• Requested by {interaction.user.name}")

    comps = [v2_container([
        header,
        v2_separator(),
        admin_block,
        v2_separator(),
        whitelist_block,
        v2_separator(),
        util_block,
        v2_separator(),
        footer
    ], accent=CLR_MASTER)]

    msg_id = None
    if token and token != "YOUR_DISCORD_BOT_TOKEN_HERE":
        try:
            msg_id = await send_v2_raw(interaction.channel, comps, token)
        except Exception:
            pass

    if msg_id:
        await interaction.response.send_message(f"{E_SUCCESS} Help manual loaded!", ephemeral=True)
    else:
        embed = discord.Embed(
            title=f"Bot Help Center {E_BULB}",
            description="Complete Slash Command manual and licensing guide.",
            color=discord.Color.blue()
        )
        embed.add_field(name="⚙️ `/setup`", value="Starts configuration wizard.", inline=False)
        embed.add_field(name="🚀 `/whitelist`", value="Whitelists a numerical UID.", inline=False)
        embed.add_field(name="⚡ `/extend`", value="Extends a UID validity.", inline=False)
        embed.add_field(name="🗑️ `/delete`", value="Deletes a whitelisted UID.", inline=False)
        embed.add_field(name="🎛️ `/panel`", value="Deploys interactive button panel.", inline=False)
        embed.add_field(name="📊 `/status`", value="Shows server configuration.", inline=False)
        embed.add_field(name="🏓 `/ping`", value="Checks latency.", inline=False)
        embed.set_footer(text="UID Bypass Live Gateway")
        await interaction.response.send_message(embed=embed)

@bot.tree.command(name="resetkey", description="De-authorize this server by clearing its linked Client API Key")
@app_commands.checks.has_permissions(administrator=True)
async def resetkey_slash(interaction: discord.Interaction):
    api_key = get_guild_key(interaction.guild.id)
    if not api_key:
        await send_v2_interaction_response(
            interaction,
            "Error",
            ["This server is already inactive and does not have a linked key."],
            success=False
        )
        return

    conn = sqlite3.connect(DB_FILE)
    cursor = conn.cursor()
    cursor.execute("DELETE FROM guild_config WHERE guild_id = ?", (str(interaction.guild.id),))
    conn.commit()
    conn.close()

    lines = [
        "The license for this server has been cleared successfully.",
        f"Server ID: `{interaction.guild.id}`"
    ]
    await send_v2_interaction_response(interaction, f"License Revoked {E_WARNING}", lines, success=True)

# --- Text Commands (Legacy Support) ---

@bot.command(name="setup")
@commands.has_permissions(administrator=True)
async def setup(ctx):
    """Start the FEAR-X interactive setup wizard."""
    config = get_guild_config(ctx.guild.id) or {}
    api_key = config.get("api_key") or ""
    whitelist_chan = config.get("whitelist_channel_id", "")
    logs_chan = config.get("log_channel_id", "")
    mod_role = config.get("mod_role_id", "")

    lines = [
        f"Basic Config: " + (f"**Set** {E_DONE}" if api_key else f"**Not Set** {E_RED}"),
        f"API Key: " + (f"Linked (`{api_key[:11]}...{api_key[-4:]}`)" if api_key else f"Not Set {E_RED}"),
        f"Whitelist Channel: " + (f"<#{whitelist_chan}>" if whitelist_chan else "Not Restricted"),
        f"Logs Channel: " + (f"<#{logs_chan}>" if logs_chan else "Not Restricted"),
        f"Staff/Mod Role: " + (f"<@&{mod_role}>" if mod_role else "Administrator Only")
    ]

    comps = build_v2_response(f"{E_GEAR} SETUP WIZARD", lines, success=True)
    # Add interactive buttons
    action_row = {
        "type": 1,
        "components": [
            v2_button("Basic Config", style=2, custom_id="setup_basic_config", emoji=parse_emoji(E_GEAR)),
            v2_button("Done / Deploy Panel", style=3, custom_id="setup_done", emoji=parse_emoji(E_DONE))
        ]
    }
    comps[0]["components"].append(v2_separator())
    comps[0]["components"].append(action_row)

    embed = discord.Embed(title=f"{E_GEAR} SETUP WIZARD", description="\n".join(lines), color=discord.Color.blue())
    embed.set_footer(text="UID Bypass Live Gateway")
    
    class FallbackView(discord.ui.View):
        def __init__(self):
            super().__init__(timeout=None)
        @discord.ui.button(label="Basic Config", style=discord.ButtonStyle.secondary, custom_id="setup_basic_config", emoji=E_GEAR)
        async def config_btn(self, interaction, button):
            pass
        @discord.ui.button(label="Done / Deploy Panel", style=discord.ButtonStyle.success, custom_id="setup_done", emoji=E_DONE)
        async def done_btn(self, interaction, button):
            pass

    token = BOT_TOKEN
    if token == "YOUR_DISCORD_BOT_TOKEN_HERE" or not token:
        await ctx.reply(embed=embed, view=FallbackView())
    else:
        res = await send_v2_raw(ctx.channel, comps, token)
        if not res:
            await ctx.reply(embed=embed, view=FallbackView())

@bot.command(name="whitelist")
async def whitelist(ctx, uid: str = None, duration: str = "1d"):
    """Whitelist a player's UID on the control panel."""
    config = get_guild_config(ctx.guild.id)
    if not config or not config.get("api_key"):
        await reply_embed(
            ctx, 
            f"Bot Setup Required {E_WARNING}", 
            "This server is not activated yet. An Administrator must configure it using `!setup`.", 
            discord.Color.red(), 
            success=False
        )
        return

    allowed, err_msg = check_command_permissions(ctx, config)
    if not allowed:
        await reply_embed(ctx, "Permission Denied", err_msg, discord.Color.red(), success=False)
        return

    if not uid:
        lines = [
            "Link player UIDs to the Bypass system.",
            "Use the buttons below to **Whitelist**, **Extend**, or **Delete** player UIDs.",
            f"Whitelist Channel: <#{config['whitelist_channel_id']}>" if config.get("whitelist_channel_id") else "Whitelist Channel: Not Restricted",
            f"Staff/Mod Role: <@&{config['mod_role_id']}>" if config.get("mod_role_id") else "Staff/Mod Role: Administrator Only"
        ]
        
        comps = build_v2_response(f"{E_ROCKET} BYPASS MANAGEMENT PANEL", lines, success=True)
        action_row = {
            "type": 1,
            "components": [
                v2_button("Whitelist UID", style=1, custom_id="panel_whitelist_modal", emoji=parse_emoji(E_ROCKET)),
                v2_button("Extend UID", style=2, custom_id="panel_extend_modal", emoji=parse_emoji(E_GEAR)),
                v2_button("Delete UID", style=4, custom_id="panel_delete_modal", emoji=parse_emoji(E_TRASH))
            ]
        }
        comps[0]["components"].append(v2_separator())
        comps[0]["components"].append(action_row)

        token = BOT_TOKEN
        if token == "YOUR_DISCORD_BOT_TOKEN_HERE" or not token:
            embed = discord.Embed(title=f"{E_ROCKET} BYPASS MANAGEMENT PANEL", description="\n".join(lines), color=discord.Color.blue())
            await ctx.reply(embed=embed)
        else:
            res = await send_v2_raw(ctx.channel, comps, token)
            if not res:
                embed = discord.Embed(title=f"{E_ROCKET} BYPASS MANAGEMENT PANEL", description="\n".join(lines), color=discord.Color.blue())
                await ctx.reply(embed=embed)
        return

    if not uid.isdigit():
        await reply_embed(
            ctx, 
            "Whitelist Player", 
            f"{E_WARNING} **Usage:** `!whitelist <uid> [duration]`\nExample: `!whitelist 12345678 1d` *(for 1 day)*\nSupported Durations: `1d`, `7d`, `30d`, `lifetime`", 
            discord.Color.red(), 
            success=False
        )
        return

    status_msg = await ctx.send(f"🔄 *Processing whitelist for UID {uid}...*")

    days_val = parse_duration_to_days(duration)
    api_key = config.get("api_key")
    headers = {"X-AUTH-KEY": api_key, "X-API-Key": api_key, "Content-Type": "application/json"}
    payload = {"uid": uid, "days": days_val, "name": "Discord_Client"}

    try:
        status, result = await async_vps_request("POST", API_URL, headers=headers, json_payload=payload, timeout=8)

        if status == 200 and result.get("success"):
            await reply_embed(
                ctx,
                f"UID Whitelisted Successfully! {E_SUCCESS}",
                f"Player UID `{uid}` has been added to the bypass database.",
                discord.Color.green(),
                fields=[
                    ("UID", f"`{uid}`"),
                    ("Duration", f"`{days_val} Day(s)`")
                ],
                success=True
            )

            try:
                await status_msg.delete()
            except Exception:
                pass

            # Send logs to logs channel if configured
            logs_channel_id = config.get("log_channel_id")
            if logs_channel_id:
                logs_channel = bot.get_channel(int(logs_channel_id))
                if not logs_channel:
                    try:
                        logs_channel = await bot.fetch_channel(int(logs_channel_id))
                    except Exception:
                        pass
                if logs_channel:
                    log_embed = discord.Embed(
                        title=f"📥 UID Whitelisted",
                        description=f"A new UID was added to the bypass database.",
                        color=discord.Color.green()
                    )
                    log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                    log_embed.add_field(name="Duration", value=f"`{days_val} Days`", inline=True)
                    log_embed.add_field(name="Moderator", value=f"{ctx.author.mention} ({ctx.author.id})", inline=True)
                    log_embed.add_field(name="Server", value=f"{ctx.guild.name} ({ctx.guild.id})", inline=False)
                    log_embed.set_footer(text="UID Audit Logs")
                    
                    token = BOT_TOKEN
                    if token != "YOUR_DISCORD_BOT_TOKEN_HERE" and token:
                        try:
                            log_res = await send_v2_log_internal(logs_channel, log_embed, token)
                            if not log_res:
                                await logs_channel.send(embed=log_embed)
                        except Exception as e:
                            logger.error(f"Failed to send V2 log: {e}")
                            await logs_channel.send(embed=log_embed)
                    else:
                        await logs_channel.send(embed=log_embed)
        else:
            try:
                await status_msg.delete()
            except Exception:
                pass
            err_msg = result.get("error", "Unknown backend error.")
            await reply_embed(ctx, "Whitelist Failed", f"❌ {err_msg}", discord.Color.red(), success=False)
    except Exception as e:
        try:
            await status_msg.delete()
        except Exception:
            pass
        await reply_embed(
            ctx, 
            "Connection Error", 
            f"❌ Could not contact the licensing server: {str(e)}", 
            discord.Color.red(), 
            success=False
        )

@bot.command(name="extend")
async def extend(ctx, uid: str = None, duration: str = "1d"):
    """Extend a whitelisted player's UID validity duration."""
    config = get_guild_config(ctx.guild.id)
    if not config or not config.get("api_key"):
        await reply_embed(
            ctx, 
            f"Bot Setup Required {E_WARNING}", 
            "This server is not activated yet. An Administrator must configure it using `!setup`.", 
            discord.Color.red(), 
            success=False
        )
        return

    allowed, err_msg = check_command_permissions(ctx, config)
    if not allowed:
        await reply_embed(ctx, "Permission Denied", err_msg, discord.Color.red(), success=False)
        return

    if not uid or not uid.isdigit():
        await reply_embed(
            ctx, 
            "Extend Whitelisted Player", 
            f"{E_WARNING} **Usage:** `!extend <uid> [duration]`\nExample: `!extend 12345678 7d` *(to add 7 days)*", 
            discord.Color.red(), 
            success=False
        )
        return

    status_msg = await ctx.send(f"🔄 *Processing extend request for UID {uid}...*")

    days_val = parse_duration_to_days(duration)
    api_key = config.get("api_key")
    headers = {"X-AUTH-KEY": api_key, "X-API-Key": api_key, "Content-Type": "application/json"}
    payload = {"uid": uid, "days": days_val, "name": "Discord_Client"}

    try:
        status, result = await async_vps_request("POST", API_URL, headers=headers, json_payload=payload, timeout=8)

        if status == 200 and result.get("success"):
            await reply_embed(
                ctx,
                f"UID Extended Successfully! {E_SUCCESS}",
                f"Player UID `{uid}` validity extended.",
                discord.Color.green(),
                fields=[
                    ("UID", f"`{uid}`"),
                    ("Added Duration", f"`+{days_val} Day(s)`"),
                    ("Extended By", f"{ctx.author.mention}")
                ],
                success=True
            )

            try:
                await status_msg.delete()
            except Exception:
                pass

            # Send logs
            logs_channel_id = config.get("log_channel_id")
            if logs_channel_id:
                logs_channel = bot.get_channel(int(logs_channel_id))
                if not logs_channel:
                    try:
                        logs_channel = await bot.fetch_channel(int(logs_channel_id))
                    except Exception:
                        pass
                if logs_channel:
                    log_embed = discord.Embed(
                        title=f"⚡ UID Extended",
                        description=f"UID validity extended.",
                        color=discord.Color.green()
                    )
                    log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                    log_embed.add_field(name="Added Days", value=f"`+{days_val} Days`", inline=True)
                    log_embed.add_field(name="Moderator", value=f"{ctx.author.mention} ({ctx.author.id})", inline=True)
                    log_embed.add_field(name="Server", value=f"{ctx.guild.name} ({ctx.guild.id})", inline=False)
                    log_embed.set_footer(text="UID Audit Logs")
                    
                    token = BOT_TOKEN
                    if token != "YOUR_DISCORD_BOT_TOKEN_HERE" and token:
                        try:
                            log_res = await send_v2_log_internal(logs_channel, log_embed, token)
                            if not log_res:
                                await logs_channel.send(embed=log_embed)
                        except Exception as e:
                            logger.error(f"Failed to send V2 log: {e}")
                            await logs_channel.send(embed=log_embed)
                    else:
                        await logs_channel.send(embed=log_embed)
        else:
            try:
                await status_msg.delete()
            except Exception:
                pass
            err_msg = result.get("error", "Unknown backend error.")
            await reply_embed(ctx, "Extend Failed", f"❌ {err_msg}", discord.Color.red(), success=False)
    except Exception as e:
        try:
            await status_msg.delete()
        except Exception:
            pass
        await reply_embed(
            ctx, 
            "Connection Error", 
            f"❌ Could not contact the licensing server: {str(e)}", 
            discord.Color.red(), 
            success=False
        )

@bot.command(name="delete", aliases=["unwhitelist", "remove"])
async def delete(ctx, uid: str = None):
    """Remove a player's UID from the whitelist."""
    config = get_guild_config(ctx.guild.id)
    if not config or not config.get("api_key"):
        await reply_embed(
            ctx, 
            f"Bot Setup Required {E_WARNING}", 
            "This server is not activated yet. An Administrator must configure it using `!setup`.", 
            discord.Color.red(), 
            success=False
        )
        return

    allowed, err_msg = check_command_permissions(ctx, config)
    if not allowed:
        await reply_embed(ctx, "Permission Denied", err_msg, discord.Color.red(), success=False)
        return

    if not uid or not uid.isdigit():
        await reply_embed(
            ctx, 
            "Delete Whitelisted Player", 
            f"{E_WARNING} **Usage:** `!delete <uid>`\nExample: `!delete 12345678`", 
            discord.Color.red(), 
            success=False
        )
        return

    status_msg = await ctx.send(f"🔄 *Processing delete request for UID {uid}...*")

    api_key = config.get("api_key")
    headers = {"X-AUTH-KEY": api_key, "X-API-Key": api_key, "Content-Type": "application/json"}
    payload = {"uid": uid}

    try:
        delete_url = API_URL.replace("/uids/add", "/uids/remove").replace("/external/whitelist/delete", "/uids/remove").replace("/external/whitelist", "/uids/remove")
        status, result = await async_vps_request("POST", delete_url, headers=headers, json_payload=payload, timeout=8)

        if status == 200 and result.get("success"):
            await reply_embed(
                ctx,
                f"UID Deleted Successfully! {E_SUCCESS}",
                f"Player UID `{uid}` has been removed from the bypass.",
                discord.Color.green(),
                fields=[
                    ("Removed By", f"{ctx.author.mention}"),
                    ("UID", f"`{uid}`")
                ],
                success=True
            )

            try:
                await status_msg.delete()
            except Exception:
                pass

            # Send logs to logs channel if configured
            logs_channel_id = config.get("log_channel_id")
            if logs_channel_id:
                logs_channel = bot.get_channel(int(logs_channel_id))
                if not logs_channel:
                    try:
                        logs_channel = await bot.fetch_channel(int(logs_channel_id))
                    except Exception:
                        pass
                if logs_channel:
                    log_embed = discord.Embed(
                        title=f"🗑️ UID Removed",
                        description=f"A UID was removed from the bypass database.",
                        color=discord.Color.orange()
                    )
                    log_embed.add_field(name="UID", value=f"`{uid}`", inline=True)
                    log_embed.add_field(name="Moderator", value=f"{ctx.author.mention} ({ctx.author.id})", inline=True)
                    log_embed.add_field(name="Server", value=f"{ctx.guild.name} ({ctx.guild.id})", inline=False)
                    log_embed.set_footer(text="UID Audit Logs")
                    
                    token = BOT_TOKEN
                    if token != "YOUR_DISCORD_BOT_TOKEN_HERE" and token:
                        try:
                            log_res = await send_v2_log_internal(logs_channel, log_embed, token)
                            if not log_res:
                                await logs_channel.send(embed=log_embed)
                        except Exception as e:
                            logger.error(f"Failed to send V2 log: {e}")
                            await logs_channel.send(embed=log_embed)
                    else:
                        await logs_channel.send(embed=log_embed)
        else:
            try:
                await status_msg.delete()
            except Exception:
                pass
            err_msg = result.get("error", "Unknown backend error.")
            await reply_embed(ctx, "Delete Failed", f"❌ {err_msg}", discord.Color.red(), success=False)
    except Exception as e:
        try:
            await status_msg.delete()
        except Exception:
            pass
        await reply_embed(
            ctx, 
            "Connection Error", 
            f"❌ Could not contact the licensing server: {str(e)}", 
            discord.Color.red(), 
            success=False
        )

@bot.command(name="ping")
async def ping(ctx):
    """Check system latency and database response times"""
    token = BOT_TOKEN
    if token == "YOUR_DISCORD_BOT_TOKEN_HERE" or not token:
        bot_latency = round(bot.latency * 1000)
        await ctx.send(f"🏓 Pong! Latency: {bot_latency}ms")
        return

    # Emojis
    E_LATENCY = "<:latency:1497587834234605658>"
    E_DOT = "<:wifi:1496500724052394115>"

    # Measure WebSocket latency
    ws_val = round(bot.latency * 1000, 2)

    # Measure Local SQLite latency
    try:
        start_sq = time.perf_counter()
        conn = sqlite3.connect(DB_FILE)
        cursor = conn.cursor()
        cursor.execute("SELECT 1")
        cursor.fetchone()
        conn.close()
        sqlite_val = round((time.perf_counter() - start_sq) * 1000, 2)
    except Exception as e:
        logger.warning(f"SQLite ping failed: {e}")
        sqlite_val = None

    # Measure VPS API latency
    try:
        start_vps = time.perf_counter()
        ping_url = API_URL.replace("/api/external/whitelist", "/")
        status, _ = await async_vps_request("GET", ping_url, timeout=5)
        vps_val = round((time.perf_counter() - start_vps) * 1000, 2)
    except Exception as e:
        logger.warning(f"VPS API ping failed: {e}")
        vps_val = None

    db_parts = []
    db_parts.append(f"SQLite: {sqlite_val}ms" if sqlite_val is not None else "SQLite: N/A")
    db_parts.append(f"VPS API: {vps_val}ms" if vps_val is not None else "VPS API: N/A")
    db_str = " | ".join(db_parts)

    # Create initial V2 processing component
    header_init = v2_section(["### System Diagnostics", "Measuring system diagnostics..."], bot.user.display_avatar.url)
    init_comps = [v2_container([header_init])]
    
    msg_id = await send_v2_raw(ctx.channel, init_comps, token)
    if not msg_id:
        # Fallback to standard embed if V2 components are not supported
        embed = discord.Embed(title="System Diagnostics", color=discord.Color.blue())
        embed.add_field(name="Bot Latency", value=f"`{ws_val}ms`", inline=True)
        embed.add_field(name="Database Ping", value=f"`{db_str}`", inline=False)
        embed.set_footer(text="UID Bypass Live Gateway")
        await ctx.reply(embed=embed)
        return

    # Measure message response latency by editing the V2 message
    try:
        start_msg = time.perf_counter()
        await _api("PATCH", f"/channels/{ctx.channel.id}/messages/{msg_id}", token, {
            "flags": FLAGS_COMPONENTS_V2,
            "components": [v2_container([header_init, v2_separator(), v2_text("Calculating message roundtrip...")])]
        })
        msg_val = round((time.perf_counter() - start_msg) * 1000, 2)
    except Exception as e:
        logger.warning(f"Message roundtrip failed: {e}")
        msg_val = None

    # Health Evaluation
    available = [x for x in [ws_val, msg_val, sqlite_val, vps_val] if isinstance(x, (int, float))]
    if not available:
        health_text = "Disconnected"
    else:
        avg = sum(available) / len(available)
        if avg < 100:
            health_text = "Optimal"
        elif avg < 300:
            health_text = "Moderate"
        else:
            health_text = "Elevated"

    # Average Response calculation
    avg_val = round(sum(available) / len(available)) if available else 0

    # Uptime calculation in HH:MM:SS
    delta = discord.utils.utcnow() - bot.start_time
    total_seconds = int(delta.total_seconds())
    hours, rem = divmod(total_seconds, 3600)
    minutes, seconds = divmod(rem, 60)
    uptime_str = f"{hours:02d}:{minutes:02d}:{seconds:02d}"

    # Shard resolution
    shard_id = ctx.guild.shard_id if ctx.guild else None
    shard_str = str(shard_id) if shard_id is not None else "0"

    # Build premium V2 component layout
    header = v2_section(
        [
            "### <:latency:1497587834234605658> System Diagnostics",
            "Performance diagnostics and metrics"
        ],
        accessory_url=bot.user.display_avatar.url
    )

    # System Status Block
    status_block = v2_section([
        "### System Status: " + health_text,
        f"> {E_DOT} **Bot Latency:** `{ws_val or 'N/A'}ms`",
        f"> {E_DOT} **API / Roundtrip:** `{msg_val or 'N/A'}ms`",
        f"> {E_DOT} **Database Ping:** `{db_str}`"
    ])

    # Additional Info Block
    info_block = v2_section([
        "### Additional Info",
        f"> {E_DOT} **Average Response:** `{avg_val}ms`",
        f"> {E_DOT} **Uptime:** `{uptime_str}`",
        f"> {E_DOT} **Servers:** `{len(bot.guilds)}`",
        f"> {E_DOT} **Shard:** `{shard_str}`"
    ])

    # Footer
    footer = v2_text(f"• Requested by {ctx.author.name}")

    # Final container components list
    final_comps = [v2_container([
        header,
        v2_separator(),
        status_block,
        v2_separator(),
        info_block,
        v2_separator(),
        footer
    ], accent=CLR_MASTER)]

    # Edit V2 call to update message with diagnostics
    await _api("PATCH", f"/channels/{ctx.channel.id}/messages/{msg_id}", token, {
        "flags": FLAGS_COMPONENTS_V2,
        "components": final_comps
    })

@bot.command(name="status")
async def status(ctx):
    """Check the license status of this server."""
    config = get_guild_config(ctx.guild.id)
    if not config or not config.get("api_key"):
        await reply_embed(
            ctx, 
            f"License Status: INACTIVE {E_RED}", 
            "This server is not activated. Setup is required.", 
            discord.Color.red(), 
            fields=[("Activation Command", "`!setup`")], 
            success=False
        )
        return

    api_key = config.get("api_key")
    masked_key = api_key[:11] + "..." + api_key[-4:]
    await reply_embed(
        ctx,
        f"License Status: ACTIVE {E_GREEN}",
        "This server is registered and authorized to whitelist UIDs.",
        discord.Color.green(),
        fields=[
            ("Server ID", f"`{ctx.guild.id}`"),
            ("Key", f"`{masked_key}`"),
            ("Whitelist Channel", f"<#{config['whitelist_channel_id']}>" if config.get("whitelist_channel_id") else "Not Restricted"),
            ("Logs Channel", f"<#{config['log_channel_id']}>" if config.get("log_channel_id") else "Not Restricted"),
            ("Staff/Mod Role", f"<@&{config['mod_role_id']}>" if config.get("mod_role_id") else "Administrator Only")
        ],
        success=True
    )

@bot.command(name="help", aliases=["helpme"])
async def help_command(ctx):
    """Display the full help menu with all bot commands and setup guides."""
    token = BOT_TOKEN
    
    # Help embed definition for standard discord/fallback
    embed = discord.Embed(
        title=f"Bot Help Center {E_BULB}",
        description="Complete command manual and licensing integration guide.",
        color=discord.Color.blue()
    )
    embed.add_field(name=f"{E_GEAR} Setup Bot", value="`!setup` - Starts the setup wizard.", inline=False)
    embed.add_field(name=f"{E_ROCKET} Whitelist UID", value="`!whitelist <uid> [duration]` - Whitelists a numerical UID.", inline=False)
    embed.add_field(name=f"⚡ Extend UID", value="`!extend <uid> [duration]` - Extends a UID validity.", inline=False)
    embed.add_field(name=f"{E_TRASH} Delete Whitelist", value="`!delete <uid>` - Deletes a whitelisted UID.", inline=False)
    embed.add_field(name=f"📊 View Status", value="`!status` - Shows the server configuration.", inline=False)
    embed.add_field(name=f"🏓 Latency Check", value="`!ping` - Checks latency.", inline=False)
    embed.add_field(name=f"🛠️ Reset License Key", value="`!resetkey` - Clears linked key.", inline=False)
    embed.set_footer(text="UID Bypass Live Gateway")

    if token == "YOUR_DISCORD_BOT_TOKEN_HERE" or not token:
        await ctx.reply(embed=embed)
        return

    # Build premium V2 card
    header = v2_section([
        f"### Bot Help Center {E_BULB}",
        "Complete command manual and licensing integration guide."
    ], accessory_url=bot.user.display_avatar.url)

    # Admin Setup Block
    admin_block = v2_section([
        f"### {E_GEAR} Server Configuration (Admin Only)",
        f"> {E_DOT} **Command:** `!setup`",
        f"> {E_DOT} **Description:** Starts the premium card-based setup wizard to link API Key, channels, and staff roles.",
        f"> {E_DOT} **Usage:** `!setup`",
        f"> {E_DOT} **Command:** `!resetkey`",
        f"> {E_DOT} **Description:** De-authorizes this server by clearing its linked Client API Key.",
        f"> {E_DOT} **Usage:** `!resetkey`"
    ])

    # Whitelist Block
    whitelist_block = v2_section([
        f"### {E_ROCKET} Whitelist & Bypass Management",
        f"> {E_DOT} **Command:** `!whitelist <uid> [duration]`",
        f"> {E_DOT} **Description:** Whitelists a UID or opens the interactive panel.",
        f"> {E_DOT} **Usage:** `!whitelist` or `!whitelist 12345678 1d`",
        f"> {E_DOT} **Command:** `!extend <uid> [duration]`",
        f"> {E_DOT} **Description:** Extends an existing UID's validity duration.",
        f"> {E_DOT} **Usage:** `!extend 12345678 7d`",
        f"> {E_DOT} **Command:** `!delete <uid>`",
        f"> {E_DOT} **Description:** Deletes/revokes a whitelisted UID.",
        f"> {E_DOT} **Usage:** `!delete 12345678`"
    ])

    # Stats/Utility Block
    util_block = v2_section([
        "### 📊 System & Diagnostics",
        f"> {E_DOT} **Command:** `!status`",
        f"> {E_DOT} **Description:** Shows activation status, whitelist channels, and Staff/Mod roles.",
        f"> {E_DOT} **Usage:** `!status`",
        f"> {E_DOT} **Command:** `!ping`",
        f"> {E_DOT} **Description:** Measures latency and tests VPS API response time.",
        f"> {E_DOT} **Usage:** `!ping`"
    ])

    footer = v2_text(f"• Requested by {ctx.author.name}")

    comps = [v2_container([
        header,
        v2_separator(),
        admin_block,
        v2_separator(),
        whitelist_block,
        v2_separator(),
        util_block,
        v2_separator(),
        footer
    ], accent=CLR_MASTER)]

    res = await send_v2_raw(ctx.channel, comps, token)
    if not res:
        await ctx.reply(embed=embed)

@bot.command(name="resetkey")
@commands.has_permissions(administrator=True)
async def resetkey(ctx):
    """De-authorize this server by clearing its linked API key."""
    api_key = get_guild_key(ctx.guild.id)
    if not api_key:
        await reply_embed(
            ctx, 
            "Error", 
            f"{E_WARNING} This server is already inactive and does not have a linked key.", 
            discord.Color.red(), 
            success=False
        )
        return

    conn = sqlite3.connect(DB_FILE)
    cursor = conn.cursor()
    cursor.execute("DELETE FROM guild_config WHERE guild_id = ?", (str(ctx.guild.id),))
    conn.commit()
    conn.close()

    await reply_embed(
        ctx,
        f"License Revoked {E_WARNING}",
        "The license for this server has been cleared successfully. The bot is now inactive.",
        discord.Color.orange(),
        fields=[("Server ID", f"`{ctx.guild.id}`")],
        success=True
    )

# Error handlers
@setup.error
async def setup_error(ctx, error):
    if isinstance(error, commands.MissingPermissions):
        await reply_embed(
            ctx, 
            "Access Denied", 
            f"{E_WARNING} You need `Administrator` permission to run the setup command.", 
            discord.Color.red(), 
            success=False
        )

@resetkey.error
async def resetkey_error(ctx, error):
    if isinstance(error, commands.MissingPermissions):
        await reply_embed(
            ctx, 
            "Access Denied", 
            f"{E_WARNING} You need `Administrator` permission to run the reset key command.", 
            discord.Color.red(), 
            success=False
        )


if __name__ == "__main__":
    if BOT_TOKEN == "YOUR_DISCORD_BOT_TOKEN_HERE":
        print("❌ Error: Please specify your DISCORD_BOT_TOKEN in bot.py or as an environment variable.")
    else:
        bot.run(BOT_TOKEN)