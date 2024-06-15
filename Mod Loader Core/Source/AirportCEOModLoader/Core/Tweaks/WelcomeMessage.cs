using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch]
public class WelcomeMessage
{
    [HarmonyPatch(typeof(UpdatePanelUI), nameof(UpdatePanelUI.DisplayOnlyUpdateButtons))]
    [HarmonyPostfix]
    public static void ShowMessages()
    {
        if (AirportCEOModLoader.showWelcomeMessage.Value)
        {
            DialogUtils.QueueDialog(MLLocalization.Loc("Welcome_M1"));
            DialogUtils.QueueDialog(MLLocalization.Loc("Welcome_M2"));
            DialogUtils.QueueDialog(MLLocalization.Loc("Welcome_M3"));
            AirportCEOModLoader.showWelcomeMessage.Value = false;
        }
    }
}
