"""
ui_login.py - License key authentication screen with slide-to-login for Fear Mods Loader
"""
from PyQt5.QtWidgets import QWidget, QVBoxLayout, QLabel, QHBoxLayout, QFrame
from PyQt5.QtCore import Qt, QTimer, pyqtSignal
from PyQt5.QtGui import QFont, QPainter, QColor, QPen, QBrush, QPainterPath

from ui_widgets import theme, bold_font, font, FearInput, FearButton, SlideLoginButton, ToastNotification, FearCheckbox


class LoginScreen(QWidget):
    login_success = pyqtSignal()

    def __init__(self, auth_manager, parent=None):
        super().__init__(parent)
        self.auth = auth_manager
        self._app_name = "AIMBOT AI EXE"
        self._build_ui()
        
        # Auto-load saved license
        saved_key = self.auth.load_license_key()
        if saved_key:
            self._key_input.setText(saved_key)

    def set_app_name(self, name):
        self._app_name = name
        if hasattr(self, '_title_lbl'):
            self._title_lbl.setText(name.upper())

    def apply_theme(self):
        th = theme()
        if hasattr(self, '_card'):
            self._card.setStyleSheet(f"""
                QFrame#LoginCard {{
                    background: {th['surface']};
                    border: 1px solid {th['border']};
                    border-radius: 12px;
                }}
            """)
        if hasattr(self, '_title_lbl'):
            self._title_lbl.setStyleSheet(f"color: {th['text']}; letter-spacing: 2px; border: none;")
        if hasattr(self, '_sep'):
            self._sep.setStyleSheet(f"QFrame {{ background-color: {th['border']}; border: none; }}")
        if hasattr(self, '_sub_lbl'):
            self._sub_lbl.setStyleSheet(f"color: {th['subtext']}; border: none;")
        if hasattr(self, '_key_input'):
            self._key_input.apply_theme()
        if hasattr(self, '_discord_link'):
            self._discord_link.setStyleSheet(f"color: {th['subtext']}; text-decoration: none;")
        if hasattr(self, '_slider'):
            self._slider.update()

    def _build_ui(self):
        th = theme()
        self._theme_labels = []
        layout = QVBoxLayout(self)
        layout.setContentsMargins(40, 10, 40, 16)
        layout.setSpacing(0)

        layout.addStretch(1)

        # Main Login Card Container matching FeatureGroupContainer
        self._card = QFrame()
        self._card.setObjectName("LoginCard")
        self._card.setStyleSheet(f"""
            QFrame#LoginCard {{
                background: {th['surface']};
                border: 1px solid {th['border']};
                border-radius: 12px;
            }}
        """)
        card_layout = QVBoxLayout(self._card)
        card_layout.setContentsMargins(20, 20, 20, 20)
        card_layout.setSpacing(14)

        # Large Centered Title
        self._title_lbl = QLabel(self._app_name.upper())
        self._title_lbl.setFont(bold_font(15))
        self._title_lbl.setStyleSheet(f"color: {th['text']}; letter-spacing: 2px; border: none;")
        self._title_lbl.setAlignment(Qt.AlignCenter)
        card_layout.addWidget(self._title_lbl)

        card_layout.addSpacing(4)

        # 1px Separator line under header
        self._sep = QFrame()
        self._sep.setFixedHeight(1)
        self._sep.setStyleSheet(f"QFrame {{ background-color: {th['border']}; border: none; }}")
        card_layout.addWidget(self._sep)

        card_layout.addSpacing(6)

        # Sub-caption
        self._sub_lbl = QLabel("Enter license key to unlock loader features")
        self._sub_lbl.setFont(font(9))
        self._sub_lbl.setStyleSheet(f"color: {th['subtext']}; border: none;")
        self._sub_lbl.setAlignment(Qt.AlignCenter)
        card_layout.addWidget(self._sub_lbl)

        card_layout.addSpacing(2)

        # Key input
        self._key_input = FearInput("XXXX-XXXX-XXXX-XXXX")
        self._key_input.setFixedHeight(44)
        card_layout.addWidget(self._key_input)
        card_layout.addSpacing(4)

        # Slide-to-login button
        self._slider = SlideLoginButton()
        card_layout.addWidget(self._slider)
        self._slider.add_callback(self._on_slide_complete)

        layout.addWidget(self._card)

        layout.addSpacing(14)
        
        # Buy Key / Discord Link
        discord_url = "https://discord.gg/rRrsuA2TWZ"
        self._discord_link = QLabel(f"<a href='{discord_url}' style='color: {th['subtext']}; text-decoration: none; font-weight: bold;'>Don't have a key? Click to Join Discord</a>")
        self._discord_link.setFont(bold_font(9))
        self._discord_link.setAlignment(Qt.AlignCenter)
        self._discord_link.setOpenExternalLinks(True)
        layout.addWidget(self._discord_link)

        layout.addStretch(1)

        # Toast notification (bottom)
        self._toast = ToastNotification(self)
        self._toast.setFixedWidth(350)
        
        self.apply_theme()

    def resizeEvent(self, e):
        super().resizeEvent(e)
        if hasattr(self, '_toast'):
            tw = self._toast.width()
            self._toast.move((self.width() - tw) // 2, self.height() - 54)

    def _on_slide_complete(self):
        key = self._key_input.text().strip()
        if not key:
            self._toast.show_message("Please enter a license key", "error")
            self._slider.reset()
            return

        self._key_input.setEnabled(False)
        self._toast.show_message("Verifying license...", "warn", 10000)

        def do_verify():
            try:
                res = self.auth.verify_key(key)
            except Exception as ex:
                res = {"success": False, "message": str(ex)}

            if res.get("success"):
                # Always save license automatically on success
                self.auth.save_license_key(key)

                self._toast.show_message("Access Granted!", "success", 1500)
                QTimer.singleShot(1200, self.login_success.emit)
            else:
                msg = res.get("message", "Invalid License Key")
                self._toast.show_message(msg, "error")
                self._slider.reset()
                self._key_input.setEnabled(True)

        # Small delay to show "Verifying" state
        QTimer.singleShot(1200, do_verify)

    def reset(self):
        """Reset UI for a fresh login attempt."""
        self._key_input.clear()
        self._key_input.setEnabled(True)
        self._slider.reset()
        self._toast.hide()

