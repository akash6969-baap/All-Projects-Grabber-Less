"""
ui_widgets.py - Shared UI primitives for Fear Mods Loader
"""
from PyQt5.QtWidgets import (QPushButton, QLineEdit, QLabel, QGraphicsDropShadowEffect,
                             QWidget, QHBoxLayout, QComboBox, QSlider)
from PyQt5.QtCore import Qt, QPropertyAnimation, QEasingCurve, pyqtProperty, QTimer, QPoint, QRectF, pyqtSignal
from PyQt5.QtGui import QFont, QColor, QPainter, QPen, QBrush, QLinearGradient, QFontDatabase

import os

# ── Colour Palette ─────────────────────────────────────────────────────────────
DARK = {
    "bg":        "#09090D",
    "surface":   "#111116",
    "card":      "#16161E",
    "border":    "#222230",
    "accent":    "#E60000",
    "accent2":   "#FF3333",
    "text":      "#FFFFFF",
    "subtext":   "#888899",
    "dim":       "#444455",
    "success":   "#00D04A",
    "warn":      "#FFB300",
}

LIGHT = {
    "bg":        "#F4F4F8",
    "surface":   "#FFFFFF",
    "card":      "#EAEAEA",
    "border":    "#D4D4D8",
    "accent":    "#E60000",
    "accent2":   "#FF3333",
    "text":      "#18181B",
    "subtext":   "#52525B",
    "dim":       "#71717A",
    "success":   "#16A34A",
    "warn":      "#D97706",
}

_current_theme = "dark"
try:
    if os.path.exists("theme.cfg"):
        with open("theme.cfg", "r") as f:
            t = f.read().strip()
            if t in ["dark", "light"]:
                _current_theme = t
except:
    pass

def theme():
    return DARK if _current_theme == "dark" else LIGHT

def set_theme(t):
    global _current_theme
    _current_theme = t
    try:
        with open("theme.cfg", "w") as f:
            f.write(t)
    except:
        pass


# ── Fonts ──────────────────────────────────────────────────────────────────────
def font(size=10, weight=QFont.Normal, family="Segoe UI"):
    f = QFont(family, size, weight)
    return f

def bold_font(size=10, family="Segoe UI"):
    return font(size, QFont.Bold, family)


# ── Drop Shadow Helper ─────────────────────────────────────────────────────────
def glow_shadow(widget, color="#E60000", blur=20, offset=(0, 2)):
    fx = QGraphicsDropShadowEffect(widget)
    fx.setBlurRadius(blur)
    fx.setColor(QColor(color))
    fx.setOffset(*offset)
    widget.setGraphicsEffect(fx)
    return fx


# ── Modern Button ──────────────────────────────────────────────────────────────
class FearButton(QPushButton):
    def __init__(self, text, variant="primary", parent=None):
        super().__init__(text, parent)
        self.variant = variant
        self._apply_style()
        self.setFixedHeight(44)
        self.setCursor(Qt.PointingHandCursor)
        self.setFont(bold_font(10))

    def _apply_style(self):
        th = theme()
        if self.variant == "primary":
            self.setStyleSheet(f"""
                QPushButton {{
                    background: qlineargradient(x1:0,y1:0,x2:1,y2:0,
                        stop:0 {th['accent']}, stop:1 {th['accent2']});
                    color: white;
                    border-radius: 10px;
                    border: none;
                    letter-spacing: 2px;
                    padding: 0 20px;
                }}
                QPushButton:hover {{
                    background: qlineargradient(x1:0,y1:0,x2:1,y2:0,
                        stop:0 {th['accent2']}, stop:1 #FF6666);
                }}
                QPushButton:pressed {{ background: {th['accent']}; }}
                QPushButton:disabled {{
                    background: {th['border']};
                    color: {th['dim']};
                }}
            """)
        elif self.variant == "ghost":
            self.setStyleSheet(f"""
                QPushButton {{
                    background: transparent;
                    color: {th['accent']};
                    border: 2px solid {th['accent']};
                    border-radius: 10px;
                    letter-spacing: 2px;
                    padding: 0 20px;
                }}
                QPushButton:hover {{
                    background: rgba(230,0,0,0.12);
                    border-color: {th['accent2']};
                }}
                QPushButton:disabled {{
                    border-color: {th['dim']};
                    color: {th['dim']};
                }}
            """)
        elif self.variant == "icon":
            self.setStyleSheet(f"""
                QPushButton {{
                    background: {th['surface']};
                    color: {th['subtext']};
                    border: 1px solid {th['border']};
                    border-radius: 8px;
                }}
                QPushButton:hover {{ color: {th['accent']}; border-color: {th['accent']}; }}
            """)


# ── Custom Checkbox ──────────────────────────────────────────────────────────
class FearCheckbox(QWidget):
    toggled = pyqtSignal(bool)
    
    def __init__(self, label, checked=False, parent=None):
        super().__init__(parent)
        self.setFixedHeight(30)
        self._checked = checked
        self._label = label
        self.setCursor(Qt.PointingHandCursor)

    def isChecked(self):
        return self._checked

    def setChecked(self, checked):
        if self._checked != checked:
            self._checked = checked
            self.update()
            self.toggled.emit(self._checked)

    def apply_theme(self):
        self.update()

    def mousePressEvent(self, e):
        self._checked = not self._checked
        self.update()
        self.toggled.emit(self._checked)


    def paintEvent(self, e):
        th = theme()
        p = QPainter(self)
        p.setRenderHint(QPainter.Antialiasing)

        # Box
        size = 22
        rect = QPoint(0, (self.height() - size) // 2)
        box_rect = QRectF(rect.x(), rect.y(), size, size)
        
        if self._checked:
            p.setBrush(QColor(th['accent']))
            p.setPen(Qt.NoPen)
            p.drawRoundedRect(box_rect, 5, 5)
            # Checkmark (Scaled for size 22)
            p.setPen(QPen(Qt.white, 2.5, Qt.SolidLine, Qt.RoundCap, Qt.RoundJoin))
            p.drawLine(rect.x() + 5, rect.y() + 11, rect.x() + 10, rect.y() + 16)
            p.drawLine(rect.x() + 10, rect.y() + 16, rect.x() + 17, rect.y() + 6)
        else:
            p.setBrush(Qt.NoBrush)
            p.setPen(QPen(QColor(th['border']), 2))
            p.drawRoundedRect(box_rect, 5, 5)

        # Label
        p.setPen(QColor(th['subtext']))
        p.setFont(font(9))
        p.drawText(32, 0, self.width() - 32, self.height(), Qt.AlignVCenter, self._label)



# ── Custom Toggle Switch ───────────────────────────────────────────────────────
class FearToggle(QWidget):
    toggled = pyqtSignal(bool)
    
    def __init__(self, label, checked=False, parent=None):
        super().__init__(parent)
        self.setFixedHeight(26)
        self._checked = checked
        self._label = label
        self.setCursor(Qt.PointingHandCursor)
        self.setMinimumWidth(160)

    def isChecked(self):
        return self._checked

    def setChecked(self, checked):
        if self._checked != checked:
            self._checked = checked
            self.update()
            self.toggled.emit(self._checked)

    def apply_theme(self):
        self.update()

    def mousePressEvent(self, e):
        self._checked = not self._checked
        self.update()
        self.toggled.emit(self._checked)

    def paintEvent(self, e):
        th = theme()
        p = QPainter(self)
        p.setRenderHint(QPainter.Antialiasing)

        # Draw Label (Left side)
        p.setPen(QColor(th['text'] if self._checked else th['subtext']))
        p.setFont(font(9))
        p.drawText(0, 0, self.width() - 45, self.height(), Qt.AlignVCenter | Qt.AlignLeft, self._label)

        # Draw Switch
        sw_width = 32
        sw_height = 18
        if self._label:
            sw_x = self.width() - sw_width - 5
        else:
            sw_x = (self.width() - sw_width) // 2
            
        sw_y = (self.height() - sw_height) // 2
        
        sw_rect = QRectF(sw_x, sw_y, sw_width, sw_height)
        
        # Track
        if self._checked:
            p.setBrush(QColor(th['accent']))
            p.setPen(Qt.NoPen)
        else:
            p.setBrush(QColor(th['surface']))
            p.setPen(QPen(QColor(th['border']), 1.5))
            
        p.drawRoundedRect(sw_rect, sw_height/2, sw_height/2)
        
        # Knob
        knob_size = 12
        knob_y = sw_y + 3
        if self._checked:
            knob_x = sw_x + sw_width - knob_size - 3
            p.setBrush(QColor("white"))
        else:
            knob_x = sw_x + 3
            p.setBrush(QColor(th['subtext']))
            
        p.setPen(Qt.NoPen)
        p.drawEllipse(QRectF(knob_x, knob_y, knob_size, knob_size))


# ── Key Input Field ────────────────────────────────────────────────────────────
class FearInput(QLineEdit):
    def __init__(self, placeholder="", is_password=False, parent=None):
        super().__init__(parent)
        self.setPlaceholderText(placeholder)
        self.setFixedHeight(50)
        self.setAlignment(Qt.AlignCenter)
        self.setFont(bold_font(11))
        if is_password:
            self.setEchoMode(QLineEdit.Password)
        self._apply_style()

    def apply_theme(self):
        self._apply_style()

    def _apply_style(self):
        th = theme()
        self.setStyleSheet(f"""
            QLineEdit {{
                background: {th['surface']};
                color: {th['text']};
                border: 2px solid {th['border']};
                border-radius: 10px;
                padding: 0 16px;
                letter-spacing: 2px;
            }}
            QLineEdit:focus {{
                border: 2px solid {th['accent']};
                background: {th['card']};
            }}
            QLineEdit::placeholder {{
                color: {th['dim']};
                letter-spacing: 1px;
            }}
        """)


# ── Toast Notification ─────────────────────────────────────────────────────────
class ToastNotification(QLabel):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setAlignment(Qt.AlignCenter)
        self.setFont(bold_font(9))
        self.setFixedHeight(38)
        self.hide()
        self._timer = QTimer(self)
        self._timer.setSingleShot(True)
        self._timer.timeout.connect(self._fade_out)

    def show_message(self, text, variant="error", duration=3000):
        th = theme()
        color = th['accent'] if variant == "error" else th['success'] if variant == "success" else th['warn']
        self.setText(text.upper())
        self.setStyleSheet(f"""
            QLabel {{
                background: rgba(0,0,0,0.85);
                color: {color};
                border: 1px solid {color};
                border-radius: 8px;
                padding: 0 16px;
                letter-spacing: 1px;
            }}
        """)
        self.show()
        self._timer.start(duration)

    def _fade_out(self):
        self.hide()


# ── Slide-to-Login Button ──────────────────────────────────────────────────────
class SlideLoginButton(QWidget):
    """A draggable slide-to-login control that emits a signal when fully slid."""

    from PyQt5.QtCore import pyqtSignal

    def __init__(self, parent=None):
        super().__init__(parent)
        from PyQt5.QtCore import pyqtSignal
        self.setFixedHeight(52)
        self._dragging = False
        self._slider_x = 0
        self._max_x = 0
        self._activated = False
        self._label = "SLIDE TO LOGIN"
        self._callbacks = []
        self.setMouseTracking(True)

    def add_callback(self, fn):
        self._callbacks.append(fn)

    def resizeEvent(self, e):
        self._max_x = self.width() - 52
        super().resizeEvent(e)

    def paintEvent(self, e):
        th = theme()
        p = QPainter(self)
        p.setRenderHint(QPainter.Antialiasing)

        # Track bg
        track_color = QColor(th['surface'])
        border_color = QColor(th['border'])  # Match FearInput border
        p.setBrush(QBrush(track_color))
        p.setPen(QPen(border_color, 2))
        p.drawRoundedRect(QRectF(0.0, 0.0, float(self.width()), float(self.height())), 10.0, 10.0)  # Radius 10

        # Label
        if not self._activated:
            p.setPen(QColor(th['subtext']))
            p.setFont(bold_font(9))
            p.drawText(0, 0, self.width(), self.height(), Qt.AlignCenter, self._label)

        # Slider knob
        knob_x = float(self._slider_x)
        knob_color = QColor(th['accent']) if not self._activated else QColor(th['success'])
        p.setBrush(QBrush(knob_color))
        p.setPen(Qt.NoPen)
        p.drawRoundedRect(QRectF(knob_x + 4.0, 4.0, 44.0, 44.0), 8.0, 8.0)  # Radius 8

        # Arrow or check
        p.setPen(QPen(QColor("white"), 2, Qt.SolidLine, Qt.RoundCap, Qt.RoundJoin))
        cx = int(knob_x + 4 + 22)
        cy = 26
        if not self._activated:
            p.drawLine(cx - 7, cy, cx + 7, cy)
            p.drawLine(cx + 1, cy - 6, cx + 7, cy)
            p.drawLine(cx + 1, cy + 6, cx + 7, cy)
        else:
            p.drawLine(cx - 8, cy, cx - 2, cy + 6)
            p.drawLine(cx - 2, cy + 6, cx + 8, cy - 6)

    def mousePressEvent(self, e):
        if e.button() == Qt.LeftButton and not self._activated:
            knob_rect_x = self._slider_x + 4
            if knob_rect_x <= e.x() <= knob_rect_x + 44:
                self._dragging = True

    def mouseMoveEvent(self, e):
        if self._dragging and not self._activated:
            new_x = e.x() - 26
            self._slider_x = max(0, min(new_x, self._max_x))
            self.update()

    def mouseReleaseEvent(self, e):
        if self._dragging:
            self._dragging = False
            if self._slider_x >= self._max_x * 0.85:
                self._activated = True
                self._slider_x = self._max_x
                self.update()
                for cb in self._callbacks:
                    cb()
            else:
                # Snap back
                self._animate_back()

    def _animate_back(self):
        steps = 8
        start = self._slider_x
        delta = start / steps
        def step(i=[0]):
            self._slider_x = max(0, start - delta * (i[0] + 1))
            self.update()
            i[0] += 1
            if i[0] < steps:
                QTimer.singleShot(20, lambda: step(i))
        step()

    def reset(self):
        self._activated = False
        self._slider_x = 0
        self.update()

# ── Grid Layout UI Components ─────────────────────────────────────────────────
from PyQt5.QtWidgets import QGroupBox, QGridLayout, QVBoxLayout, QFrame, QSizePolicy

class FeatureGroupContainer(QWidget):
    def __init__(self, title, icon, parent=None):
        super().__init__(parent)
        self.setAttribute(Qt.WA_StyledBackground, True)
        
        # Main layout — zero spacing, everything tight
        self.main_layout = QVBoxLayout(self)
        self.main_layout.setContentsMargins(12, 10, 12, 10)
        self.main_layout.setSpacing(0)
        self.main_layout.setAlignment(Qt.AlignTop)
        
        # Title row
        title_layout = QHBoxLayout()
        title_layout.setContentsMargins(0, 0, 0, 5)
        title_layout.setSpacing(0)
        
        self.icon_lbl = QLabel(icon)
        self.icon_lbl.setFont(QFont("Segoe MDL2 Assets", 9))
        
        self.title_lbl = QLabel(title)
        self.title_lbl.setFont(bold_font(9))
        
        title_layout.addWidget(self.icon_lbl)
        title_layout.addWidget(self.title_lbl)
        title_layout.addStretch()
        self.main_layout.addLayout(title_layout)
        
        # Separator Line — right under title
        self.separator = QFrame()
        self.separator.setFixedHeight(1)
        self.main_layout.addWidget(self.separator)
        
        # Content layout — features start 5px below separator with 5px balanced spacing between rows
        self.content_layout = QVBoxLayout()
        self.content_layout.setContentsMargins(0, 6, 0, 6)
        self.content_layout.setSpacing(5)
        self.main_layout.addLayout(self.content_layout)
        
        # Spacer at the bottom so extra vertical height goes to the bottom of the card
        self.main_layout.addStretch()
        self.apply_theme()

    def addWidget(self, widget):
        self.content_layout.addWidget(widget)

    def apply_theme(self):
        th = theme()
        self.setStyleSheet(f"""
            FeatureGroupContainer {{
                border: 1px solid {th['border']};
                border-radius: 8px;
                background: {th['surface']};
            }}
        """)
        if hasattr(self, 'icon_lbl'):
            self.icon_lbl.setStyleSheet(f"color: {th['accent']}; background: transparent;")
        if hasattr(self, 'title_lbl'):
            self.title_lbl.setStyleSheet(f"color: {th['text']}; background: transparent; padding-left: 5px;")
        if hasattr(self, 'separator'):
            self.separator.setStyleSheet(f"QFrame {{ background-color: {th['border']}; border: none; }}")

class CompactFeatureRow(QWidget):
    def __init__(self, title, keybind_widget=None, control_widget=None, parent=None):
        super().__init__(parent)
        
        from PyQt5.QtWidgets import QComboBox, QSlider, QPushButton
        is_slider = isinstance(control_widget, QSlider) or type(control_widget).__name__ == "QSlider"
        is_combo = isinstance(control_widget, QComboBox) or type(control_widget).__name__ == "QComboBox"
        is_btn = (isinstance(control_widget, QPushButton) or type(control_widget).__name__ == "QPushButton") and type(control_widget).__name__ != "KeybindButton"
        
        self.title_label = None
        self.value_label = None
        self.btn_key = keybind_widget
        self.toggle = control_widget
        self.control_widget = control_widget

        if is_slider:
            self.setFixedHeight(35)
            self.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Fixed)
            
            main_layout = QVBoxLayout(self)
            main_layout.setContentsMargins(0, 2, 0, 2)
            main_layout.setSpacing(3)
            
            top_layout = QHBoxLayout()
            top_layout.setContentsMargins(0, 0, 0, 0)
            
            self.title_label = QLabel(title)
            self.title_label.setFont(bold_font(8))
            top_layout.addWidget(self.title_label)
            
            top_layout.addStretch()
            
            self.value_label = QLabel(str(control_widget.value()))
            self.value_label.setFont(bold_font(8))
            top_layout.addWidget(self.value_label)
            
            main_layout.addLayout(top_layout)
            
            control_widget.setMaximumWidth(16777215)
            control_widget.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Fixed)
            
            control_widget.valueChanged.connect(lambda v: self.value_label.setText(str(v)) if hasattr(self, 'value_label') and self.value_label else None)
            main_layout.addWidget(control_widget)

        elif is_combo:
            self.setFixedHeight(28)
            self.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Fixed)
            
            layout = QHBoxLayout(self)
            layout.setContentsMargins(0, 0, 0, 0)
            layout.setSpacing(10)
            
            self.title_label = QLabel(title)
            self.title_label.setFont(bold_font(8))
            layout.addWidget(self.title_label)
            
            layout.addStretch()
            control_widget.setFixedSize(140, 24)
            layout.addWidget(control_widget)

        elif is_btn:
            self.setFixedHeight(30)
            self.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Fixed)
            
            layout = QHBoxLayout(self)
            layout.setContentsMargins(0, 0, 0, 0)
            layout.setSpacing(10)
            
            self.title_label = QLabel(title)
            self.title_label.setFont(bold_font(8))
            layout.addWidget(self.title_label)
            
            layout.addStretch()
            control_widget.setFixedSize(110, 26)
            layout.addWidget(control_widget)

        else:
            self.setFixedHeight(25)
            self.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Fixed)
            
            layout = QHBoxLayout(self)
            layout.setContentsMargins(0, 0, 0, 0)
            layout.setSpacing(10)
            
            self.title_label = QLabel(title)
            self.title_label.setFont(bold_font(8))
            layout.addWidget(self.title_label)
            
            layout.addStretch()
            
            self.keybind_container = QWidget()
            self.keybind_container.setFixedHeight(21)
            klayout = QHBoxLayout(self.keybind_container)
            klayout.setContentsMargins(0, 0, 0, 0)
            klayout.setSpacing(0)
            
            if keybind_widget:
                keybind_widget.setMinimumWidth(30)
                keybind_widget.setFixedHeight(21)
                klayout.addWidget(keybind_widget)
                
            layout.addWidget(self.keybind_container)
            
            if control_widget:
                if type(control_widget).__name__ in ("FearCheckbox", "FearToggle") or "Checkbox" in type(control_widget).__name__:
                    control_widget.setFixedSize(22, 22)
                layout.addWidget(control_widget)
                
        self.apply_theme()

    def apply_theme(self):
        th = theme()
        if hasattr(self, 'title_label') and self.title_label:
            self.title_label.setStyleSheet(f"color: {th['text']}; background: transparent;")
        if hasattr(self, 'value_label') and self.value_label:
            self.value_label.setStyleSheet(f"color: {th['accent2']}; background: transparent; font-weight: bold;")
        if hasattr(self, 'btn_key') and self.btn_key:
            self.btn_key.setStyleSheet(f"""
                QPushButton {{
                    background: {th['card']};
                    color: {th['text']};
                    border: 1px solid {th['border']};
                    border-radius: 4px;
                    font-size: 9px;
                    font-weight: bold;
                    padding: 0 4px;
                }}
                QPushButton:hover {{
                    border-color: {th['accent']};
                    color: {th['accent']};
                }}
            """)
        if hasattr(self, 'control_widget') and self.control_widget:
            cw = self.control_widget
            if type(cw).__name__ == "CustomComboBox" or isinstance(cw, QComboBox):
                from PyQt5.QtWidgets import QListView
                from PyQt5.QtGui import QPalette, QColor
                if hasattr(cw, 'setView'):
                    cw.setView(QListView())
                p = cw.palette()
                p.setColor(QPalette.Highlight, QColor(th['border']))
                p.setColor(QPalette.HighlightedText, QColor(th['text']))
                cw.setPalette(p)
                cw.setStyleSheet(f"""
                    QComboBox {{
                        background: {th['card']};
                        color: {th['text']};
                        border: 1px solid {th['border']};
                        border-radius: 4px;
                        padding: 2px 20px 2px 8px;
                        font-size: 10px;
                        font-weight: bold;
                    }}
                    QComboBox:hover {{
                        border-color: {th['accent']};
                    }}
                    QComboBox::drop-down {{
                        subcontrol-origin: padding;
                        subcontrol-position: center right;
                        width: 18px;
                        background: transparent;
                        border: none;
                    }}
                    QComboBox::down-arrow {{
                        image: none;
                        width: 0px;
                        height: 0px;
                        border: none;
                    }}
                    QComboBox QAbstractItemView {{
                        background: {th['surface']};
                        color: {th['text']};
                        border: 1px solid {th['border']};
                        border-radius: 6px;
                        padding: 2px;
                        outline: 0px;
                        selection-background-color: {th['border']};
                        selection-color: {th['text']};
                    }}
                    QComboBox QAbstractItemView::item {{
                        min-height: 22px;
                        padding: 3px 6px;
                        border-radius: 3px;
                    }}
                    QComboBox QAbstractItemView::item:hover, QComboBox QAbstractItemView::item:selected {{
                        background-color: {th['border']};
                        color: {th['text']};
                        outline: 0px;
                    }}
                """)
            elif type(cw).__name__ == "QSlider" or isinstance(cw, QSlider):
                cw.setStyleSheet(f"""
                    QSlider {{
                        background: transparent;
                        height: 22px;
                    }}
                    QSlider::groove:horizontal {{
                        border: none;
                        height: 4px;
                        background: {th['border']};
                        border-radius: 2px;
                    }}
                    QSlider::sub-page:horizontal {{
                        background: {th['accent']};
                        border-radius: 2px;
                    }}
                    QSlider::add-page:horizontal {{
                        background: {th['border']};
                        border-radius: 2px;
                    }}
                    QSlider::handle:horizontal {{
                        background: qlineargradient(x1: 0, y1: 0, x2: 0, y2: 1, stop: 0 #FFFFFF, stop: 0.5 #E4E4E7, stop: 1 #A1A1AA);
                        border: 1px solid {th['dim']};
                        width: 7px;
                        height: 16px;
                        margin: -6px 0;
                        border-radius: 2px;
                    }}
                    QSlider::handle:horizontal:hover {{
                        background: #FFFFFF;
                        border: 1px solid {th['accent']};
                    }}
                    QSlider::handle:horizontal:pressed {{
                        background: {th['accent']};
                        border: 1px solid #FFFFFF;
                    }}
                """)
            elif hasattr(cw, 'apply_theme') and callable(cw.apply_theme):
                try: cw.apply_theme()
                except: pass
