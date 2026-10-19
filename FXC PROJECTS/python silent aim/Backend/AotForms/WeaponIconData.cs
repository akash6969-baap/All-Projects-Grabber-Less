

namespace AotForms
{
    internal static class WeaponIconData
    {
        internal static Dictionary<uint, string> WeaponIconNames = new Dictionary<uint, string>
        {
         
            { 1, string.Empty }, 
            { 16, nkvt.Pan }, 
            { 17, nkvt.Parang },
            { 27, nkvt.Bat }, 
            { 34, nkvt.Scythe }, 
            { 51, nkvt.Scythe },
            { 66, nkvt.Scythe }, 
            { 68, nkvt.Scythe },
            { 84, nkvt.Scythe }, 
            { 85, nkvt.Parang }, 
            { 87, nkvt.Scythe }, 
            { 132, nkvt.FFKnife }, 
            { 133, nkvt.FFKnife }, 
            { 134, nkvt.FFKnife }, 
            { 135, nkvt.Bat }, 
            { 136, nkvt.Bat }, 
            { 137, nkvt.Pan }, 
            { 138, nkvt.Bat }, 
            { 139, nkvt.Bat }, 
            { 176, nkvt.Scythe }, 
            { 177, nkvt.Bat }, 
            
            // Pistols
            { 3, nkvt.USP }, // USP
            { 9, nkvt.DesertEagle }, // DESERT EAGLE
            { 10, nkvt.G18 }, // G18
            { 20, nkvt.M1873 }, // M1873
            { 25, nkvt.M500 }, // M500
            { 44, nkvt.M500 }, // SHORTY - using M500 icon as fallback
            { 52, nkvt.DesertEagle }, // HAND CANNON - using DesertEagle icon as fallback
            { 53, nkvt.DesertEagle }, // EXECUTIONER - using DesertEagle icon as fallback
            { 55, nkvt.M1917 }, // M1917
            { 56, nkvt.USP2 }, // USP-2
            { 58, nkvt.Uzi }, // MINI UZI
            { 93, nkvt.HealPistol }, // HEAL PISTOL
            { 21001, nkvt.HealPistol }, // HEAL PISTOL-Y
            { 100, nkvt.Flamethrower }, // FLAMETHROWER
            { 101, nkvt.USP }, // FLARE GUN - using USP icon as fallback
            { 147, nkvt.DesertEagle }, // DESERT EAGLE-GOLD
            { 159, nkvt.G18 }, // G18-GOLD
            { 162, nkvt.M500 }, // M500-GOLD
            { 170, nkvt.M1873 }, // M1873-GOLD
            { 187, nkvt.M500 }, // M500-X
            { 188, nkvt.M500 }, // M500-Y
            { 189, nkvt.M500 }, // M500-Z
            { 190, nkvt.Flamethrower }, // FLAMETHROWER-X
            { 191, nkvt.Flamethrower }, // FLAMETHROWER-Y
            { 192, nkvt.Flamethrower }, // FLAMETHROWER-Z
            
            // Snipers
            { 4, nkvt.AWM }, // AWM
            { 65, nkvt.AWM }, // AWM-Y
            { 21, nkvt.Kar98k }, // KAR98K
            { 64, nkvt.Kar98k }, // KAR98K-I
            { 78, nkvt.HealSniper }, // HEAL SNIPER
            { 94, nkvt.AWM }, // AS50 - using AWM icon as fallback
            { 95, nkvt.SVD }, // DRAGUNOV - using SVD icon as fallback
            { 96, nkvt.AWM }, // INTERVENTION - using AWM icon as fallback
            { 97, nkvt.M82B }, // BARRET - using M82B icon as fallback
            { 98, nkvt.AWM }, // L96A1 - using AWM icon as fallback
            { 128, nkvt.Kar98k }, // KAR98K-II
            { 129, nkvt.Kar98k }, // KAR98K-III
            { 45, nkvt.M82B }, // M82B
            { 75, nkvt.M24 }, // M24
            { 142, nkvt.AWM }, // AWM-GOLD
            { 154, nkvt.Kar98k }, // KAR98K-GOLD
            { 165, nkvt.M82B }, // M82B-GOLD
            { 197, nkvt.VSK94 }, // VSK94
            { 198, nkvt.AWM }, // AWM-DRAGON
            
            // Shotguns
            { 5, nkvt.M1014 }, // M1014
            { 29, nkvt.Spas }, // SPAS12
            { 41, nkvt.M1887 }, // M1887
            { 50, nkvt.Mag7 }, // MAG-7
            { 69, nkvt.M1887 }, // DOUBLE BARREL - using M1887 icon as fallback
            { 86, nkvt.ChargeBuster }, // CHARGE BUSTER
            { 119, nkvt.M1887 }, // M1887-X
            { 143, nkvt.M1887 }, // M1887-GOLD
            { 157, nkvt.Spas }, // SPAS12-GOLD
            { 168, nkvt.Mag7 }, // MAG-7-GOLD
            { 173, nkvt.M1014 }, // M1014-GOLD
            { 181, nkvt.Trogon }, // TROGON
            { 182, nkvt.ChargeBuster }, // CHARGE BUSTER-GOLD
            { 183, nkvt.Trogon }, // TROGON-GOLD
            { 184, nkvt.M1014 }, // M1014-I
            { 185, nkvt.M1014 }, // M1014-II
            { 186, nkvt.M1014 }, // M1014-III
            { 21002, nkvt.M590 }, // M590
            
            // SMGs
            { 7, nkvt.UMP }, // UMP
            { 8, nkvt.MP5 }, // MP5
            { 13, nkvt.VSS }, // VSS
            { 15, nkvt.MP40 }, // MP40
            { 32, nkvt.P90 }, // P90
            { 35, nkvt.CG15 }, // CG15
            { 40, nkvt.Vector }, // VIPER - using Vector icon as fallback
            { 42, nkvt.MP5 }, // MP7 - using MP5 icon as fallback
            { 43, nkvt.Thompson }, // THOMPSON
            { 49, nkvt.Vector }, // VECTOR
            { 59, nkvt.P90 }, // P90-X
            { 60, nkvt.MP5 }, // MP5-I
            { 62, nkvt.VSS }, // VSS-I
            { 88, nkvt.Mac10 }, // MAC10
            { 120, nkvt.MP5 }, // MP5-II
            { 121, nkvt.MP5 }, // MP5-III
            { 125, nkvt.VSS }, // VSS-III
            { 141, nkvt.MP5 }, // MP5-GOLD
            { 150, nkvt.Bizon }, // BIZON
            { 151, nkvt.Vector }, // VECTOR-GOLD
            { 152, nkvt.VSS }, // VSS-GOLD
            { 153, nkvt.MP40 }, // MP40-GOLD
            { 155, nkvt.UMP }, // UMP-GOLD
            { 160, nkvt.P90 }, // P90-GOLD
            { 164, nkvt.Thompson }, // THOMPSON-GOLD
            { 166, nkvt.Mac10 }, // MAC10-GOLD
            
            // Machine Guns
            { 19, nkvt.M249 }, // M249
            { 30, nkvt.M60 }, // M60
            { 54, nkvt.M60 }, // KORD - using M60 icon as fallback
            { 61, nkvt.M60 }, // M60-I
            { 71, nkvt.M249 }, // M249-X
            { 122, nkvt.M60 }, // M60-II
            { 123, nkvt.M60 }, // M60-III
            { 146, nkvt.M60 }, // M60-GOLD
            { 158, nkvt.M249 }, // M249-GOLD
            { 172, nkvt.M60 }, // KORD-GOLD
            
            // AR Snipers
            { 18, nkvt.SKS }, // SKS
            { 23, nkvt.M82B }, // AK50 - using M82B icon as fallback
            { 26, nkvt.SVD }, // SVD
            { 31, nkvt.SVD }, // SVG - using SVD icon as fallback
            { 48, nkvt.Woodpecker }, // WOODPECKER
            { 72, nkvt.SVD }, // SVD-Y
            { 89, nkvt.AC80 }, // AC80
            { 126, nkvt.Woodpecker }, // THUMPER - using Woodpecker icon as fallback
            { 161, nkvt.SVD }, // SVD-GOLD
            { 163, nkvt.Woodpecker }, // WOODPECKER-GOLD
            { 167, nkvt.SKS }, // SKS-GOLD
            { 196, nkvt.AC80 }, // AC80-X
            
            // Assault Rifles
            { 2, nkvt.M4A1 }, // M4A1
            { 6, nkvt.AK47 }, // AK-47
            { 11, nkvt.M14 }, // M14
            { 12, nkvt.SCAR }, // SCAR
            { 14, nkvt.Groza }, // GROZA
            { 24, nkvt.FAMAS }, // FAMAS
            { 28, nkvt.XM8 }, // XM8
            { 33, nkvt.AN94 }, // AN94
            { 36, nkvt.G36 }, // G36K - using G36 icon
            { 37, nkvt.M4A1 }, // ACR - using M4A1 icon as fallback
            { 38, nkvt.SCAR }, // SCAR-HEAVY - using SCAR icon
            { 39, nkvt.Plasma }, // PLASMA
            { 46, nkvt.AUG }, // AUG
            { 47, nkvt.ParaFAL }, // PARAFAL
            { 57, nkvt.Kingfisher }, // KINGFISHER
            { 63, nkvt.M14 }, // M14-I
            { 67, nkvt.FAMAS }, // FAMAS-I
            { 70, nkvt.Groza }, // GROZA-X
            { 73, nkvt.G36 }, // G36-RANGE
            { 74, nkvt.G36 }, // G36-ASSAULT
            { 79, nkvt.G36 }, // G36C - using G36 icon
            { 80, nkvt.M4A1 }, // M4A1-I
            { 81, nkvt.M4A1 }, // M4A1-II
            { 82, nkvt.M4A1 }, // M4A1-III
            { 83, nkvt.XM8 }, // XM8-X
            { 99, nkvt.Plasma }, // SHIELD GUN - using Plasma icon as fallback
            { 124, nkvt.M14 }, // M14-II
            { 127, nkvt.M14 }, // M14-III
            { 130, nkvt.FAMAS }, // FAMAS-II
            { 131, nkvt.FAMAS }, // FAMAS-III
            { 140, nkvt.M4A1 }, // M4A1-GOLD
            { 144, nkvt.AK47 }, // AK47-GOLD
            { 145, nkvt.Groza }, // GROZA-GOLD
            { 148, nkvt.SCAR }, // SCAR-GOLD
            { 149, nkvt.M14 }, // M14-GOLD
            { 156, nkvt.XM8 }, // XM8-GOLD
            { 169, nkvt.AN94 }, // AN94-GOLD
            { 171, nkvt.AUG }, // AUG-GOLD
            { 174, nkvt.FAMAS }, // FAMAS-GOLD
            { 175, nkvt.ParaFAL }, // PARAFAL-GOLD
            { 178, nkvt.SCAR }, // SCAR-I
            { 179, nkvt.SCAR }, // SCAR-II
            { 180, nkvt.SCAR }, // SCAR-III
            { 193, nkvt.AUG }, // AUG-I
            { 194, nkvt.AUG }, // AUG-II
            { 195, nkvt.AUG }, // AUG-III
            { 199, nkvt.M4A1 }, // M4A1-DRAGON
            { 200, nkvt.AK47 }, // AK47-DRAGON
            
            // Other equipment
            { 76, nkvt.Grenade }, // GRENADE
            { 77, nkvt.Smoke }, // SMOKE
            { 90, nkvt.Plasma }, // RIOT SHIELD - using Plasma icon as fallback
            { 91, nkvt.Grenade }, // MOLOTOV - using Grenade icon as fallback
            { 92, nkvt.Flashbang }, // FLASHBANG
            { 102, nkvt.GlooWall } // GLOO WALL
        };

        public static string GetIconForWeaponId(uint weaponId)
        {
            if (weaponId == 0)
                return string.Empty;

            if (WeaponIconNames.TryGetValue(weaponId, out string icon))
                return icon;

            return string.Empty;
        }
    }
}
