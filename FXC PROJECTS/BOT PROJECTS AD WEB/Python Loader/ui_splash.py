"""
ui_splash.py - Circular buffering splash screen for Fear Mods Loader
"""
from PyQt5.QtWidgets import QWidget, QVBoxLayout, QLabel
from PyQt5.QtCore import Qt, QTimer, QRectF, pyqtSignal
from PyQt5.QtGui import QPainter, QPen, QColor, QConicalGradient, QFont, QBrush, QLinearGradient
import math


class SplashScreen(QWidget):
    finished = pyqtSignal()

    def __init__(self, parent=None):
        super().__init__(parent)
        self._angle = 0
        self._alpha = 255
        self._dots = 0
        self._phase = "loading"  # loading | success | fadeout
        self._progress = 0
        self._target_progress = 0
        self._timer = QTimer(self)
        self._timer.timeout.connect(self._tick)
        self._timer.start(16)  # ~60fps

        # Auto-advance loading bar
        self._progress_timer = QTimer(self)
        self._progress_timer.timeout.connect(self._advance_progress)
        self._progress_timer.start(40)

        self._dot_timer = QTimer(self)
        self._dot_timer.timeout.connect(self._advance_dots)
        self._dot_timer.start(400)

    def _advance_dots(self):
        self._dots = (self._dots + 1) % 4
        self.update()

    def _advance_progress(self):
        if self._phase != "loading":
            return
        if self._progress < 95:
            self._progress = min(95, self._progress + 1.2)
            self.update()

    def complete_loading(self):
        """Call this when auth / init is done — fills bar to 100%, shows checkmark."""
        self._progress_timer.stop()
        self._phase = "success"
        self._fill_timer = QTimer(self)
        self._fill_timer.timeout.connect(self._fill_to_100)
        self._fill_timer.start(15)

    def _fill_to_100(self):
        self._progress = min(100, self._progress + 2)
        self.update()
        if self._progress >= 100:
            self._fill_timer.stop()
            QTimer.singleShot(600, self._start_fadeout)

    def _start_fadeout(self):
        self._phase = "fadeout"
        self._fade_timer = QTimer(self)
        self._fade_timer.timeout.connect(self._do_fade)
        self._fade_timer.start(16)

    def _do_fade(self):
        self._alpha = max(0, self._alpha - 12)
        self.update()
        if self._alpha <= 0:
            self._fade_timer.stop()
            self.finished.emit()

    def _tick(self):
        self._angle = (self._angle - 4) % 360
        self.update()

    def paintEvent(self, event):
        w, h = self.width(), self.height()
        p = QPainter(self)
        p.setRenderHint(QPainter.Antialiasing)
        p.setOpacity(self._alpha / 255)

        # Background (rounded bottom corners to match outer frame)
        p.setPen(Qt.NoPen)
        p.setBrush(QColor("#09090D"))
        p.drawRoundedRect(0, 0, w, h, 16, 16)

        cx, cy = w // 2, h // 2 - 30  # Shift up to balance the progress bar/text below
        ring_r = min(w, h) * 0.22
        ring_w = 6

        # Background ring
        p.setPen(QPen(QColor("#1C1C2E"), ring_w, Qt.SolidLine, Qt.RoundCap))
        r = QRectF(cx - ring_r, cy - ring_r, ring_r * 2, ring_r * 2)
        p.drawArc(r, 0, 360 * 16)

        # Spinner arc (conical gradient effect using multiple segments)
        for i in range(120):
            frac = i / 120
            alpha_val = int(30 + frac * 220)
            hue_shift = int(frac * 30)
            color = QColor(230, min(255, hue_shift * 3), hue_shift, alpha_val)
            pen = QPen(color, ring_w, Qt.SolidLine, Qt.RoundCap)
            p.setPen(pen)
            angle_start = (self._angle + i * 3) * 16
            p.drawArc(r, angle_start, 3 * 16)

        if self._phase == "success":
            # Draw checkmark inside ring
            p.setPen(QPen(QColor("#00D04A"), 3, Qt.SolidLine, Qt.RoundCap, Qt.RoundJoin))
            scale = ring_r * 0.4
            x1, y1 = cx - scale * 0.6, cy
            x2, y2 = cx - scale * 0.1, cy + scale * 0.5
            x3, y3 = cx + scale * 0.6, cy - scale * 0.4
            p.drawLine(int(x1), int(y1), int(x2), int(y2))
            p.drawLine(int(x2), int(y2), int(x3), int(y3))
        else:
            # Fear Mods logo text in centre
            p.setPen(QColor("white"))
            p.setFont(QFont("Segoe UI", 9, QFont.Bold))
            p.drawText(int(cx - 35), int(cy - 8), 70, 20, Qt.AlignCenter, "FEAR")
            p.setPen(QColor("#E60000"))
            p.drawText(int(cx - 35), int(cy + 6), 70, 20, Qt.AlignCenter, "MODS")

        # Progress bar
        bar_w = min(w * 0.55, 260)
        bar_h = 4
        bx = cx - bar_w / 2
        by = cy + ring_r + 28
        p.setPen(Qt.NoPen)
        p.setBrush(QColor("#1C1C2E"))
        p.drawRoundedRect(int(bx), int(by), int(bar_w), bar_h, 2, 2)
        fill_w = int(bar_w * self._progress / 100)
        if fill_w > 0:
            grad = QLinearGradient(bx, 0, bx + bar_w, 0)
            grad.setColorAt(0, QColor("#E60000"))
            grad.setColorAt(1, QColor("#FF6666"))
            p.setBrush(QBrush(grad))
            p.drawRoundedRect(int(bx), int(by), fill_w, bar_h, 2, 2)

        # Loading text
        dots = "." * self._dots
        text = "INITIALIZING" + dots if self._phase == "loading" else "ACCESS GRANTED..."
        p.setPen(QColor("#555566"))
        p.setFont(QFont("Segoe UI", 8, QFont.Bold))
        p.drawText(0, int(by + 14), w, 20, Qt.AlignCenter, text)
