# FEAR X CORPORATION - Discord Whitelist Bot

This Discord bot allows server administrators to link their custom FEAR-X API Key to their Discord server and allow users to whitelist UIDs directly.

The bot is designed to be **disabled by default** when added to a new server. It will strictly reject all commands until an Administrator authorizes it using a valid API key.

## 🚀 Setup Instructions

### 1. Installation & Prerequisites
Make sure you have Python 3.8+ installed. Install the dependencies:
```bash
pip install -r requirements.txt
```

### 2. Enter Discord Token
Open `bot.py` and replace `"YOUR_DISCORD_BOT_TOKEN_HERE"` with your Discord bot token from the Discord Developer Portal:
```python
BOT_TOKEN = "your_actual_bot_token"
```

### 3. Running the Bot
To run the bot locally or on your VPS:
```bash
python bot.py
```

---

## ⚙️ How it Works & Commands

### 1. Inactive State (Default)
When a user adds the bot to their server, it is inactive. If anyone tries to run a command (like `!whitelist`), the bot will respond with:
> ⚠️ **Bot Setup Required:**
> This server is not activated yet. An Administrator must configure it using: `!setup <api_key>`

### 2. Administrator Activation
A server administrator must run the setup command:
`!setup <your_FEAR-X-API-Key>`

**What happens behind the scenes:**
1. The bot verifies the key with your live VPS endpoint (`http://54.210.158.98:5000/api/external/whitelist`).
2. If verified, the server's Discord Guild ID and key are stored securely in `guild_keys.db` (SQLite).
3. The server is now **Activated**.

### 3. Server Commands
Once activated, the following commands are available:
*   `!whitelist <uid> [region] [duration]` - Add a numerical player UID to the control panel bypass.
    *   *Example:* `!whitelist 12345678 GLOBAL 24` (Whitelists UID `12345678` for 24 hours).
*   `!status` - Check if this server is activated, showing the masked API key.
*   `!setup <key>` - (Admin Only) Set up/update the server's API key.
