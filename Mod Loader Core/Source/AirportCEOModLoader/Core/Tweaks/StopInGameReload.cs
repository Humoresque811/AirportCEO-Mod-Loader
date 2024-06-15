using AirportCEOModLoader.SaveLoadUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.Core.Tweaks;

internal class StopInGameReload
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

        GameMenuPanelUI.Instance.loadGameButton.interactable = false;
        HoverToolTip hoverToolTip;
        if (!GameMenuPanelUI.Instance.loadGameButton.TryGetComponent(out hoverToolTip))
        {
            hoverToolTip = GameMenuPanelUI.Instance.loadGameButton.gameObject.AddComponent<HoverToolTip>();
        }

        //hoverToolTip.textColor = UnityEngine.Color.white;
        //hoverToolTip.panelColor = UnityEngine.Color.black;
        hoverToolTip.showPanel = true;

        hoverToolTip.headerToDisplay = MLLocalization.Loc("Tooltip_Stop-Load_Header");
        hoverToolTip.textToDisplay = MLLocalization.Loc("Tooltip_Stop-Load_Text");
    }
}
