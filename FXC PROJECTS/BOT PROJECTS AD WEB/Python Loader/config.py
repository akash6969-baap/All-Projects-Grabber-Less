import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

# FearAuth Configuration
FEARAUTH_APP_NAME = os.getenv("FEARAUTH_APP_NAME", "Fear Loader")
FEARAUTH_OWNER_ID = os.getenv("FEARAUTH_OWNER_ID", "")
FEARAUTH_SECRET = os.getenv("FEARAUTH_SECRET", "")
FEARAUTH_VERSION = os.getenv("FEARAUTH_VERSION", "1.0")
