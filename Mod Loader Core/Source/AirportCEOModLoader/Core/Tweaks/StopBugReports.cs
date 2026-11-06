using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirportCEOModLoader.SaveLoadUtils;
using HarmonyLib;
using UnityEngine.CrashReportHandler;

namespace AirportCEOModLoader.Core.Tweaks;

public class StopBugReports
{
    internal static void Awake()
    {
        EventDispatcher.EndOfLoad += MakeInactive;
        StopCrashReportUploads();
    }

    private static void StopCrashReportUploads()
    {
        if (!AirportCEOModLoader.RestrictMenuActions.Value)
        {
            return;
        }

        CrashReportHandler.enableCaptureExceptions = false;
        AirportCEOModLoader.ModLoaderLogger.LogInfo("Unity crash report uploads are off while code based mods are loaded.");
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

