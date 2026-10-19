"""
ui_options.py - Post-authentication game selection screen for Fear Mods Loader
"""
from PyQt5.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QLabel,
                             QPushButton, QFrame, QCheckBox)
from PyQt5.QtCore import Qt, QTimer, pyqtSignal, QRectF
from PyQt5.QtGui import (QPainter, QColor, QFont, QPen, QBrush,
                          QLinearGradient, QPainterPath)

from ui_widgets import theme, bold_font, font, FearButton, glow_shadow, ToastNotification


class GameOptionButton(QPushButton):
    """Full-width game option button with hover animation."""

    def __init__(self, text, parent=None):
        super().__init__(text, parent)
        self.setFixedHeight(48)
        self.setCursor(Qt.PointingHandCursor)
        self.setFont(bold_font(10))
        self._apply_style()

    def _apply_style(self):
        th = theme()
        self.setStyleSheet(f"""
            QPushButton {{
                background: {th['surface']};
                color: {th['text']};
                border: 2px solid {th['border']};
                border-radius: 12px;
                letter-spacing: 3px;
                padding: 0 20px;
            }}
            QPushButton:hover {{
                background: {th['card']};
                border-color: {th['accent']};
                color: {th['accent']};
            }}
            QPushButton:pressed {{
                background: {th['accent']};
                color: white;
                border-color: {th['accent']};
            }}
        """)


class OptionsScreen(QWidget):
    game_selected = pyqtSignal(str)

    def __init__(self, parent=None):
        super().__init__(parent)
        self._build_ui()

    def _build_ui(self):
        th = theme()
        layout = QVBoxLayout(self)
        layout.setContentsMargins(30, 10, 30, 16)
        layout.setSpacing(0)

        layout.addStretch(1)

        # Red bordered card
        card = QFrame()
        card.setStyleSheet(f"""
            QFrame {{
                background: transparent;
                border: 2px solid {th['accent']};
                border-radius: 16px;
            }}
        """)
        card_layout = QVBoxLayout(card)
        card_layout.setContentsMargins(28, 28, 28, 28)
        card_layout.setSpacing(14)

        title = QLabel("SELECT GAME")
        title.setFont(bold_font(16))
        title.setStyleSheet(f"color: {th['text']}; letter-spacing: 4px; border: none;")
        title.setAlignment(Qt.AlignCenter)
        card_layout.addWidget(title)
        card_layout.addSpacing(8)

        # Game buttons
        games = [
            ("FREE FIRE [PLAYSTORE]", "ff_playstore"),
            ("FREE FIRE (X86)", "ff_x86"),
            ("FREE FIRE MAX", "ff_max"),
        ]

        for label, game_id in games:
            btn = GameOptionButton(label)
            btn.clicked.connect(lambda checked, gid=game_id: self._on_game_click(gid))
            card_layout.addWidget(btn)

        layout.addWidget(card)

        layout.addStretch(1)

        # Skip updates toggle row
        bottom_row = QHBoxLayout()
        bottom_row.setContentsMargins(30, 0, 30, 0)
        skip_lbl = QLabel("SKIP UPDATES")
        skip_lbl.setFont(font(8))
        skip_lbl.setStyleSheet(f"color: {th['subtext']}; letter-spacing: 2px;")
        bottom_row.addWidget(skip_lbl)
        bottom_row.addStretch()

        self._toggle = QCheckBox()
        self._toggle.setStyleSheet(f"""
            QCheckBox::indicator {{
                width: 18px;
                height: 18px;
                border-radius: 9px;
                border: 2px solid {th['border']};
                background: {th['surface']};
            }}
            QCheckBox::indicator:checked {{
                background: {th['accent']};
                border-color: {th['accent']};
            }}
        """)
        bottom_row.addWidget(self._toggle)
        layout.addLayout(bottom_row)
        layout.addSpacing(24)

        # Toast
        self._toast = ToastNotification(self)
        self._toast.setFixedWidth(320)

    def resizeEvent(self, e):
        super().resizeEvent(e)
        if hasattr(self, '_toast'):
            tw = self._toast.width()
            self._toast.move((self.width() - tw) // 2, self.height() - 54)

    def _on_game_click(self, game_id):
        self._toast.show_message(f"Launching {game_id.upper().replace('_', ' ')}...", "success", 2000)
        self.game_selected.emit(game_id)
