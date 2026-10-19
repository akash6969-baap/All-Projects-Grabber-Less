# -*- mode: python ; coding: utf-8 -*-


a = Analysis(
    ['FearMods.py'],
    pathex=[],
    binaries=[],
    datas=[('internal.py', '.'), ('injector.py', '.'), ('keyauth.py', '.'), ('Memory.py', '.'), ('connection.py', '.'), ('offset.py', '.'), ('Succes.wav', '.')],
    hiddenimports=['pyqt5', 'pymem', 'requests', 'psutil', 'win32gui', 'win32process', 'win32security', 'flask', 'flask_cors'],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
    optimize=0,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='SilentAim',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
