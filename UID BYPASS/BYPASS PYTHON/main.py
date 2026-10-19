import os
import sys
import re
import socket
import ctypes
import random
import hashlib
import subprocess
import time
import winsound
import psutil
import threading
import urllib.request
import urllib.error
from PyQt5.QtWidgets import (QApplication, QWidget, QStackedWidget, QVBoxLayout, QHBoxLayout,
                             QLabel, QLineEdit, QFrame, QPushButton, QComboBox, QPlainTextEdit,
                             QGraphicsDropShadowEffect, QGridLayout)
from PyQt5.QtCore import Qt, QTimer, QRect, QPoint, pyqtSignal, QSize, QPointF
from PyQt5.QtGui import QFont, QPainter, QColor, QPen, QBrush, QIcon, QPixmap, QPainterPath


# Constants
CertHash = "c8750f0d"
EmbeddedResourceName = "c8750f0d.0"
DllUrl = "https://raw.githubusercontent.com/akash6969-baap/UID-BYPASS-DLL/main/UIDBypassDll.dll"
DllName = "UIDBypassDll.dll"
PackageName = "com.dts.freefireth"

# Set styling


# Get resource path for PyInstaller compatibility
def get_resource_path(relative_path):
    try:
        base_path = sys._MEIPASS
    except Exception:
        base_path = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(base_path, relative_path)

# Helper to set font with fallback for Segoe MDL2 Assets icons
def set_button_font(button, size=9, bold=True):
    font = QFont()
    font.setFamilies(["Segoe UI", "Segoe MDL2 Assets"])
    font.setPointSize(size)
    font.setBold(bold)
    button.setFont(font)


# ─────────────────────────────────────────────
# CUSTOM ICONS GENERATORS (drawn via QPainter)
# ─────────────────────────────────────────────

def create_lightning_icon(color_hex):
    pixmap = QPixmap(64, 64)
    pixmap.fill(Qt.transparent)
    
    painter = QPainter(pixmap)
    painter.setRenderHint(QPainter.Antialiasing)
    
    # Modern sleek outline lightning bolt points
    points = [
        QPointF(35, 8),
        QPointF(47, 28),
        QPointF(35, 28),
        QPointF(40, 56),
        QPointF(19, 34),
        QPointF(31, 34)
    ]
    
    path = QPainterPath()
    path.moveTo(points[0])
    for pt in points[1:]:
        path.lineTo(pt)
    path.closeSubpath()
    
    color = QColor(color_hex)
    painter.setBrush(Qt.NoBrush)
    pen = QPen(color, 3.0, Qt.SolidLine, Qt.RoundCap, Qt.RoundJoin)
    painter.setPen(pen)
    painter.drawPath(path)
    painter.end()
    
    return QIcon(pixmap)

def create_gear_icon(color_hex):
    pixmap = QPixmap(64, 64)
    pixmap.fill(Qt.transparent)
    
    painter = QPainter(pixmap)
    painter.setRenderHint(QPainter.Antialiasing)
    
    color = QColor(color_hex)
    painter.setBrush(Qt.NoBrush)
    pen = QPen(color, 3.0, Qt.SolidLine, Qt.RoundCap, Qt.RoundJoin)
    painter.setPen(pen)
    
    center_x, center_y = 32.0, 32.0
    
    import math
    path = QPainterPath()
    num_teeth = 8
    r_inner = 15.0
    r_outer = 22.0
    tooth_width_angle = 0.18 # radians (~10 degrees)
    
    for i in range(num_teeth):
        angle_center = i * (2 * math.pi / num_teeth)
        
        a1 = angle_center - tooth_width_angle / 2
        a2 = angle_center + tooth_width_angle / 2
        
        x1 = center_x + r_inner * math.cos(a1)
        y1 = center_y + r_inner * math.sin(a1)
        
        x2 = center_x + r_outer * math.cos(a1)
        y2 = center_y + r_outer * math.sin(a1)
        
        x3 = center_x + r_outer * math.cos(a2)
        y3 = center_y + r_outer * math.sin(a2)
        
        x4 = center_x + r_inner * math.cos(a2)
        y4 = center_y + r_inner * math.sin(a2)
        
        if i == 0:
            path.moveTo(x1, y1)
        else:
            path.lineTo(x1, y1)
            
        path.lineTo(x2, y2)
        path.lineTo(x3, y3)
        path.lineTo(x4, y4)
        
    path.closeSubpath()
    painter.drawPath(path)
    
    # Center circle hole
    painter.drawEllipse(QPointF(center_x, center_y), 6.0, 6.0)
    painter.end()
    
    return QIcon(pixmap)

# Unused custom drawing icon functions removed (using native Segoe MDL2 Assets font icons).

# ─────────────────────────────────────────────
# HWID & SYSTEM INFO
# ─────────────────────────────────────────────

def get_wmi_property(table, prop):
    """WMI se system property fetch karna (C# GetWmiProperty equivalent)"""
    try:
        import wmi
        c = wmi.WMI()
        items = getattr(c, table)()
        if items:
            val = getattr(items[0], prop, None)
            return str(val).strip() if val else ""
    except Exception:
        pass
    try:
        result = subprocess.run(
            ["wmic", table.replace("Win32_", ""), "get", prop],
            capture_output=True, text=True,
            creationflags=subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0
        )
        lines = [l.strip() for l in result.stdout.splitlines() if l.strip()]
        if len(lines) > 1:
            return lines[1]
    except Exception:
        pass
    return ""

def get_hwid():
    """Hardware ID generate karna (C# GetHWID equivalent)"""
    try:
        identifier = ""
        identifier += get_wmi_property("Win32_BaseBoard", "SerialNumber")
        identifier += get_wmi_property("Win32_Processor", "ProcessorId")
        identifier += get_wmi_property("Win32_DiskDrive", "SerialNumber")

        if not identifier.strip() or "To be filled" in identifier or len(identifier) < 5:
            identifier = str(os.cpu_count()) + os.environ.get("COMPUTERNAME", "") + os.environ.get("USERNAME", "")

        hash_bytes = hashlib.sha256(identifier.encode("utf-8")).digest()
        return hash_bytes.hex().upper()[:16]
    except Exception:
        return "UNKNOWN_ID"

def get_local_ipv4():
    """Local IPv4 address detect karna (C# GetLocalIPv4 equivalent)"""
    try:
        hostname = socket.gethostname()
        addresses = socket.getaddrinfo(hostname, None)
        for addr in addresses:
            ip = addr[4][0]
            # IPv4 check - no colon means it's IPv4
            if ":" not in ip and not ip.startswith("127."):
                return ip
    except Exception:
        pass
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        ip = s.getsockname()[0]
        s.close()
        return ip
    except Exception:
        return "127.0.0.1"

def mask_proxy(proxy):
    """Proxy IP ko mask karna privacy ke liye (C# MaskProxy equivalent)"""
    if not proxy:
        return "****"
    parts = proxy.split(":")
    ip = parts[0]
    port = parts[1] if len(parts) > 1 else ""
    segments = ip.split(".")
    if len(segments) == 4:
        return f"{segments[0]}.{segments[1]}.***.***:{'*' * len(port)}"
    return "****.****.****.****:****"

def is_administrator():
    """Admin privileges check karna (C# IsAdministrator equivalent)"""
    try:
        import ctypes
        return ctypes.windll.shell32.IsUserAnAdmin() != 0
    except Exception:
        return False

def restart_as_admin():
    """Admin privileges ke saath restart karna (C# RestartAsAdmin equivalent)"""
    try:
        import ctypes
        script = os.path.abspath(sys.argv[0])
        ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, f'"{script}"', None, 1)
    except Exception as e:
        print(f"[!] Admin restart failed: {e}")
    sys.exit(0)

# ─────────────────────────────────────────────
# LOCAL CERTIFICATE LOADER
# ─────────────────────────────────────────────

def get_local_cert():
    """Local resources ya local folder se certificate copy karke temp path return karna"""
    try:
        import tempfile
        import shutil
        
        # Priority 0: Look in "mycerts" directory bundled with EXE or script
        local_mycerts_path = get_resource_path(os.path.join("mycerts", "mitmproxy-ca-cert.pem"))
        if os.path.exists(local_mycerts_path):
            tmp_path = os.path.join(tempfile.gettempdir(), f"{CertHash}.0")
            shutil.copy(local_mycerts_path, tmp_path)
            return tmp_path
            
        local_mycerts_cer = get_resource_path(os.path.join("mycerts", "mitmproxy-ca-cert.cer"))
        if os.path.exists(local_mycerts_cer):
            tmp_path = os.path.join(tempfile.gettempdir(), f"{CertHash}.0")
            shutil.copy(local_mycerts_cer, tmp_path)
            return tmp_path

        # Priority 1: Check in current directory
        local_files = [f"{CertHash}.0", "mitmproxy-ca-cert.pem", "mitmproxy-ca-cert.cer"]
        for lf in local_files:
            if os.path.exists(lf):
                tmp_path = os.path.join(tempfile.gettempdir(), f"{CertHash}.0")
                shutil.copy(lf, tmp_path)
                return tmp_path
        
        # Priority 2: Check inside a local "mycerts" directory relative to current working directory
        cwd_mycerts_pem = os.path.join("mycerts", "mitmproxy-ca-cert.pem")
        if os.path.exists(cwd_mycerts_pem):
            tmp_path = os.path.join(tempfile.gettempdir(), f"{CertHash}.0")
            shutil.copy(cwd_mycerts_pem, tmp_path)
            return tmp_path
            
        return None
    except Exception:
        return None

# ─────────────────────────────────────────────
# PATH HELPERS
# ─────────────────────────────────────────────

def get_adb_path():
    paths = [
        r"C:\Program Files\BlueStacks_msi5\HD-Adb.exe",
        r"C:\Program Files\BlueStacks_nxt\HD-Adb.exe",
        r"C:\Android\platform-tools\adb.exe"
    ]
    for path in paths:
        if os.path.exists(path):
            return path
    return "adb.exe"

def get_memu_adb_path():
    """MEmu ADB path dhundhna (C# GetMemuAdbPath equivalent)"""
    paths = [
        r"C:\Program Files\Microvirt\MEmu\adb.exe",
        r"C:\Program Files (x86)\Microvirt\MEmu\adb.exe",
        r"D:\Program Files\Microvirt\MEmu\adb.exe"
    ]
    for path in paths:
        if os.path.exists(path):
            return path
    return None

def resolve_adb_path(emulator_type):
    if "memu" in emulator_type.lower():
        memu_adb = get_memu_adb_path()
        if memu_adb:
            return memu_adb
    msi = r"C:\Program Files\Bluestacks_msi5\HD-Adb.exe"
    nxt = r"C:\Program Files\BlueStacks_nxt\HD-Adb.exe"
    if "msi" in emulator_type.lower() and os.path.exists(msi):
        return msi
    if "bluestacks" in emulator_type.lower() and os.path.exists(nxt):
        return nxt
    return get_adb_path()

# ─────────────────────────────────────────────
# PROCESS HELPERS
# ─────────────────────────────────────────────

def is_process_running(name):
    """Process running hai ya nahi check karna (C# IsProcessRunning equivalent)"""
    for proc in psutil.process_iter(['name']):
        try:
            if proc.info['name'] and name.lower() in proc.info['name'].lower():
                return True
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            pass
    return False

def kill_processes(name):
    """Specific name ke saare processes kill karna (C# KillProcesses equivalent)"""
    for proc in psutil.process_iter(['name']):
        try:
            if proc.info['name'] and name.lower() in proc.info['name'].lower():
                proc.kill()
                proc.wait(timeout=2)
        except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.TimeoutExpired):
            pass

def terminate_processes(targets=["HD-Player", "HD-Adb", "HD-MultiInstanceManager", "BstkSVC"]):
    killed_any = False
    for proc in psutil.process_iter(['name']):
        try:
            name_lower = proc.info['name'].lower() if proc.info['name'] else ""
            for target in targets:
                if target.lower() in name_lower:
                    proc.kill()
                    killed_any = True
        except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.ZombieProcess):
            pass
    return killed_any

# ─────────────────────────────────────────────
# EMULATOR AUTO DETECT
# ─────────────────────────────────────────────

def auto_detect_emulator():
    """Running emulator detect karna (C# AutoDetectEmulator equivalent)"""
    # MEmu check
    if is_process_running("MEmu") or is_process_running("MEmuHeadless"):
        adb = get_memu_adb_path()
        return {"name": "MEmu Player", "adb": adb, "device_id": "127.0.0.1:21503"}

    # BlueStacks running check
    if is_process_running("HD-Player"):
        for proc in psutil.process_iter(['name', 'exe']):
            try:
                if proc.info['name'] and "hd-player" in proc.info['name'].lower():
                    exe = proc.info.get('exe', '') or ''
                    exe_lower = exe.lower()
                    if "bluestacks_msi5" in exe_lower:
                        return {"name": "MSI App Player", "adb": r"C:\Program Files\BlueStacks_msi5\HD-Adb.exe", "device_id": "127.0.0.1:5555"}
                    elif "bluestacks_nxt" in exe_lower:
                        return {"name": "BlueStacks NXT", "adb": r"C:\Program Files\BlueStacks_nxt\HD-Adb.exe", "device_id": "127.0.0.1:5555"}
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                pass
        
        # Fallback if loop couldn't read the exe path due to Windows permissions, but HD-Player is running!
        if os.path.exists(r"C:\Program Files\BlueStacks_msi5"):
            return {"name": "MSI App Player", "adb": r"C:\Program Files\BlueStacks_msi5\HD-Adb.exe", "device_id": "127.0.0.1:5555"}
        elif os.path.exists(r"C:\Program Files\BlueStacks_nxt"):
            return {"name": "BlueStacks NXT", "adb": r"C:\Program Files\BlueStacks_nxt\HD-Adb.exe", "device_id": "127.0.0.1:5555"}

    # Installed but not running check
    if os.path.exists(r"C:\Program Files\BlueStacks_nxt\HD-Adb.exe"):
        return {"name": "BlueStacks NXT (Offline)", "adb": r"C:\Program Files\BlueStacks_nxt\HD-Adb.exe", "device_id": "127.0.0.1:5555"}
    elif os.path.exists(r"C:\Program Files\Bluestacks_msi5\HD-Adb.exe"):
        return {"name": "MSI App Player (Offline)", "adb": r"C:\Program Files\Bluestacks_msi5\HD-Adb.exe", "device_id": "127.0.0.1:5555"}
    else:
        memu_adb = get_memu_adb_path()
        if memu_adb:
            return {"name": "MEmu Player (Offline)", "adb": memu_adb, "device_id": "127.0.0.1:21503"}

    return {"name": "None Detected", "adb": "", "device_id": "127.0.0.1:5555"}

# ─────────────────────────────────────────────
# ADB CONTROLS
# ─────────────────────────────────────────────

def run_adb(adb_exe, args_str):
    try:
        import shlex
        if isinstance(args_str, str):
            args_list = shlex.split(args_str, posix=False)
            args_list = [a.strip('"') for a in args_list]
        else:
            args_list = list(args_str)
            
        cmd = [adb_exe] + args_list
        result = subprocess.run(
            cmd,
            shell=False,
            capture_output=True,
            text=True,
            creationflags=subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0
        )
        return result.stdout, result.stderr
    except Exception as e:
        return "", str(e)

def run_adb_device(adb_exe, device_ip, args_str):
    return run_adb(adb_exe, f"-s {device_ip} {args_str}")

def connect_adb(adb_exe, device_ip):
    # Check if the device is already connected
    stdout, _ = run_adb(adb_exe, "devices")
    for line in stdout.splitlines():
        parts = line.strip().split()
        if len(parts) >= 2 and parts[0] == device_ip and parts[1] == "device":
            return True, "Already connected"
            
    # Attempt network connection if not already active
    stdout, stderr = run_adb(adb_exe, f"connect {device_ip}")
    combined = (stdout + "\n" + stderr).strip()
    if "connected" in stdout.lower() or "already connected" in stdout.lower():
        return True, combined
    return False, combined

def file_exists_on_device(adb_exe, device_ip, path):
    stdout, _ = run_adb_device(adb_exe, device_ip, f"shell \"[ -f {path} ] && echo yes || echo no\"")
    lines = [line.strip().lower() for line in stdout.splitlines()]
    return "yes" in lines

def get_emulator_ip(adb_exe, default_port):
    try:
        # Get list of all currently connected devices
        stdout, _ = run_adb(adb_exe, "devices")
        devices = []
        for line in stdout.splitlines():
            line = line.strip()
            if line.endswith("\tdevice"):
                ip = line.split("\t")[0]
                devices.append(ip)
        
        # If the user typed a specific port, search for a device matching that port
        if default_port:
            port_str = str(default_port).strip()
            for ip in devices:
                if f":{port_str}" in ip or ip.endswith(port_str):
                    return ip
            # If no active device matches the target port, attempt to connect to it
            return f"127.0.0.1:{port_str}"
            
        # If no port was specified, fall back to the first connected device
        if devices:
            return devices[0]
    except Exception:
        pass
    return f"127.0.0.1:{default_port}" if default_port else "127.0.0.1:5555"

# ─────────────────────────────────────────────
# CONFIG & LOG OPERATIONS
# ─────────────────────────────────────────────

def clear_emulator_logs(engine_root, log_fn):
    manager_dir = os.path.join(engine_root, "Manager")
    if os.path.exists(manager_dir):
        try:
            for file in os.listdir(manager_dir):
                if file.startswith("BstkServer.log"):
                    file_path = os.path.join(manager_dir, file)
                    try:
                        os.remove(file_path)
                        log_fn(f"Removed log: {file}", "info")
                    except Exception:
                        try:
                            with open(file_path, "w") as f:
                                pass
                            log_fn(f"Cleared log: {file}", "info")
                        except Exception:
                            pass
        except Exception:
            pass
            
    try:
        for root, dirs, files in os.walk(engine_root):
            for file in files:
                if file.startswith("BstkCore.log"):
                    file_path = os.path.join(root, file)
                    try:
                        os.remove(file_path)
                        log_fn(f"Removed log: {file}", "info")
                    except Exception:
                        pass
    except Exception:
        pass

def edit_configs(engine_root, log_fn):
    if not os.path.exists(engine_root):
        raise FileNotFoundError("Engine path not found.")
        
    for item in os.listdir(engine_root):
        dir_path = os.path.join(engine_root, item)
        if os.path.isdir(dir_path):
            files = [
                os.path.join(dir_path, "Android.bstk.in"),
                os.path.join(dir_path, f"{item}.bstk"),
                os.path.join(dir_path, f"{item}.bstk-prev")
            ]
            for fpath in files:
                if os.path.exists(fpath):
                    try:
                        with open(fpath, "r", encoding="utf-8", errors="ignore") as f:
                            content = f.read()
                        
                        # Root.vhd type from Readonly to Normal
                        content = re.sub(
                            r'(<HardDisk\b[^>]*location\s*=\s*"Root\.vhd"[^>]*type\s*=\s*")Readonly("\s*/?>)',
                            r'\g<1>Normal\g<2>',
                            content,
                            flags=re.IGNORECASE
                        )
                        
                        # Data.vhdx type from Readonly to Normal
                        content = re.sub(
                            r'(<HardDisk\b[^>]*location\s*=\s*"Data\.vhdx"[^>]*type\s*=\s*")Readonly("\s*/?>)',
                            r'\g<1>Normal\g<2>',
                            content,
                            flags=re.IGNORECASE
                        )
                        
                        with open(fpath, "w", encoding="utf-8") as f:
                            f.write(content)
                        log_fn(f"Modified configuration: {os.path.basename(fpath)}", "info")
                    except Exception as e:
                        log_fn(f"Failed to modify config {fpath}: {str(e)}", "error")

# ─────────────────────────────────────────────
# ACTION HANDLERS
# ─────────────────────────────────────────────

def handle_access(emulator_type, log_fn):
    """Grant Access - BlueStacks/MSI + MEmu support (C# GrantAccessAndRestart equivalent)"""
    log_fn("Granting Access...", "info")
    try:
        log_fn("Terminating active emulator processes...", "info")

        if "memu" in emulator_type.lower():
            # MEmu specific
            memu_procs = ["MEmu", "MEmuHeadless", "MEmuSVC", "MEmuManage"]
            for p in memu_procs:
                kill_processes(p)

            adb_path = get_memu_adb_path()
            if adb_path:
                player_path = os.path.join(os.path.dirname(adb_path), "MEmu.exe")
                if os.path.exists(player_path):
                    log_fn("Starting MEmu Player...", "info")
                    subprocess.Popen(player_path, shell=True)
                    log_fn("MEmu started successfully!", "success")
                    winsound.Beep(800, 300)
                else:
                    log_fn("MEmu executable not found!", "error")
            else:
                log_fn("MEmu ADB not found!", "error")
        else:
            # BlueStacks / MSI
            bstk_procs = ["HD-Player", "HD-Adb", "HD-MultiInstanceManager", "BstkSVC"]
            for p in bstk_procs:
                kill_processes(p)
            time.sleep(1)

            if "msi" in emulator_type.lower():
                engine_root = r"C:\ProgramData\Bluestacks_msi5\Engine"
                player_path = r"C:\Program Files\BlueStacks_msi5\HD-Player.exe"
            else:
                engine_root = r"C:\ProgramData\BlueStacks_nxt\Engine"
                player_path = r"C:\Program Files\BlueStacks_nxt\HD-Player.exe"

            edit_configs(engine_root, log_fn)
            clear_emulator_logs(engine_root, log_fn)

            log_fn("Access Granted successfully!", "success")
            winsound.Beep(800, 300)

            if os.path.exists(player_path):
                log_fn("Starting emulator...", "info")
                subprocess.Popen(player_path, shell=True)
                log_fn("Emulator player started successfully!", "success")
            else:
                log_fn(f"Emulator player path not found: {player_path}", "error")
    except Exception as e:
        log_fn(f"Access Grant failed: {str(e)}", "error")

def handle_install_cert(emulator_type, port, log_fn):
    """Certificate install karna - BlueStacks/MSI + MEmu support (C# InstallCertificate equivalent)"""
    log_fn("Installing System Certificate...", "info")
    try:
        adb_exe = resolve_adb_path(emulator_type)
        emulator_ip = get_emulator_ip(adb_exe, port)
        
        log_fn(f"Connecting to device serial/ip: {emulator_ip}...", "info")
        success, msg = connect_adb(adb_exe, emulator_ip)
        if not success:
            log_fn(f"ADB Connection Failed: {msg}", "error")
            winsound.Beep(500, 300)
            return
            
        log_fn("ADB connected successfully.", "success")

        # Load local certificate
        log_fn("Loading local certificate...", "info")
        cert_src = get_local_cert()
        if not cert_src or not os.path.exists(cert_src):
            log_fn("Failed to locate local certificate in mycerts folder!", "error")
            winsound.Beep(500, 300)
            return
            
        log_fn("Pushing cert file to /sdcard/...", "info")
        run_adb_device(adb_exe, emulator_ip, f"push \"{cert_src}\" /sdcard/{CertHash}.0")

        if "memu" in emulator_type.lower():
            # MEmu specific commands
            run_adb_device(adb_exe, emulator_ip, "root")
            run_adb_device(adb_exe, emulator_ip, "remount")
            cmd = (
                f"mount -o rw,remount /system && "
                f"mount -o rw,remount / && "
                f"mkdir -p /system/etc/security/cacerts/ && "
                f"cp /sdcard/{CertHash}.0 /system/etc/security/cacerts/{CertHash}.0 && "
                f"chmod 644 /system/etc/security/cacerts/{CertHash}.0 && "
                f"chown root:root /system/etc/security/cacerts/{CertHash}.0 && "
                f"mount -o ro,remount /system && "
                f"rm /sdcard/{CertHash}.0"
            )
            log_fn("Executing MEmu system copy script...", "info")
            run_adb_device(adb_exe, emulator_ip, f"shell \"{cmd}\"")
        else:
            # BlueStacks / MSI
            possible_su = [
                "/system/xbin/su",
                "/system/bin/su",
                "/sbin/su",
                "/boot/android/android/system/xbin/bstk/su"
            ]
            su_path = None
            for p in possible_su:
                stdout, _ = run_adb_device(adb_exe, emulator_ip, f"shell \"[ -x {p} ] && echo yes || echo no\"")
                lines = [line.strip().lower() for line in stdout.splitlines()]
                if "yes" in lines:
                    su_path = p
                    break
            
            if su_path is None:
                su_path = "/boot/android/android/system/xbin/bstk/su"
                log_fn(f"Warning: Root check skipped. Using default su: {su_path}", "info")
                
            cmd = (
                f"{su_path} -c '(mount -o rw,remount /system || mount -o rw,remount /dev/sda1 /system) && "
                f"cp /sdcard/{CertHash}.0 /system/etc/security/cacerts/{CertHash}.0 && "
                f"chmod 644 /system/etc/security/cacerts/{CertHash}.0 && "
                f"chcon u:object_r:system_file:s0 /system/etc/security/cacerts/{CertHash}.0 && "
                f"(mount -o ro,remount /system || mount -o ro,remount /dev/sda1 /system) && "
                f"rm /sdcard/{CertHash}.0 && "
                f"setprop ctl.restart zygote'"
            )
            log_fn(f"Executing system copy script via su ({su_path})...", "info")
            run_adb_device(adb_exe, emulator_ip, f"shell \"{cmd}\"")
        
        # Wait for device to reconnect and stabilize after zygote restart
        time.sleep(2)
        log_fn("Waiting for emulator to stabilize after runtime restart...", "info")
        run_adb_device(adb_exe, emulator_ip, "wait-for-device")
        
        # Verify with retries
        success_install = False
        for attempt in range(1, 6):
            time.sleep(1)
            if file_exists_on_device(adb_exe, emulator_ip, f"/system/etc/security/cacerts/{CertHash}.0"):
                success_install = True
                break
                
        if success_install:
            log_fn("System Certificate successfully installed!", "success")
            winsound.Beep(1000, 300)
        else:
            log_fn("Certificate file missing on device system path. Install failed.", "error")
            winsound.Beep(500, 300)
    except Exception as e:
        log_fn(f"Certificate installation error: {str(e)}", "error")

def handle_remove_cert(emulator_type, port, log_fn):
    """Certificate remove karna - BlueStacks/MSI + MEmu support (C# RemoveCertificate equivalent)"""
    log_fn("Removing System Certificate...", "info")
    try:
        adb_exe = resolve_adb_path(emulator_type)
        emulator_ip = get_emulator_ip(adb_exe, port)
        
        success, msg = connect_adb(adb_exe, emulator_ip)
        if not success:
            log_fn(f"ADB Connection Failed: {msg}", "error")
            return
 
        device_path = f"/system/etc/security/cacerts/{CertHash}.0"
 
        if "memu" in emulator_type.lower():
            # MEmu specific
            run_adb_device(adb_exe, emulator_ip, "root")
            run_adb_device(adb_exe, emulator_ip, "remount")
            cmd = (
                f"(mount -o rw,remount /system || mount -o rw,remount /) && "
                f"rm -f /system/etc/security/cacerts/{CertHash}.0 && "
                f"(mount -o ro,remount /system || mount -o ro,remount /)"
            )
            run_adb_device(adb_exe, emulator_ip, f"shell \"{cmd}\"")
        else:
            # BlueStacks / MSI
            has_cert = file_exists_on_device(adb_exe, emulator_ip, device_path)
            
            possible_su = [
                "/system/xbin/su",
                "/system/bin/su",
                "/sbin/su",
                "/boot/android/android/system/xbin/bstk/su"
            ]
            su_path = None
            for p in possible_su:
                stdout, _ = run_adb_device(adb_exe, emulator_ip, f"shell \"[ -x {p} ] && echo yes || echo no\"")
                lines = [line.strip().lower() for line in stdout.splitlines()]
                if "yes" in lines:
                    su_path = p
                    break
                    
            if su_path is None:
                su_path = "/boot/android/android/system/xbin/bstk/su"
                log_fn(f"Warning: Root check skipped. Using default su: {su_path}", "info")
                
            if has_cert:
                timestamp = time.strftime("%Y%m%d%H%M%S")
                backup_name = f"{CertHash}.0.bak.{timestamp}"
                log_fn(f"Backing up cert to /sdcard/{backup_name}...", "info")
                run_adb_device(adb_exe, emulator_ip, f"shell \"{su_path} -c 'cp {device_path} /sdcard/{backup_name}'\"")
            else:
                log_fn("Certificate check: Not detected in system folder. Skipping backup...", "info")
            
            log_fn("Deleting system certificate file...", "info")
            cmd = (
                f"{su_path} -c '(mount -o rw,remount /system || mount -o rw,remount /dev/sda1 /system) && "
                f"rm -f {device_path} && "
                f"(mount -o ro,remount /system || mount -o ro,remount /dev/sda1 /system)'"
            )
            run_adb_device(adb_exe, emulator_ip, f"shell \"{cmd}\"")
        
        time.sleep(0.5)
        if not file_exists_on_device(adb_exe, emulator_ip, device_path):
            log_fn(f"Certificate successfully removed!", "success")
            winsound.Beep(1000, 300)
        else:
            log_fn("Certificate removal command failed to delete file.", "error")
            winsound.Beep(500, 300)
    except Exception as e:
        log_fn(f"Certificate removal error: {str(e)}", "error")

def handle_connect_proxy(emulator_type, port, log_fn, manual_ip=None):
    """Proxy connect karna with IP input support (C# ConnectBypass equivalent)"""
    log_fn("Activating proxy bypass...", "info")
    try:
        adb_exe = resolve_adb_path(emulator_type)
        emulator_ip = get_emulator_ip(adb_exe, port)

        # Use provided IP or default to local IP
        proxy_ip = manual_ip if manual_ip else get_local_ipv4()
        proxy = f"{proxy_ip}:54233"

        log_fn(f"Using Local Proxy: {mask_proxy(proxy)}...", "info")

        # Kill HD-Adb for bluestacks/msi
        if "msi" in emulator_type.lower() or "bluestacks" in emulator_type.lower():
            kill_processes("HD-Adb")

        success, msg = connect_adb(adb_exe, emulator_ip)
        if not success:
            log_fn(f"ADB Connection Failed: {msg}", "error")
            winsound.Beep(500, 300)
            return False

        log_fn(f"Force-stopping app: {PackageName}...", "info")
        run_adb_device(adb_exe, emulator_ip, f"shell am force-stop {PackageName}")
        
        log_fn(f"Setting global HTTP proxy value: {mask_proxy(proxy)}...", "info")
        run_adb_device(adb_exe, emulator_ip, f"shell settings put global http_proxy {proxy}")
        
        log_fn("Launching app via launcher category...", "info")
        run_adb_device(adb_exe, emulator_ip, f"shell monkey -p {PackageName} -c android.intent.category.LAUNCHER 1")
        
        log_fn("Proxy successfully connected", "success")
        winsound.Beep(1200, 100)
        winsound.Beep(1500, 100)
        return True
    except Exception as e:
        log_fn(f"Bypass activation failed: {str(e)}", "error")
        return False

def handle_disconnect_proxy(emulator_type, port, log_fn):
    log_fn("Deactivating proxy bypass...", "info")
    try:
        adb_exe = resolve_adb_path(emulator_type)
        emulator_ip = get_emulator_ip(adb_exe, port)
        
        connect_adb(adb_exe, emulator_ip)
        log_fn("Clearing global HTTP proxy settings...", "info")
        run_adb_device(adb_exe, emulator_ip, "shell settings put global http_proxy :0")
        
        log_fn("Proxy successfully disconnected", "success")
        winsound.Beep(1000, 400)
        return True
    except Exception as e:
        log_fn(f"Bypass deactivation failed: {str(e)}", "error")
        return False

def handle_terminate_all(log_fn):
    log_fn("Terminating all emulator processes...", "info")
    try:
        killed = terminate_processes(["HD-Player", "HD-Adb"])
        if killed:
            log_fn("Processes terminated successfully.", "success")
        else:
            log_fn("No active emulator processes found.", "info")
        winsound.Beep(1000, 300)
    except Exception as e:
        log_fn(f"Process termination failed: {str(e)}", "error")

# ─────────────────────────────────────────────
# Dropdown Arrow Temp Image Generation
# ─────────────────────────────────────────────
import base64
import tempfile
try:
    arrow_png_path = os.path.join(tempfile.gettempdir(), "bypass_dropdown_arrow.png").replace("\\", "/")
    png_bytes = base64.b64decode("iVBORw0KGgoAAAANSUhEUgAAAAwAAAAICAYAAADN5B7xAAAACXBIWXMAAA7EAAAOxAGVKw4bAAAAJ0lEQVQYlWNgoAv4//9/w3/CoIEUTQ2k2IRdMQ5N+BWjaSJOMSEAAA/ccpOoC/fhAAAAAElFTkSuQmCC")
    with open(arrow_png_path, "wb") as f:
        f.write(png_bytes)
except Exception:
    arrow_png_path = ""

# ─────────────────────────────────────────────
# Color Palette (Copied from design demo ui_widgets.py)
BG        = "#09090D"  # Deep dark bg
BG2       = "#111116"  # Surface
BG3       = "#16161E"  # Card background
BORDER    = "#222230"  # Border color
RED       = "#E60000"  # Neon crimson red accent
RED_HOVER = "#FF3333"  # Accent2
RED_DIM   = "#1f0505"  # Dim red highlight
TEXT      = "#FFFFFF"  # Primary white text
TEXT_DIM  = "#888899"  # Dim grey text
TEXT_MID  = "#444455"  # Medium dim text
GREEN     = "#32D769"  # Console log green (QPlainTextEdit match)
ORANGE    = "#FFB300"  # Warn orange

BUTTON_STYLE_PRIMARY = f"""
    QPushButton {{
        background: qlineargradient(x1:0, y1:0, x2:1, y2:0, stop:0 {RED}, stop:1 {RED_HOVER});
        color: white;
        border-radius: 8px;
        border: none;
        font-size: 10pt;
        font-weight: bold;
        letter-spacing: 1px;
    }}
    QPushButton:hover {{
        background: qlineargradient(x1:0, y1:0, x2:1, y2:0, stop:0 {RED_HOVER}, stop:1 #FF6666);
    }}
    QPushButton:pressed {{
        background: {RED};
    }}
"""

BUTTON_STYLE_SECONDARY = f"""
    QPushButton {{
        background: {BG3};
        color: #888899;
        border: 1px solid {BORDER};
        border-radius: 8px;
        font-size: 9pt;
        font-weight: bold;
    }}
    QPushButton:hover {{
        color: white;
        border-color: {RED_HOVER};
        background: #2b2b35;
    }}
"""

BUTTON_STYLE_DANGER = f"""
    QPushButton {{
        background: #2a0505;
        color: #ff5555;
        border: 1px solid #441111;
        border-radius: 8px;
        font-size: 9pt;
        font-weight: bold;
    }}
    QPushButton:hover {{
        background: {RED};
        color: white;
        border-color: {RED};
    }}
"""

ENTRY_STYLE = f"""
    QLineEdit {{
        background: {BG2};
        color: {TEXT};
        border: 1px solid {BORDER};
        border-radius: 8px;
        padding: 0 12px;
        font-family: "Segoe UI";
        font-size: 10pt;
    }}
    QLineEdit:focus {{
        border: 1px solid {RED};
    }}
"""

COMBO_STYLE = f"""
    QComboBox {{
        background: {BG3};
        color: {TEXT};
        border: 1px solid {BORDER};
        border-radius: 6px;
        padding: 4px 12px;
        font-family: "Segoe UI";
        font-size: 10pt;
    }}
    QComboBox:hover {{
        border: 1px solid {RED_HOVER};
        background: #2b2b35;
    }}
    QComboBox::drop-down {{
        subcontrol-origin: padding;
        subcontrol-position: top right;
        width: 20px;
        border-left: none;
        border-top-right-radius: 6px;
        border-bottom-right-radius: 6px;
    }}
    QComboBox::down-arrow {{
        image: url({arrow_png_path});
        width: 12px;
        height: 8px;
    }}
    QComboBox QAbstractItemView {{
        background-color: {BG3};
        color: {TEXT};
        border: 1px solid {BORDER};
        selection-background-color: {RED};
        selection-color: white;
        outline: none;
    }}
"""


class TitleBar(QFrame):
    def __init__(self, window, parent=None):
        super().__init__(parent)
        self._window = window
        self.setFixedHeight(42)
        self.setObjectName("TitleBar")
        self.setStyleSheet(f"QFrame#TitleBar {{ background-color: {BG2}; border: 1px solid {BORDER}; border-radius: 8px; }}")

        layout = QHBoxLayout(self)
        layout.setContentsMargins(16, 0, 8, 0)

        self.brand = QLabel('FEAR <span style="color:#E60000; font-weight: bold;">MODS</span>')
        self.brand.setFont(QFont("Segoe UI", 11, QFont.Bold))
        self.brand.setStyleSheet(f"color: {TEXT}; letter-spacing: 1px;")
        layout.addWidget(self.brand, alignment=Qt.AlignVCenter)

        self.sub_brand = QLabel('[ BYPASS CLIENT ]')
        self.sub_brand.setFont(QFont("Segoe UI", 9))
        self.sub_brand.setStyleSheet("color: #888888; letter-spacing: 0.5px; margin-top: 1px;")
        layout.addWidget(self.sub_brand, alignment=Qt.AlignVCenter)
        layout.addStretch()

        self.btn_min = QPushButton("—")
        self.btn_min.setFixedSize(30, 30)
        self.btn_min.setCursor(Qt.PointingHandCursor)
        self.btn_min.setStyleSheet(f"""
            QPushButton {{
                background: transparent;
                color: {TEXT_DIM};
                border: 1px solid {BORDER};
                border-radius: 6px;
                font-weight: bold;
            }}
            QPushButton:hover {{
                color: white;
                background-color: #2b2b35;
            }}
        """)
        self.btn_min.clicked.connect(self._window.showMinimized)
        layout.addWidget(self.btn_min)

        self.btn_close = QPushButton("✕")
        self.btn_close.setFixedSize(30, 30)
        self.btn_close.setCursor(Qt.PointingHandCursor)
        self.btn_close.setStyleSheet(f"""
            QPushButton {{
                background: transparent;
                color: {TEXT_DIM};
                border: 1px solid {BORDER};
                border-radius: 6px;
                font-weight: bold;
            }}
            QPushButton:hover {{
                color: white;
                background-color: {RED};
                border-color: {RED};
            }}
        """)
        self.btn_close.clicked.connect(self._window.close)
        layout.addWidget(self.btn_close)

    def mousePressEvent(self, e):
        if e.button() == Qt.LeftButton:
            self._window._drag_pos = e.globalPos()

    def mouseMoveEvent(self, e):
        if e.buttons() == Qt.LeftButton and hasattr(self._window, '_drag_pos'):
            self._window.move(self._window.pos() + e.globalPos() - self._window._drag_pos)
            self._window._drag_pos = e.globalPos()


class App(QWidget):
    log_signal = pyqtSignal(str, str)
    trigger_connect_proxy = pyqtSignal()
    trigger_switch_tab = pyqtSignal(int)

    def __init__(self):
        super().__init__()
        self.setWindowFlags(Qt.FramelessWindowHint)
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setFixedSize(580, 480)

        self._manual_proxy_ip = ""
        self._detected_emu = None
        self._proxy_connected = False
        self._drag_pos = QPoint()

        self.log_signal.connect(self._log_message_slot)
        self.trigger_connect_proxy.connect(self._connect_proxy_action)
        self.trigger_switch_tab.connect(self._select_tab)

        self.wrapper = QFrame(self)
        self.wrapper.setGeometry(0, 0, 580, 480)
        self.wrapper.setObjectName("Wrapper")
        self.wrapper.setStyleSheet(f"QFrame#Wrapper {{ background-color: {BG}; border: 1px solid {BORDER}; border-radius: 12px; }}")

        wrapper_layout = QVBoxLayout(self.wrapper)
        wrapper_layout.setContentsMargins(12, 12, 12, 10)
        wrapper_layout.setSpacing(10)

        self.titlebar = TitleBar(self)
        wrapper_layout.addWidget(self.titlebar)

        self.body_widget = QWidget()
        body_layout = QHBoxLayout(self.body_widget)
        body_layout.setContentsMargins(0, 0, 0, 0)
        body_layout.setSpacing(10)

        self.sidebar = QFrame()
        self.sidebar.setObjectName("Sidebar")
        self.sidebar.setFixedWidth(64)
        self.sidebar.setStyleSheet(f"QFrame#Sidebar {{ border: 1px solid {BORDER}; background-color: {BG2}; border-radius: 8px; }}")
        
        sidebar_layout = QVBoxLayout(self.sidebar)
        sidebar_layout.setContentsMargins(10, 15, 10, 15)
        sidebar_layout.setSpacing(12)

        self.tabs_btns = []
        
        # Custom vector icons via Segoe MDL2 Assets
        btn_lightning = QPushButton("\uE945")
        btn_lightning.setFixedSize(42, 42)
        btn_lightning.setCursor(Qt.PointingHandCursor)
        btn_lightning.clicked.connect(lambda _, idx=0: self._select_tab(idx))
        sidebar_layout.addWidget(btn_lightning)
        self.tabs_btns.append(btn_lightning)
        
        btn_gear = QPushButton("\uE713")
        btn_gear.setFixedSize(42, 42)
        btn_gear.setCursor(Qt.PointingHandCursor)
        btn_gear.clicked.connect(lambda _, idx=1: self._select_tab(idx))
        sidebar_layout.addWidget(btn_gear)
        self.tabs_btns.append(btn_gear)

        sidebar_layout.addStretch()
        body_layout.addWidget(self.sidebar)

        # Right Card Container matching sidebar style
        self.right_card = QFrame()
        self.right_card.setObjectName("RightCard")
        self.right_card.setStyleSheet(f"QFrame#RightCard {{ border: 1px solid {BORDER}; background-color: {BG2}; border-radius: 8px; }}")
        
        right_card_layout = QVBoxLayout(self.right_card)
        right_card_layout.setContentsMargins(0, 0, 0, 0)
        right_card_layout.setSpacing(0)

        self.stack = QStackedWidget()
        right_card_layout.addWidget(self.stack)
        body_layout.addWidget(self.right_card, 1)

        self._build_bypass_tab()
        self._build_settings_tab()

        self._select_tab(0)
        wrapper_layout.addWidget(self.body_widget, 1)

        self.log_card = QFrame()
        self.log_card.setFixedHeight(120)
        self.log_card.setObjectName("LogCard")
        self.log_card.setStyleSheet(f"QFrame#LogCard {{ border: 1px solid {BORDER}; background-color: {BG2}; border-radius: 8px; }}")
        
        log_card_layout = QVBoxLayout(self.log_card)
        log_card_layout.setContentsMargins(12, 8, 12, 8)
        log_card_layout.setSpacing(4)

        log_lbl = QLabel("📜 SYSTEM LOGS")
        log_lbl.setFont(QFont("Segoe UI", 9, QFont.Bold))
        log_lbl.setStyleSheet(f"color: {TEXT}; border: none; background: transparent;")
        log_card_layout.addWidget(log_lbl)

        self.console = QPlainTextEdit()
        self.console.setReadOnly(True)
        self.console.setFont(QFont("Consolas", 9))
        self.console.setStyleSheet(f"""
            QPlainTextEdit {{
                color: {GREEN};
                background-color: transparent;
                border: none;
            }}
            QScrollBar:vertical {{
                border: none;
                background: transparent;
                width: 6px;
                margin: 0px;
                border-radius: 3px;
            }}
            QScrollBar::handle:vertical {{
                background: #3a3a47;
                min-height: 20px;
                border-radius: 3px;
            }}
            QScrollBar::handle:vertical:hover {{
                background-color: {RED};
            }}
            QScrollBar::add-line:vertical, QScrollBar::sub-line:vertical {{
                border: none;
                background: none;
                height: 0px;
            }}
            QScrollBar::add-page:vertical, QScrollBar::sub-page:vertical {{
                background: none;
            }}
        """)
        log_card_layout.addWidget(self.console)
        wrapper_layout.addWidget(self.log_card)


        self.log_message("FEAR MODS Bypass client initialized successfully.", "success")
        self.log_message("HWID: " + get_hwid(), "info")
        self.do_auto_detect()

    def _select_tab(self, index):
        self.stack.setCurrentIndex(index)
        
        for i, btn in enumerate(self.tabs_btns):
            if i == index:
                btn.setStyleSheet(f"""
                    QPushButton {{
                        background-color: {RED};
                        color: #FFFFFF;
                        border: none;
                        border-radius: 8px;
                        font-family: "Segoe MDL2 Assets";
                        font-size: 16pt;
                        font-weight: bold;
                    }}
                """)
            else:
                btn.setStyleSheet(f"""
                    QPushButton {{
                        background-color: transparent;
                        color: #888899;
                        border: none;
                        border-radius: 8px;
                        font-family: "Segoe MDL2 Assets";
                        font-size: 16pt;
                    }}
                    QPushButton:hover {{
                        background-color: #2b2b35;
                        color: #FFFFFF;
                    }}
                """)

    def _build_bypass_tab(self):
        page = QWidget()
        layout = QVBoxLayout(page)
        layout.setContentsMargins(12, 12, 12, 12)
        layout.setSpacing(6)

        hdr_banner = QLabel("BYPASS FEATURES")
        hdr_banner.setFont(QFont("Segoe UI", 9, QFont.Bold))
        hdr_banner.setAlignment(Qt.AlignCenter)
        hdr_banner.setFixedHeight(30)
        hdr_banner.setStyleSheet(f"QLabel {{ background-color: {RED}; color: white; border-radius: 12px; letter-spacing: 1.5px; font-weight: bold; }}")
        layout.addWidget(hdr_banner)

        self.status_card = QFrame()
        self.status_card.setObjectName("StatusCard")
        self.status_card.setStyleSheet(f"QFrame#StatusCard {{ border: 1px solid {BORDER}; background-color: {BG3}; border-radius: 8px; }}")
        
        sc_layout = QGridLayout(self.status_card)
        sc_layout.setContentsMargins(12, 10, 12, 10)
        sc_layout.setVerticalSpacing(6)
        sc_layout.setHorizontalSpacing(10)

        emu_lbl = QLabel("Bypass Version: 2.1.000")
        emu_lbl.setFont(QFont("Segoe UI", 9))
        emu_lbl.setStyleSheet(f"color: {TEXT_DIM}; border: none; background: transparent;")
        sc_layout.addWidget(emu_lbl, 0, 0, Qt.AlignLeft)

        self._emu_badge = QLabel("● None Detected")
        self._emu_badge.setFont(QFont("Segoe UI", 9, QFont.Bold))
        self._emu_badge.setStyleSheet("color: #ff4444; border: none; background: transparent;")
        sc_layout.addWidget(self._emu_badge, 0, 1, Qt.AlignRight)

        conn_lbl = QLabel("System Status:")
        conn_lbl.setFont(QFont("Segoe UI", 9))
        conn_lbl.setStyleSheet(f"color: {TEXT_DIM}; border: none; background: transparent;")
        sc_layout.addWidget(conn_lbl, 1, 0, Qt.AlignLeft)

        self.proxy_status_label = QLabel("● Disconnected")
        self.proxy_status_label.setFont(QFont("Segoe UI", 9, QFont.Bold))
        self.proxy_status_label.setStyleSheet("color: #ff4444; border: none; background: transparent;")
        sc_layout.addWidget(self.proxy_status_label, 1, 1, Qt.AlignRight)

        # 3rd Row: Local IPv4 Address
        ip_lbl = QLabel("Local IPv4:")
        ip_lbl.setFont(QFont("Segoe UI", 9))
        ip_lbl.setStyleSheet(f"color: {TEXT_DIM}; border: none; background: transparent;")
        sc_layout.addWidget(ip_lbl, 2, 0, Qt.AlignLeft)

        local_ip = get_local_ipv4() or "127.0.0.1"
        self.ip_val_label = QLabel(f"● {local_ip}")
        self.ip_val_label.setFont(QFont("Segoe UI", 9, QFont.Bold))
        self.ip_val_label.setStyleSheet("color: #00bcd4; border: none; background: transparent;")
        sc_layout.addWidget(self.ip_val_label, 2, 1, Qt.AlignRight)

        layout.addWidget(self.status_card)


        self.btn_activate = QPushButton("\uE768  ACTIVATE BYPASS")
        set_button_font(self.btn_activate, 10, True)
        self.btn_activate.setFixedHeight(38)
        self.btn_activate.setCursor(Qt.PointingHandCursor)
        self.btn_activate.setStyleSheet(BUTTON_STYLE_PRIMARY)
        self.btn_activate.clicked.connect(self._connect_proxy_action)
        layout.addWidget(self.btn_activate)

        self.btn_deactivate = QPushButton("\uE711  DEACTIVATE BYPASS")
        set_button_font(self.btn_deactivate, 9, True)
        self.btn_deactivate.setFixedHeight(34)
        self.btn_deactivate.setCursor(Qt.PointingHandCursor)
        self.btn_deactivate.setStyleSheet(BUTTON_STYLE_SECONDARY)
        self.btn_deactivate.clicked.connect(self._disconnect_proxy_action)
        layout.addWidget(self.btn_deactivate)

        self.btn_terminate = QPushButton("\uE74D  TERMINATE EMULATORS")
        set_button_font(self.btn_terminate, 9, True)
        self.btn_terminate.setFixedHeight(32)
        self.btn_terminate.setCursor(Qt.PointingHandCursor)
        self.btn_terminate.setStyleSheet(BUTTON_STYLE_DANGER)
        self.btn_terminate.clicked.connect(lambda: self.run_async_task(handle_terminate_all, self.log_message))
        layout.addWidget(self.btn_terminate)

        layout.addStretch()
        self.stack.addWidget(page)

    def _build_settings_tab(self):
        page = QWidget()
        layout = QVBoxLayout(page)
        layout.setContentsMargins(12, 12, 12, 12)
        layout.setSpacing(6)

        hdr_banner = QLabel("EMULATOR SETTINGS")
        hdr_banner.setFont(QFont("Segoe UI", 9, QFont.Bold))
        hdr_banner.setAlignment(Qt.AlignCenter)
        hdr_banner.setFixedHeight(30)
        hdr_banner.setStyleSheet(f"QLabel {{ background-color: {RED}; color: white; border-radius: 12px; letter-spacing: 1.5px; font-weight: bold; }}")
        layout.addWidget(hdr_banner)

        row1 = QWidget()
        row1_lay = QHBoxLayout(row1)
        row1_lay.setContentsMargins(0, 0, 0, 0)
        row1_lay.setSpacing(6)

        self.adb_combo = QComboBox()
        self.adb_combo.addItems(["Msi", "Bluestacks", "MEmu"])
        self.adb_combo.setFixedHeight(34)
        self.adb_combo.setStyleSheet(COMBO_STYLE)
        row1_lay.addWidget(self.adb_combo, 1)

        self.adb_text = QLineEdit()
        self.adb_text.setText("5555")
        self.adb_text.setFixedWidth(80)
        self.adb_text.setFixedHeight(34)
        self.adb_text.setStyleSheet(ENTRY_STYLE)
        row1_lay.addWidget(self.adb_text)
        layout.addWidget(row1)

        self.btn_auto = QPushButton("\uE945  AUTO SETUP BYPASS")
        set_button_font(self.btn_auto, 10, True)
        self.btn_auto.setFixedHeight(38)
        self.btn_auto.setCursor(Qt.PointingHandCursor)
        self.btn_auto.setStyleSheet(BUTTON_STYLE_PRIMARY)
        self.btn_auto.clicked.connect(self.do_auto_setup)
        layout.addWidget(self.btn_auto)

        self.btn_grant = QPushButton("\uE717  GRANT SYSTEM ACCESS")
        set_button_font(self.btn_grant, 9, True)
        self.btn_grant.setFixedHeight(32)
        self.btn_grant.setCursor(Qt.PointingHandCursor)
        self.btn_grant.setStyleSheet(BUTTON_STYLE_SECONDARY)
        self.btn_grant.clicked.connect(lambda: self.run_async_task(
            handle_access, self.adb_combo.currentText(), self.log_message))
        layout.addWidget(self.btn_grant)


        row2 = QWidget()
        row2_lay = QHBoxLayout(row2)
        row2_lay.setContentsMargins(0, 0, 0, 0)
        row2_lay.setSpacing(6)

        self.btn_inst = QPushButton("\uE118  INSTALL CERT")
        set_button_font(self.btn_inst, 9, True)
        self.btn_inst.setFixedHeight(32)
        self.btn_inst.setCursor(Qt.PointingHandCursor)
        self.btn_inst.setStyleSheet(BUTTON_STYLE_SECONDARY)
        self.btn_inst.clicked.connect(lambda: self.run_async_task(
            handle_install_cert, self.adb_combo.currentText(), self.adb_text.text(), self.log_message))
        row2_lay.addWidget(self.btn_inst)

        self.btn_rem = QPushButton("\uE74D  REMOVE CERT")
        set_button_font(self.btn_rem, 9, True)
        self.btn_rem.setFixedHeight(32)
        self.btn_rem.setCursor(Qt.PointingHandCursor)
        self.btn_rem.setStyleSheet(BUTTON_STYLE_SECONDARY)
        self.btn_rem.clicked.connect(lambda: self.run_async_task(
            handle_remove_cert, self.adb_combo.currentText(), self.adb_text.text(), self.log_message))
        row2_lay.addWidget(self.btn_rem)
        layout.addWidget(row2)

        row3 = QWidget()
        row3_lay = QHBoxLayout(row3)
        row3_lay.setContentsMargins(0, 0, 0, 0)
        row3_lay.setSpacing(6)

        self.btn_det = QPushButton("\uE71E  AUTO DETECT EMU")
        set_button_font(self.btn_det, 9, True)
        self.btn_det.setFixedHeight(32)
        self.btn_det.setCursor(Qt.PointingHandCursor)
        self.btn_det.setStyleSheet(BUTTON_STYLE_SECONDARY)
        self.btn_det.clicked.connect(self.do_auto_detect)
        row3_lay.addWidget(self.btn_det)

        self.btn_hwid = QPushButton("\uE890  SHOW HWID")
        set_button_font(self.btn_hwid, 9, True)
        self.btn_hwid.setFixedHeight(32)
        self.btn_hwid.setCursor(Qt.PointingHandCursor)
        self.btn_hwid.setStyleSheet(BUTTON_STYLE_SECONDARY)
        self.btn_hwid.clicked.connect(self.show_hwid)
        row3_lay.addWidget(self.btn_hwid)
        layout.addWidget(row3)

        layout.addStretch()
        self.stack.addWidget(page)

    def _log_message_slot(self, msg, level):
        ts = time.strftime("%H:%M:%S")
        self.console.appendPlainText(f"[{ts}] {msg}")
        scrollbar = self.console.verticalScrollBar()
        scrollbar.setValue(scrollbar.maximum())

    def log_message(self, msg, level="info"):
        self.log_signal.emit(msg, level)

    def _connect_proxy_action(self):
        def run():
            self.log_message("Connecting proxy...", "info")
            self.proxy_status_label.setStyleSheet("color: #ffa500; border: none;")
            self.proxy_status_label.setText("● Connecting...")
            ip = self._manual_proxy_ip if self._manual_proxy_ip else None
            ok = handle_connect_proxy(
                self.adb_combo.currentText(), self.adb_text.text(),
                self.log_message, manual_ip=ip)
            if ok:
                self._proxy_connected = True
                self.proxy_status_label.setStyleSheet("color: #00D04A; border: none;")
                self.proxy_status_label.setText("● Connected")
            else:
                self._proxy_connected = False
                self.proxy_status_label.setStyleSheet("color: #ff4444; border: none;")
                self.proxy_status_label.setText("● Disconnected")
        threading.Thread(target=run, daemon=True).start()

    def _disconnect_proxy_action(self):
        def run():
            self.log_message("Disconnecting proxy...", "info")
            self.proxy_status_label.setStyleSheet("color: #ffa500; border: none;")
            self.proxy_status_label.setText("● Disconnecting...")
            handle_disconnect_proxy(
                self.adb_combo.currentText(), self.adb_text.text(), self.log_message)
            self._proxy_connected = False
            self.proxy_status_label.setStyleSheet("color: #ff4444; border: none;")
            self.proxy_status_label.setText("● Disconnected")
        threading.Thread(target=run, daemon=True).start()
    def do_auto_setup(self):
        def run():
            emu = self.adb_combo.currentText()
            port = self.adb_text.text()
            self.log_message("STARTING ONE-CLICK AUTO SETUP...", "info")
            
            # Step 1: Grant Access and Restart
            self.log_message("[AUTO] Step 1/5: Granting system access & launching emulator...", "info")
            try:
                handle_access(emu, self.log_message)
            except Exception as e:
                self.log_message(f"[AUTO] Step 1 failed: {str(e)}", "error")
            
            # Step 2: Poll ADB Connection until Emulator is booted
            self.log_message("[AUTO] Step 2/5: Waiting for emulator player to load (max 30s)...", "info")
            try:
                adb_exe = resolve_adb_path(emu)
                emulator_ip = get_emulator_ip(adb_exe, port)
                connected = False
                for attempt in range(1, 16):
                    time.sleep(2)
                    self.log_message(f"[AUTO] Checking ADB connection (Attempt {attempt}/15)...", "info")
                    success, _ = connect_adb(adb_exe, emulator_ip)
                    if success:
                        connected = True
                        break
                if connected:
                    self.log_message("[AUTO] Emulator detected and connected successfully!", "success")
                else:
                    self.log_message("[AUTO] Warning: ADB connection timed out. Trying to proceed...", "info")
            except Exception as e:
                self.log_message(f"[AUTO] Step 2 check failed: {str(e)}", "error")

            # Step 3: Remove Old Cert
            self.log_message("[AUTO] Step 3/5: Removing old certificate...", "info")
            try:
                handle_remove_cert(emu, port, self.log_message)
            except Exception as e:
                self.log_message(f"[AUTO] Step 3 failed: {str(e)}", "error")

            # Step 4: Install New Cert
            self.log_message("[AUTO] Step 4/5: Pushing and installing new system certificate...", "info")
            try:
                handle_install_cert(emu, port, self.log_message)
            except Exception as e:
                self.log_message(f"[AUTO] Step 4 failed: {str(e)}", "error")

            # Step 5: Wait and Activate Bypass
            self.log_message("[AUTO] Step 5/5: Finalizing certificate installation and activating bypass...", "info")
            time.sleep(3)
            
            self.log_message("ACTIVATING BYPASS NETWORK...", "success")
            self.trigger_switch_tab.emit(0)
            self.trigger_connect_proxy.emit()
            
        threading.Thread(target=run, daemon=True).start()

    def run_async_task(self, target_fn, *args):
        threading.Thread(target=target_fn, args=args, daemon=True).start()

    def do_auto_detect(self):
        def _detect():
            self.log_message("Auto-detecting emulator...", "info")
            result = auto_detect_emulator()
            self._detected_emu = result
            name = result.get("name", "None Detected")
            adb  = result.get("adb", "")
            device_id = result.get("device_id", "127.0.0.1:5555")
            self.log_message(f"Detected: {name}", "success" if adb else "error")
            
            if adb:
                self._emu_badge.setStyleSheet("color: #00D04A; border: none;")
                self._emu_badge.setText(f"● {name}")
            else:
                self._emu_badge.setStyleSheet("color: #ff4444; border: none;")
                self._emu_badge.setText(f"● {name}")

            if "MEmu" in name:
                self.adb_combo.setCurrentText("MEmu")
            elif "MSI" in name:
                self.adb_combo.setCurrentText("Msi")
            else:
                self.adb_combo.setCurrentText("Bluestacks")
            port = device_id.split(":")[-1] if ":" in device_id else "5555"
            self.adb_text.setText(port)
        threading.Thread(target=_detect, daemon=True).start()

    def show_hwid(self):
        def _get():
            self.log_message("Fetching Hardware ID...", "info")
            hwid = get_hwid()
            self.log_message(f"HWID: {hwid}", "success")
        threading.Thread(target=_get, daemon=True).start()

# ─────────────────────────────────────────────
# DLL DOWNLOAD + LOAD SYSTEM  (C# DownloadDllAsync + LoadLibrary)
# ─────────────────────────────────────────────

def get_dll_path():
    """DLL save karne ki path (same folder as script/exe)"""
    if getattr(sys, 'frozen', False):
        base = os.path.dirname(sys.executable)
    else:
        base = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(base, DllName)


def draw_progress_bar(progress, width=30, prefix_len=28):
    """Console mein progress bar draw karna (C# DrawProgressBar equivalent)"""
    if sys.stdout is not None:
        try:
            position = int(progress / 100 * width)
            bar = "█" * position + " " * (width - position)
            sys.stdout.write(f"\r{' ' * prefix_len}[{bar}] {str(progress) + '%':>4}")
            sys.stdout.flush()
        except Exception:
            pass


def download_dll(log_fn=None):
    """
    DLL ko online se download karna with progress bar.
    C# DownloadDllAsync equivalent.
    Returns True on success, False on failure.
    """
    dll_path = get_dll_path()

    def _log(msg, level="info"):
        if log_fn:
            log_fn(msg, level)
        else:
            print(f"  [{level.upper()}] {msg}")

    _log("Syncing Security Modules...", "info")

    try:
        req = urllib.request.Request(
            DllUrl,
            headers={"User-Agent": "Mozilla/5.0"}
        )
        with urllib.request.urlopen(req, timeout=30) as response:
            total_size = response.headers.get("Content-Length")
            total_size = int(total_size) if total_size else -1

            downloaded = 0
            last_progress = -1
            chunk_size = 8192

            with open(dll_path, "wb") as f:
                while True:
                    chunk = response.read(chunk_size)
                    if not chunk:
                        break
                    f.write(chunk)
                    downloaded += len(chunk)

                    if total_size > 0:
                        progress = int(downloaded / total_size * 100)
                        if progress != last_progress:
                            draw_progress_bar(progress)
                            if progress % 25 == 0:
                                _log(f"Downloading Security Module: {progress}%", "info")
                            last_progress = progress

        # Final 100%
        draw_progress_bar(100)
        _log("Security Module Synced & Loaded! [DONE]", "success")
        return True

    except Exception as ex:
        print()  # newline
        _log(f"Sync Failed: {ex}", "error")

        if os.path.exists(dll_path):
            _log("Using cached module...", "info")
            return True
        else:
            _log("Essential module missing. Cannot proceed.", "error")
            return False


def load_dll(log_fn=None):
    """
    Downloaded DLL ko ctypes se load karna.
    C# LoadLibrary equivalent.
    Returns the loaded dll handle or None.
    """
    dll_path = get_dll_path()

    def _log(msg, level="info"):
        if log_fn:
            log_fn(msg, level)
        else:
            print(f"  [{level.upper()}] {msg}")

    if not os.path.exists(dll_path):
        _log(f"FATAL ERROR: {DllName} not found at {dll_path}", "error")
        winsound.Beep(400, 800)
        return None

    try:
        dll_handle = ctypes.WinDLL(dll_path)
        _log(f"{DllName} loaded successfully.", "success")
        return dll_handle
    except OSError as e:
        _log(f"FATAL ERROR: Failed to load {DllName} - {e}", "error")
        winsound.Beep(400, 800)
        return None


def startup_load_dll(log_fn=None):
    """
    Download + Load DLL in sequence.
    GUI ke andar log_fn pass karo ya None for console.
    """
    success = download_dll(log_fn=log_fn)
    if not success:
        return None
    return load_dll(log_fn=log_fn)


if __name__ == "__main__":
    if not is_administrator():
        restart_as_admin()

    # Launch PyQt5 application
    app_qt = QApplication(sys.argv)
    window = App()
    window.show()

    # DLL Download + Load - GUI console thread-safe log handler
    def _dll_startup():
        time.sleep(0.4)
        handle = startup_load_dll(log_fn=window.log_message)
        if handle is None:
            window.log_message("FATAL: Security module failed to load!", "error")
        else:
            window.log_message(f"{DllName} - Security Module Active.", "success")
    threading.Thread(target=_dll_startup, daemon=True).start()

    sys.exit(app_qt.exec_())
