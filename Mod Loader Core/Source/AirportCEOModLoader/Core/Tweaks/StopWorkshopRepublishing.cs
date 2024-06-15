using HarmonyLib;
using LapinerTools.Steam.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch]
public static class StopWorkshopRepublishing
{
    [HarmonyPatch(typeof(TemplateModePanelUI), "UploadTemplate")]
    [HarmonyPrefix]
    public static bool StopWorkshopModRepublishing(TemplateModePanelUI __instance, TMP_InputField ___templateNameInput)
    {
        if (!AirportCEOModLoader.stopWorkshopRepublishing)
        {
            return true;
        }

        string templateName = ___templateNameInput.text;

        foreach (WorkshopItem workshopItem in WorkshopUtils.WorkshopUtils.workshopItems.Items)
        {
            if (!workshopItem.Name.Equals(templateName))
            {
                continue;
            }

            DialogPanel.Instance.HidePanel();
            DialogUtils.QueueDialog(MLLocalization.Loc("General_Workshop-Republish"));
            return false;
        }

        return true;
    }
}

