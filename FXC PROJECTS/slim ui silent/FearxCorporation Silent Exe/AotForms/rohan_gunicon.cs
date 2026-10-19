using System;
using System.Collections.Generic;
using System.Linq;

namespace AotForms
{
    public static class broken
    {
        private static readonly string[] GunNames;

        static broken()
        {
            GunNames = new string[1500];
            for (int i = 0; i < GunNames.Length; i++)
            {
                GunNames[i] = "";
            }
            GunNames[0] = "";
            GunNames[1] = "\ue005";
            GunNames[2] = "\ue075";
            GunNames[3] = "\ue04e";
            GunNames[4] = "\ue04f";
            GunNames[5] = "\ue059";
            GunNames[6] = "\ue074";
            GunNames[7] = "\ue063";
            GunNames[8] = "\ue061";
            GunNames[9] = "\ue051";
            GunNames[10] = "\ue052";
            GunNames[11] = "\ue079";
            GunNames[12] = "\ue078";
            GunNames[13] = "\ue065";
            GunNames[14] = "\ue07b";
            GunNames[15] = "\ue064";
            GunNames[16] = "\ue04a";
            GunNames[17] = "\ue04b";
            GunNames[18] = "\ue071";
            GunNames[19] = "\ue06f";
            GunNames[20] = "\ue057";
            GunNames[21] = "\ue050";
            GunNames[24] = "\ue07a";
            GunNames[25] = "\ue058";
            GunNames[26] = "\ue070";
            GunNames[27] = "\ue04c";
            GunNames[28] = "\ue077";
            GunNames[29] = "\ue05a";
            GunNames[30] = "\ue06e";
            GunNames[33] = "\ue07d";
            GunNames[34] = "\ue04d";
            GunNames[35] = "\ue076";
            GunNames[39] = "\ue07c";
            GunNames[41] = "\ue05d";
            GunNames[43] = "\ue06b";
            GunNames[45] = "\ue053";
            GunNames[46] = "\ue076";
            GunNames[47] = "\ue07f";
            GunNames[48] = "\ue073";
            GunNames[49] = "\ue06a";
            GunNames[50] = "\ue05b";
            GunNames[51] = "\ue04d";
            GunNames[56] = "\ue060";
            GunNames[57] = "\ue07e";
            GunNames[58] = "\ue062";
            GunNames[65] = "\ue04f";
            GunNames[70] = "\ue07b";
            GunNames[71] = "\ue06f";
            GunNames[72] = "\ue070";
            GunNames[73] = "\ue087";
            GunNames[75] = "\ue054";
            GunNames[86] = "\ue05F";
            GunNames[88] = "\ue06D";
            GunNames[89] = "\ue072";
            GunNames[93] = "\ue066";
            GunNames[100] = "\ue068";
            GunNames[119] = "\ue05d";
            GunNames[150] = "\ue06c";
            GunNames[178] = "\ue078";
            GunNames[179] = "\ue078";
            GunNames[180] = "\ue078";
            GunNames[181] = "\ue05e";
            GunNames[193] = "\ue076";
            GunNames[194] = "\ue076";
            GunNames[195] = "\ue076";
            GunNames[197] = "\ue056";
            GunNames[228] = "\ue06d";
            GunNames[229] = "\ue06d";
            GunNames[230] = "\ue06d";
            GunNames[121] = "\ue061";
            GunNames[120] = "\ue061";
            GunNames[60] = "\ue061";
            GunNames[82] = "\ue075";
            GunNames[81] = "\ue075";
            GunNames[80] = "\ue075";
            GunNames[123] = "\ue06e";
            GunNames[122] = "\ue06e";
            GunNames[61] = "\ue06e";
            GunNames[131] = "\ue07a";
            GunNames[130] = "\ue07a";
            GunNames[67] = "\ue07a";
            GunNames[127] = "\ue079";
            GunNames[126] = "\ue079";
            GunNames[63] = "\ue079";
            GunNames[32] = "\ue069";
        }

        public static List<string> GetAllGunNames()
        {
            return GunNames
                .Where(name => !string.IsNullOrEmpty(name) && name != "Unknown" && !name.StartsWith("Unknown ("))
                .Distinct()
                .ToList();
        }

        public static string GetGunName(short gunId)
        {
            try
            {
                if (gunId == 601) return "\ue080";
                if (gunId == 603) return "\ue086";
                if (gunId == 21001) return "\ue066";
                if (gunId == 21002) return "\ue084";

                if (gunId >= 0 && gunId < GunNames.Length)
                {
                    return GunNames[gunId];
                }

                return $"";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting gun name: {ex.Message}");
                return "Unknown";
            }
        }
    }
}