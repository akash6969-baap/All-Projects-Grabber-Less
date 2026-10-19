import discord
import aiohttp
import asyncio

# For logging robustness: use loguru if present, otherwise fall back to standard print
try:
    from loguru import logger
except ImportError:
    import logging
    logging.basicConfig(level=logging.INFO)
    logger = logging.getLogger("discord_v2_logger")

COLOR_WHITE = 0xFFFFFF
CLR_MASTER = 0x242629
CLR_ERROR = 0xda373c
FLAGS_COMPONENTS_V2 = 32768

# Custom Emojis (You can replace these IDs with your own custom emojis)
E_SUCCESS = "<:success:1479801710267338873>"
E_WARNING = "<:warning:1479802268990701680>"
E_DOT = "<:wifi:1496500724052394115>"

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
                            ignore_reason = "User DM Blocked (DMs closed or no mutual servers)"
                        elif "10008" in err_body:
                            is_ignorable = True
                            ignore_reason = "Unknown Message (already deleted or doesn't exist)"
                        elif "10003" in err_body:
                            is_ignorable = True
                            ignore_reason = "Unknown Channel"
                        elif "10011" in err_body:
                            is_ignorable = True
                            ignore_reason = "Unknown Role"
                        elif "10013" in err_body:
                            is_ignorable = True
                            ignore_reason = "Unknown User"
                        elif "10015" in err_body:
                            is_ignorable = True
                            ignore_reason = "Unknown Webhook"
                        elif "50013" in err_body:
                            is_ignorable = True
                            ignore_reason = "Missing Permissions"
                        elif "10004" in err_body:
                            is_ignorable = True
                            ignore_reason = "Unknown Guild (bot left or has no access)"

                        if is_ignorable:
                            logger.info(f"API Expected Response ({r.status}) on {method} {endpoint}: {ignore_reason} - {err_body}")
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

async def send_v2_log(channel, embed: discord.Embed, token: str, message_id: str = None, content: str = None) -> str | None:
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
        elif color_val in [0xda373c, 0xff0000, 0xe74c3c, 0x992d22, 0x2d0000, 0xff5555]:
            success = False

    title_lower = title.lower()
    critical_words = ["ban", "kick", "delete", "warn", "limit", "alert", "error", "warning", "fail", "timeout", "remove"]
    if any(word in title_lower for word in critical_words):
        success = False

    # Check for visual emojis already in title, if not, prepend relevant prefix emojis
    if not any(emoji in title for emoji in ["<:", "<a:", "🚫", "⚠️", "✅", "🛑", "⚙️", "🔧", "👤", "💬"]):
        # Add basic visual prefix emojis to title to make it super premium
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
        return await send_v2(channel, comps, token, content=content)

async def send_v2(channel, components: list, token: str, content: str = None) -> str | None:
    if not channel: return None
    logger.debug(f"send_v2 called for channel {channel.id}")
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
            logger.error(f"Failed to send legacy content message: {e}")
            
    payload = {"flags": FLAGS_COMPONENTS_V2, "components": components}
    data = await _api("POST", f"/channels/{channel.id}/messages", token, payload)
    logger.debug(f"send_v2 response: {data}")
    return data.get("id") if data else None

async def edit_v2(channel, message_id, components: list, token: str) -> bool:
    if not channel or not message_id: return False
    logger.debug(f"edit_v2 called for message {message_id} in channel {channel.id}")
    result = await _api("PATCH",
        f"/channels/{channel.id}/messages/{message_id}",
        token, {"flags": FLAGS_COMPONENTS_V2, "components": components})
    logger.debug(f"edit_v2 response: {result}")
    return result is not None

async def delete_v2(channel, message_id, token: str) -> bool:
    if not channel or not message_id: return False
    logger.debug(f"delete_v2 called for message {message_id} in channel {channel.id}")
    result = await _api("DELETE",
        f"/channels/{channel.id}/messages/{message_id}", token)
    logger.debug(f"delete_v2 response: {result}")
    return result is not None

def get_token(ctx_or_channel_or_it) -> str:
    if hasattr(ctx_or_channel_or_it, 'bot') and hasattr(ctx_or_channel_or_it.bot, 'token'):
        return ctx_or_channel_or_it.bot.token
    elif hasattr(ctx_or_channel_or_it, 'client') and hasattr(ctx_or_channel_or_it.client, 'token'):
        return ctx_or_channel_or_it.client.token
    elif hasattr(ctx_or_channel_or_it, '_state') and hasattr(ctx_or_channel_or_it._state, 'http'):
        return ctx_or_channel_or_it._state.http.token
    elif hasattr(ctx_or_channel_or_it, 'channel') and hasattr(ctx_or_channel_or_it.channel, '_state'):
        return ctx_or_channel_or_it.channel._state.http.token
    elif hasattr(ctx_or_channel_or_it, 'http'):
        return ctx_or_channel_or_it.http.token
    return ""
