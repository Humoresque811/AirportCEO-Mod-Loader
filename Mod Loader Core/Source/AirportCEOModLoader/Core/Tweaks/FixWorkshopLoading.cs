using HarmonyLib;
using LapinerTools.Steam.Data;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch(typeof(SteamWorkshopManager))]
static class FixWorkshopLoading
{
    // This code is from Zekew (all credits to him), and is needed till a game patch!


    static uint page = 1U; // 1-indexed so initilize as 1                // 
    static WorkshopItemList workshopItemList = new WorkshopItemList();   // OnAvailableItemsCallCompleted is iterated now, so this all needs to be external

    const float QueryTimeoutSeconds = 30f;
    static int queryRun = 0;
    static bool queryFinished = false;

    [HarmonyPatch("InitializeWorkshop")]
    [HarmonyPrefix]
    public static bool Patch_InitializeWorkshop()          // replace InitilizeWorkshop() with InitilizeWorkshopAtPage(1U)
    {
        queryFinished = false;
        queryRun++;
        AirportCEOModLoader.Instance.StartCoroutine(WaitForWorkshopQuery(queryRun));
        InitilizeWorkshopAtPage(1U);
        return false;
    }
        
    private static void InitilizeWorkshopAtPage(uint page = 1U)
    {
        SteamAPICall_t hAPICall =
            SteamUGC.SendQueryUGCRequest(
                SteamUGC.CreateQueryUserUGCRequest(
                    SteamUser.GetSteamID().GetAccountID(),
                    EUserUGCList.k_EUserUGCList_Subscribed,
                    EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items,
                    EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc,
                    AppId_t.Invalid,
                    SteamUtils.GetAppID(),
                    page));// -----------------------------Changed from 1U

        CallResult<SteamUGCQueryCompleted_t>.Create(new CallResult<SteamUGCQueryCompleted_t>.APIDispatchDelegate(OnAvailableItemsCallCompleted)).Set(hAPICall, null);
    }

    private static void OnAvailableItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
    {
        if (queryFinished)
        {
            return;
        }
        //WorkshopItemList workshopItemList = new WorkshopItemList()      //
        //{                                                               //
        //    PagesItems = SteamWorkshopManager.GetPageCount(p_callback)  // Is this important?
        //};                                                              //

        uint num;
        for (num = 0U; num < p_callback.m_unNumResultsReturned; num += 1U)
        {
            SteamUGCDetails_t p_itemDetails;
            if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, num, out p_itemDetails))
            {
                WorkshopItem workshopItem = SteamWorkshopManager.ParseItem(p_callback.m_handle, num, p_itemDetails);
                if (!SteamUGC.DownloadItem(workshopItem.SteamNative.m_nPublishedFileId, true))
                {
                    AirportCEOModLoader.ModLoaderLogger.LogWarning("Could not update workshop item: " + workshopItem.Name);
                }
                workshopItemList.Items.Add(workshopItem);
            }
        }
        if (num >= 50U) //API returns max 50 results at a time, so if 50 ther might be more pages...
        {
            page++;
            InitilizeWorkshopAtPage(page);
        }
        else   // else this is the last page
        {
            queryFinished = true;
            ModManager.InitalizeSteamWorkshopMods(workshopItemList); //now external, so contains all pages
        }
    }

    private static IEnumerator WaitForWorkshopQuery(int run)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < QueryTimeoutSeconds)
        {
            if (queryFinished || run != queryRun)
            {
                yield break;
            }
            yield return null;
        }
        if (queryFinished || run != queryRun)
        {
            yield break;
        }
        queryFinished = true;
        WorkshopItemList installedItems = GetInstalledWorkshopItems();
        AirportCEOModLoader.ModLoaderLogger.LogWarning($"Steam did not send the subscribed workshop items within {QueryTimeoutSeconds} seconds. Loading {installedItems.Items.Count} installed workshop items from disk instead.");
        ModManager.InitalizeSteamWorkshopMods(installedItems);
    }

    private static WorkshopItemList GetInstalledWorkshopItems()
    {
        WorkshopItemList installedItems = new WorkshopItemList();
        try
        {
            uint count = SteamUGC.GetNumSubscribedItems();
            PublishedFileId_t[] ids = new PublishedFileId_t[count];
            count = Math.Min(count, SteamUGC.GetSubscribedItems(ids, count));
            for (uint i = 0U; i < count; i++)
            {
                if (SteamUGC.GetItemInstallInfo(ids[i], out ulong _, out string folder, 1024U, out uint _) && !string.IsNullOrEmpty(folder))
                {
                    installedItems.Items.Add(new WorkshopItem
                    {
                        SteamNative = new WorkshopItem.SteamNativeData(ids[i]),
                        Name = "",
                        IsSubscribed = true,
                        IsInstalled = true,
                        InstalledLocalFolder = folder
                    });
                }
            }
        }
        catch (Exception ex)
        {
            AirportCEOModLoader.ModLoaderLogger.LogError("Could not read the installed workshop items. " + ExceptionUtils.ProccessException(ex));
        }
        return installedItems;
    }
}
