using HarmonyLib;
using LapinerTools.Steam.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace AirportCEOModLoader.Core;

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
            DialogPanel.Instance.ShowMessagePanel("Please do not re-upload mods found on the workshop back to the workshop. " +
                "This invalidates the amount of time creators put into their mods! If this seems incorrect, please contact Humoresque about how to bypass.");
            return false;
        }

        return true;
    }
}

