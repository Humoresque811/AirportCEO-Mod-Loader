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
            DialogUtils.QueueDialog("Thank you for installing the AirportCEO Mod Loader! Configuration for mods can be accessed via the F1 key.");
            DialogUtils.QueueDialog("Please note that to disable a mod, you must unsubscribe from it. Disabling it will not do anything.");
            DialogUtils.QueueDialog("This message will not show up again. To show it again, you must renable the option in the configuration.");
            AirportCEOModLoader.showWelcomeMessage.Value = false;
        }
    }
}
