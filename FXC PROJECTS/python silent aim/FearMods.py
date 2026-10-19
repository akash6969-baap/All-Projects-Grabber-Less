import sys
import os
import ctypes
from PyQt5.QtWidgets import (
    QApplication, QWidget, QTabWidget, QVBoxLayout, QHBoxLayout,
    QLabel, QLineEdit, QFrame, QPushButton, QCheckBox, QScrollArea, QMessageBox, QComboBox, QSlider,
    QStyledItemDelegate, QStyle, QStyleOptionComboBox
)
from PyQt5.QtCore import Qt, QSize, QThread, pyqtSignal, QTimer, QRect, QPoint
from PyQt5.QtGui import QFont, QPainter, QColor, QPen, QPixmap, QMouseEvent, QPalette
import sys, hashlib
from internal import *
from injector import *
from keyauth import api
import sys
from pymem.memory import read_bytes, write_bytes
from pymem.pattern import pattern_scan_all
from pymem import *
import winsound
import pydivert
import threading
import time
import math
import winsound

# Enhanced admin check with better Windows 10 compatibility
def is_admin():
    try:
        return ctypes.windll.shell32.IsUserAnAdmin()
    except:
        return False

if not is_admin():
    print("[DEBUG] Running without admin - UI preview mode")
    # Bypass admin check for UI preview

W_O = None
W_I = None
W_F = None
R_I = False
T_L = threading.Lock()

def F_WD():
    global W_O, W_I, W_F
    try:
        for w in [W_O, W_I, W_F]:
            if w:
                w.close()
        W_O = W_I = W_F = None
    except Exception as e:
        print(f"Lỗi đóng WinDivert: {e}")

def getchecksum():
    try:
        with open(sys.argv[0], "rb") as file:
            return hashlib.md5(file.read()).hexdigest()
    except Exception as e:
        print(f"Error computing checksum: {e}")
        return ""

def update_fov(value):
    result = send_command_to_esp(f"aimfov:{value:.1f}")
    return f"FOV updated to {value:.1f} | Return: {result}"

# ======= FREEZE AND TELE LOGIC =======
class FreezeTeleManager:
    def __init__(self):
        self.running = True
        self.lock = threading.Lock()
        
        # Freeze mode
        self.freeze_mode = False
        self.packet_freeze = []
        self.freeze_hotkey = "f1"
        
        # Tele mode
        self.tele_mode = False
        self.packet_tele = []
        self.tele_hotkey = "f2"
        
        # Packet filters
        self.FILTER_FREEZE = "(udp.SrcPort >= 10011 and udp.SrcPort <= 10019) and ip and ip.Protocol == 17 and ip.Length >= 52 and ip.Length <= 1491"
        self.FILTER_TELE = "udp and outbound and (udp.PayloadLength >= 35 or ip.Length < 72 and ip.Length >= 193 or udp.PayloadLength == 25)"
        
        # Start packet handling threads
        self.freeze_thread = threading.Thread(target=self.divert_freeze, daemon=True)
        self.tele_thread = threading.Thread(target=self.divert_tele, daemon=True)
        self.freeze_thread.start()
        self.tele_thread.start()

    def send_bursts(self, to_send, burst_size=300):
        """Send packets in bursts to avoid detection"""
        with pydivert.WinDivert("outbound", layer=pydivert.Layer.NETWORK) as sender:
            for i in range(0, len(to_send), burst_size):
                burst = to_send[i:i+burst_size]
                for pkt in burst:
                    try:
                        pkt_rebuilt = pydivert.Packet(pkt.raw, pkt.interface, pkt.direction)
                        sender.send(pkt_rebuilt)
                        time.sleep(0.0073)
                    except Exception as e:
                        print(f"Error sending packet: {e}")

    def toggle_freeze(self):
        """Toggle freeze mode on/off"""
        with self.lock:
            if self.freeze_mode:
                self.freeze_mode = False
                to_send = list(self.packet_freeze)
                self.packet_freeze.clear()
                threading.Thread(target=lambda: self.send_bursts(to_send), daemon=True).start()
                return "Freeze Mode: Disabled | Releasing packets..."
            else:
                self.freeze_mode = True
                self.packet_freeze.clear()
                return "Freeze Mode: Enabled | Holding packets..."

    def toggle_tele(self):
        """Toggle tele mode on/off"""
        with self.lock:
            if self.tele_mode:
                self.tele_mode = False
                to_send = list(self.packet_tele)
                self.packet_tele.clear()
                threading.Thread(target=lambda: self.send_bursts(to_send), daemon=True).start()
                return "Tele Mode: Disabled | Releasing packets..."
            else:
                self.tele_mode = True
                self.packet_tele.clear()
                return "Tele Mode: Enabled | Holding packets..."

    def divert_freeze(self):
        """Handle freeze packet diversion"""
        try:
            with pydivert.WinDivert(self.FILTER_FREEZE) as w:
                for packet in w:
                    if not self.running:
                        break
                    with self.lock:
                        if self.freeze_mode:
                            self.packet_freeze.append(packet)
                            continue
                    try:
                        w.send(packet)
                    except Exception as e:
                        print(f"Error sending freeze packet: {e}")
        except Exception as e:
            print(f"Freeze divert error: {e}")

    def divert_tele(self):
        """Handle tele packet diversion"""
        try:
            with pydivert.WinDivert(self.FILTER_TELE) as w:
                for packet in w:
                    if not self.running:
                        break
                    with self.lock:
                        if self.tele_mode:
                            self.packet_tele.append(packet)
                            continue
                    try:
                        w.send(packet)
                    except Exception as e:
                        print(f"Error sending tele packet: {e}")
        except Exception as e:
            print(f"Tele divert error: {e}")

    def stop(self):
        """Stop all packet handling"""
        self.running = False

class TimerOverlay(QWidget):
    def __init__(self):
        super().__init__()
        self.setWindowFlags(Qt.FramelessWindowHint | Qt.WindowStaysOnTopHint | Qt.Tool)
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setFixedSize(120, 40)

        screen_geometry = QApplication.primaryScreen().availableGeometry()
        self.move(screen_geometry.width() - 130, 10)
        
        self.timer = QTimer()
        self.timer.timeout.connect(self.update_timer)
        self.remaining_time = 180
        self.is_running = False
        
        self.setStyleSheet("""
            QWidget {
                background-color: rgba(30, 30, 30, 200);
                border: 1px solid #cc33cc;
                border-radius: 8px;
                color: white;
            }
        """)
        
    def paintEvent(self, event):
        painter = QPainter(self)
        painter.setRenderHint(QPainter.Antialiasing)

        painter.fillRect(self.rect(), QColor(30, 30, 30, 200))

        painter.setPen(QPen(QColor(204, 51, 204), 2))
        painter.drawRoundedRect(1, 1, self.width()-2, self.height()-2, 8, 8)

        painter.setPen(QColor(255, 255, 255))
        painter.setFont(QFont("Segoe UI", 10, QFont.Bold))
        
        minutes = self.remaining_time // 60
        seconds = self.remaining_time % 60
        timer_text = f"{minutes:02d}:{seconds:02d}"
        
        painter.drawText(self.rect(), Qt.AlignCenter, timer_text)
        
    def start_timer(self):
        if not self.is_running:
            self.remaining_time = 180
            self.is_running = True
            self.timer.start(1000)
            self.show()
            self.update()
            
    def stop_timer(self):
        self.is_running = False
        self.timer.stop()
        self.hide()
        
    def update_timer(self):
        if self.remaining_time > 0:
            self.remaining_time -= 1
            self.update()
        else:
            self.stop_timer()
            
    def reset_timer(self):
        self.remaining_time = 180
        self.update()

class CustomComboBox(QComboBox):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setFixedHeight(32)
        self.setCursor(Qt.PointingHandCursor)
        
        # Set custom style
        self.setStyleSheet("""
            CustomComboBox {
                background-color: #2d2d2d;
                color: #e0e0e0;
                border: 1px solid #555;
                border-radius: 6px;
                padding: 6px 12px;
                font-size: 10pt;
                min-width: 140px;
            }
            CustomComboBox:hover {
                border: 1px solid #777;
                background-color: #333333;
            }
            CustomComboBox:pressed {
                background-color: #2a2a2a;
            }
            CustomComboBox::drop-down {
                subcontrol-origin: padding;
                subcontrol-position: top right;
                width: 25px;
                border-left: 1px solid #555;
                border-top-right-radius: 6px;
                border-bottom-right-radius: 6px;
            }
            CustomComboBox::down-arrow {
                image: none;
                border-left: 5px solid transparent;
                border-right: 5px solid transparent;
                border-top: 5px solid #e0e0e0;
                width: 0px;
                height: 0px;
                margin-right: 8px;
            }
            CustomComboBox QAbstractItemView {
                background-color: #2d2d2d;
                color: #e0e0e0;
                border: 1px solid #555;
                border-radius: 6px;
                padding: 4px;
                outline: none;
                selection-background-color: #E60000;
                selection-color: white;
            }
            CustomComboBox QAbstractItemView::item {
                padding: 6px 12px;
                border-radius: 4px;
                margin: 1px;
            }
            CustomComboBox QAbstractItemView::item:hover {
                background-color: #3d3d3d;
            }
        """)

    def paintEvent(self, event):
        painter = QPainter(self)
        painter.setRenderHint(QPainter.Antialiasing)
        
        # Draw background
        bg_color = QColor(45, 45, 45)
        if self.underMouse():
            bg_color = QColor(51, 51, 51)
        
        painter.setBrush(bg_color)
        painter.setPen(QPen(QColor(85, 85, 85), 1))
        painter.drawRoundedRect(0, 0, self.width(), self.height(), 6, 6)
        
        # Draw text
        painter.setPen(QColor(224, 224, 224))
        painter.setFont(QFont("Segoe UI", 10))
        text_rect = QRect(12, 0, self.width() - 40, self.height())
        painter.drawText(text_rect, Qt.AlignLeft | Qt.AlignVCenter, self.currentText())
        
        # Draw dropdown arrow
        arrow_x = self.width() - 20
        arrow_y = self.height() // 2
        
        painter.setPen(QPen(QColor(224, 224, 224), 2))
        painter.setBrush(QColor(224, 224, 224))
        
        # Draw custom arrow (triangle)
        points = [
            QPoint(arrow_x - 4, arrow_y - 3),
            QPoint(arrow_x + 4, arrow_y - 3),
            QPoint(arrow_x, arrow_y + 3)
        ]
        painter.drawPolygon(points)

# Enhanced keyboard import with fallback for Windows 10 compatibility
try:
    import keyboard
    KEYBOARD_AVAILABLE = True
except ImportError as e:
    KEYBOARD_AVAILABLE = False
    print(f"Keyboard module not available. Global keybinds will not work. Error: {e}")

# Alternative key detection for Windows 10 compatibility
class UniversalKeybindManager(QThread):
    key_pressed = pyqtSignal(str)
    
    def __init__(self):
        super().__init__()
        self.active_keybinds = {}
        self.running = False
        
    def register_keybind(self, key, callback_id):
        normalized_key = key.lower().replace(' ', '')
        self.active_keybinds[normalized_key] = callback_id
        print(f"Registered keybind: {normalized_key} -> {callback_id}")
        
    def unregister_keybind(self, key):
        normalized_key = key.lower().replace(' ', '')
        if normalized_key in self.active_keybinds:
            del self.active_keybinds[normalized_key]
            print(f"Unregistered keybind: {normalized_key}")
            
    def clear_keybinds(self):
        self.active_keybinds.clear()
        print("Cleared all keybinds")
        
    def run(self):
        if not KEYBOARD_AVAILABLE:
            print("Keyboard module not available. Global keybinds disabled.")
            return
            
        self.running = True
        print("Global keybind manager started")
        
        def on_key_event(event):
            if event.event_type == keyboard.KEY_DOWN:
                key_name = event.name.lower()
                
                # Handle special keys
                if key_name == 'space':
                    key_name = 'space'
                elif key_name == 'ctrl':
                    key_name = 'ctrl'
                elif key_name == 'alt':
                    key_name = 'alt'
                elif key_name == 'shift':
                    key_name = 'shift'
                elif key_name == 'enter':
                    key_name = 'enter'
                elif key_name == 'tab':
                    key_name = 'tab'
                elif key_name == 'esc':
                    key_name = 'escape'
                elif len(key_name) > 1:
                    # For other special keys, keep as is but lowercase
                    key_name = key_name
                
                # Check for modifier combinations
                modifiers = []
                if keyboard.is_pressed('ctrl'):
                    modifiers.append('ctrl')
                if keyboard.is_pressed('alt'):
                    modifiers.append('alt')
                if keyboard.is_pressed('shift'):
                    modifiers.append('shift')
                
                # Create combined key name if modifiers are pressed
                if modifiers:
                    combined_key = '+'.join(modifiers + [key_name])
                    if combined_key in self.active_keybinds:
                        print(f"Key pressed: {combined_key} -> {self.active_keybinds[combined_key]}")
                        self.key_pressed.emit(self.active_keybinds[combined_key])
                        return
                
                # Check for regular key
                if key_name in self.active_keybinds:
                    print(f"Key pressed: {key_name} -> {self.active_keybinds[key_name]}")
                    self.key_pressed.emit(self.active_keybinds[key_name])
                    
        try:
            keyboard.hook(on_key_event)
            print("Keyboard hook installed successfully")
            
            # Keep the thread alive
            while self.running:
                QThread.msleep(100)
                
        except Exception as e:
            print(f"Error setting up keyboard hook: {e}")
            # Fallback: try to set up a simpler hook
            self.setup_fallback_hook()
            
    def setup_fallback_hook(self):
        """Fallback method for Windows 10 compatibility"""
        print("Setting up fallback keyboard hook")
        try:
            import threading as fallback_threading
            
            def check_keys():
                while self.running:
                    for key_name in list(self.active_keybinds.keys()):
                        try:
                            if keyboard.is_pressed(key_name):
                                self.key_pressed.emit(self.active_keybinds[key_name])
                                # Small delay to prevent multiple triggers
                                time.sleep(0.2)
                        except:
                            pass
                    time.sleep(0.01)
            
            fallback_thread = fallback_threading.Thread(target=check_keys, daemon=True)
            fallback_thread.start()
            print("Fallback key detection started")
            
        except Exception as e:
            print(f"Fallback hook also failed: {e}")
            
    def stop(self):
        self.running = False
        if KEYBOARD_AVAILABLE:
            try:
                keyboard.unhook_all()
                print("Keyboard unhooked")
            except Exception as e:
                print(f"Error unhooking keyboard: {e}")



# FEAR UI IMPORTS
from ui_widgets import theme, set_theme, bold_font, font, glow_shadow, FearButton, FearCheckbox, FearToggle, FearInput, ToastNotification
from ui_splash import SplashScreen
from ui_login import LoginScreen
from auth import fear_auth
from PyQt5.QtWidgets import (QMainWindow, QStackedWidget, QGraphicsDropShadowEffect, QGridLayout, QScrollArea, QPlainTextEdit)

# ==========================================
# FEAR UI REPLACEMENT
# ==========================================

class KeybindButton(QPushButton):
    key_bound = pyqtSignal(int, str)

    def __init__(self, callback_id=None, keybind_manager=None, parent=None):
        super().__init__("NONE", parent)
        self.setFixedSize(60, 25)
        self.is_binding = False
        self.vk = None
        self.callback_id = callback_id
        self.keybind_manager = keybind_manager
        self.clicked.connect(self.start_binding)

    def start_binding(self):
        if self.vk is not None:
            self.vk = None
            self.setText("NONE")
            self.is_binding = False
            self.key_bound.emit(0, "")
            self.clearFocus()
            if self.keybind_manager and self.callback_id:
                for k, v in list(self.keybind_manager.active_keybinds.items()):
                    if v == self.callback_id:
                        self.keybind_manager.unregister_keybind(k)
            return
            
        self.is_binding = True
        self.setText("[...]")
        self.setFocus()

    def keyPressEvent(self, e):
        if not self.is_binding:
            super().keyPressEvent(e)
            return

        qt_key = e.key()
        vk = e.nativeVirtualKey()
        name = e.text().upper()

        if qt_key == Qt.Key_Shift: name = "SHIFT"
        elif qt_key == Qt.Key_Control: name = "CTRL"
        elif qt_key == Qt.Key_Alt: name = "ALT"
        elif qt_key == Qt.Key_Tab: name = "TAB"
        elif qt_key == Qt.Key_Escape:
            self.vk = None
            self.setText("NONE")
            self.is_binding = False
            self.key_bound.emit(0, "")
            self.clearFocus()
            if self.keybind_manager and self.callback_id:
                for k, v in list(self.keybind_manager.active_keybinds.items()):
                    if v == self.callback_id:
                        self.keybind_manager.unregister_keybind(k)
            return
        elif not name or not name.isprintable():
            key_map = {
                0x20: "SPACE", 0x21: "PGUP", 0x22: "PGDN", 0x23: "END", 0x24: "HOME",
                0x25: "LEFT", 0x26: "UP", 0x27: "RIGHT", 0x28: "DOWN",
                0x2D: "INS", 0x2E: "DEL", 0x14: "CAPS", 0x90: "NUMLCK",
                0xC0: "~", 0xDB: "[", 0xDD: "]", 0xDC: "\\\\", 0xBA: ";", 0xDE: "'",
                0xBC: ",", 0xBE: ".", 0xBF: "/"
            }
            if vk in key_map:
                name = key_map[vk]
            elif 0x70 <= vk <= 0x87:
                name = f"F{vk - 0x70 + 1}"
            elif 0x60 <= vk <= 0x69:
                name = f"NUM {vk - 0x60}"
            else:
                name = f"VK_{vk}"
        
        if vk:
            for btn in self.window().findChildren(KeybindButton):
                if btn != self and btn.vk == vk:
                    btn.vk = None
                    btn.setText("NONE")
                    btn.key_bound.emit(0, "")
                    if btn.keybind_manager and btn.callback_id:
                        for k, v in list(btn.keybind_manager.active_keybinds.items()):
                            if v == btn.callback_id:
                                btn.keybind_manager.unregister_keybind(k)
                    
            self.vk = vk
            self.setText(name)
            self.is_binding = False
            self.key_bound.emit(vk, name)
            self.clearFocus()
            
            if self.keybind_manager and self.callback_id:
                for k, v in list(self.keybind_manager.active_keybinds.items()):
                    if v == self.callback_id:
                        self.keybind_manager.unregister_keybind(k)
                self.keybind_manager.register_keybind(name.lower(), self.callback_id)
        else:
            super().keyPressEvent(e)

    def mousePressEvent(self, e):
        if self.is_binding:
            btn = e.button()
            vk = 0
            name = ""
            if btn == Qt.MiddleButton: vk, name = 0x04, "M3"
            elif btn == Qt.XButton1: vk, name = 0x05, "MB4"
            elif btn == Qt.XButton2: vk, name = 0x06, "MB5"
            if btn in (Qt.LeftButton, Qt.RightButton):
                self.vk = None
                self.setText("NONE")
                self.is_binding = False
                self.key_bound.emit(0, "")
                self.clearFocus()
                if self.keybind_manager and self.callback_id:
                    for k, v in list(self.keybind_manager.active_keybinds.items()):
                        if v == self.callback_id:
                            self.keybind_manager.unregister_keybind(k)
                return
            
            if vk:
                for other in self.window().findChildren(KeybindButton):
                    if other != self and other.vk == vk:
                        other.vk = None
                        other.setText("NONE")
                        other.key_bound.emit(0, "")
                        if other.keybind_manager and other.callback_id:
                            for k, v in list(other.keybind_manager.active_keybinds.items()):
                                if v == other.callback_id:
                                    other.keybind_manager.unregister_keybind(k)
                        
                self.vk = vk
                self.setText(name)
                self.is_binding = False
                self.key_bound.emit(vk, name)
                self.clearFocus()
                if self.keybind_manager and self.callback_id:
                    for k, v in list(self.keybind_manager.active_keybinds.items()):
                        if v == self.callback_id:
                            self.keybind_manager.unregister_keybind(k)
                    self.keybind_manager.register_keybind(name.lower(), self.callback_id)
                return
        super().mousePressEvent(e)


class TitleBar(QFrame):
    def __init__(self, window, parent=None):
        super().__init__(parent)
        self._window = window
        self.setFixedHeight(42)
        self.setObjectName("FeatureCard")
        th = theme()
        self.setStyleSheet(f"QFrame#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        
        layout = QHBoxLayout(self)
        layout.setContentsMargins(16, 0, 8, 0)
        
        self.brand = QLabel('FEAR <span style="color:#E60000;">MODS</span>')
        self.brand.setFont(bold_font(13))
        self.brand.setStyleSheet(f"color: {th['text']}; letter-spacing: 1px;")
        layout.addWidget(self.brand, alignment=Qt.AlignVCenter)
        
        self.sub_brand = QLabel('[ SILENT AIM EXE ]')
        self.sub_brand.setFont(font(10))
        self.sub_brand.setStyleSheet("color: #888888; letter-spacing: 0.5px; margin-top: 1px;")
        layout.addWidget(self.sub_brand, alignment=Qt.AlignVCenter)
        layout.addStretch()

        from ui_widgets import _current_theme
        self.btn_theme = FearButton("🌞" if _current_theme == "dark" else "🌙", variant="icon")
        self.btn_theme.setFixedSize(30, 30)
        self.btn_theme.clicked.connect(self._toggle_theme)
        layout.addWidget(self.btn_theme)

        self.btn_min = FearButton("—", variant="icon")
        self.btn_min.setFixedSize(30, 30)
        self.btn_min.clicked.connect(self._window.showMinimized)
        layout.addWidget(self.btn_min)

        self.btn_close = FearButton("✕", variant="icon")
        self.btn_close.setFixedSize(30, 30)
        self.btn_close.setStyleSheet(self.btn_close.styleSheet() + "QPushButton:hover { color: #FF3333; border-color: #FF3333; }")
        self.btn_close.clicked.connect(self._window.close)
        layout.addWidget(self.btn_close)

    def apply_theme(self):
        from ui_widgets import theme, _current_theme
        th = theme()
        self.setStyleSheet(f"QFrame#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        if hasattr(self, 'brand'):
            self.brand.setStyleSheet(f"color: {th['text']}; letter-spacing: 1px;")
        if hasattr(self, 'btn_theme'):
            self.btn_theme.setText("🌞" if _current_theme == "dark" else "🌙")

    def _toggle_theme(self):
        from ui_widgets import _current_theme, set_theme
        new_theme = "light" if _current_theme == "dark" else "dark"
        set_theme(new_theme)
        if hasattr(self._window, 'apply_theme'):
            self._window.apply_theme()

    def mousePressEvent(self, e):
        if e.button() == Qt.LeftButton:
            self._window._drag_pos = e.globalPos()

    def mouseMoveEvent(self, e):
        if e.buttons() == Qt.LeftButton and hasattr(self._window, '_drag_pos'):
            self._window.move(self._window.pos() + e.globalPos() - self._window._drag_pos)
            self._window._drag_pos = e.globalPos()


class FearListRow(QWidget):
    def apply_theme(self):
        th = theme()
        self.setStyleSheet(f"FearListRow {{ border-bottom: 1px solid {th['border']}; }}")
        if hasattr(self, 'lbl_title'):
            self.lbl_title.setStyleSheet(f"color: {th['text']}; border: none;")
        if hasattr(self, 'lbl_desc'):
            self.lbl_desc.setStyleSheet(f"color: {th['subtext']}; border: none;")
        if hasattr(self, 'btn_key') and self.btn_key:
            self.btn_key.setStyleSheet(f"QPushButton {{ background: transparent; color: {th['subtext']}; border: 1px solid {th['border']}; border-radius: 4px; font-size: 10px; font-weight: bold; }}")

    def __init__(self, title, desc, callback_id, cheat_screen, keybind_manager=None, widget=None):
        super().__init__()
        th = theme()
        self.setStyleSheet(f"FearListRow {{ border-bottom: 1px solid {th['border']}; }}")
        
        layout = QHBoxLayout(self)
        layout.setContentsMargins(8, 5, 12, 5) # Tightened left margin so text starts closer to the left border
        
        left_layout = QVBoxLayout()
        left_layout.setSpacing(0)
        
        self.lbl_title = QLabel(title)
        self.lbl_title.setFont(bold_font(10))
        self.lbl_title.setStyleSheet(f"color: {th['text']}; border: none;")
        left_layout.addWidget(self.lbl_title)
        
        self.lbl_desc = QLabel(desc)
        self.lbl_desc.setFont(font(9))
        self.lbl_desc.setStyleSheet(f"color: {th['subtext']}; border: none;")
        left_layout.addWidget(self.lbl_desc)
        
        layout.addLayout(left_layout)
        layout.addStretch()
        
        if widget:
            layout.addWidget(widget)
            self.btn_key = None
            self.toggle = None
        else:
            if keybind_manager and callback_id:
                self.btn_key = KeybindButton(callback_id, keybind_manager)
                self.btn_key.setStyleSheet(f"QPushButton {{ background: transparent; color: {th['subtext']}; border: 1px solid {th['border']}; border-radius: 4px; font-size: 10px; font-weight: bold; }}")
                layout.addWidget(self.btn_key)
            else:
                self.btn_key = None
                
            layout.addSpacing(15)
            
            cb = FearToggle("")
            cb.setFixedSize(40, 26)
            self.toggle = cb
            
            # Map callback
            def handle_toggle(is_enabled):
                res = cheat_screen.handle_feature_toggle(callback_id, is_enabled)
                if res:
                    cheat_screen.log_signal.emit(f"{title}: {res}", "CONNECTED")
                else:
                    state = "ON" if is_enabled else "OFF"
                    cheat_screen.log_signal.emit(f"{title} -> {state}", "CONNECTED")
                    
            cb.toggled.connect(handle_toggle)
            layout.addWidget(cb)


class CheatScreen(QWidget):
    log_signal = pyqtSignal(str, str) 

    def apply_theme(self):
        th = theme()
        
        for lbl in getattr(self, '_theme_labels', []):
            try:
                lbl.setStyleSheet(f"color: {th['text']}; border: none; background: transparent;")
            except: pass
            
        for sep in getattr(self, '_theme_seps', []):
            try:
                sep.setStyleSheet("color: #737580; border: none; background: transparent;")
            except: pass

        if hasattr(self, 'stats_card'):
            self.stats_card.setStyleSheet(f"QWidget {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        
        if hasattr(self, 'log_card'):
            self.log_card.setStyleSheet(f"QWidget {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")

        for w in self.findChildren(QWidget, "FeatureCard"):
            w.setStyleSheet(f"QWidget#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")

        if hasattr(self, 'stack') and hasattr(self, '_set_tab'):
            self._set_tab(self.stack.currentIndex())
            
        for widget in self.findChildren(QWidget):
            if hasattr(widget, 'apply_theme') and callable(widget.apply_theme) and widget != self:
                try:
                    widget.apply_theme()
                except Exception as e:
                    pass

    def __init__(self, cheat_logic):
        super().__init__()
        self.logic = cheat_logic
        self._theme_labels = []
        self._theme_seps = []
        th = theme()
        main_layout = QVBoxLayout(self)
        main_layout.setContentsMargins(15, 10, 15, 10)
        main_layout.setSpacing(10)
        
        
        # 1. HEADER (Spacing only, Stats card removed)
        main_layout.addSpacing(5)
        
        # 2. MIDDLE AREA (Vertical Sidebar + Stacked Widget)
        middle_layout = QHBoxLayout()
        middle_layout.setSpacing(6)
        middle_layout.setContentsMargins(0, 0, 0, 0)
        
        self.sidebar = QFrame()
        self.sidebar.setObjectName("FeatureCard")
        self.sidebar.setFixedWidth(75) # Widened sidebar card to 75px
        self.sidebar.setStyleSheet(f"QFrame#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        
        sidebar_layout = QVBoxLayout(self.sidebar)
        sidebar_layout.setContentsMargins(13, 15, 13, 15) # Centered 48px buttons
        sidebar_layout.setSpacing(14)
        
        self.tabs_btns = []
        # Native Windows Segoe MDL2 Assets glyph codes:
        # \uF158 = Clean Bold Crosshair Target (Aim)
        # \uE7B3 = Clean Eye outline (ESP)
        # \uE945 = Clean Lightning Bolt (Advanced)
        # \uE713 = Clean Gear Symbol (Settings)
        tab_names = ["\uF158", "\uE7B3", "\uE945", "\uE713"]
        
        for i, icon in enumerate(tab_names):
            btn = QPushButton(icon)
            btn.setFixedSize(48, 48) # Increased button size to 48x48
            btn.setFont(QFont("Segoe MDL2 Assets", 16, QFont.Bold)) 
            btn.setCursor(Qt.PointingHandCursor)
            btn.clicked.connect(lambda _, idx=i: self._set_tab(idx))
            sidebar_layout.addWidget(btn)
            self.tabs_btns.append(btn)
            
        sidebar_layout.addStretch()
        middle_layout.addWidget(self.sidebar)
        
        # 3. CONTENT AREA
        self.stack = QStackedWidget()
        middle_layout.addWidget(self.stack, 1) # Restored stack to stretch normally to the right
        
        main_layout.addLayout(middle_layout, 1)
        
        # Page 0: Aim Features
        slider_style = """
            QSlider::groove:horizontal {
                border: 1px solid #2B2B36;
                height: 6px;
                background: #1E1E26;
                border-radius: 3px;
            }
            QSlider::sub-page:horizontal {
                background: qlineargradient(x1: 0, y1: 0, x2: 1, y2: 0,
                                            stop: 0 #CC0000, stop: 1 #FF3333);
                border-radius: 3px;
            }
            QSlider::add-page:horizontal {
                background: #1E1E26;
                border-radius: 3px;
            }
            QSlider::handle:horizontal {
                background: #FFFFFF;
                border: 2px solid #FF3333;
                width: 14px;
                height: 14px;
                margin-top: -5px;
                margin-bottom: -5px;
                border-radius: 7px;
            }
            QSlider::handle:horizontal:hover {
                background: #FFDDDD;
                border: 2px solid #CC0000;
            }
        """
        fov_slider = QSlider(Qt.Horizontal)
        fov_slider.setRange(10, 700)
        fov_slider.setValue(100)
        fov_slider.setFixedWidth(180)
        fov_slider.setStyleSheet(slider_style)
        fov_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_fov(fov_slider.value())))

        rf_slider = QSlider(Qt.Horizontal)
        rf_slider.setRange(1, 100)
        rf_slider.setValue(10)
        rf_slider.setFixedWidth(180)
        rf_slider.setStyleSheet(slider_style)
        rf_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_rapidfire_speed(rf_slider.value() / 10.0)))

        spin_slider = QSlider(Qt.Horizontal)
        spin_slider.setRange(1, 50)
        spin_slider.setValue(8)
        spin_slider.setFixedWidth(180)
        spin_slider.setStyleSheet(slider_style)
        spin_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_spinspeed(float(spin_slider.value()))))
        aim_smooth_slider = QSlider(Qt.Horizontal)
        aim_smooth_slider.setRange(1, 50)
        aim_smooth_slider.setValue(3)
        aim_smooth_slider.setFixedWidth(180)
        aim_smooth_slider.setStyleSheet(slider_style)
        aim_smooth_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_aimsmooth(float(aim_smooth_slider.value()))))

        bone_combo = CustomComboBox()
        bone_combo.addItems(["HEAD", "NECK", "CHEST", "LSHOULDER", "RSHOULDER"])
        bone_combo.setFixedWidth(140)
        bone_combo.currentTextChanged.connect(lambda part: self.logic.update_status_with_return(set_aim_target_part(part) or f"Bone -> {part}"))
        
        self.stack.addWidget(self._create_list_page([
            ("Aimbot Rage", "Controls the aimbot", "sniper_scope", True),
            ("Rage Aimbot [ Safe ]", "Precision aim assist", "sniper_scope_main", True),
            ("Aim Target", "Select aim position", None, False, bone_combo, "sniper_scope_main"),
            ("Cover Silent", "Track bullet to head", "sniper_switch", True),
            ("Aimbot Visible", "Aim Head Legit", "aimbot", True),
            ("Aim Smoothness", "Adjust aim movement", None, False, aim_smooth_slider, "aimbot"),
            ("Draw Fov", "Draw a Circle for aimbot", "fov_on", False),
            ("Range Fov", "Resize aimbot Fov", None, False, fov_slider, "fov_on"),
            ("Fast Reload", "Forces gun fire fast", "fastreload", True),
            ("No Recoil", "Removes gun recoil", "no_recoil", True),
            ("Glitch Fire", "Removes gun glitch", "glitchfire", True),
            ("Unlimited Ammo", "No need to reload", "unlimitedammo", True),
            ("Rapid Fire", "Shoot extremely fast", "rapidfire", True),
            ("Rapid Fire Speed", "Adjust firing speed", None, False, rf_slider, "rapidfire"),
            ("Ignore Knocked", "Do not target knocked enemies", "ignoreknocked", True)
        ], "Aim Features"))
        
        # Page 1: ESP Features
        self.stack.addWidget(self._create_list_page([
            ("Esp line", "Draw Line on players", "espline", False),
            ("Esp Box", "Show Box around the player", "espbox", False),
            ("Esp Health", "Show Health of the players", "esphealth", False),
            ("Esp Name", "Show Name of players", "espname", False),
            ("Esp Distance", "Show Distance of players", "espdistace", False),
            ("Esp Skeleton", "Show Bone movement of players", "espskeleton", False),
            ("Esp Aim-Track", "Show Nearest enemy aim", "espaimtrack", False),
            ("Esp Mini-Map", "Show Map radius 30", "minimap", False),
            ("Esp Weapon", "Show Enemy holding weapon", "espweapon", False)
        ], "ESP Features"))
        
        # Page 2: Advanced Tools
        self.stack.addWidget(self._create_list_page([
            ("Up Player", "Makes enemy Up", "upplayer", True),
            ("Telekill", "Teleport enemy to charecter", "telekill", True),
            ("Climb Up", "Climb up to a building", "climb", True),
            ("Down Player", "Moves enemy Down", "downplayer", True),
            ("Fly Hack", "Impact you to fly", "flyhack", True),
            ("Fly Hack X40", "Advanced Flyhack Logic", "flyhackx40", True),
            ("Teleport [ All Rounds ]", "Teleport to enemy base", "teleport", True),
            ("Teleport [ Beta ]", "Teleport to enemy base M590", "base", True),
            ("Enemy Pull", "forces enemy to you", "pull", True),
            ("Shake Kill", "Makes the enemy shake", "shake", True),
            ("Under Kill", "forces enemy downwards", "underkill", True),
            ("Spin Bot", "Rotates player 360 rapidly", "spinbot", True),
            ("Spin Speed", "Adjust rotation speed", None, False, spin_slider, "spinbot"),
            ("Wall Hack", "Allows to penetrate wall", "wall", True),
            ("Speed Hack", "Allows to Run fast as fuck", "speed", True),
            ("Enemy Stop", "Block inbound traffic to stop enemies", "enemy_stop", True),
            ("Speed Hack (Int)", "Alternative speed hack logic", "speedint", True)
        ], "Advanced Tools"))

        # Page 3: Settings
        self.stack.addWidget(self._create_list_page([
            ("Connect Server", "Dynamics of ADB", "server", True),
            ("Streamer Mode", "Hides Esp in ScreenShare", "notif", False),
            ("3-Minute Timer", "Shows timer overlay at top right", "timer", False),
            ("Freeze Mode", "Hold & release packets for lag", "freeze", True),
            ("Tele Mode", "Redirect and terminate packets", "tele", True)
        ], "System Settings"))
        

        
        # LOG AREA
        self.log_card = QWidget()
        self.log_card.setFixedHeight(120)
        self.log_card.setStyleSheet(f"QWidget {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        log_layout = QVBoxLayout(self.log_card)
        log_layout.setContentsMargins(15, 10, 15, 10)
        log_layout.setSpacing(5)
        
        log_title = QLabel("📜 SYSTEM LOGS")
        log_title.setFont(bold_font(9))
        log_title.setStyleSheet(f"color: {th['text']}; border: none; background: transparent;")
        self._theme_labels.append(log_title)
        log_layout.addWidget(log_title)
        
        self.console = QPlainTextEdit()
        self.console.setReadOnly(True)
        self.console.setPlainText("FearMods Core Loaded.")
        self.console.setFont(font(9))
        self.console.setStyleSheet("""
            QPlainTextEdit {
                color: #32D769; 
                border: none; 
                background: transparent;
            }
            QScrollBar:vertical {
                border: none;
                background: transparent;
                width: 6px;
                margin: 0px;
                border-radius: 3px;
            }
            QScrollBar::handle:vertical {
                background: #3a3a47;
                min-height: 20px;
                border-radius: 3px;
            }
            QScrollBar::handle:vertical:hover {
                background-color: #E60000;
            }
            QScrollBar::add-line:vertical, QScrollBar::sub-line:vertical {
                border: none;
                background: none;
                height: 0px;
            }
            QScrollBar::up-arrow:vertical, QScrollBar::down-arrow:vertical {
                border: none;
                background: none;
            }
            QScrollBar::add-page:vertical, QScrollBar::sub-page:vertical {
                background: none;
            }
        """)
        log_layout.addWidget(self.console)
        
        self._theme_labels.append(self.console)
        main_layout.addWidget(self.log_card)
        
        # Hotkey Timer removed to prevent conflict with UniversalKeybindManager
        
        self._set_tab(0)
        self.log_signal.connect(self._append_log)
        
    def _create_list_page(self, features, page_title):
        th = theme()
        
        card_widget = QWidget()
        card_widget.setObjectName("FeatureCard")
        card_widget.setStyleSheet(f"QWidget#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        
        card_layout = QVBoxLayout(card_widget)
        card_layout.setContentsMargins(2, 2, 2, 2) # Reduced inner card padding to bring content to the left
        card_layout.setSpacing(0)
        
        # Stationed Pill Category Header Banner at the top of the card
        banner_container = QWidget()
        banner_layout = QVBoxLayout(banner_container)
        banner_layout.setContentsMargins(12, 12, 12, 6) # Proper margins around the capsule
        banner_layout.setSpacing(0)
        
        header_banner = QLabel(page_title.upper())
        header_banner.setFont(bold_font(10))
        header_banner.setAlignment(Qt.AlignCenter)
        header_banner.setFixedHeight(30)
        header_banner.setStyleSheet(f"""
            QLabel {{
                background-color: {th['accent']};
                color: #FFFFFF;
                border-radius: 12px;
                font-weight: bold;
                letter-spacing: 1.5px;
                border: none;
            }}
        """)
        banner_layout.addWidget(header_banner)
        card_layout.addWidget(banner_container)
        
        scroll = QScrollArea()
        scroll.setStyleSheet(f"""
            QScrollArea {{
                background: transparent;
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
                background-color: {th['accent']};
            }}
            QScrollBar::add-line:vertical, QScrollBar::sub-line:vertical {{
                border: none;
                background: none;
                height: 0px;
            }}
            QScrollBar::up-arrow:vertical, QScrollBar::down-arrow:vertical {{
                border: none;
                background: none;
            }}
            QScrollBar::add-page:vertical, QScrollBar::sub-page:vertical {{
                background: none;
            }}
        """)
        scroll.setWidgetResizable(True)
        
        container = QWidget()
        container.setStyleSheet("QWidget { background: transparent; }")
        
        layout = QVBoxLayout(container)
        layout.setContentsMargins(0, 0, 0, 0)
        layout.setSpacing(0)
        
        for item in features:
            parent_id = None
            widget = None
            if len(item) == 6:
                title, desc, mid, has_key, widget, parent_id = item
            elif len(item) == 5:
                title, desc, mid, has_key, widget = item
            else:
                title, desc, mid, has_key = item
                
            row = FearListRow(title, desc, mid, self, self.logic.keybind_manager if has_key else None, widget)
            layout.addWidget(row)
            
            if parent_id:
                if not hasattr(self, 'sub_rows'):
                    self.sub_rows = {}
                if parent_id not in self.sub_rows:
                    self.sub_rows[parent_id] = []
                self.sub_rows[parent_id].append(row)
                row.setVisible(False) # Start hidden
            
        layout.addStretch()
        scroll.setWidget(container)
        
        card_layout.addWidget(scroll)
        return card_widget

    def handle_feature_toggle(self, callback_id, is_checked):
        if hasattr(self.logic, 'feature_states'):
            self.logic.feature_states[callback_id] = is_checked
        
        # Call the corresponding method
        res = None
        if callback_id == "fov_on":
            res = self.logic.aimfov(is_checked)
        elif callback_id == "notif":
            res = self.logic.Streamer(is_checked)
        elif callback_id in self.logic.callback_map:
            res = self.logic.callback_map[callback_id](is_checked)
            
        # Toggle sub-rows visibility dynamically!
        if hasattr(self, 'sub_rows') and callback_id in self.sub_rows:
            for child_row in self.sub_rows[callback_id]:
                child_row.setVisible(is_checked)
                
        return res
        
    def _set_tab(self, index):
        self.stack.setCurrentIndex(index)
        th = theme()
        
        for i, btn in enumerate(self.tabs_btns):
            if i == index:
                btn.setStyleSheet(f"""
                    QPushButton {{
                        background-color: {th['accent']};
                        color: white;
                        border: none;
                        border-radius: 8px;
                    }}
                """)
            else:
                btn.setStyleSheet(f"""
                    QPushButton {{
                        background-color: transparent;
                        color: #555562;
                        border: none;
                        border-radius: 8px;
                    }}
                    QPushButton:hover {{
                        color: #ffffff;
                        background-color: #2b2b35;
                    }}
                """)
                
    def _append_log(self, text, status_text=None):
        import datetime
        try:
            now = datetime.datetime.now().strftime("%H:%M:%S")
            new_log = f"[{now}] {text}"
            self.console.appendPlainText(new_log)
            scrollbar = self.console.verticalScrollBar()
            scrollbar.setValue(scrollbar.maximum())
        except:
            pass

    def _check_hotkeys(self):
        import ctypes
        for row in self.findChildren(FearListRow):
            if hasattr(row, 'btn_key') and row.btn_key and row.btn_key.vk:
                vk = row.btn_key.vk
                if row.btn_key.is_binding:
                    continue
                is_pressed = (ctypes.windll.user32.GetAsyncKeyState(vk) & 0x8000) != 0
                was_pressed = self.prev_key_states.get(vk, False)
                if is_pressed and not was_pressed:
                    if hasattr(row, 'toggle') and row.toggle:
                        row.toggle.setChecked(not row.toggle.isChecked())
                self.prev_key_states[vk] = is_pressed


class FearMods(QMainWindow):
    PAGE_SPLASH  = 0
    PAGE_LOGIN   = 1
    PAGE_CHEAT   = 2

    def __init__(self):
        super().__init__()
        self.setWindowFlags(Qt.FramelessWindowHint)
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setFixedSize(620, 560)
        
        # Initialize Logic components
        self.keybind_manager = UniversalKeybindManager()
        self.keybind_manager.key_pressed.connect(self.handle_global_keypress)
        self.keybind_manager.start()

        self.feature_states = {
            "server": False, "aimbot": False, "sniper_scope": False, "no_recoil": False, "sniper_switch": False,
            "speed": False, "climb": False, "pull": False, "base": False, "shake": False, "wall": False,
            "timer": False, "enemy_stop": False, "freeze": False, "tele": False, "upplayer": False, "telekill": False,
            "downplayer": False, "flyhack": False, "teleport": False, "underkill": False, "fastreload": False,
            "glitchfire": False, "unlimitedammo": False, "rapidfire": False, "speedint": False, "espweapon": False,
            "flyhackx40": False, "notif": False, "fov_on": False, "ignoreknocked": False,
            "espline": False, "espbox": False, "esphealth": False, "espname": False, "espdistace": False,
            "espskeleton": False, "espaimtrack": False, "minimap": False, "spinbot": False,
            "sniper_scope_main": False
        }
        
        self.callback_map = {
            "server": self.server_toggled, "no_recoil": self.no_recoil, "aimbot": self.aimbot_toggled,
            "sniper_scope": self.sniper_scope_toggled, "sniper_switch": self.sniper_switch_toggled,
            "climb": self.climb_up, "pull": self.pullenemy, "base": self.basetele, "shake": self.shakekill,
            "speed": self.speedenabled, "wall": self.wallenabled, "timer": self.timer_toggled,
            "enemy_stop": self.enemy_stop_toggled, "freeze": self.freeze_toggled, "tele": self.tele_toggled,
            "upplayer": self.upplayer, "telekill": self.telekill, "downplayer": self.downplayer,
            "flyhack": self.flyhack, "teleport": self.teleport, "underkill": self.underkil,
            "fastreload": self.fastrealod, "glitchfire": self.glitch, "unlimitedammo": self.unlimitedammo,
            "rapidfire": self.rapidfire, "speedint": self.speedint, "espweapon": self.espweapon,
            "flyhackx40": self.flyhackx40, "espline": self.espline, "espbox": self.espbox, "esphealth": self.esphealth,
            "espname": self.espname_checked, "espdistace": self.espdistace, "espskeleton": self.espskeleton,
            "espaimtrack": self.espaimtrack, "minimap": self.minimap, "wallload": self.wallload, "speedload": self.speedload,
            "spinbot": self.spinbot_toggled,
            "sniper_scope_main": self.aimbot_rage_main_toggled,
            "ignoreknocked": self.ignore_knocked_toggled
        }

        self.W_O = None
        self.W_I = None
        self.W_F = None
        self.R_I = False
        self.T_L = threading.Lock()
        self.timer_overlay = TimerOverlay()
        self.freeze_tele_manager = FreezeTeleManager()
        
        self._build_window()
        self._start_splash()
        
    def apply_theme(self):
        th = theme()
        self._bg.setStyleSheet(f"#BgFrame {{ background-color: {th['bg']}; border-radius: 18px; border: 1.5px solid {th['border']}; }}")
        for widget in self.findChildren(QWidget):
            if hasattr(widget, 'apply_theme') and callable(widget.apply_theme) and widget != self:
                try: widget.apply_theme()
                except Exception as e: pass

    def _build_window(self):
        th = theme()
        central = QWidget(self)
        self.setCentralWidget(central)

        self._bg = QFrame(central)
        self._bg.setObjectName("BgFrame")
        self._bg.setGeometry(10, 10, self.width() - 20, self.height() - 20)
        self._bg.setStyleSheet(f"#BgFrame {{ background-color: {th['bg']}; border-radius: 18px; border: 1.5px solid {th['border']}; }}")
        
        shadow = QGraphicsDropShadowEffect(self._bg)
        shadow.setBlurRadius(20)
        shadow.setColor(QColor(0, 0, 0, 150))
        shadow.setOffset(0, 4)
        self._bg.setGraphicsEffect(shadow)

        root_layout = QVBoxLayout(self._bg)
        root_layout.setContentsMargins(0, 0, 0, 0)
        root_layout.setSpacing(0)

        self._title_bar = TitleBar(self)
        
        # Wrapped TitleBar inside a layout container to give it matching card alignments
        title_container = QWidget()
        title_container_layout = QHBoxLayout(title_container)
        title_container_layout.setContentsMargins(15, 10, 15, 0)
        title_container_layout.addWidget(self._title_bar)
        
        root_layout.addWidget(title_container)

        self._pages = QStackedWidget()
        root_layout.addWidget(self._pages)

        # 0: Splash
        self._splash = SplashScreen()
        self._splash.finished.connect(self._on_splash_done)
        self._pages.addWidget(self._splash)

        # 1: Login
        self._login = LoginScreen(fear_auth)
        self._login.set_app_name("Silent Aim Exe")
        self._login.login_success.connect(self._on_login_success)
        self._pages.addWidget(self._login)

        # 2: Cheat UI
        self._cheat = CheatScreen(self)
        self._pages.addWidget(self._cheat)

    def _start_splash(self):
        def init_auth():
            fear_auth.initialize()
            QTimer.singleShot(0, self._splash.complete_loading)
        threading.Thread(target=init_auth, daemon=True).start()

    def _on_splash_done(self):
        self._pages.setCurrentIndex(self.PAGE_LOGIN)

    def _on_login_success(self):
        self._pages.setCurrentIndex(self.PAGE_CHEAT)

    # =============== MANI272 BACKEND LOGIC ===============
    
    def update_status_with_return(self, return_command):
        if return_command and hasattr(self, '_cheat'):
            self._cheat.log_signal.emit(return_command, "CONNECTED")

    def handle_global_keypress(self, callback_id):
        if callback_id in self.callback_map:
            new_state = not self.feature_states.get(callback_id, False)
            print(f"Global keypress: {callback_id} -> {new_state}")
            
            # Update toggle visually, which will trigger the backend automatically
            toggled_visually = False
            for row in self._cheat.findChildren(FearListRow):
                if row.btn_key and row.btn_key.callback_id == callback_id:
                    if hasattr(row, 'toggle') and row.toggle:
                        row.toggle.setChecked(new_state)
                        toggled_visually = True
            
            # If there's no visual toggle, call backend directly
            if not toggled_visually:
                self.feature_states[callback_id] = new_state
                result = self.callback_map[callback_id](new_state)
                self.update_status_with_return(result)

    def enable_enemy_stop(self):
        with self.T_L:
            if not self.R_I:
                try:
                    winsound.Beep(600, 100)
                    self.W_I = pydivert.WinDivert("inbound")
                    self.W_I.open()
                    self.R_I = True
                    return "Enemy Stop: RAW blocking enabled successfully"
                except Exception as e:
                    return f"Enemy Stop Error: {str(e)}"
        return "Enemy Stop: Already running"

    def disable_enemy_stop(self):
        with self.T_L:
            if self.R_I:
                winsound.Beep(400, 100)
                if self.W_I:
                    self.W_I.close()
                self.R_I = False
                return "Enemy Stop: RAW blocking disabled"
        return "Enemy Stop: Already stopped"

    # Callbacks
    def freeze_toggled(self, is_checked):
        result = self.freeze_tele_manager.toggle_freeze()
        winsound.Beep(1000, 500)
        return f"Freeze: {result}"

    def tele_toggled(self, is_checked):
        result = self.freeze_tele_manager.toggle_tele()
        winsound.Beep(1000, 500)
        return f"Tele: {result}"

    def timer_toggled(self, is_checked):
        if is_checked:
            esptimeron()
            return "Timer: Started | 3:00 remaining"
        else:
            esptimeroff()
            self.timer_overlay.stop_timer()
            return "Timer: Stopped"

    def server_toggled(self, is_checked):
        if is_checked and hasattr(self, '_cheat'):
            streameesp(lambda msg: self._cheat.log_signal.emit(msg, "SUCCESS"))
        return "Server: " + ("Enabled" if is_checked else "Disabled")

    def aimbot_toggled(self, is_checked):
        if is_checked: aimbotvisible()
        else: aimbotvisibleoff()
        return "Aimbot Visible: " + ("Enabled" if is_checked else "Disabled")

    def sniper_scope_toggled(self, is_checked):
        if is_checked: enablefunction()
        else: enablefunctionoff()
        return "Aimbot Rage: " + ("Enabled" if is_checked else "Disabled")

    def sniper_switch_toggled(self, is_checked):
        if is_checked: silentaim()
        else: silentaimoff()
        return "Silent Aim: " + ("Enabled" if is_checked else "Disabled")

    def no_recoil(self, is_checked):
        if is_checked: norecoil()
        else: norecoiloff()
        return "No Recoil: " + ("Enabled" if is_checked else "Disabled")

    def espline(self, is_checked):
        if is_checked: espline()
        else: esplineoff()
        return "Esp Line: " + ("Enabled" if is_checked else "Disabled")
    
    def espdistace(self, is_checked):
        if is_checked: espdistanceon()
        else: espdistanceoff()
        return "Esp Distance: " + ("Enabled" if is_checked else "Disabled")

    def espbox(self, is_checked):
        if is_checked: espbox()
        else: espboxoff()
        return "Esp Box: " + ("Enabled" if is_checked else "Disabled")

    def esphealth(self, is_checked):
        if is_checked: esphealth()
        else: esphealthoff()
        return "Esp Health: " + ("Enabled" if is_checked else "Disabled")

    def espskeleton(self, is_checked):
        if is_checked: espskeleton()
        else: espskeletonoff()
        return "Esp Skeleton: " + ("Enabled" if is_checked else "Disabled")

    def espaimtrack(self, is_checked):
        if is_checked: espaimtrack()
        else: espaimtrackoff()
        return "Esp Aim-Track: " + ("Enabled" if is_checked else "Disabled")

    def upplayer(self, is_checked):
        if is_checked: upplayer()
        else: upplayeroff()
        return "Up-Player: " + ("Enabled" if is_checked else "Disabled")

    def aimfov(self, is_checked):
        if is_checked: drawfov()
        else: drawfovoff()
        return "Draw Fov: " + ("Enabled" if is_checked else "Disabled")

    def telekill(self, is_checked):
        if is_checked: telekill()
        else: telekilloff()
        return "Telekill: " + ("Enabled" if is_checked else "Disabled")

    def climb_up(self, is_checked):
        if is_checked: climbupon()
        else: climbupoff()
        return "ClimbUp: " + ("Enabled" if is_checked else "Disabled")
        
    def teleport(self, is_checked):
        if is_checked: teleanywhere()
        else: teleanywhereoff()
        return "Teleport: " + ("Enabled" if is_checked else "Disabled")
        
    def shakekill(self, is_checked):
        if is_checked: shakekillon()
        else: shakekilloff()
        return "Shakekill: " + ("Enabled" if is_checked else "Disabled")

    def downplayer(self, is_checked):
        if is_checked: downplayeron()
        else: downplayeroff()
        return "DownPlayer: " + ("Enabled" if is_checked else "Disabled")
        
    def pullenemy(self, is_checked):
        if is_checked: pullenemyon()
        else: pullenemyoff()
        return "PullEnemy: " + ("Enabled" if is_checked else "Disabled")

    def flyhack(self, is_checked):
        if is_checked: flyhackon()
        else: flyhackoff()
        return "Fly Hack: " + ("Enabled" if is_checked else "Disabled")

    def minimap(self, is_checked):
        if is_checked: espmini()
        else: espminioff()
        return "Esp map: " + ("Enabled" if is_checked else "Disabled")

    def underkil(self, is_checked):
        if is_checked: underkillon()
        else: underkilloff()
        return "UnderKill: " + ("Enabled" if is_checked else "Disabled")
            
    def fastrealod(self, is_checked):
        if is_checked: fastreloadon()
        else: fastreloadoff()
        return "Fast Reload: " + ("Enabled" if is_checked else "Disabled")

    def wallload(self, is_checked):
        if is_checked: wallload()
        return "Wall hack: Loaded" if is_checked else "Wall hack: Disabled"

    def wallenabled(self, is_checked):
        if is_checked: wallon()
        else: walloff()
        return "Wall hack: " + ("Enabled" if is_checked else "Disabled")

    def speedload(self, is_checked):
        if is_checked: speedload()
        return "Speed hack: Loaded" if is_checked else "Speed hack: Disabled"

    def espname_checked(self, is_checked):
        if is_checked: espname()
        else: espnameoff()
        return "Esp Name: " + ("Enabled" if is_checked else "Disabled")
    
    def basetele(self, is_checked):
        if is_checked: baseon()
        else: baseoff()
        return "Teleport Base: " + ("Enabled" if is_checked else "Disabled")

    def speedenabled(self, is_checked):
        if is_checked: speedon()
        else: speedoff()
        return "Speed hack: " + ("Enabled" if is_checked else "Disabled")
        
    def glitch(self, is_checked):
        if is_checked: glitchfireon()
        else: glitchfireoff()
        return "Glitch Fire: " + ("Enabled" if is_checked else "Disabled")
            
    def unlimitedammo(self, is_checked):
        if is_checked: unlimitedammoon()
        else: unlimitedammooff()
        return "Unlimited Ammo: " + ("Enabled" if is_checked else "Disabled")

    def rapidfire(self, is_checked):
        if is_checked: rapidfireon()
        else: rapidfireoff()
        return "Rapid Fire: " + ("Enabled" if is_checked else "Disabled")
            
    def speedint(self, is_checked):
        if is_checked: speedinton()
        else: speedintoff()
        return "Speed Hack (Int): " + ("Enabled" if is_checked else "Disabled")

    def ignore_knocked_toggled(self, is_checked):
        if is_checked: ignoreknockedon()
        else: ignoreknockedoff()
        return "Ignore Knocked: " + ("Enabled" if is_checked else "Disabled")

    def espweapon(self, is_checked):
        if is_checked: espweaponon()
        else: espweaponoff()
        return "ESP Weapon: " + ("Enabled" if is_checked else "Disabled")

    def flyhackx40(self, is_checked):
        if is_checked: flyhackx40on()
        else: flyhackx40off()
        return "Fly Hack X40: " + ("Enabled" if is_checked else "Disabled")

    def spinbot_toggled(self, is_checked):
        if is_checked: spinboton()
        else: spinbotoff()
        return "Spin Bot: " + ("Enabled" if is_checked else "Disabled")

    def aimbot_rage_main_toggled(self, is_checked):
        if is_checked: aimbotragemainson()
        else: aimbotragemainoff()
        return "Rage Aimbot Main: " + ("Enabled" if is_checked else "Disabled")
    
    def Streamer(self, is_checked):
        if is_checked: streamermode()
        else: streamermodeoff()
        return "Streamer Mode: " + ("Enabled" if is_checked else "Disabled")

    def enemy_stop_toggled(self, is_checked):
        try:
            if is_checked:
                if self.enable_enemy_stop():
                    return "Enemy Stop: Enabled"
                else:
                    return "Enemy Stop: Failed to enable"
            else:
                self.disable_enemy_stop()
                return "Enemy Stop: Disabled"
        except Exception as e:
            return f"Enemy Stop Error: {str(e)}"
            
    def closeEvent(self, event):
        self.disable_enemy_stop()
        F_WD()
        self.freeze_tele_manager.stop()
        if self.keybind_manager:
            self.keybind_manager.stop()
            self.keybind_manager.wait(1000)
        self.timer_overlay.close()
        event.accept()

if __name__ == "__main__":
    app = QApplication(sys.argv)
    main_ui = FearMods()
    main_ui.show()
    sys.exit(app.exec_())
