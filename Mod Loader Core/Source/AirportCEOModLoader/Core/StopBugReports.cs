using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace AirportCEOModLoader.Core;

[HarmonyPatch]
public class StopBugReports
{
    [HarmonyPatch(typeof(GameSettingsPanelUI), "ShowIssueReportingPanel")]
    [HarmonyPrefix]
    public static bool StopBugReport(GameSettingsPanelUI __instance)
    {
        if (!AirportCEOModLoader.stopBugReporting.Value)
        {
            return true;
        }

        DialogPanel.Instance.ShowMessagePanel("Please do not report bugs with code based mods enabled! You can report mod bugs to their respective developers on Airport CEO Forum. " +
            "To report bugs with the game, please disable mods to make sure they are not interfereing.");
        return false;
    }
}

