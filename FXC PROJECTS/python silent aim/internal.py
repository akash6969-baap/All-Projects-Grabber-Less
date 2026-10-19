from flask_cors import CORS
from flask import *
from keyauth import *


def send_command_to_esp(command: str):
    pipe_path = r'\\.\pipe\esp_pipe'
    try:
        with open(pipe_path, 'w') as pipe:
            pipe.write(command + '\n')
    except FileNotFoundError:
        print("Pipe not found. Make sure the C# ESP app is running.")
    except Exception as e:
        print(f"Pipe error: {e}")

def aimbotvisible():
    send_command_to_esp("aimbotvisible")
    return "Aimbot Initialized Successfully."

def aimbotvisibleoff():
    send_command_to_esp("aimbotvisibleoff")
    return "Aimbot Turned off."
    
def silentaim():
    send_command_to_esp("silentaim")

def aimkill():
    send_command_to_esp("aimkill")

def aimkilloff():
    send_command_to_esp("aimkilloff")

def autoswtich():
    send_command_to_esp("autoswitch")

def aimkilloff():
    send_command_to_esp("autoswitchoff")

def telekill():
    send_command_to_esp("telekil")

def telekilloff():
    send_command_to_esp("telekiloff")

def fastswitch():
    send_command_to_esp("fastswitch")

def fastswitchoff():
    send_command_to_esp("fastswitchoff")

def silentaimoff():
    send_command_to_esp("silentaimoff")

def upplayer():
    send_command_to_esp("upplayer")
    
def upplayeroff():
    send_command_to_esp("upplayeroff")

def enablefunction():
    send_command_to_esp("enablefunction")

def enablefunctionoff():
    send_command_to_esp("enablefunctionoff")

def aimbotrage():
    send_command_to_esp("aimbotrage")
    

def aimbotrageoff():
    send_command_to_esp("aimbotrageoff")

def streamermode():
    send_command_to_esp("streammode")
    return "Streamer mode enabled."

def streamermodeoff():
    send_command_to_esp("streammodeoff")
    return "Streamer mode disabled."

def drawfov():
    send_command_to_esp("drawfov")
    return "Fov Drawn on Screen."

def drawfovoff():
    send_command_to_esp("drawfovoff")
    return "Fov Drawn off from Screen."

def update_fov(value):
    send_command_to_esp(f"aimfov:{value:.1f}")


def espline():
    send_command_to_esp("espline")
    return "Esp line On."

def esplineoff():
    send_command_to_esp("esplineoff")
    return "Esp map off."

def espmini():
    send_command_to_esp("mapon")
    return "Esp map on"

def espminioff():
    send_command_to_esp("mapoff")
    return "Esp map Off."


def espbox():
    send_command_to_esp("espbox")
    return "Esp Box On."

def espboxoff():
    send_command_to_esp("espboxoff")
    return "Esp Box Off."

def espname():
    send_command_to_esp("espname")
    return "Esp Name On."

def espnameoff():
    send_command_to_esp("espnameoff")
    return "Esp Name Off."

def autorefresh():
    send_command_to_esp("autorefresh")
    return "Esp autorefresh On."

def autorefreshoff():
    send_command_to_esp("autorefreshoff")
    return "Esp autorefresh Off."

def esphealth():
    send_command_to_esp("esphealth")
    return "Esp Health On."

def esphealthoff():
    send_command_to_esp("esphealthoff")
    return "Esp Health Off."

def espskeleton():
    send_command_to_esp("espskeleton")

def espskeletonoff():
    send_command_to_esp("espskeletonoff")

def espaimtrack():
    send_command_to_esp("espaimtrack")
    return "Esp Aim Track On."

def espaimtrackoff():
    send_command_to_esp("espaimtrackoff")
    return "Esp Aim Track Off."

def espskeleton():
    send_command_to_esp("espskeleton")
    return "Esp Skeleton On."

def espskeletonoff():
    send_command_to_esp("espskeletonoff")
    return "Esp Skeleton Off."

def norecoil():
    send_command_to_esp("norecoil")
    return "Recoil mode set to 0."

def norecoiloff():
    send_command_to_esp("norecoiloff")
    return "Recoil mode set to normal."


def climbupon():
    send_command_to_esp("climbupon")

def climbupoff():
    send_command_to_esp("climbupoff")


def teleanywhere():
    send_command_to_esp("teleanywhere")

def teleanywhereoff():
    send_command_to_esp("teleanywhereoff")

def shakekillon():
    send_command_to_esp("shakekillon")

def shakekilloff():
    send_command_to_esp("shakekilloff")

def downplayeron():
    send_command_to_esp("downplayeron")

def downplayeroff():
    send_command_to_esp("downplayeroff")

def pullenemyon():
    send_command_to_esp("pullenemyon")

def pullenemyoff():
    send_command_to_esp("pullenemyoff")

def update_pull_strength(value):
    send_command_to_esp(f"pullstrength:{float(value):.2f}")

def flyhackon():
    send_command_to_esp("flyhackon")

def flyhackoff():
    send_command_to_esp("flyhackoff")

def underkillon():
    send_command_to_esp("underkillon")

def underkilloff():
    send_command_to_esp("underkilloff")

def fastreloadon():
    send_command_to_esp("fastreloadon")

def fastreloadoff():
    send_command_to_esp("fastreloadoff")

def speedload():
    send_command_to_esp("speedload")

def mapon():
    send_command_to_esp("mapon")

def mapoff():
    send_command_to_esp("mapoff")

def speedon():
    send_command_to_esp("speedon")
def speedoff():
    send_command_to_esp("speedoff")

def esptimeron():
    send_command_to_esp("esptimeron")
def esptimeroff():
    send_command_to_esp("esptimeroff")

def ignoreknockedon():
    send_command_to_esp("ignoreknockedon")
def ignoreknockedoff():
    send_command_to_esp("ignoreknockedoff")


def wallload():
    send_command_to_esp("wallload")

def wallon():
    send_command_to_esp("wallon")
def walloff():
    send_command_to_esp("walloff")

def glitchfireon():
    send_command_to_esp("gfon")
def glitchfireoff():
    send_command_to_esp("gfoff")

def baseon():
    send_command_to_esp("baseon")
def baseoff():
    send_command_to_esp("baseoff")

def espdistanceon():
    send_command_to_esp("espdistanceon")
def espdistanceoff():
    send_command_to_esp("espdistanceoff")

def closest():
    send_command_to_esp("closest")

def lowhp():
    send_command_to_esp("lowhp")
def target360():
    send_command_to_esp("target360")

def closestcross():
    send_command_to_esp("closestcross")

def unlimitedammoon():
    send_command_to_esp("unlimitedammoon")

def unlimitedammooff():
    send_command_to_esp("unlimitedammooff")

def rapidfireon():
    send_command_to_esp("rapidfireon")

def rapidfireoff():
    send_command_to_esp("rapidfireoff")

def update_rapidfire_speed(value):
    send_command_to_esp(f"rapidfirespeed:{value:.1f}")

def speedinton():
    send_command_to_esp("speedinton")

def speedintoff():
    send_command_to_esp("speedintoff")

def espweaponon():
    send_command_to_esp("espweaponon")

def espweaponoff():
    send_command_to_esp("espweaponoff")

def flyhackx40on():
    send_command_to_esp("flyhackx40on")

def flyhackx40off():
    send_command_to_esp("flyhackx40off")

def spinboton():
    send_command_to_esp("spinboton")

def spinbotoff():
    send_command_to_esp("spinbotoff")

def update_spinspeed(value):
    send_command_to_esp(f"spinspeed:{value:.1f}")

def aimbotragemainson():
    send_command_to_esp("aimbotragemainson")

def aimbotragemainoff():
    send_command_to_esp("aimbotragemainoff")

def set_aim_target_part(part: str):
    valid = ["HEAD", "NECK", "CHEST", "LSHOULDER", "RSHOULDER"]
    part = part.upper()
    if part in valid:
        send_command_to_esp(f"aimtargetpart:{part}")

def update_aimsmooth(value):
    send_command_to_esp(f"aimsmooth:{float(value):.1f}")

def update_smoothness(ms: int):
    send_command_to_esp(f"smoothness:{int(ms)}")
