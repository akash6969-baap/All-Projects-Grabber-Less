"""
ui_overlay.py - Standalone, floating, borderless Keybind Overlay HUD for Fear Mods
"""
from PyQt5.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QLabel, QFrame
from PyQt5.QtCore import Qt, QPoint
from PyQt5.QtGui import QFont, QColor

from ui_widgets import theme, bold_font, font

class KeybindOverlayWidget(QWidget):
    """
    Floating, frameless, stay-on-top Keybind HUD Overlay window.
    Dynamically displays only features that have a keybind registered.
    """
    def __init__(self, keybind_manager=None, parent=None):
        super().__init__(parent)
        self.keybind_manager = keybind_manager
        self.drag_position = QPoint()
        
        # Window Flags: Stay on Top, Frameless Tool Window (doesn't take taskbar space)
        self.setWindowFlags(Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint | Qt.Tool)
        self.setAttribute(Qt.WA_TranslucentBackground, True)
        self.setMinimumWidth(190)
        self.setFixedHeight(160)
        
        self._build_ui()
        self.refresh_keybinds()

    def _build_ui(self):
        th = theme()
        self.main_layout = QVBoxLayout(self)
        self.main_layout.setContentsMargins(0, 0, 0, 0)
        
        # Container Card
        self.card = QFrame(self)
        self.card.setObjectName("OverlayCard")
        self.card.setStyleSheet(f"""
            QFrame#OverlayCard {{
                background: {th['surface']};
                border: 1px solid {th['border']};
                border-radius: 10px;
            }}
        """)
        
        card_layout = QVBoxLayout(self.card)
        card_layout.setContentsMargins(12, 10, 12, 10)
        card_layout.setSpacing(8)
        
        # Centered Header Title without emoji
        self.title_lbl = QLabel("KEYBIND MANAGER", self.card)
        self.title_lbl.setFont(bold_font(9))
        self.title_lbl.setAlignment(Qt.AlignCenter)
        self.title_lbl.setStyleSheet(f"color: {th['text']}; letter-spacing: 1.5px; background: transparent; border: none;")
        
        card_layout.addWidget(self.title_lbl)
        
        # 1px Separator Line
        self.sep = QFrame(self.card)
        self.sep.setFixedHeight(1)
        self.sep.setStyleSheet(f"QFrame {{ background-color: {th['border']}; border: none; }}")
        card_layout.addWidget(self.sep)
        
        # Container for Keybind Rows
        self.rows_container = QWidget(self.card)
        self.rows_container.setStyleSheet("background: transparent; border: none;")
        self.rows_layout = QVBoxLayout(self.rows_container)
        self.rows_layout.setContentsMargins(0, 2, 0, 2)
        self.rows_layout.setSpacing(6)
        
        card_layout.addWidget(self.rows_container)
        card_layout.addStretch()
        
        self.main_layout.addWidget(self.card)

    def apply_theme(self):
        th = theme()
        if hasattr(self, 'card'):
            self.card.setStyleSheet(f"""
                QFrame#OverlayCard {{
                    background: {th['surface']};
                    border: 1px solid {th['border']};
                    border-radius: 10px;
                }}
            """)
        if hasattr(self, 'title_lbl'):
            self.title_lbl.setStyleSheet(f"color: {th['text']}; letter-spacing: 1.5px; background: transparent; border: none;")
        if hasattr(self, 'sep'):
            self.sep.setStyleSheet(f"QFrame {{ background-color: {th['border']}; border: none; }}")
            
        self.refresh_keybinds()

    def refresh_keybinds(self):
        th = theme()
        # Clear existing rows
        while self.rows_layout.count():
            item = self.rows_layout.takeAt(0)
            w = item.widget()
            if w:
                w.deleteLater()
                
        active = {}
        if self.keybind_manager and hasattr(self.keybind_manager, 'active_keybinds'):
            # Exclude insert/hide_gui keybind from overlay display
            active = {k: v for k, v in self.keybind_manager.active_keybinds.items() if v != "hide_gui" and k.lower() != "insert"}
            
        # Friendly feature names mapping
        name_map = {
            "hide_gui": "Toggle Loader GUI",
            "server": "Connect Server",
            "aimbot": "Aimbot Visible",
            "sniper_scope": "Aimbot Rage",
            "sniper_switch": "Aim Silent",
            "no_recoil": "No Recoil",
            "fastreload": "Fast Reload",
            "glitchfire": "Glitch Fire",
            "unlimitedammo": "Unlimited Ammo",
            "rapidfire": "Rapid Fire",
            "ignoreknocked": "Ignore Knocked",
            "espline": "ESP Line",
            "espbox": "ESP Box",
            "esphealth": "ESP Health",
            "espname": "ESP Name",
            "espdistace": "ESP Distance",
            "espskeleton": "ESP Skeleton",
            "espaimtrack": "ESP Aim-Track",
            "pull": "Enemy Pull",
            "base": "Teleport Base",
            "spinbot": "Spin Bot",
            "underground": "Underground Bypass",
            "temp_clean": "Temp Clean",
            "pc_opt": "PC Optimizer",
            "fps_boost": "FPS Boost",
            "notif": "Streamer Mode"
        }

        if not active:
            empty_lbl = QLabel("No active keybinds set", self.rows_container)
            empty_lbl.setFont(font(8))
            empty_lbl.setAlignment(Qt.AlignCenter)
            empty_lbl.setStyleSheet(f"color: {th['subtext']}; font-style: italic; background: transparent;")
            self.rows_layout.addWidget(empty_lbl)
        else:
            # Fetch feature_states from main window if available
            feature_states = {}
            from PyQt5.QtWidgets import QApplication
            app = QApplication.instance()
            if app:
                for widget in app.topLevelWidgets():
                    if hasattr(widget, 'feature_states') and isinstance(widget.feature_states, dict):
                        feature_states = widget.feature_states
                        break

            for key, action_id in active.items():
                title = name_map.get(action_id, action_id.upper())
                is_on = feature_states.get(action_id, False)
                
                row = QWidget(self.rows_container)
                row.setFixedHeight(24)
                row_layout = QHBoxLayout(row)
                row_layout.setContentsMargins(0, 0, 0, 0)
                row_layout.setSpacing(10)
                
                # Feature Name Label (White when ON, Subtext Grey when OFF)
                t_lbl = QLabel(title, row)
                t_lbl.setFont(bold_font(9) if is_on else font(9))
                t_color = "#FFFFFF" if is_on else th['subtext']
                t_lbl.setStyleSheet(f"color: {t_color}; background: transparent;")
                
                # Bracketed Key Label format [1] [2] [3] (No box container)
                key_text = f"[{key.upper()}]"
                k_lbl = QLabel(key_text, row)
                k_lbl.setFont(bold_font(9) if is_on else font(9))
                k_fg = th['accent'] if is_on else th['subtext']
                k_lbl.setStyleSheet(f"color: {k_fg}; background: transparent; border: none;")
                
                row_layout.addWidget(t_lbl)
                row_layout.addStretch()
                row_layout.addWidget(k_lbl)
                
                self.rows_layout.addWidget(row)
                
        # Auto-adjust height based on number of items
        needed_height = 55 + (max(1, len(active)) * 28)
        self.setFixedHeight(min(340, max(110, needed_height)))

    # Window Dragging Logic
    def mousePressEvent(self, event):
        if event.button() == Qt.LeftButton:
            self.drag_position = event.globalPos() - self.frameGeometry().topLeft()
            event.accept()

    def mouseMoveEvent(self, event):
        if event.buttons() == Qt.LeftButton and not self.drag_position.isNull():
            self.move(event.globalPos() - self.drag_position)
            event.accept()
