import os
try:
    from dotenv import load_dotenv
    env_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".env")
    if os.path.exists(env_file):
        load_dotenv(dotenv_path=env_file)
except Exception:
    pass

# FearAuth Configuration
FEARAUTH_APP_NAME = os.getenv("FEARAUTH_APP_NAME", "AIMBOT AI")
FEARAUTH_OWNER_ID = os.getenv("FEARAUTH_OWNER_ID", "fc991c8a-f128-4f50-8b51-82c224aa168a")
FEARAUTH_SECRET = os.getenv("FEARAUTH_SECRET", "b82e248f1c2d3578ce5fa95e77da2b0b30ab89fd7f2e4fcd")
FEARAUTH_VERSION = os.getenv("FEARAUTH_VERSION", "1.0")
