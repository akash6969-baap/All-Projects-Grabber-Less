import os
try:
    from dotenv import load_dotenv
    env_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".env")
    if os.path.exists(env_file):
        load_dotenv(dotenv_path=env_file)
except Exception:
    pass

# FearAuth Configuration
FEARAUTH_APP_NAME = os.getenv("FEARAUTH_APP_NAME", "BASIC PANEL")
FEARAUTH_OWNER_ID = os.getenv("FEARAUTH_OWNER_ID", "a3ff464f-a581-46f4-8357-d6436115e79b")
FEARAUTH_SECRET = os.getenv("FEARAUTH_SECRET", "ce1d8a10c5b754c2929210b164026c5dcf3a2356eec4fce6")
FEARAUTH_VERSION = os.getenv("FEARAUTH_VERSION", "1.0")
