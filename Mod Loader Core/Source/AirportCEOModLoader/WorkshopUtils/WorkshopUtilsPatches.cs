using AirportCEOModLoader.Core;
using HarmonyLib;
using LapinerTools.Steam.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.WorkshopUtils;

[HarmonyPatch]
public static class WorkshopUtilsPatches
{
    [HarmonyPatch(typeof(ModManager), "QueueMods")]
    [HarmonyPostfix]
    public static void GetMods(string path)
    {
        try
        {
            string[] directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                foreach (var subFolderOption in WorkshopUtils.subFoldersToLookFor.Keys)
                {
                    if (!directory.SafeSubstring(directory.Length - subFolderOption.Length, directory.Length).Equals(subFolderOption))
                    {
                        continue;
                    }

                    WorkshopUtils.subFoldersToLookFor[subFolderOption](directory);
                }
            }
        }
        catch (Exception ex)
        {
            AirportCEOModLoader.ModLoaderLogger.LogError($"Error occurred in GetMods function. {ExceptionUtils.ProccessException(ex)}");
        }
    }

    [HarmonyPatch(typeof(ModManager), "InitalizeSteamWorkshopMods")]
    [HarmonyPrefix]
    public static void GetAllWorkshopMods(WorkshopItemList itemList)
    {
        WorkshopUtils.workshopItems = itemList;
    }
}

