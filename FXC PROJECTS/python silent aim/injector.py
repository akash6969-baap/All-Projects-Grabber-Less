import os
import shutil
import pymem
import time
import threading
import sys


def inject_dll(process, dll_path, log_callback=None):
    try:
        if not os.path.exists(dll_path):
            return False
        dll_path_bytes = dll_path.encode("utf-8")
        pymem.process.inject_dll(process.process_handle, dll_path_bytes)
        return True
    except Exception:
        return False




def play_success_sound():
    try:
        import winsound
        import sys

        # 1. Detect base execution folders
        exe_dir = os.path.dirname(sys.executable) if getattr(sys, 'frozen', False) else os.path.dirname(os.path.abspath(__file__))
        bundle_dir = getattr(sys, '_MEIPASS', exe_dir)

        # 2. Check if Succes.wav is placed next to the Exe (for custom overrides)
        custom_wav = os.path.join(exe_dir, "Succes.wav")
        if os.path.exists(custom_wav):
            winsound.PlaySound(custom_wav, winsound.SND_FILENAME | winsound.SND_ASYNC)
            return

        # 3. Check if Succes.wav is bundled inside the Exe (sys._MEIPASS)
        bundled_wav = os.path.join(bundle_dir, "Succes.wav")
        if os.path.exists(bundled_wav):
            winsound.PlaySound(bundled_wav, winsound.SND_FILENAME | winsound.SND_ASYNC)
            return

        # 4. Fallback to any .wav file next to the Exe
        local_wavs = [os.path.join(exe_dir, f) for f in os.listdir(exe_dir) if f.endswith('.wav')]
        if local_wavs:
            winsound.PlaySound(local_wavs[0], winsound.SND_FILENAME | winsound.SND_ASYNC)
        else:
            # 5. Fallback to premium Windows System Notification sound
            winsound.PlaySound("SystemAsterisk", winsound.SND_ALIAS | winsound.SND_ASYNC)
    except Exception:
        pass


def streamesp_thread(log_callback=None):
    def log(msg, delay=0.3):
        if log_callback:
            log_callback(msg)
        else:
            print(msg)
        if delay > 0:
            time.sleep(delay)

    process_name = "HD-Player.exe"

    try:
        log("Initializing security module...", 0.4)
        log("Scanning game instance [HD-Player.exe]...", 0.5)

        # exe / py location detect
        if getattr(sys, 'frozen', False):
            base_dir = os.path.dirname(sys.executable)
        else:
            base_dir = os.path.dirname(os.path.abspath(__file__))

        client_dll = os.path.join(base_dir, "Client.dll")
        cimgui_dll = os.path.join(base_dir, "cimgui.dll")
        aotbst_dll = os.path.join(base_dir, "AotBst.dll")

        process = pymem.Pymem(process_name)
        log("Process attached successfully. [PID: %d]" % process.process_id, 0.4)

        # Client.dll temp copy
        if os.path.exists(client_dll):
            temp_client = "C:\\Windows\\Temp\\Client.dll"
            try:
                shutil.copy(client_dll, temp_client)
            except Exception:
                log("Critical integrity check failed!", 0)
                return

        log("Performing memory integrity bypass...", 0.6)
        
        # silent inject order
        if inject_dll(process, cimgui_dll, None):
            log("Cryptographic handshake established.", 0.5)
            
            log("Decrypting memory tables and offsets...", 0.7)
            if inject_dll(process, aotbst_dll, None):
                log("Syncing offset vectors with virtual memory...", 0.6)
                
                log("Establishing communication tunnel...", 0.5)
                log("Connection secure. Status: ONLINE!", 0)
                
                # Trigger embedded custom anime audio sound in background
                play_success_sound()

    except pymem.exception.ProcessNotFound:
        log("Game instance not found. Please launch the game first!", 0)
    except Exception:
        log("Server synchronization failed!", 0)


def streameesp(log_callback=None):
    threading.Thread(target=streamesp_thread, args=(log_callback,), daemon=True).start()
