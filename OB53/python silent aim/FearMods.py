import sys
import os
import ctypes
import traceback

# ======= GLOBAL CRASH LOGGER =======
def _write_crash(exc_type, exc_value, exc_tb):
    try:
        log_path = os.path.join(os.path.dirname(sys.executable if getattr(sys, 'frozen', False) else __file__), 'crash_log.txt')
        with open(log_path, 'w') as f:
            f.write("=== CRASH LOG ===\n")
            traceback.print_exception(exc_type, exc_value, exc_tb, file=f)
    except Exception:
        pass
    # Also show a message box so user sees the error
    try:
        import ctypes as _ct
        msg = ''.join(traceback.format_exception(exc_type, exc_value, exc_tb))
        _ct.windll.user32.MessageBoxW(0, msg[:1000], "SilentAim - Fatal Error", 0x10)
    except Exception:
        pass

sys.excepthook = _write_crash
from PyQt5.QtWidgets import (
    QApplication, QWidget, QTabWidget, QVBoxLayout, QHBoxLayout,
    QLabel, QLineEdit, QFrame, QPushButton, QCheckBox, QScrollArea, QMessageBox, QComboBox, QSlider,
    QStyledItemDelegate, QStyle, QStyleOptionComboBox, QSizePolicy
)
from PyQt5.QtCore import Qt, QSize, QThread, pyqtSignal, QTimer, QRect, QPoint
from PyQt5.QtGui import QFont, QPainter, QColor, QPen, QPixmap, QMouseEvent, QPalette, QPolygon
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

# ======= DELETED PACKET/TIMER LOGIC =======

class CustomComboBox(QComboBox):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setFixedHeight(28)
        self.setCursor(Qt.PointingHandCursor)
        
        # Set custom dark theme style
        self.setStyleSheet("""
            QComboBox {
                background-color: #1e1e24;
                color: #ffffff;
                border: 1px solid #27272a;
                border-radius: 4px;
                padding: 2px 22px 2px 8px;
                font-size: 10px;
                font-weight: bold;
            }
            QComboBox:hover {
                border-color: #ef4444;
            }
            QComboBox::drop-down {
                subcontrol-origin: padding;
                subcontrol-position: center right;
                width: 20px;
                background: transparent;
                border: none;
            }
            QComboBox::down-arrow {
                image: none;
                width: 0px;
                height: 0px;
                border: none;
            }
            QComboBox QAbstractItemView {
                background-color: #18181b;
                color: #ffffff;
                border: 1px solid #27272a;
                border-radius: 6px;
                padding: 4px;
                outline: 0px;
                selection-background-color: #27272a;
                selection-color: #ffffff;
            }
            QComboBox QAbstractItemView::item {
                min-height: 24px;
                padding: 4px 8px;
                border-radius: 4px;
                margin: 1px;
            }
            QComboBox QAbstractItemView::item:hover, QComboBox QAbstractItemView::item:selected {
                background-color: #27272a;
                color: #ffffff;
                outline: 0px;
            }
        """)

    def paintEvent(self, event):
        super().paintEvent(event)
        painter = QPainter(self)
        painter.setRenderHint(QPainter.Antialiasing)
        
        arrow_x = self.width() - 12
        arrow_y = self.height() // 2
        
        painter.setPen(Qt.NoPen)
        painter.setBrush(QColor(255, 255, 255))
        
        points = [
            QPoint(arrow_x - 4, arrow_y - 2),
            QPoint(arrow_x + 4, arrow_y - 2),
            QPoint(arrow_x, arrow_y + 3)
        ]
        painter.drawPolygon(QPolygon(points))

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
    keybind_changed = pyqtSignal()
    
    def __init__(self):
        super().__init__()
        self.active_keybinds = {}
        self.running = False
        
    def register_keybind(self, key, callback_id):
        normalized_key = key.lower().replace(' ', '')
        self.active_keybinds[normalized_key] = callback_id
        print(f"Registered keybind: {normalized_key} -> {callback_id}")
        self.keybind_changed.emit()
        
    def unregister_keybind(self, key):
        normalized_key = key.lower().replace(' ', '')
        if normalized_key in self.active_keybinds:
            del self.active_keybinds[normalized_key]
            print(f"Unregistered keybind: {normalized_key}")
            self.keybind_changed.emit()
            
    def clear_keybinds(self):
        self.active_keybinds.clear()
        print("Cleared all keybinds")
        self.keybind_changed.emit()
        
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
from sys_monitor import SystemMonitorWidget, SystemMonitorButton, ThemeToggleButton, KeybindOverlayButton
from ui_overlay import KeybindOverlayWidget
from PyQt5.QtWidgets import (QMainWindow, QStackedWidget, QGraphicsDropShadowEffect, QGridLayout, QScrollArea, QPlainTextEdit)

# ==========================================
# FEAR UI REPLACEMENT
# ==========================================

class KeybindButton(QPushButton):
    key_bound = pyqtSignal(int, str)

    def __init__(self, callback_id=None, keybind_manager=None, parent=None):
        initial_text = "[+]"
        if keybind_manager and callback_id:
            for k, v in list(keybind_manager.active_keybinds.items()):
                if v == callback_id:
                    initial_text = k.upper()
                    break
        super().__init__(initial_text, parent)
        self.setMinimumWidth(30)
        self.setFixedHeight(21)
        self.is_binding = False
        # Set default vk for insert if registered
        self.vk = 0x2D if initial_text == "INSERT" else None
        self.callback_id = callback_id
        self.keybind_manager = keybind_manager
        self.clicked.connect(self.start_binding)

    def start_binding(self):
        if self.vk is not None:
            self.vk = None
            self.setText("[+]")
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
            self.setText("[+]")
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
                    btn.setText("[+]")
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
                self.setText("[+]")
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
                        other.setText("[+]")
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
    tab_requested = pyqtSignal(int)
    
    def __init__(self, window, parent=None):
        super().__init__(parent)
        self._window = window
        self.setFixedHeight(42)
        self.setObjectName("FeatureCard")
        th = theme()
        self.setStyleSheet(f"QFrame#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        
        layout = QHBoxLayout(self)
        layout.setContentsMargins(16, 0, 8, 0)
        layout.setSpacing(6)
        
        self.brand = QLabel('FEAR <span style="color:#E60000;">MODS</span>')
        self.brand.setFont(bold_font(13))
        self.brand.setStyleSheet(f"color: {th['text']}; letter-spacing: 1px;")
        layout.addWidget(self.brand, alignment=Qt.AlignVCenter)
        
        layout.addStretch()

        # Container for tabs to keep them tightly grouped
        self.tabs_container = QWidget()
        self.tabs_layout = QHBoxLayout(self.tabs_container)
        self.tabs_layout.setContentsMargins(0, 0, 0, 0)
        self.tabs_layout.setSpacing(4)
        
        self.tabs_btns = []
        tab_names = ["\uF158", "\uE7B3", "\uE945", "\uE713", "\uEA8F"]
        for i, icon in enumerate(tab_names):
            if i > 0:
                # Separator between all tabs
                sep = QFrame()
                sep.setFixedSize(1, 16)
                sep.setStyleSheet(f"background-color: {th['border']};")
                self.tabs_layout.addWidget(sep)
                
            if i == 4:
                btn = NotificationTabButton(icon)
            else:
                btn = QPushButton(icon)
            btn.setFixedSize(30, 30)
            btn.setFont(QFont("Segoe MDL2 Assets", 11, QFont.Bold)) 
            btn.setCursor(Qt.PointingHandCursor)
            btn.setStyleSheet(f"""
                QPushButton {{
                    background: {th['card']};
                    color: {th['subtext']};
                    border: 1px solid {th['border']};
                    border-radius: 6px;
                }}
                QPushButton:hover {{
                    background: {th['surface']};
                    color: {th['text']};
                }}
            """)
            btn.clicked.connect(lambda _, idx=i: self.tab_requested.emit(idx))
            self.tabs_layout.addWidget(btn)
            self.tabs_btns.append(btn)
            
        # Separator before window controls
        sep_win = QFrame()
        sep_win.setFixedSize(1, 16)
        sep_win.setStyleSheet(f"background-color: {th['border']};")
        self.tabs_layout.addWidget(sep_win)

        self.btn_theme = ThemeToggleButton(self)
        self.btn_theme.clicked.connect(self._toggle_theme)
        self.tabs_layout.addWidget(self.btn_theme)

        self.btn_close = QPushButton("✕")
        self.btn_close.setFixedSize(30, 30)
        self.btn_close.setFont(bold_font(10))
        self.btn_close.setCursor(Qt.PointingHandCursor)
        self.btn_close.setStyleSheet(f"""
            QPushButton {{
                background: {th['card']};
                color: {th['subtext']};
                border: 1px solid {th['border']};
                border-radius: 6px;
            }}
            QPushButton:hover {{
                background: #E60000;
                color: #FFFFFF;
                border: 1px solid #E60000;
            }}
        """)
        self.btn_close.clicked.connect(self._window.close)
        self.tabs_layout.addWidget(self.btn_close)
        
        layout.addWidget(self.tabs_container)

    def apply_theme(self):
        from ui_widgets import theme
        th = theme()
        self.setStyleSheet(f"QFrame#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        if hasattr(self, 'tabs_container'):
            self.tabs_container.setStyleSheet("background: transparent; border: none;")
        if hasattr(self, 'brand'):
            self.brand.setStyleSheet(f"color: {th['text']}; letter-spacing: 1px;")
        if hasattr(self, 'btn_close'):
            self.btn_close.setStyleSheet(f"""
                QPushButton {{
                    background: {th['card']};
                    color: {th['subtext']};
                    border: 1px solid {th['border']};
                    border-radius: 6px;
                }}
                QPushButton:hover {{
                    background: {th['accent']};
                    color: #FFFFFF;
                    border: 1px solid {th['accent']};
                }}
            """)
        self.set_tab(getattr(self, 'current_tab_idx', 0))

    def set_tab(self, index):
        from ui_widgets import theme
        th = theme()
        self.current_tab_idx = index
        for i, btn in enumerate(self.tabs_btns):
            if i == index:
                btn.setStyleSheet(f"""
                    QPushButton {{
                        background: {th['card']};
                        color: {th['accent']};
                        border: 2px solid {th['accent']};
                        border-radius: 6px;
                    }}
                """)
            else:
                btn.setStyleSheet(f"""
                    QPushButton {{
                        background: {th['card']};
                        color: {th['subtext']};
                        border: 1px solid {th['border']};
                        border-radius: 6px;
                    }}
                    QPushButton:hover {{
                        background: {th['surface']};
                        color: {th['text']};
                    }}
                """)
        if hasattr(self, 'brand'):
            self.brand.setStyleSheet(f"color: {th['text']}; letter-spacing: 1px;")

    def _toggle_theme(self):
        from ui_widgets import _current_theme, set_theme
        new_theme = "light" if _current_theme == "dark" else "dark"
        set_theme(new_theme)
        if hasattr(self._window, 'apply_theme'):
            self._window.apply_theme()

    def _toggle_monitor(self):
        if hasattr(self._window, 'toggle_monitor_page'):
            self._window.toggle_monitor_page()

    def _toggle_overlay(self):
        if hasattr(self._window, 'toggle_overlay_hud'):
            self._window.toggle_overlay_hud()

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
        self.callback_id = callback_id
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
        
        if desc:
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
            
            cb = FearCheckbox("")
            cb.setFixedSize(26, 26)
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


class NotificationTabButton(QPushButton):
    def __init__(self, text, parent=None):
        super().__init__(text, parent)
        self.show_red_dot = False

    def set_notification(self, active):
        self.show_red_dot = active
        self.update()

    def paintEvent(self, event):
        super().paintEvent(event)
        if self.show_red_dot:
            painter = QPainter(self)
            painter.setRenderHint(QPainter.Antialiasing)
            
            # Subtle outer glow halo
            painter.setPen(Qt.NoPen)
            painter.setBrush(QColor(230, 0, 0, 100))
            painter.drawEllipse(self.width() - 11, 3, 9, 9)
            
            # Vibrant Red Dot Badge
            painter.setBrush(QColor("#FF1A1A"))
            painter.drawEllipse(self.width() - 10, 4, 7, 7)


class CheatScreen(QWidget):
    log_signal = pyqtSignal(str, str) 
    update_check_finished = pyqtSignal(dict)
    update_download_finished = pyqtSignal(bool)
    update_progress_log = pyqtSignal(str)

    def apply_theme(self):
        th = theme()
        
        for lbl in getattr(self, '_theme_labels', []):
            try:
                lbl.setStyleSheet(f"color: {th['text']}; border: none; background: transparent;")
            except: pass
            
        for sep in getattr(self, '_theme_seps', []):
            try:
                sep.setStyleSheet(f"background-color: {th['border']}; border: none;")
            except: pass

        if hasattr(self, 'console_icon'):
            self.console_icon.setStyleSheet(f"color: {th['accent']}; border: none; background: transparent;")
        if hasattr(self, 'console_title'):
            self.console_title.setStyleSheet(f"color: {th['text']}; border: none; background: transparent; padding-left: 5px;")

        if hasattr(self, 'stats_card'):
            self.stats_card.setStyleSheet(f"QWidget {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        
        if hasattr(self, 'log_card'):
            self.log_card.setStyleSheet(f"QWidget {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")

        if hasattr(self, 'console'):
            self.console.setStyleSheet(f"""
                QPlainTextEdit {{
                    color: {th['success']}; 
                    border: none; 
                    background: transparent;
                }}
                QScrollBar:vertical {{
                    border: none;
                    background: transparent;
                    width: 4px;
                    margin: 0px;
                    border-radius: 2px;
                }}
                QScrollBar::handle:vertical {{
                    background: {th['border']};
                    min-height: 16px;
                    border-radius: 2px;
                }}
                QScrollBar::handle:vertical:hover {{
                    background: {th['accent']};
                }}
                QScrollBar::add-line:vertical, QScrollBar::sub-line:vertical {{
                    border: none;
                    background: none;
                    height: 0px;
                    width: 0px;
                }}
                QScrollBar::up-arrow:vertical, QScrollBar::down-arrow:vertical {{
                    border: none;
                    background: none;
                    height: 0px;
                    width: 0px;
                }}
                QScrollBar::add-page:vertical, QScrollBar::sub-page:vertical {{
                    background: none;
                }}
            """)

        for w in self.findChildren(QWidget, "FeatureCard"):
            w.setStyleSheet(f"QWidget#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")

        if hasattr(self, 'stack') and hasattr(self, '_set_tab'):
            self._set_tab(self.stack.currentIndex())
            
        for widget in self.findChildren(QWidget):
            try:
                if hasattr(widget, 'apply_theme') and callable(getattr(widget, 'apply_theme', None)) and widget != self:
                    widget.apply_theme()
            except Exception:
                pass

    def __init__(self, cheat_logic):
        super().__init__()
        self.logic = cheat_logic
        self._theme_labels = []
        self._theme_seps = []
        th = theme()
        main_layout = QVBoxLayout(self)
        main_layout.setContentsMargins(8, 8, 8, 8)
        main_layout.setSpacing(8)
        
        # 2. MIDDLE AREA (Vertical Sidebar + Stacked Widget)
        middle_layout = QHBoxLayout()
        middle_layout.setSpacing(8)
        middle_layout.setContentsMargins(0, 0, 0, 0)
        
        # 2. MIDDLE AREA (Vertical Sidebar + Stacked Widget)
        middle_layout = QHBoxLayout()
        middle_layout.setSpacing(8)
        middle_layout.setContentsMargins(0, 0, 0, 0)
        
        # Sidebar removed; Tabs are now in TitleBar
        
        # 3. CONTENT AREA
        self.stack = QStackedWidget()
        middle_layout.addWidget(self.stack, 1)
        
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
        fov_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_fov(fov_slider.value())))

        rf_slider = QSlider(Qt.Horizontal)
        rf_slider.setRange(1, 100)
        rf_slider.setValue(10)
        rf_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_rapidfire_speed(rf_slider.value() / 10.0)))

        spin_slider = QSlider(Qt.Horizontal)
        spin_slider.setRange(1, 50)
        spin_slider.setValue(8)
        spin_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_spinspeed(float(spin_slider.value()))))
        aim_smooth_slider = QSlider(Qt.Horizontal)
        aim_smooth_slider.setRange(1, 50)
        aim_smooth_slider.setValue(3)
        aim_smooth_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_aimsmooth(float(aim_smooth_slider.value()))))

        aim_distance_slider = QSlider(Qt.Horizontal)
        aim_distance_slider.setRange(50, 500)
        aim_distance_slider.setValue(200)
        aim_distance_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_aimdistance(aim_distance_slider.value())))

        drag_slider = QSlider(Qt.Horizontal)
        drag_slider.setRange(1, 20)
        drag_slider.setValue(3)
        drag_slider.sliderReleased.connect(lambda: self.logic.update_status_with_return(update_dragthreshold(float(drag_slider.value()))))

        bone_combo = CustomComboBox()
        bone_combo.addItems(["HEAD", "NECK", "CHEST", "LSHOULDER", "RSHOULDER"])
        bone_combo.setFixedWidth(140)
        bone_combo.currentTextChanged.connect(lambda part: self.logic.update_status_with_return(set_aim_target_part(part) or f"Bone -> {part}"))
        
        # Free Fire Version Selection Combo & Hook Button
        ff_ver_combo = CustomComboBox()
        ff_ver_combo.addItems(list(FF_VERSIONS.keys()))
        ff_ver_combo.setCurrentText("Free Fire 1.132.1")
        ff_ver_combo.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Fixed)

        btn_hook_ff = QPushButton("APPLY HOOK")
        btn_hook_ff.setFont(bold_font(9))
        btn_hook_ff.setFixedSize(125, 30)
        btn_hook_ff.setCursor(Qt.PointingHandCursor)
        btn_hook_ff.setStyleSheet(f"""
            QPushButton {{
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0, stop:0 {th['accent']}, stop:1 {th['accent2']});
                color: #FFFFFF;
                border-radius: 6px;
                border: none;
                font-weight: bold;
                letter-spacing: 0.5px;
                padding: 0px 8px;
            }}
            QPushButton:hover {{
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0, stop:0 {th['accent2']}, stop:1 #FF5555);
            }}
            QPushButton:pressed {{
                background: {th['accent']};
            }}
        """)
        def _on_click_hook():
            selected_ver = ff_ver_combo.currentText()
            init_base = FF_VERSIONS.get(selected_ver, "0xA342EFC")
            
            def log_seq():
                self.log_signal.emit(f"Connecting to ADB Port 5555 for {selected_ver}...", "CONNECTED")
                time.sleep(0.3)
                self.log_signal.emit("Hooking game process (InitBase Sent)...", "CONNECTED")
                send_adb_hook(init_base)
                time.sleep(0.4)
                self.log_signal.emit(f"ADB Hook Active! Modules initialized for {selected_ver}", "SUCCESS")
                
            threading.Thread(target=log_seq, daemon=True).start()
        btn_hook_ff.clicked.connect(_on_click_hook)

        # Page 0: Aim Features (Grid Layout)
        aim_groups = [
            ("Targeting Core", "\uE945", [
                ("Aimbot Rage", "sniper_scope", True, None),
                ("Aim Silent", "sniper_switch", True, None),
                ("Aimbot Visible", "aimbot", True, None),
                ("Draw Fov", "fov_on", False, None),
            ]),
            ("Combat Core", "\uE713", [
                ("Fast Reload", "fastreload", True, None),
                ("No Recoil", "no_recoil", True, None),
                ("Glitch Fire", "glitchfire", True, None),
                ("Unlimited Ammo", "unlimitedammo", True, None),
                ("Rapid Fire", "rapidfire", True, None),
                ("Ignore Knocked", "ignoreknocked", True, None),
            ])
        ]
        self.stack.addWidget(self._create_grid_page(aim_groups))
        
        # Page 1: ESP Features
        esp_groups = [
            ("Visuals Core", "\uE7B3", [
                ("Esp line", "espline", True, None),
                ("Esp Box", "espbox", True, None),
                ("Esp Health", "esphealth", True, None),
                ("Esp Name", "espname", True, None),
                ("Esp Distance", "espdistace", True, None),
                ("Esp Skeleton", "espskeleton", True, None),
                ("Esp Aim-Track", "espaimtrack", True, None),
            ]),
            ("Configuration", "\uE713", [
                ("Rapid Fire Speed", None, False, rf_slider),
                ("Range Fov", None, False, fov_slider),
                ("Aim Smoothness", None, False, aim_smooth_slider),
                ("Aim Distance", None, False, aim_distance_slider),
                ("Spin Speed", None, False, spin_slider),
            ])
        ]
        self.stack.addWidget(self._create_grid_page(esp_groups))
        
        # Page 2: Advanced Tools
        adv_groups = [
            ("Movement Core", "\uE8A5", [
                ("Fly Hack", "flyhack", True, None),
                ("Fly Hack X40", "flyhackx40", True, None),
                ("Fly Hack X80", "flyhackx80", True, None),
                ("Fly Hack Rage", "flyrage", True, None),
                ("Speed Hack", "speed", True, None),
                ("Speed Hack (Int)", "speedint", True, None),
            ]),
            ("Exploits Core", "\uE730", [
                ("Teleport [ Beta ]", "base", True, None),
                ("Enemy Pull", "pull", True, None),
                ("Spin Bot", "spinbot", True, None),
                ("Underground Bypass", "underground", True, None),
            ])
        ]
        self.stack.addWidget(self._create_grid_page(adv_groups))

        # Page 3: System Settings (Grid Layout)
        settings_groups = [
            ("ADB & Connection", "\uE700", [
                ("Connect Server", "server", True, None),
                ("Keybind HUD Overlay", "overlay_hud_toggle", False, None),
                ("Select FF Version", "ff_ver", False, ff_ver_combo),
                ("Apply ADB Hook", "apply_hook", False, btn_hook_ff),
            ]),
            ("System Utilities", "\uE770", [
                ("Streamer Mode", "notif", True, None),
                ("Hide Loader GUI", "hide_gui", True, None),
                ("Clear All Keybinds", "clear_keybinds", False, None),
                ("Temp Clean", "temp_clean", True, None),
                ("PC Optimizer", "pc_opt", True, None),
                ("FPS Boost", "fps_boost", True, None),
            ])
        ]
        self.stack.addWidget(self._create_grid_page(settings_groups))

        # Page 4: Notifications & Updates
        self.stack.addWidget(self._create_notification_page())
        
        # Page 5: System Monitor Dashboard
        self.monitor_page = SystemMonitorWidget(self)
        self.stack.addWidget(self.monitor_page)
        

        
        # LOG AREA
        self.log_card = QWidget()
        self.log_card.setFixedHeight(108)
        self.log_card.setStyleSheet(f"QWidget {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        log_layout = QVBoxLayout(self.log_card)
        log_layout.setContentsMargins(15, 10, 15, 10)
        log_layout.setSpacing(5)
        
        # Updated Premium Log Title with Red Accent Segoe MDL2 Icon & Separator
        log_header_layout = QHBoxLayout()
        log_header_layout.setContentsMargins(0, 0, 0, 5)
        log_header_layout.setSpacing(0)
        
        self.console_icon = QLabel("\uE756")
        self.console_icon.setFont(QFont("Segoe MDL2 Assets", 9))
        self.console_icon.setStyleSheet(f"color: {th['accent']}; border: none; background: transparent;")
        
        self.console_title = QLabel("LIVE CONSOLE")
        self.console_title.setFont(bold_font(9))
        self.console_title.setStyleSheet(f"color: {th['text']}; border: none; background: transparent; padding-left: 5px;")
        
        log_header_layout.addWidget(self.console_icon)
        log_header_layout.addWidget(self.console_title)
        log_header_layout.addStretch()
        
        log_layout.addLayout(log_header_layout)

        # Separator Line under LIVE CONSOLE header
        log_sep = QFrame()
        log_sep.setFrameShape(QFrame.HLine)
        log_sep.setFixedHeight(1)
        log_sep.setStyleSheet(f"background-color: {th['border']}; border: none;")
        self._theme_seps.append(log_sep)
        log_layout.addWidget(log_sep)
        
        self.console = QPlainTextEdit()
        self.console.setReadOnly(True)
        self.console.setPlainText("Waiting for initialization...")
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
                width: 4px;
                margin: 0px;
                border-radius: 2px;
            }
            QScrollBar::handle:vertical {
                background: #3a3a47;
                min-height: 16px;
                border-radius: 2px;
            }
            QScrollBar::handle:vertical:hover {
                background-color: #E60000;
            }
            QScrollBar::add-line:vertical, QScrollBar::sub-line:vertical {
                border: none;
                background: none;
                height: 0px;
                width: 0px;
            }
            QScrollBar::up-arrow:vertical, QScrollBar::down-arrow:vertical {
                border: none;
                background: none;
                height: 0px;
                width: 0px;
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

    def _create_grid_page(self, groups, cols=2):
        from ui_widgets import FeatureGroupContainer, CompactFeatureRow, FearCheckbox
        from PyQt5.QtWidgets import QGridLayout
        
        th = theme()
        card_widget = QWidget()
        card_widget.setObjectName("FeatureCard")
        card_widget.setStyleSheet(f"QWidget#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        
        card_layout = QVBoxLayout(card_widget)
        card_layout.setContentsMargins(15, 15, 15, 15)
        
        scroll = QScrollArea()
        scroll.setStyleSheet("QScrollArea { background: transparent; border: none; }")
        scroll.setWidgetResizable(True)
        scroll.setVerticalScrollBarPolicy(Qt.ScrollBarAlwaysOff)
        scroll.setHorizontalScrollBarPolicy(Qt.ScrollBarAlwaysOff)
        
        container = QWidget()
        container.setStyleSheet("QWidget { background: transparent; }")
        grid_layout = QGridLayout(container)
        grid_layout.setContentsMargins(0, 0, 0, 0)
        grid_layout.setSpacing(15)
        
        row_idx = 0
        col_idx = 0
        
        for group_title, icon, features in groups:
            group_box = FeatureGroupContainer(group_title, icon)
            
            for item in features:
                if len(item) == 4:
                    title, mid, has_key, widget = item
                else:
                    continue
                    
                keybind_widget = None
                if has_key:
                    km = getattr(self.logic, 'keybind_manager', None) or getattr(self, 'keybind_manager', None)
                    keybind_widget = KeybindButton(mid, km)
                
                control = widget
                if not control:
                    control = FearCheckbox("")
                
                if hasattr(control, 'toggled'):
                    def _make_handler(mid_id, title_name):
                        def _handler(is_checked):
                            res = self.handle_feature_toggle(mid_id, is_checked)
                            if res:
                                self.log_signal.emit(f"{title_name}: {res}", "CONNECTED")
                            else:
                                state = "ON" if is_checked else "OFF"
                                self.log_signal.emit(f"{title_name} -> {state}", "CONNECTED")
                        return _handler
                    control.toggled.connect(_make_handler(mid, title))
                    
                row_widget = CompactFeatureRow(title, keybind_widget, control)
                group_box.addWidget(row_widget)
                
            grid_layout.addWidget(group_box, row_idx, col_idx)
            
            col_idx += 1
            if col_idx >= cols:
                col_idx = 0
                row_idx += 1
                
        grid_layout.setRowStretch(row_idx + 1, 1)
        
        scroll.setWidget(container)
        card_layout.addWidget(scroll)
        return card_widget

    def _create_notification_page(self):
        from ui_widgets import FeatureGroupContainer
        th = theme()
        
        card_widget = QWidget()
        card_widget.setObjectName("FeatureCard")
        card_widget.setStyleSheet(f"QWidget#FeatureCard {{ border: 1px solid {th['border']}; background: {th['surface']}; border-radius: 8px; }}")
        
        card_layout = QVBoxLayout(card_widget)
        card_layout.setContentsMargins(15, 15, 15, 15)
        card_layout.setSpacing(12)
        
        group_box = FeatureGroupContainer("MODULE SYNC & UPDATES", "\uE777")
        
        # Row 1: Status Badge
        r1 = QWidget()
        r1.setFixedHeight(28)
        r1_layout = QHBoxLayout(r1)
        r1_layout.setContentsMargins(0, 0, 0, 0)
        self.lbl_s_title = QLabel("Sync Status")
        self.lbl_s_title.setFont(bold_font(8))
        self.lbl_s_title.setStyleSheet(f"color: {th['text']}; background: transparent;")
        self._theme_labels.append(self.lbl_s_title)
        r1_layout.addWidget(self.lbl_s_title)
        r1_layout.addStretch()
        self.lbl_update_status = QLabel("Checking update status...")
        self.lbl_update_status.setFont(bold_font(8))
        self.lbl_update_status.setStyleSheet(f"color: {th['subtext']}; background: {th['surface']}; border: 1px solid {th['border']}; border-radius: 4px; padding: 2px 8px;")
        r1_layout.addWidget(self.lbl_update_status)
        group_box.addWidget(r1)
        
        # Row 2: Local Build Signature
        r2 = QWidget()
        r2.setFixedHeight(28)
        r2_layout = QHBoxLayout(r2)
        r2_layout.setContentsMargins(0, 0, 0, 0)
        self.lbl_l_title = QLabel("Local Build Signature")
        self.lbl_l_title.setFont(bold_font(8))
        self.lbl_l_title.setStyleSheet(f"color: {th['text']}; background: transparent;")
        self._theme_labels.append(self.lbl_l_title)
        r2_layout.addWidget(self.lbl_l_title)
        r2_layout.addStretch()
        self.lbl_local_version = QLabel("Loading...")
        self.lbl_local_version.setFont(bold_font(8))
        self.lbl_local_version.setStyleSheet(f"color: {th['subtext']}; background: transparent;")
        r2_layout.addWidget(self.lbl_local_version)
        group_box.addWidget(r2)
        
        # Row 3: Server Build Signature
        r3 = QWidget()
        r3.setFixedHeight(28)
        r3_layout = QHBoxLayout(r3)
        r3_layout.setContentsMargins(0, 0, 0, 0)
        self.lbl_r_title = QLabel("Server Build Signature")
        self.lbl_r_title.setFont(bold_font(8))
        self.lbl_r_title.setStyleSheet(f"color: {th['text']}; background: transparent;")
        self._theme_labels.append(self.lbl_r_title)
        r3_layout.addWidget(self.lbl_r_title)
        r3_layout.addStretch()
        self.lbl_remote_version = QLabel("Loading...")
        self.lbl_remote_version.setFont(bold_font(8))
        self.lbl_remote_version.setStyleSheet(f"color: {th['subtext']}; background: transparent;")
        r3_layout.addWidget(self.lbl_remote_version)
        group_box.addWidget(r3)
        
        # Console Log Message
        self.update_console = QLabel("Select 'Update Modules' to check and force sync updates.")
        self.update_console.setFont(font(9))
        self.update_console.setStyleSheet(f"color: {th['success']}; background: {th['surface']}; border: 1px solid {th['border']}; border-radius: 6px; padding: 8px;")
        self.update_console.setWordWrap(True)
        self.update_console.setAlignment(Qt.AlignCenter)
        group_box.addWidget(self.update_console)
        
        # Action Button
        self.btn_update_action = QPushButton("UPDATE MODULES")
        self.btn_update_action.setFont(bold_font(9))
        self.btn_update_action.setFixedHeight(30)
        self.btn_update_action.setCursor(Qt.PointingHandCursor)
        self.btn_update_action.setStyleSheet(f"""
            QPushButton {{
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0, stop:0 {th['accent']}, stop:1 {th['accent2']});
                color: #FFFFFF;
                border-radius: 6px;
                border: none;
                font-weight: bold;
                letter-spacing: 0.5px;
            }}
            QPushButton:hover {{
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0, stop:0 {th['accent2']}, stop:1 #FF5555);
            }}
        """)
        self.btn_update_action.clicked.connect(self._run_update)
        group_box.addWidget(self.btn_update_action)
        
        card_layout.addWidget(group_box)
        card_layout.addStretch()
        
        # Connect thread-safe signals for updates
        self.update_check_finished.connect(self._on_check_finished)
        self.update_download_finished.connect(self._on_update_finished)
        self.update_progress_log.connect(self.update_console.setText)

        QTimer.singleShot(1000, self._check_for_updates_async)
        
        # Periodic check timer: check every 5 minutes (300,000 milliseconds)
        self.update_check_timer = QTimer(self)
        self.update_check_timer.timeout.connect(self._check_for_updates_async)
        self.update_check_timer.start(300000)
        
        return card_widget

    def _check_for_updates_async(self):
        def work():
            res = check_update_status()
            self.update_check_finished.emit(res)
        threading.Thread(target=work, daemon=True).start()

    def _on_check_finished(self, res):
        th = theme()
        update_available = res.get("update_available", False)
        
        # Target the title bar tabs_btns from main window
        main_win = getattr(self, 'logic', None) or self.window()
        title_bar = getattr(main_win, '_title_bar', None)
        if title_bar and hasattr(title_bar, 'tabs_btns') and len(title_bar.tabs_btns) > 4:
            title_bar.tabs_btns[4].set_notification(update_available)

        if res.get("error"):
            self.lbl_update_status.setText("OFFLINE")
            self.lbl_update_status.setStyleSheet("color: #FF5555; background: #261515; border: 1px solid #552727; border-radius: 4px; padding: 2px 8px;")
            self.lbl_local_version.setText(res.get('local_etag', 'N/A')[:16])
            self.lbl_remote_version.setText("Connection Failed")
            return

        local_etag = res.get("local_etag", "")
        remote_etag = res.get("remote_etag", "")
        
        local_display = local_etag[:16] + "..." if len(local_etag) > 16 else local_etag
        remote_display = remote_etag[:16] + "..." if len(remote_etag) > 16 else remote_etag
        
        self.lbl_local_version.setText(local_display)
        self.lbl_remote_version.setText(remote_display)

        if update_available:
            self.lbl_update_status.setText("⚠️ UPDATE AVAILABLE")
            self.lbl_update_status.setStyleSheet("color: #FF5555; background: #261515; border: 1px solid #552727; border-radius: 4px; padding: 2px 8px;")
            self.btn_update_action.setEnabled(True)
            self.update_console.setText("A newer update is available. Click 'Update Modules' to download.")
        else:
            self.lbl_update_status.setText("✅ UP TO DATE")
            self.lbl_update_status.setStyleSheet("color: #32D769; background: #15261b; border: 1px solid #275536; border-radius: 4px; padding: 2px 8px;")
            self.update_console.setText("You are running the latest compiled version.")

    def _run_update(self):
        self.btn_update_action.setEnabled(False)
        self.lbl_update_status.setText("Updating modules...")
        self.update_progress_log.emit("Starting download thread...")
        
        def work():
            def log(msg):
                self.update_progress_log.emit(msg)
            success = download_updates(log)
            self.update_download_finished.emit(success)
        threading.Thread(target=work, daemon=True).start()

    def _on_update_finished(self, success):
        self.btn_update_action.setEnabled(True)
        if success:
            self.update_console.setText("All modules updated successfully! Connect Server will now load the updated files.")
            main_win = getattr(self, 'logic', None) or self.window()
            title_bar = getattr(main_win, '_title_bar', None)
            if title_bar and hasattr(title_bar, 'tabs_btns') and len(title_bar.tabs_btns) > 4:
                title_bar.tabs_btns[4].set_notification(False)
            self._check_for_updates_async()
        else:
            self.update_console.setText("Failed to download updates. Please check your network connection.")
            self.lbl_update_status.setText("Update Failed")

    def handle_feature_toggle(self, callback_id, is_checked):
        if hasattr(self.logic, 'feature_states'):
            self.logic.feature_states[callback_id] = is_checked
        
        # Call the corresponding method
        res = None
        if callback_id == "fov_on":
            res = self.logic.aimfov(is_checked)
        elif callback_id == "notif":
            res = self.logic.Streamer(is_checked)
        elif callback_id == "overlay_hud_toggle":
            res = self.logic.overlay_hud_toggled(is_checked)
        elif callback_id in self.logic.callback_map:
            res = self.logic.callback_map[callback_id](is_checked)
            
        # Refresh Keybind HUD Overlay live status (ON/OFF badges)
        if hasattr(self.logic, 'overlay_hud') and self.logic.overlay_hud:
            self.logic.overlay_hud.refresh_keybinds()

        # Toggle sub-rows visibility dynamically!
        if hasattr(self, 'sub_rows') and callback_id in self.sub_rows:
            for child_row in self.sub_rows[callback_id]:
                child_row.setVisible(is_checked)
                
        return res
        
    def _set_tab(self, index):
        self.stack.setCurrentIndex(index)
                
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
        for btn in self.findChildren(KeybindButton):
            if btn and btn.vk:
                if getattr(btn, 'is_binding', False):
                    continue
                is_pressed = (ctypes.windll.user32.GetAsyncKeyState(btn.vk) & 0x8000) != 0
                was_pressed = self.prev_key_states.get(btn.vk, False)
                if is_pressed and not was_pressed:
                    p = btn.parent()
                    while p and not getattr(p, 'toggle', None) and not getattr(p, 'control_widget', None):
                        p = p.parent()
                    if p:
                        cb = getattr(p, 'toggle', None) or getattr(p, 'control_widget', None)
                        if cb and hasattr(cb, 'setChecked'):
                            cb.setChecked(not cb.isChecked())
                self.prev_key_states[btn.vk] = is_pressed


class FearMods(QMainWindow):
    PAGE_SPLASH  = 0
    PAGE_LOGIN   = 1
    PAGE_CHEAT   = 2

    def __init__(self):
        super().__init__()
        self.setWindowFlags(Qt.FramelessWindowHint)
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setFixedSize(620, 500)
        
        # Initialize Logic components
        self.keybind_manager = UniversalKeybindManager()
        self.keybind_manager.key_pressed.connect(self.handle_global_keypress)
        self.keybind_manager.register_keybind("insert", "hide_gui")
        self.keybind_manager.start()

        # Standalone Keybind HUD Overlay
        self.overlay_hud = KeybindOverlayWidget(self.keybind_manager)
        self.keybind_manager.keybind_changed.connect(self.overlay_hud.refresh_keybinds)

        self.feature_states = {
            "server": False, "aimbot": False, "sniper_scope": False, "no_recoil": False, "sniper_switch": False, "silentaimbody_switch": False,
            "speed": False, "pull": False, "base": False,
            "flyhack": False, "fastreload": False,
            "glitchfire": False, "unlimitedammo": False, "rapidfire": False, "speedint": False, "espweapon": False,
            "flyhackx40": False, "flyhackx80": False, "flyrage": False, "notif": False, "fov_on": False, "ignoreknocked": False,
            "espline": False, "espbox": False, "esphealth": False, "espname": False, "espdistace": False,
            "espskeleton": False, "espaimtrack": False, "minimap": False, "spinbot": False,
            "sniper_scope_main": False, "underground": False,
            "temp_clean": False, "pc_opt": False, "fps_boost": False,
            "hide_gui": False, "overlay_hud_toggle": False, "clear_keybinds": False
        }
        
        self.callback_map = {
            "server": self.server_toggled, "no_recoil": self.no_recoil, "aimbot": self.aimbot_toggled,
            "sniper_scope": self.sniper_scope_toggled, "sniper_switch": self.sniper_switch_toggled, "silentaimbody_switch": self.silentaimbody_toggled,
            "pull": self.pullenemy, "base": self.basetele,
            "speed": self.speedenabled,
            "flyhack": self.flyhack,
            "fastreload": self.fastrealod, "glitchfire": self.glitch, "unlimitedammo": self.unlimitedammo,
            "rapidfire": self.rapidfire, "speedint": self.speedint, "espweapon": self.espweapon,
            "flyhackx40": self.flyhackx40, "flyhackx80": self.flyhackx80_toggled, "flyrage": self.flyrage_toggled,
            "espline": self.espline, "espbox": self.espbox, "esphealth": self.esphealth,
            "espname": self.espname_checked, "espdistace": self.espdistace, "espskeleton": self.espskeleton,
            "espaimtrack": self.espaimtrack, "minimap": self.minimap,
            "spinbot": self.spinbot_toggled,
            "sniper_scope_main": self.aimbot_rage_main_toggled,
            "ignoreknocked": self.ignore_knocked_toggled,
            "underground": self.underground_toggled,
            "temp_clean": self.temp_clean_toggled,
            "pc_opt": self.pc_opt_toggled,
            "fps_boost": self.fps_boost_toggled,
            "hide_gui": self.hide_gui_toggled,
            "overlay_hud_toggle": self.overlay_hud_toggled,
            "clear_keybinds": self.clear_keybinds_toggled
        }

        self.W_O = None
        self.W_I = None
        self.W_F = None
        self.R_I = False
        self.T_L = threading.Lock()
        
        self._build_window()
        self._start_splash()
        
    def toggle_monitor_page(self):
        if self._pages.currentIndex() == self.PAGE_CHEAT:
            if self._cheat.stack.currentIndex() == 5:
                self._cheat._set_tab(0)
            else:
                self._cheat._set_tab(5)

    def toggle_overlay_hud(self):
        if hasattr(self, 'overlay_hud') and self.overlay_hud:
            if self.overlay_hud.isVisible():
                self.overlay_hud.hide()
                if hasattr(self._title_bar, 'btn_overlay'):
                    self._title_bar.btn_overlay.set_active(False)
            else:
                self.overlay_hud.refresh_keybinds()
                self.overlay_hud.show()
                if hasattr(self._title_bar, 'btn_overlay'):
                    self._title_bar.btn_overlay.set_active(True)

    def apply_theme(self):
        th = theme()
        self._bg.setStyleSheet(f"#BgFrame {{ background-color: {th['bg']}; border-radius: 18px; border: 1.5px solid {th['border']}; }}")
        if hasattr(self, 'overlay_hud') and self.overlay_hud:
            self.overlay_hud.apply_theme()
        for widget in self.findChildren(QWidget):
            try:
                if hasattr(widget, 'apply_theme') and callable(getattr(widget, 'apply_theme', None)) and widget != self:
                    widget.apply_theme()
            except Exception:
                pass

    def _on_tab_requested(self, index):
        if hasattr(self, '_cheat'):
            self._cheat._set_tab(index)
            self._title_bar.set_tab(index)

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
        self._title_bar.tab_requested.connect(self._on_tab_requested)
        
        # Wrapped TitleBar inside a layout container to give it matching card alignments
        title_container = QWidget()
        title_container_layout = QHBoxLayout(title_container)
        title_container_layout.setContentsMargins(8, 8, 8, 0)
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
        # Auto fallback safety: if auth initialization hangs, force advance after 3 seconds
        safety_timer = QTimer(self)
        safety_timer.setSingleShot(True)
        safety_timer.timeout.connect(self._splash.complete_loading)
        safety_timer.start(3000)

        def init_auth():
            try:
                fear_auth.initialize()
                # If successful within 3 seconds, trigger immediately
                QTimer.singleShot(0, lambda: [safety_timer.stop(), self._splash.complete_loading()])
            except Exception as e:
                print(f"Auth Init Exception: {e}")
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
            for btn in self._cheat.findChildren(KeybindButton):
                if getattr(btn, 'callback_id', None) == callback_id:
                    p = btn.parent()
                    while p and not getattr(p, 'toggle', None) and not getattr(p, 'control_widget', None):
                        p = p.parent()
                    if p:
                        cb = getattr(p, 'toggle', None) or getattr(p, 'control_widget', None)
                        if cb and hasattr(cb, 'setChecked'):
                            cb.setChecked(new_state)
                            toggled_visually = True
            
            # If there's no visual toggle, call backend directly
            if not toggled_visually:
                self.feature_states[callback_id] = new_state
                result = self.callback_map[callback_id](new_state)
                self.update_status_with_return(result)

    def reset_toggle_visual(self, callback_id):
        from ui_widgets import CompactFeatureRow
        for btn in self.findChildren(KeybindButton):
            if getattr(btn, 'callback_id', None) == callback_id:
                p = btn.parent()
                while p and not getattr(p, 'toggle', None) and not getattr(p, 'control_widget', None):
                    p = p.parent()
                if p:
                    cb = getattr(p, 'toggle', None) or getattr(p, 'control_widget', None)
                    if cb and hasattr(cb, 'isChecked') and cb.isChecked():
                        cb.blockSignals(True)
                        cb.setChecked(False)
                        cb.blockSignals(False)
                        if callback_id in self.feature_states:
                            self.feature_states[callback_id] = False

        for row in self.findChildren(CompactFeatureRow):
            lbl = getattr(row, 'title_label', None)
            cb = getattr(row, 'control_widget', None) or getattr(row, 'toggle', None)
            if lbl and cb and hasattr(cb, 'isChecked') and cb.isChecked():
                if (callback_id == "clear_keybinds" and "Clear All Keybinds" in lbl.text()) or (callback_id == "temp_clean" and "Temp Clean" in lbl.text()) or (callback_id == "pc_opt" and "PC Optimizer" in lbl.text()) or (callback_id == "fps_boost" and "FPS Boost" in lbl.text()):
                    cb.blockSignals(True)
                    cb.setChecked(False)
                    cb.blockSignals(False)
                    if callback_id in self.feature_states:
                        self.feature_states[callback_id] = False

    # Callbacks

    def server_toggled(self, is_checked):
        if is_checked and hasattr(self, '_cheat'):
            streameesp(lambda msg: self._cheat.log_signal.emit(msg, "SUCCESS"))
        return "Server: " + ("Enabled" if is_checked else "Disabled")

    def overlay_hud_toggled(self, is_checked):
        if hasattr(self, 'overlay_hud') and self.overlay_hud:
            if is_checked:
                self.overlay_hud.refresh_keybinds()
                self.overlay_hud.show()
            else:
                self.overlay_hud.hide()
        return "Keybind HUD Overlay: " + ("Enabled" if is_checked else "Disabled")

    def clear_keybinds_toggled(self, is_checked):
        if is_checked:
            # 1. Unregister all user keybinds except default insert
            if hasattr(self, 'keybind_manager') and self.keybind_manager:
                active_keys = list(self.keybind_manager.active_keybinds.keys())
                for k in active_keys:
                    if k.lower() != "insert" and self.keybind_manager.active_keybinds.get(k) != "hide_gui":
                        self.keybind_manager.unregister_keybind(k)
                        
            # 2. Reset all KeybindButtons visual UI state back to [+]
            for btn in self.findChildren(KeybindButton):
                if getattr(btn, 'callback_id', None) != "hide_gui":
                    btn.vk = None
                    btn.setText("[+]")
                    btn.is_binding = False

            # 3. Refresh Overlay HUD UI if visible
            if hasattr(self, 'overlay_hud') and self.overlay_hud:
                self.overlay_hud.refresh_keybinds()

            # 4. Auto-turn OFF checkbox visually after reset action
            QTimer.singleShot(400, lambda: self.reset_toggle_visual("clear_keybinds"))
            return "All Keybinds Cleared Successfully!"
        return "Clear Keybinds: Ready"

    def temp_clean_toggled(self, is_checked):
        if is_checked:
            def run():
                from optimizer import clean_temp_files
                def log(msg):
                    if hasattr(self, '_cheat'):
                        self._cheat.log_signal.emit(msg, "SUCCESS")
                res = clean_temp_files(log)
                self.update_status_with_return(res)
                QTimer.singleShot(1000, lambda: self.reset_toggle_visual("temp_clean"))
            threading.Thread(target=run, daemon=True).start()
            return "Temp Clean: Initializing..."
        return "Temp Clean: Standby"

    def pc_opt_toggled(self, is_checked):
        if is_checked:
            def run():
                from optimizer import pc_optimize
                def log(msg):
                    if hasattr(self, '_cheat'):
                        self._cheat.log_signal.emit(msg, "SUCCESS")
                res = pc_optimize(log)
                self.update_status_with_return(res)
                QTimer.singleShot(1000, lambda: self.reset_toggle_visual("pc_opt"))
            threading.Thread(target=run, daemon=True).start()
            return "PC Optimizer: Initializing..."
        return "PC Optimizer: Standby"

    def fps_boost_toggled(self, is_checked):
        if is_checked:
            def run():
                from optimizer import fps_boost
                def log(msg):
                    if hasattr(self, '_cheat'):
                        self._cheat.log_signal.emit(msg, "SUCCESS")
                res = fps_boost(log)
                self.update_status_with_return(res)
                QTimer.singleShot(1000, lambda: self.reset_toggle_visual("fps_boost"))
            threading.Thread(target=run, daemon=True).start()
            return "FPS Boost: Initializing..."
        return "FPS Boost: Standby"

    def aimbot_toggled(self, is_checked):
        if is_checked: aimbotvisible()
        else: aimbotvisibleoff()
        return "Aimbot Visible: " + ("Enabled" if is_checked else "Disabled")

    def aimbot_drag_toggled(self, is_checked):
        if is_checked: aimbotdragon()
        else: aimbotdragoff()
        return "Aimbot Drag: " + ("Enabled" if is_checked else "Disabled")

    def sniper_scope_toggled(self, is_checked):
        if is_checked: enablefunction()
        else: enablefunctionoff()
        return "Aimbot Rage: " + ("Enabled" if is_checked else "Disabled")

    def uncheck_visually(self, callback_id):
        self.feature_states[callback_id] = False
        if hasattr(self, '_cheat') and self._cheat:
            for row in self._cheat.findChildren(FearListRow):
                if row.btn_key and row.btn_key.callback_id == callback_id:
                    if hasattr(row, 'toggle') and row.toggle:
                        if row.toggle.isChecked():
                            row.toggle.setChecked(False)

    def sniper_switch_toggled(self, is_checked):
        if is_checked:
            silentaim()
            if self.feature_states.get("silentaimbody_switch"):
                self.uncheck_visually("silentaimbody_switch")
        else:
            silentaimoff()
        return "Silent Aim [ Head ]: " + ("Enabled" if is_checked else "Disabled")

    def silentaimbody_toggled(self, is_checked):
        if is_checked:
            silentaimbody()
            if self.feature_states.get("sniper_switch"):
                self.uncheck_visually("sniper_switch")
        else:
            silentaimbodyoff()
        return "Silent Aim [ Body ]: " + ("Enabled" if is_checked else "Disabled")

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

    def aimfov(self, is_checked):
        if is_checked: drawfov()
        else: drawfovoff()
        return "Draw Fov: " + ("Enabled" if is_checked else "Disabled")

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
            
    def fastrealod(self, is_checked):
        if is_checked: fastreloadon()
        else: fastreloadoff()
        return "Fast Reload: " + ("Enabled" if is_checked else "Disabled")



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

    def flyhackx80_toggled(self, is_checked):
        if is_checked: flyhackx80on()
        else: flyhackx80off()
        return "Fly Hack X80: " + ("Enabled" if is_checked else "Disabled")

    def flyrage_toggled(self, is_checked):
        if is_checked: flyrageon()
        else: flyrageoff()
        return "Fly Hack Rage: " + ("Enabled" if is_checked else "Disabled")

    def spinbot_toggled(self, is_checked):
        if is_checked: spinboton()
        else: spinbotoff()
        return "Spin Bot: " + ("Enabled" if is_checked else "Disabled")

    def aimbot_rage_main_toggled(self, is_checked):
        if is_checked: aimbotragemainson()
        else: aimbotragemainoff()
        return "Rage Aimbot Main: " + ("Enabled" if is_checked else "Disabled")
    
    def underground_toggled(self, is_checked):
        if is_checked: undergroundon()
        else: undergroundoff()
        return "Underground Bypass: " + ("Enabled" if is_checked else "Disabled")
    

    def Streamer(self, is_checked):
        import ctypes
        if is_checked:
            streamermode()
            try:
                hwnd = int(self.winId())
                top_hwnd = ctypes.windll.user32.GetAncestor(hwnd, 2)  # GA_ROOT = 2
                ctypes.windll.user32.SetWindowDisplayAffinity(top_hwnd, 0x00000011)
            except Exception:
                pass
        else:
            streamermodeoff()
            try:
                hwnd = int(self.winId())
                top_hwnd = ctypes.windll.user32.GetAncestor(hwnd, 2)  # GA_ROOT = 2
                ctypes.windll.user32.SetWindowDisplayAffinity(top_hwnd, 0x00000000)
            except Exception:
                pass
        return "Streamer Mode: " + ("Enabled" if is_checked else "Disabled")

    def hide_gui_toggled(self, is_checked):
        if is_checked:
            self.hide()
        else:
            self.show()
            self.raise_()
            self.activateWindow()
        return "Hide GUI: " + ("Enabled" if is_checked else "Disabled")


    def closeEvent(self, event):
        F_WD()
        if self.keybind_manager:
            self.keybind_manager.stop()
            self.keybind_manager.wait(1000)
        event.accept()

def check_hyperv_enabled() -> bool:
    import subprocess
    # Layer 1: Check active services (very fast)
    for svc in ["vmcompute", "vmms"]:
        try:
            startupinfo = subprocess.STARTUPINFO()
            startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            startupinfo.wShowWindow = subprocess.SW_HIDE
            res = subprocess.run(["sc", "query", svc], capture_output=True, text=True, startupinfo=startupinfo, timeout=2)
            if "RUNNING" in res.stdout:
                return True
        except Exception:
            pass
            
    # Layer 2: Check Windows Optional Features via DISM (for checked checkboxes in optional features)
    for feature in ["HypervisorPlatform", "Microsoft-Hyper-V-All", "Microsoft-Hyper-V", "VirtualMachinePlatform"]:
        try:
            startupinfo = subprocess.STARTUPINFO()
            startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            startupinfo.wShowWindow = subprocess.SW_HIDE
            res = subprocess.run(["dism", "/online", "/get-featureinfo", f"/featurename:{feature}"], capture_output=True, text=True, startupinfo=startupinfo, timeout=4)
            if "State : Enabled" in res.stdout or "State: Enabled" in res.stdout:
                return True
        except Exception:
            pass
            
    # Layer 3: Check boot settings (hypervisor active in boot loader)
    try:
        startupinfo = subprocess.STARTUPINFO()
        startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        startupinfo.wShowWindow = subprocess.SW_HIDE
        res = subprocess.run(["bcdedit"], capture_output=True, text=True, startupinfo=startupinfo, timeout=2)
        if "hypervisorlaunchtype" in res.stdout and "Off" not in res.stdout:
            return True
    except Exception:
        pass
        
    return False

def disable_hyperv():
    import subprocess
    try:
        startupinfo = subprocess.STARTUPINFO()
        startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        startupinfo.wShowWindow = subprocess.SW_HIDE
        subprocess.run(["bcdedit", "/set", "hypervisorlaunchtype", "off"], startupinfo=startupinfo, timeout=5)
        for feature in ["Microsoft-Hyper-V-All", "HypervisorPlatform", "VirtualMachinePlatform"]:
            subprocess.run(["dism", "/online", "/disable-feature", f"/featurename:{feature}", "/norestart"], startupinfo=startupinfo, timeout=15)
        return True
    except Exception:
        return False

class HyperVWarningDialog(QMessageBox):
    def __init__(self, parent=None):
        super().__init__(parent)
        th = theme()
        self.setWindowTitle("Security Alert - Hyper-V Detected")
        self.setIcon(QMessageBox.Warning)
        self.setText("⚠️ HYPER-V DETECTED!")
        self.setInformativeText(
            "Hyper-V is currently active on your system. This may interfere with virtualization services, protection modules, and game memory hooks.\n\n"
            "Would you like to automatically disable Hyper-V and restart your computer to apply changes?"
        )
        self.btn_disable = self.addButton("DISABLE HYPER-V & RESTART", QMessageBox.YesRole)
        self.btn_exit = self.addButton("EXIT LOADER", QMessageBox.NoRole)
        self.setStyleSheet(f"""
            QMessageBox {{
                background-color: {th['bg']};
                color: {th['text']};
                border: 2px solid {th['accent']};
                border-radius: 12px;
            }}
            QLabel {{
                color: {th['text']};
                font-size: 11px;
                font-weight: bold;
                border: none;
            }}
            QPushButton {{
                background: {th['surface']};
                color: {th['text']};
                border: 1px solid {th['border']};
                border-radius: 6px;
                padding: 6px 14px;
                font-weight: bold;
            }}
            QPushButton:hover {{
                border-color: {th['accent']};
                color: {th['accent']};
            }}
        """)

if __name__ == "__main__":
    try:
        app = QApplication(sys.argv)
        if check_hyperv_enabled():
            dialog = HyperVWarningDialog()
            dialog.exec_()
            if dialog.clickedButton() == dialog.btn_disable:
                disable_hyperv()
                import os
                os.system("shutdown /r /t 2")
            sys.exit(0)
            
        main_ui = FearMods()
        main_ui.show()
        sys.exit(app.exec_())
    except Exception as _e:
        _write_crash(type(_e), _e, _e.__traceback__)
        sys.exit(1)

