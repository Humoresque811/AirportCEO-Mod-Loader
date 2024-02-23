using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;

namespace AirportCEOModLoader.WatermarkUtils
{
    [HarmonyPatch]
    public static class WatermarkUtilsPatches
    {
        public static TMP_Text WatermarkText { get; private set; }

        [HarmonyPatch(typeof(ApplicationVersionLabelUI), "Awake")]
        [HarmonyPostfix]
        public static void GetWatermarkText(ApplicationVersionLabelUI __instance)
        {
            WatermarkText = __instance.transform.GetComponent<TextMeshProUGUI>();
        }
    }
}
