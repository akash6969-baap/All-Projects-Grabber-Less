"""
ui_selector.py - Application selection carousel for Fear Mods Loader
"""
import os
from PyQt5.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QLabel,
                             QPushButton, QFrame, QSizePolicy, QGraphicsDropShadowEffect)
from PyQt5.QtCore import Qt, QTimer, QPropertyAnimation, QEasingCurve, QRect, pyqtSignal, QPoint
from PyQt5.QtGui import (QPixmap, QPainter, QColor, QFont, QBrush, QPen,
                         QLinearGradient, QPainterPath)

from ui_widgets import theme, bold_font, font, FearButton, glow_shadow

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

# ── App definitions ────────────────────────────────────────────────────────────
APPS = [
    {
        "id":    "aimkill",
        "name":  "AIMKILL",
        "tag":   "Premium Module",
        "desc":  "Silent Aim · ESP · Radar",
        "image": os.path.join(BASE_DIR, "aimkill.png"),
        "badge": "PRO",
    },
    {
        "id":    "basic",
        "name":  "BASIC PANEL",
        "tag":   "All Safe Features",
        "desc":  "Aimbot · ESP · Safe",
        "image": os.path.join(BASE_DIR, "basic.png"),
        "badge": "FREE",
    },
    {
        "id":    "bypass",
        "name":  "BYPASS",
        "tag":   "Anti-Detection",
        "desc":  "AntiB · HWID Spoof",
        "image": os.path.join(BASE_DIR, "bypass.png"),
        "badge": "PRO",
    },
    {
        "id":    "internal",
        "name":  "INTERNAL",
        "tag":   "Injected Module",
        "desc":  "Full Feature · Internal",
        "image": os.path.join(BASE_DIR, "internal.png"),
        "badge": "VIP",
    },
    {
        "id":    "silent",
        "name":  "SILENT EXE",
        "tag":   "Stealth Mode",
        "desc":  "Silent Aim · No Recoil",
        "image": os.path.join(BASE_DIR, "silent.png"),
        "badge": "VIP",
    },
]

BADGE_COLORS = {
    "FREE":  ("#00C853", "#00A040"),
    "PRO":   ("#E60000", "#AA0000"),
    "VIP":   ("#9B59B6", "#7D3C98"),
}


class AppCard(QWidget):
    """Single application card with image, labels, and selection glow."""

    clicked_signal = pyqtSignal(dict)

    def __init__(self, app_data, parent=None):
        super().__init__(parent)
        self.app_data = app_data
        self.setFixedSize(170, 210)
        self.setCursor(Qt.PointingHandCursor)
        self._selected = False
        self._hover = False
        self._pixmap = None

        img_path = app_data.get("image", "")
        if os.path.exists(img_path):
            px = QPixmap(img_path)
            if not px.isNull():
                self._pixmap = px.scaled(170, 130, Qt.KeepAspectRatioByExpanding, Qt.SmoothTransformation)

    def set_selected(self, val):
        self._selected = val
        self.update()

    def enterEvent(self, e):
        self._hover = True
        self.update()

    def leaveEvent(self, e):
        self._hover = False
        self.update()

    def mousePressEvent(self, e):
        if e.button() == Qt.LeftButton:
            self.clicked_signal.emit(self.app_data)

    def paintEvent(self, e):
        th = theme()
        p = QPainter(self)
        p.setRenderHint(QPainter.Antialiasing)

        # Card background
        path = QPainterPath()
        path.addRoundedRect(0, 0, self.width(), self.height(), 14, 14)

        # Border glow when selected/hovered
        if self._selected:
            pen_color = QColor("#E60000")
            p.setPen(QPen(pen_color, 2.5))
            glow = QColor(230, 0, 0, 50)
        elif self._hover:
            pen_color = QColor("#555566")
            p.setPen(QPen(pen_color, 1.5))
            glow = QColor(80, 80, 100, 30)
        else:
            pen_color = QColor(th["border"])
            p.setPen(QPen(pen_color, 1))
            glow = QColor(0, 0, 0, 0)

        p.setBrush(QBrush(QColor(th["card"])))
        p.drawPath(path)

        # Image area (top 130px)
        if self._pixmap:
            img_path2 = QPainterPath()
            img_path2.addRoundedRect(0, 0, self.width(), 130, 14, 14)
            img_path2.addRect(0, 100, self.width(), 30)
            p.setClipPath(img_path2)
            p.drawPixmap(0, 0, self._pixmap)
            p.setClipping(False)

            # Gradient overlay on image
            grad = QLinearGradient(0, 80, 0, 130)
            grad.setColorAt(0, QColor(0, 0, 0, 0))
            grad.setColorAt(1, QColor(th["card"]))
            p.fillRect(0, 80, self.width(), 50, grad)
        else:
            # Fallback placeholder
            p.fillRect(0, 0, self.width(), 130, QColor("#1A1A24"))
            p.setPen(QColor(th["dim"]))
            p.setFont(bold_font(9))
            p.drawText(0, 0, self.width(), 130, Qt.AlignCenter, "NO IMAGE")

        # Badge
        badge = self.app_data.get("badge", "")
        if badge in BADGE_COLORS:
            c1, c2 = BADGE_COLORS[badge]
            bw, bh = 38, 18
            bx, by = self.width() - bw - 8, 8
            badge_rect = QPainterPath()
            badge_rect.addRoundedRect(bx, by, bw, bh, 5, 5)
            bg = QLinearGradient(bx, 0, bx + bw, 0)
            bg.setColorAt(0, QColor(c1))
            bg.setColorAt(1, QColor(c2))
            p.setBrush(QBrush(bg))
            p.setPen(Qt.NoPen)
            p.drawPath(badge_rect)
            p.setPen(QColor("white"))
            p.setFont(bold_font(7))
            p.drawText(int(bx), int(by), bw, bh, Qt.AlignCenter, badge)

        # Name
        p.setPen(QColor(th["text"]))
        p.setFont(bold_font(10))
        p.drawText(10, 134, self.width() - 20, 22, Qt.AlignLeft | Qt.AlignVCenter,
                   self.app_data["name"])

        # Tag
        p.setPen(QColor(th["subtext"]))
        p.setFont(font(7))
        p.drawText(10, 154, self.width() - 20, 16, Qt.AlignLeft | Qt.AlignVCenter,
                   self.app_data["tag"])

        # Desc chips
        tags = self.app_data["desc"].split(" · ")
        chip_x = 10
        for tag in tags:
            cw = len(tag) * 6 + 10
            if chip_x + cw > self.width() - 8:
                break
            cr = QPainterPath()
            cr.addRoundedRect(chip_x, 174, cw, 16, 4, 4)
            p.setBrush(QColor(th["surface"]))
            p.setPen(QPen(QColor(th["border"]), 1))
            p.drawPath(cr)
            p.setPen(QColor(th["dim"]))
            p.setFont(font(6))
            p.drawText(chip_x, 174, cw, 16, Qt.AlignCenter, tag)
            chip_x += cw + 5


class SelectorScreen(QWidget):
    app_selected = pyqtSignal(dict)

    def __init__(self, parent=None):
        super().__init__(parent)
        self._current_index = 1
        self._cards = []
        self._build_ui()
        self._refresh_cards()

    def _build_ui(self):
        th = theme()
        layout = QVBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        layout.setSpacing(0)

        # Title
        title_row = QHBoxLayout()
        title_row.setContentsMargins(0, 0, 0, 8)
        lbl = QLabel("SELECT APPLICATION")
        lbl.setFont(bold_font(11))
        lbl.setStyleSheet(f"color: {th['text']}; letter-spacing: 3px;")
        lbl.setAlignment(Qt.AlignCenter)
        layout.addWidget(lbl)

        sub = QLabel("Choose your module to continue")
        sub.setFont(font(8))
        sub.setStyleSheet(f"color: {th['subtext']}; letter-spacing: 1px;")
        sub.setAlignment(Qt.AlignCenter)
        layout.addWidget(sub)

        layout.addSpacing(14)

        # Cards carousel container
        self._carousel = QWidget()
        self._carousel.setFixedHeight(220)
        self._carousel_layout = QHBoxLayout(self._carousel)
        self._carousel_layout.setContentsMargins(10, 0, 10, 0)
        self._carousel_layout.setSpacing(14)
        self._carousel_layout.setAlignment(Qt.AlignCenter)

        # Build cards
        for app in APPS:
            card = AppCard(app)
            card.clicked_signal.connect(self._card_clicked)
            self._cards.append(card)
            self._carousel_layout.addWidget(card)

        layout.addWidget(self._carousel)
        layout.addSpacing(16)

        # Navigation row
        nav_row = QHBoxLayout()
        nav_row.setContentsMargins(20, 0, 20, 0)
        nav_row.setSpacing(16)

        self._btn_prev = self._make_nav_btn("◀")
        self._btn_prev.clicked.connect(self._go_prev)

        self._btn_select = FearButton("SELECT", variant="primary")
        self._btn_select.setFixedHeight(44)
        self._btn_select.clicked.connect(self._confirm_selection)

        self._btn_next = self._make_nav_btn("▶")
        self._btn_next.clicked.connect(self._go_next)

        nav_row.addWidget(self._btn_prev)
        nav_row.addWidget(self._btn_select, 1)
        nav_row.addWidget(self._btn_next)
        layout.addLayout(nav_row)

        layout.addSpacing(12)

        # Footer
        footer = QLabel("WWW.FEARMODS.XYZ")
        footer.setAlignment(Qt.AlignCenter)
        footer.setFont(font(8))
        footer.setStyleSheet(f"color: {th['dim']}; letter-spacing: 2px;")
        layout.addWidget(footer)
        layout.addSpacing(24)

    def _make_nav_btn(self, text):
        th = theme()
        btn = QPushButton(text)
        btn.setFixedSize(44, 44)
        btn.setCursor(Qt.PointingHandCursor)
        btn.setFont(bold_font(12))
        btn.setStyleSheet(f"""
            QPushButton {{
                background: {th['surface']};
                color: {th['subtext']};
                border: 2px solid {th['border']};
                border-radius: 22px;
            }}
            QPushButton:hover {{
                background: {th['card']};
                color: {th['accent']};
                border-color: {th['accent']};
            }}
        """)
        return btn

    def _refresh_cards(self):
        """Show only 3 cards centered on _current_index with scale effect."""
        for i, card in enumerate(self._cards):
            card.hide()

        visible_indices = []
        n = len(APPS)
        for offset in [-1, 0, 1]:
            idx = (self._current_index + offset) % n
            visible_indices.append((offset, idx))

        for offset, idx in visible_indices:
            card = self._cards[idx]
            card.show()
            is_center = (offset == 0)
            card.set_selected(is_center)
            card.setFixedSize(170 if is_center else 145, 210 if is_center else 185)
            op = 1.0 if is_center else 0.6
            card.setGraphicsEffect(None)
            if is_center:
                glow_shadow(card, color="#E60000", blur=30, offset=(0, 4))

        # Update select button text
        self._btn_select.setText(f"SELECT  {APPS[self._current_index]['name']}")

    def _go_prev(self):
        self._current_index = (self._current_index - 1) % len(APPS)
        self._refresh_cards()

    def _go_next(self):
        self._current_index = (self._current_index + 1) % len(APPS)
        self._refresh_cards()

    def _card_clicked(self, app_data):
        idx = next((i for i, a in enumerate(APPS) if a["id"] == app_data["id"]), 0)
        self._current_index = idx
        self._refresh_cards()

    def _confirm_selection(self):
        self.app_selected.emit(APPS[self._current_index])
