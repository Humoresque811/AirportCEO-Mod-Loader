using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirportCEOModLoader.SaveLoadUtils;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

public class StopBugReports
{
    internal static void Awake()
    {
        EventDispatcher.EndOfLoad += MakeInactive;
    }

    internal static void MakeInactive(SaveLoadGameDataController _)
    {
        if (!AirportCEOModLoader.RestrictMenuActions.Value)
        {
            return;
        }

        GameMenuPanelUI.Instance.reportBugButton.interactable = false;
        HoverToolTip hoverToolTip = GameMenuPanelUI.Instance.reportBugButton.GetComponent<HoverToolTip>();
        hoverToolTip.textToDisplay = MLLocalization.Loc("Tooltip_Stop-Bug_Text");
        hoverToolTip.headerToDisplay = MLLocalization.Loc("Tooltip_Stop-Bug_Header");
    }
}

