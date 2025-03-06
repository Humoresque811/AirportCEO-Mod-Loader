using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch]
public class UIRescale
{
    [HarmonyPatch(typeof(GameSettingManager), nameof(GameSettingManager.UiScaler), MethodType.Getter)]
    [HarmonyPostfix]
    public static void ClampPatch(ref float __result)
    {
        __result = GameSettingManager.GameSettings.uiScaler.Clamp(0.5f, 1f) + AirportCEOModLoader.additionalUIScaleIncrease.Value;
    }

    internal static void SetNewValue(object _, EventArgs __)
    {
        GameSettingManager.UiScaler = GameSettingManager.GameSettings.uiScaler.Clamp(0.5f, 1f) + AirportCEOModLoader.additionalUIScaleIncrease.Value;
    }
}
