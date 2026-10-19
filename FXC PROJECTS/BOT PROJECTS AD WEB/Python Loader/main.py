"""
main.py - Fear Mods Loader entry point
─────────────────────────────────────────
Flow:
  1. Splash (circular loading) → init FearAuth in background
  2. App Selector carousel (aimkill, bypass, basic, internal, silent PNGs)
  3. Login screen (license key + slide-to-login)
  4. Success animation → Game selection (FF Playstore / FF Max / FF x86)
"""
import sys
import threading
from PyQt5.QtWidgets import (QApplication, QMainWindow, QWidget, QVBoxLayout,
                             QHBoxLayout, QPushButton, QLabel, QFrame,
                             QStackedWidget, QGraphicsDropShadowEffect)
from PyQt5.QtCore import Qt, QTimer, QPoint
from PyQt5.QtGui import QFont, QColor, QPainter, QBrush, QLinearGradient, QPainterPath, QPen

from ui_widgets import theme, set_theme, bold_font, font, glow_shadow
from ui_splash import SplashScreen
from ui_selector import SelectorScreen
from ui_login import LoginScreen
from ui_options import OptionsScreen
from auth import fear_auth


# ── Custom title bar with theme toggle ─────────────────────────────────────────
class TitleBar(QWidget):
    def __init__(self, window, parent=None):
        super().__init__(parent)
        self._window = window
        self.setFixedHeight(42)
        self._build()

    def _build(self):
        th = theme()
        layout = QHBoxLayout(self)
        layout.setContentsMargins(16, 0, 8, 0)
        layout.setSpacing(0)

        # Brand
        brand = QLabel('FEAR <span style="color:#E60000;">MODS</span>')
        brand.setFont(bold_font(13))
        brand.setStyleSheet(f"color: {th['text']}; letter-spacing: 1px;")
        layout.addWidget(brand)

        layout.addStretch()

        # Theme toggle (sun/moon)
        self._theme_btn = QPushButton("☀")
        self._theme_btn.setFixedSize(30, 30)
        self._theme_btn.setCursor(Qt.PointingHandCursor)
        self._theme_btn.setFont(QFont("Segoe UI Emoji", 12))
        self._theme_btn.setStyleSheet(f"""
            QPushButton {{
                background: transparent;
                color: {th['subtext']};
                border: none;
            }}
            QPushButton:hover {{ color: {th['accent']}; }}
        """)
        self._theme_btn.clicked.connect(self._toggle_theme)
        layout.addWidget(self._theme_btn)

        # Minimize
        btn_min = QPushButton("—")
        btn_min.setFixedSize(30, 30)
        btn_min.setCursor(Qt.PointingHandCursor)
        btn_min.setFont(bold_font(11))
        btn_min.setStyleSheet(f"""
            QPushButton {{
                background: transparent;
                color: {th['subtext']};
                border: none;
            }}
            QPushButton:hover {{ color: {th['accent']}; }}
        """)
        btn_min.clicked.connect(self._window.showMinimized)
        layout.addWidget(btn_min)

        # Close
        btn_close = QPushButton("✕")
        btn_close.setFixedSize(30, 30)
        btn_close.setCursor(Qt.PointingHandCursor)
        btn_close.setFont(bold_font(11))
        btn_close.setStyleSheet(f"""
            QPushButton {{
                background: transparent;
                color: {th['subtext']};
                border: none;
            }}
            QPushButton:hover {{ color: #FF3333; }}
        """)
        btn_close.clicked.connect(self._window.close)
        layout.addWidget(btn_close)

    def _toggle_theme(self):
        from ui_widgets import _current_theme
        new = "light" if _current_theme == "dark" else "dark"
        set_theme(new)
        self._theme_btn.setText("🌙" if new == "light" else "☀")
        # Full rebuild of the window to apply the theme
        self._window.rebuild_ui()

    def mousePressEvent(self, e):
        if e.button() == Qt.LeftButton:
            self._window._drag_pos = e.globalPos()

    def mouseMoveEvent(self, e):
        if e.buttons() == Qt.LeftButton and hasattr(self._window, '_drag_pos'):
            self._window.move(self._window.pos() + e.globalPos() - self._window._drag_pos)
            self._window._drag_pos = e.globalPos()


# ── Success transition overlay ─────────────────────────────────────────────────
class SuccessOverlay(QWidget):
    """Brief circular success animation before showing game selection."""
    finished = type(SplashScreen.finished)()  # pyqtSignal

    def __init__(self, parent=None):
        super().__init__(parent)
        from PyQt5.QtCore import pyqtSignal
        self._angle = 0
        self._alpha = 255
        self._phase = "check"   # check → fadeout
        self._check_progress = 0.0
        self._timer = QTimer(self)
        self._timer.timeout.connect(self._tick)

    def start(self):
        self._angle = 0
        self._alpha = 255
        self._phase = "check"
        self._check_progress = 0.0
        self.show()
        self._timer.start(16)
        QTimer.singleShot(1400, self._begin_fadeout)

    def _tick(self):
        self._angle = (self._angle - 5) % 360
        if self._phase == "check" and self._check_progress < 1.0:
            self._check_progress = min(1.0, self._check_progress + 0.06)
        if self._phase == "fadeout":
            self._alpha = max(0, self._alpha - 12)
            if self._alpha <= 0:
                self._timer.stop()
                self.hide()
                # emit finished
                self._on_finished()
                return
        self.update()

    def _begin_fadeout(self):
        self._phase = "fadeout"

    def _on_finished(self):
        pass  # overridden externally

    def paintEvent(self, e):
        w, h = self.width(), self.height()
        p = QPainter(self)
        p.setRenderHint(QPainter.Antialiasing)
        p.setOpacity(self._alpha / 255.0)
        p.setPen(Qt.NoPen)
        p.setBrush(QColor(theme()["bg"]))
        p.drawRoundedRect(0, 0, w, h, 16, 16)

        cx, cy = w // 2, h // 2
        ring_r = min(w, h) * 0.18
        ring_w = 5

        # Ring
        from PyQt5.QtCore import QRectF
        r = QRectF(cx - ring_r, cy - ring_r, ring_r * 2, ring_r * 2)

        # Glow ring segments
        for i in range(120):
            frac = i / 120
            alpha_v = int(60 + frac * 195)
            color = QColor(0, 208, 74, alpha_v)
            p.setPen(QPen(color, ring_w, Qt.SolidLine, Qt.RoundCap))
            angle_start = (self._angle + i * 3) * 16
            p.drawArc(r, angle_start, 3 * 16)

        # Checkmark (animated draw)
        scale = ring_r * 0.45
        prog = self._check_progress
        p.setPen(QPen(QColor("#00D04A"), 3.5, Qt.SolidLine, Qt.RoundCap, Qt.RoundJoin))

        x1, y1 = cx - scale * 0.55, cy + scale * 0.05
        x2, y2 = cx - scale * 0.05, cy + scale * 0.5
        x3, y3 = cx + scale * 0.6, cy - scale * 0.45

        if prog <= 0.4:
            t = prog / 0.4
            ex = x1 + (x2 - x1) * t
            ey = y1 + (y2 - y1) * t
            p.drawLine(int(x1), int(y1), int(ex), int(ey))
        else:
            p.drawLine(int(x1), int(y1), int(x2), int(y2))
            t = (prog - 0.4) / 0.6
            ex = x2 + (x3 - x2) * t
            ey = y2 + (y3 - y2) * t
            p.drawLine(int(x2), int(y2), int(ex), int(ey))

        # Text
        p.setPen(QColor("#00D04A"))
        p.setFont(bold_font(9))
        p.drawText(0, int(cy + ring_r + 18), w, 24, Qt.AlignCenter, "ACCESS GRANTED")


# ── Main Loader Window ─────────────────────────────────────────────────────────
class FearLoader(QMainWindow):
    PAGE_SPLASH    = 0
    PAGE_SELECTOR  = 1
    PAGE_LOGIN     = 2
    PAGE_SUCCESS   = 3
    PAGE_OPTIONS   = 4

    def __init__(self):
        super().__init__()
        self.setWindowFlags(Qt.FramelessWindowHint)
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setFixedSize(560, 460)
        self._selected_app = None
        self._current_page = self.PAGE_SPLASH
        self._build_window()
        self._start_splash()

    def _build_window(self):
        """Build the full window chrome + stacked pages."""
        th = theme()

        central = QWidget(self)
        self.setCentralWidget(central)

        # Background frame
        self._bg = QFrame(central)
        self._bg.setGeometry(8, 8, self.width() - 16, self.height() - 16)
        self._bg.setObjectName("MainBG")
        self._bg.setStyleSheet(f"""
            QFrame#MainBG {{
                background-color: {th['bg']};
                border-radius: 18px;
                border: 1.5px solid {th['border']};
            }}
        """)

        # Drop shadow on window
        shadow = QGraphicsDropShadowEffect(self._bg)
        shadow.setBlurRadius(30)
        shadow.setColor(QColor(0, 0, 0, 100))
        shadow.setOffset(0, 4)
        self._bg.setGraphicsEffect(shadow)

        root_layout = QVBoxLayout(self._bg)
        root_layout.setContentsMargins(0, 0, 0, 0)
        root_layout.setSpacing(0)

        # Title bar
        self._title_bar = TitleBar(self)
        root_layout.addWidget(self._title_bar)

        # Thin accent line
        line = QFrame()
        line.setFixedHeight(1)
        line.setStyleSheet(f"background: {th['border']};")
        root_layout.addWidget(line)

        # Stacked pages (clipped and rounded at bottom to match outer frame)
        self._pages = QStackedWidget()
        self._pages.setStyleSheet(f"""
            QStackedWidget {{
                background-color: {th['bg']};
                border-bottom-left-radius: 18px;
                border-bottom-right-radius: 18px;
            }}
        """)
        root_layout.addWidget(self._pages)

        # -- Page 0: Splash
        self._splash = SplashScreen()
        self._splash.finished.connect(self._on_splash_done)
        self._pages.addWidget(self._splash)

        # -- Page 1: Selector
        self._selector = SelectorScreen()
        self._selector.app_selected.connect(self._on_app_selected)
        self._pages.addWidget(self._selector)

        # -- Page 2: Login
        self._login = LoginScreen(fear_auth)
        self._login.login_success.connect(self._on_login_success)
        self._pages.addWidget(self._login)

        # -- Page 3: Success overlay
        self._success = SuccessOverlay()
        self._success._on_finished = self._on_success_done
        self._pages.addWidget(self._success)

        # -- Page 4: Options
        self._options = OptionsScreen()
        self._options.game_selected.connect(self._on_game_selected)
        self._pages.addWidget(self._options)

        self._pages.setCurrentIndex(self.PAGE_SPLASH)

    def rebuild_ui(self):
        """Tear down and rebuild after theme change (preserves page state)."""
        page = self._pages.currentIndex()
        self._build_window()
        self._pages.setCurrentIndex(page)

    # ── Flow ───────────────────────────────────────────────────────────────
    def _start_splash(self):
        # Init auth in background thread so UI stays smooth
        def init_auth():
            fear_auth.initialize()
            QTimer.singleShot(0, self._splash.complete_loading)

        t = threading.Thread(target=init_auth, daemon=True)
        t.start()

    def _on_splash_done(self):
        self._pages.setCurrentIndex(self.PAGE_SELECTOR)

    def _on_app_selected(self, app_data):
        self._selected_app = app_data
        self._login.set_app_name(app_data["name"])
        self._login.reset()
        self._pages.setCurrentIndex(self.PAGE_LOGIN)

    def _on_login_success(self):
        self._pages.setCurrentIndex(self.PAGE_SUCCESS)
        self._success.start()

    def _on_success_done(self):
        self._pages.setCurrentIndex(self.PAGE_OPTIONS)

    def _on_game_selected(self, game_id):
        """Future hook: implement DLL injection per game_id."""
        print(f"[FearLoader] Game selected: {game_id}")
        # Placeholder — injection logic will go here later

    # ── Window drag ────────────────────────────────────────────────────────
    def mousePressEvent(self, e):
        if e.button() == Qt.LeftButton:
            self._drag_pos = e.globalPos()

    def mouseMoveEvent(self, e):
        if e.buttons() == Qt.LeftButton and hasattr(self, '_drag_pos'):
            self.move(self.pos() + e.globalPos() - self._drag_pos)
            self._drag_pos = e.globalPos()


# ── Entry point ────────────────────────────────────────────────────────────────
if __name__ == '__main__':
    app = QApplication(sys.argv)
    app.setStyle("Fusion")

    # Global font fallback
    app.setFont(QFont("Segoe UI", 9))

    loader = FearLoader()
    loader.show()
    sys.exit(app.exec_())
