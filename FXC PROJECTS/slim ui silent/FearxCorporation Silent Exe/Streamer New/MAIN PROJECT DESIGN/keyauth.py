import os
import json as jsond
import time
import binascii
import platform
import subprocess
import sys
from datetime import datetime, timezone, timedelta

try:
    if os.name == 'nt':
        import win32security  # Windows SID
    import requests
    import qrcode
    from nacl.signing import VerifyKey  # For signature verify
    from nacl.exceptions import BadSignatureError
except ModuleNotFoundError:
    print("Missing modules. Installing...")
    if os.path.isfile("requirements.txt"):
        os.system("pip install -r requirements.txt")
    else:
        if os.name == 'nt':
            os.system("pip install pywin32")
        os.system("pip install requests qrcode pynacl")
    print("Modules installed. Restart script.")
    time.sleep(2)
    sys.exit(1)


def verify_key(response_text, signature, timestamp, public_key):
    try:
        verifyKey = VerifyKey(bytes.fromhex(public_key))
        verifyKey.verify(
            timestamp.encode() + response_text,
            bytes.fromhex(signature)
        )
        return True
    except BadSignatureError:
        return False


class api:
    name = ownerid = version = hash_to_check = ""
    sessionid = enckey = ""
    initialized = False

    def __init__(self, name, ownerid, version, hash_to_check):
        if len(ownerid) != 10:
            print("Invalid ownerid. Copy correct code from KeyAuth dashboard.")
            time.sleep(3)
            sys.exit(1)

        self.name = name
        self.ownerid = ownerid
        self.version = version
        self.hash_to_check = hash_to_check
        self.init()

    def init(self):
        if self.sessionid != "":
            print("Already initialized!")
            time.sleep(3)
            sys.exit(1)

        post_data = {
            "type": "init",
            "ver": self.version,
            "hash": self.hash_to_check,
            "name": self.name,
            "ownerid": self.ownerid
        }

        response = self.__do_request(post_data)

        if response == "KeyAuth_Invalid":
            print("The application doesn't exist")
            time.sleep(3)
            sys.exit(1)

        json = jsond.loads(response)

        if json["message"] == "invalidver":
            if json["download"] != "":
                print("New Version Available")
                os.system(f"start {json['download']}")
                time.sleep(3)
                sys.exit(1)
            else:
                print("Invalid Version. Contact developer.")
                time.sleep(3)
                sys.exit(1)

        if not json["success"]:
            print(json["message"])
            time.sleep(3)
            sys.exit(1)

        self.sessionid = json["sessionid"]
        self.initialized = True

    def login(self, user, password, code=None, hwid=None):
        self.checkinit()
        if hwid is None:
            hwid = others.get_hwid()

        post_data = {
            "type": "login",
            "username": user,
            "pass": password,
            "hwid": hwid,
            "sessionid": self.sessionid,
            "name": self.name,
            "ownerid": self.ownerid,
        }

        if code:
            post_data["code"] = code

        response = self.__do_request(post_data)
        json = jsond.loads(response)

        if json["success"]:
            self.__load_user_data(json["info"])
            print(f"[+] {json['message']}")
            return True
        else:
            print(f"[x] {json['message']}")
            time.sleep(2)
            return False

    def license(self, key, code=None, hwid=None):
        self.checkinit()
        if hwid is None:
            hwid = others.get_hwid()

        post_data = {
            "type": "license",
            "key": key,
            "hwid": hwid,
            "sessionid": self.sessionid,
            "name": self.name,
            "ownerid": self.ownerid
        }

        if code:
            post_data["code"] = code

        response = self.__do_request(post_data)
        json = jsond.loads(response)

        if json["success"]:
            self.__load_user_data(json["info"])
            print(f"[+] {json['message']}")
            return True
        else:
            print(f"[x] {json['message']}")
            return False

    def checkinit(self):
        if not self.initialized:
            print("Initialize first before using functions")
            time.sleep(2)
            sys.exit(1)

    def __do_request(self, post_data):
        try:
            response = requests.post(
                "https://keyauth.win/api/1.3/", data=post_data, timeout=10
            )

            if post_data["type"] in ["log", "file", "2faenable", "2fadisable"]:
                return response.text

            signature = response.headers.get("x-signature-ed25519")
            timestamp = response.headers.get("x-signature-timestamp")

            if not signature or not timestamp:
                print("Missing signature headers.")
                sys.exit(1)

            server_time = datetime.fromtimestamp(int(timestamp), timezone.utc)
            current_time = datetime.now(timezone.utc)

            if (current_time - server_time) > timedelta(seconds=25):
                print("Timestamp expired.")
                sys.exit(1)

            if not verify_key(
                response.text.encode("utf-8"),
                signature,
                timestamp,
                "5586b4bc69c7a4b487e4563a4cd96afd39140f919bd31cea7d1c6a1e8439422b"
            ):
                print("Signature verification failed.")
                sys.exit(1)

            return response.text

        except requests.exceptions.Timeout:
            print("Request timed out. Server might be down.")
            sys.exit(1)

    class user_data_class:
        username = ip = hwid = expires = createdate = lastlogin = subscription = subscriptions = ""

    class application_data_class:
        numUsers = numKeys = app_ver = customer_panel = onlineUsers = ""

    user_data = user_data_class()
    app_data = application_data_class()

    def __load_user_data(self, data):
        self.user_data.username = data["username"]
        self.user_data.ip = data["ip"]
        self.user_data.hwid = data.get("hwid", "N/A")
        self.user_data.expires = data["subscriptions"][0]["expiry"]
        self.user_data.createdate = data["createdate"]
        self.user_data.lastlogin = data["lastlogin"]
        self.user_data.subscription = data["subscriptions"][0]["subscription"]
        self.user_data.subscriptions = data["subscriptions"]


class others:
    @staticmethod
    def get_hwid():
        if platform.system() == "Linux":
            with open("/etc/machine-id") as f:
                return f.read().strip()
        elif platform.system() == "Windows":
            winuser = os.getlogin()
            sid = win32security.LookupAccountName(None, winuser)[0]
            return win32security.ConvertSidToStringSid(sid)
        elif platform.system() == "Darwin":
            output = subprocess.Popen(
                "ioreg -l | grep IOPlatformSerialNumber",
                stdout=subprocess.PIPE, shell=True
            ).communicate()[0]
            return output.decode().split('=')[-1].strip().replace('"', "")
        return "UNKNOWN"
