import ctypes
from flask import Flask, request, redirect, url_for, session, jsonify, send_file
import hashlib
import sys
import psutil
from FearAuth import api
import winreg
import webbrowser
import os
import threading
import time

import socket

# ─────────────────────────────────────────────────────────────────────────────
# Single Instance & Silent Mode Handler
# ─────────────────────────────────────────────────────────────────────────────
def is_server_running(port=5000):
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.settimeout(0.5)
            return s.connect_ex(('127.0.0.1', port)) == 0
    except Exception:
        return False

is_silent = ('--silent' in sys.argv) or ('-s' in sys.argv)

# If the background service is already running on port 5000, exit cleanly
if is_server_running(5000):
    sys.exit(0)

# ─────────────────────────────────────────────────────────────────────────────
# Auto-Elevation: Re-launch as Administrator if not already elevated
# Required for DLL injection into game processes (pymem needs OpenProcess rights)
# ─────────────────────────────────────────────────────────────────────────────
def is_admin():
    try:
        return ctypes.windll.shell32.IsUserAnAdmin()
    except Exception:
        return False

if not is_admin() and not getattr(sys, 'frozen', False) == False:
    # When frozen as EXE - re-launch with admin
    ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, " ".join(sys.argv), None, 1)
    sys.exit(0)
elif not is_admin() and os.name == 'nt':
    # When running as script - warn but continue (user must manually run as admin)
    print("[WARNING] Not running as Administrator! DLL injection will fail.", flush=True)
    print("[WARNING] Please restart CMD as Administrator and run: py app.py", flush=True)


def resource_path(relative_path):
    """ Get absolute path to resource, works for PyInstaller and Nuitka """
    if hasattr(sys, '_MEIPASS'):
        return os.path.join(sys._MEIPASS, relative_path)
    
    # Check Nuitka/frozen app path directory
    if getattr(sys, 'frozen', False):
        base_path = os.path.dirname(os.path.abspath(sys.argv[0]))
    else:
        base_path = os.path.dirname(os.path.abspath(__file__))
        
    target_path = os.path.join(base_path, relative_path)
    if os.path.exists(target_path):
        return target_path
    return os.path.join(os.path.abspath("."), relative_path)

app = Flask(__name__)
app.secret_key = '703822238'

# Global Authentication Middleware: Enforce login check on every single route except login screen
@app.before_request
def require_login():
    allowed_endpoints = ['login', 'static']
    if request.endpoint and request.endpoint not in allowed_endpoints:
        if not session.get('logged_in'):
            # For AJAX/API calls return JSON unauthorized response
            if request.path.startswith('/action') or request.path == '/execute' or request.path.startswith('/status') or request.path == '/userinfo':
                return jsonify({"message": "Unauthorized"}), 401
            # For browser navigation redirect to login screen
            return redirect(url_for('login'))

feature_states = {
    "aimbotv1": "off",
    "fearcollider": "off",
    "norecoil": "off",
    "ignoreknock": "off",
    "aimfovcircle": "off",
    "espline": "off",
    "espbox": "off",
    "espfillbox": "off",
    "espconfig": "off",
    "espskeleton": "off",
    "minimap": "off",
    "espfix": "off",
    "streammod": "off",
    "mutesound": "off",
    "aimfovcircle_val": 100,
    "aipower_val": 200
}

def getchecksum():
    try:
        md5_hash = hashlib.md5()
        target_file = sys.executable if getattr(sys, 'frozen', False) else __file__
        if not os.path.exists(target_file):
            target_file = sys.argv[0]
        with open(target_file, "rb") as file:
            md5_hash.update(file.read())
        return md5_hash.hexdigest()
    except Exception:
        return "checksum_fallback"

# ============================================================
# FearAuth Credentials — Fill in your own details below
# ============================================================
APP_NAME    = "STREAMER"      # App name from FearAuth dashboard
APP_ID      = "308984bf-c745-480d-b7ae-76bd435cb2e4"        # Owner ID / App ID from FearAuth dashboard
APP_SECRET  = "4e2fa23cc71f070424c3b8dbb00893a29a7cc48310ed8fed"    # Secret key from FearAuth dashboard
APP_VERSION = "1.0"           # Declared App Version for FearAuth API
LOCAL_TEST = False  # Set to False to enforce real server authentication

if LOCAL_TEST:
    print("[INFO] Running in Local Testing/Bypass mode. FearAuth is bypassed.", flush=True)
    class DummyAuth:
        class UserData:
            username = "OfflineDev"
            expires = "Unlimited"
        user_data = UserData()
        def login(self, username, password):
            self.user_data.username = username
            return True
    fearauthapp = DummyAuth()
else:
    try:
        # Prevent os._exit crash during API initialization issues
        real_exit = os._exit
        os._exit = lambda c: None
        fearauthapp = api(
            name=APP_NAME,
            ownerid=APP_ID,
            version=APP_VERSION,
            hash_to_check=getchecksum()
        )
        os._exit = real_exit
    except Exception as e:
        # Show graphic error popup and exit. No bypass possible.
        import ctypes
        ctypes.windll.user32.MessageBoxW(0, f"Authentication Connection Failed: {e}", "FEAR Security", 0x10)
        sys.exit(1)

# if sys.platform == "win32":
#   ctypes.windll.user32.ShowWindow(ctypes.windll.kernel32.GetConsoleWindow(), 0)

@app.route('/', methods=['GET', 'POST'])
def login():
    if request.method == 'POST':
        username = request.form.get('username')
        password = request.form.get('password')

        from urllib.parse import quote

        try:
            # Re-initialize FearAuth to get a fresh session before each login
            # This prevents stale session issues when the app has been running a while
            real_exit = os._exit
            os._exit = lambda c: None  # Block os._exit during login
            
            try:
                fearauthapp.sessionid = ""
                fearauthapp.initialized = False
                fearauthapp.init()
            except Exception as init_err:
                print(f"[Auth] Re-init error: {init_err}", flush=True)
                os._exit = real_exit
                return redirect(url_for('login') + '?error=' + quote('Server connection failed. Try again.'))

            # Clear previous user data
            fearauthapp.user_data.username = ""
            fearauthapp.user_data.expires = ""

            # Attempt login
            try:
                fearauthapp.login(username, password)
            except Exception:
                pass  # FearAuth raises Exception on SUCCESS — expected behavior

            os._exit = real_exit

            # Check if user_data was populated (indicates successful login)
            if fearauthapp.user_data.username:
                session['logged_in'] = True
                session['username'] = fearauthapp.user_data.username
                session['expiry'] = fearauthapp.user_data.expires or "Unlimited"
                return redirect(url_for('dashboard'))
            else:
                return redirect(url_for('login') + '?error=' + quote('Invalid username or password.'))

        except Exception as e:
            print(f"[Auth] Login error: {e}", flush=True)
            return redirect(url_for('login') + '?error=' + quote('Authentication error. Try again.'))

    return send_file(resource_path('login.html'))

def add_to_startup():
    file_path = sys.executable  # Path to .exe once converted with PyInstaller
    reg_key = r"Software\Microsoft\Windows\CurrentVersion\Run"
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, reg_key, 0, winreg.KEY_SET_VALUE) as key:
            winreg.SetValueEx(key, "MirrorShopApp", 0, winreg.REG_SZ, file_path)
    except Exception as e:
        print(f"Startup registration failed: {e}")


@app.route('/index.html')
def dashboard():
    if not session.get('logged_in'):
        return redirect(url_for('login'))
    return send_file(resource_path('index.html'))


@app.route('/logout')
def logout():
    session.clear()
    return redirect(url_for('login'))





@app.route('/userinfo')
def user_info():
    if not session.get('logged_in'):
        return jsonify({"success": False}), 401
    return jsonify({
        "success": True,
        "username": session.get('username', 'Unknown'),
        "expires": session.get('expiry', 'Expired')
    })

@app.route('/status')
def check_status():
    process_name = "HD-Player.exe"
    for proc in psutil.process_iter(['name']):
        if proc.info['name'] == process_name:
            return jsonify({"status": "online"})
    return jsonify({"status": "offline"})

# ─────────────────────────────────────────────────────────
# NEW: Update and DLL Injection Routes
# ─────────────────────────────────────────────────────────
@app.route('/status/update')
def web_update_status():
    from injector import check_update_status
    status = check_update_status()
    return jsonify(status)

@app.route('/action/update', methods=['POST'])
def web_action_update():
    from injector import download_updates
    # We will log status text or return simple status
    success = download_updates(log_callback=lambda msg: print(f"[Update Log] {msg}"))
    if success:
        return jsonify({"success": True, "message": "Updates completed successfully!"})
    else:
        return jsonify({"success": False, "message": "Failed to download update packages!"})

# Global variable to track log messages from injection thread
injection_logs = []

def capture_inject_log(msg):
    global injection_logs
    print(f"[Injection Log] {msg}", flush=True)
    injection_logs.append(msg)

@app.route('/action/inject', methods=['POST'])
def web_action_inject():
    global injection_logs
    injection_logs.clear()
    from injector import streameesp
    streameesp(log_callback=capture_inject_log)

    # Auto-send StreamMode ON via pipe after injection settles (3 sec delay)
    def auto_streammode():
        time.sleep(3)
        send_command_to_esp("streammodon")
        print("[AUTO] StreamMode ON sent via pipe after injection", flush=True)
    threading.Thread(target=auto_streammode, daemon=True).start()

    return jsonify({"success": True, "message": "Injection process initialized!"})

@app.route('/action/inject_logs')
def web_inject_logs():
    global injection_logs
    return jsonify({"logs": list(injection_logs)})

# ─────────────────────────────────────────────────────────
# Named Pipe Client Helper Functions (Embedded directly inside app.py)
# ─────────────────────────────────────────────────────────
def send_command_to_esp(command: str):
    pipe_path = r'\\.\pipe\esp_pipe'
    try:
        with open(pipe_path, 'w') as pipe:
            pipe.write(command + '\n')
        return f"Successfully sent command: {command}"
    except FileNotFoundError:
        return "Pipe not found. Make sure the C# ESP app is running."
    except Exception as e:
        return f"Pipe error: {e}"

# ─────────────────────────────────────────────────────────
# Execute route handling exact mappings for C# Pipe commands
# ─────────────────────────────────────────────────────────
@app.route('/execute', methods=['POST'])
def execute_command():
    try:
        data = request.get_json()
        command = data.get('command')
        value = data.get('value')  # Optional parameter for slider inputs

        if not command:
            return jsonify({"message": "No command received."}), 400

        print(f"[DEBUG] Received command: {command} (Value: {value})", flush=True)

        # Update global state tracking for refresh persistence
        if command.endswith("on"):
            base_cmd = command[:-2]
            if base_cmd in feature_states:
                feature_states[base_cmd] = "on"
        elif command.endswith("off"):
            base_cmd = command[:-3]
            if base_cmd in feature_states:
                feature_states[base_cmd] = "off"
        elif command == "update_aimfovcircle":
            feature_states["aimfovcircle_val"] = int(value)
        elif command == "update_aipower":
            feature_states["aipower_val"] = int(value)
        
        response_message = ""

        # Check explicitly for Aimbot V1 and V2 names and map them to C# commands
        if command == "aimbotv1on":
            response_message = send_command_to_esp("aimbotaion")
        elif command == "aimbotv1off":
            response_message = send_command_to_esp("aimbotaioff")
        elif command == "fearcollideron":
            response_message = send_command_to_esp("fearcollideron")
        elif command == "fearcollideroff":
            response_message = send_command_to_esp("fearcollideroff")
            
        # Core Aimbot & Recoil settings
        elif command == "norecoilon":
            response_message = send_command_to_esp("norecoilon")
        elif command == "norecoiloff":
            response_message = send_command_to_esp("norecoiloff")
        elif command == "ignoreknockon":
            response_message = send_command_to_esp("ignoreknockon")
        elif command == "ignoreknockoff":
            response_message = send_command_to_esp("ignoreknockoff")
        elif command == "aimfovcircleon":
            response_message = send_command_to_esp("aimfovcircleon")
        elif command == "aimfovcircleoff":
            response_message = send_command_to_esp("aimfovcircleoff")

        # ESP features
        elif command == "esplineon":
            response_message = send_command_to_esp("esplineon")
        elif command == "esplineoff":
            response_message = send_command_to_esp("esplineoff")
        elif command == "espboxon":
            response_message = send_command_to_esp("espboxon")
        elif command == "espboxoff":
            response_message = send_command_to_esp("espboxoff")
        elif command == "espfillboxon":
            response_message = send_command_to_esp("espfillboxon")
        elif command == "espfillboxoff":
            response_message = send_command_to_esp("espfillboxoff")
        elif command == "espconfigon":
            response_message = send_command_to_esp("espconfigon")
        elif command == "espconfigoff":
            response_message = send_command_to_esp("espconfigoff")
        elif command == "espskeletonon":
            response_message = send_command_to_esp("espskeletonon")
        elif command == "espskeletonoff":
            response_message = send_command_to_esp("espskeletonoff")
        elif command == "minimapon":
            response_message = send_command_to_esp("minimapon")
        elif command == "minimapoff":
            response_message = send_command_to_esp("minimapoff")
        elif command == "espfixon":
            response_message = send_command_to_esp("espfixon")
        elif command == "espfixoff":
            response_message = send_command_to_esp("espfixoff")

        # System controls & extra features
        elif command == "streammodon":
            response_message = send_command_to_esp("streammodon")
        elif command == "streammodoff":
            response_message = send_command_to_esp("streammodoff")
        elif command == "mutesoundon":
            response_message = send_command_to_esp("mutesoundon")
        elif command == "mutesoundoff":
            response_message = send_command_to_esp("mutesoundoff")
        elif command == "hidetaskbar":
            response_message = send_command_to_esp("hidetaskbar")
        elif command == "showtaskbar":
            response_message = send_command_to_esp("showtaskbar")
        elif command == "reload":
            response_message = send_command_to_esp("reload")
        elif command == "environment.exit":
            response_message = send_command_to_esp("environment.exit")

        # Value sliders
        elif command == "update_aimfovcircle":
            response_message = send_command_to_esp(f"aimfovcircle:{int(value)}")
        elif command == "update_aipower":
            response_message = send_command_to_esp(f"aipower:{int(value)}")
        else:
            response_message = f"Command {command} is not mapped in app.py logic."
            
        print(f"[DEBUG] Response: {response_message}", flush=True)
        return jsonify({"message": str(response_message)})
    except Exception as e:
        print(f"[FATAL ERROR] {e}", flush=True)
        return jsonify({"message": f"Server Error: {str(e)}"}), 500


@app.route('/action/get_states')
def get_states():
    return jsonify(feature_states)


if __name__ == '__main__':
    # Run server completely headless in background - do NOT auto-open browser
    app.run(host='0.0.0.0', port=5000, debug=False)
