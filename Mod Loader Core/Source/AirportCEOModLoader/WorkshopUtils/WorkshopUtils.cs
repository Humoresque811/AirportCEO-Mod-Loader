using LapinerTools.Steam.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using AirportCEOModLoader.SaveLoadUtils;
using UnityEngine;

namespace AirportCEOModLoader.WorkshopUtils;

public static class WorkshopUtils
{
    internal static Dictionary<string, Action<string>> subFoldersToLookFor { get; private set; }
    internal static Dictionary<string, List<Func<string, IEnumerator>>> subFoldersToLookForCoroutines { get; private set; }

    public static WorkshopItemList workshopItems;

    internal static Dictionary<string, List<string>> directoryInfoFound = new Dictionary<string, List<string>>();

    internal static void Awake()
    {
        subFoldersToLookFor = new();
        subFoldersToLookForCoroutines = new();
        CoroutineEventDispatcher.RegisterToLaunchGamePhase(CallbackToMods, CoroutineEventDispatcher.CorountineAttachmentType.Before);
    }

    internal static IEnumerator CallbackToMods()
    {
        AirportCEOModLoader.ModLoaderLogger.LogMessage($"Starting callbacks to code based mods from WorkshopUtils");
        yield return null;

        int maxProccessors = directoryInfoFound.Keys.Count;
        int currentCallbackNumber = 0;
        float percentPerProccess = 100f / (float)maxProccessors;
        string message = "Loading Code Based Mods ";
        float currentPercent = 0;

        CoroutineEventDispatcher.GetTextUpdater()(message + $"({currentCallbackNumber}/{maxProccessors})", 5);
        yield return new WaitForSecondsRealtime(0.75f);

        foreach (string subFolderFound in directoryInfoFound.Keys)
        {
            if (subFoldersToLookForCoroutines.ContainsKey(subFolderFound))
            {
                foreach (Func<string, IEnumerator> enumerator in subFoldersToLookForCoroutines[subFolderFound])
                {
                    foreach (string directory in directoryInfoFound[subFolderFound])
                    {
                        yield return AirportCEOModLoader.Instance.StartCoroutine(enumerator(directory));
                    }
                }
            }

            currentPercent += percentPerProccess;
            currentCallbackNumber++;
            CoroutineEventDispatcher.GetTextUpdater()(message + $"({currentCallbackNumber}/{maxProccessors})", Mathf.Clamp(currentPercent, 5f, 100f).RoundToIntLikeANormalPerson());
            yield return new WaitForSecondsRealtime(0.75f);
        }

        CoroutineEventDispatcher.GetTextUpdater()("Finished Loading Code Based Mods", 100);
        if (maxProccessors == 0)
        {
            yield return new WaitForSecondsRealtime(0.75f);
        }
        else
        {
            yield return null;
        }
        AirportCEOModLoader.ModLoaderLogger.LogMessage($"Finished callbacks to code based mods from WorkshopUtils");
    }

    /// <summary>
    /// Calls action given when <paramref name="subFolderName"/> is a sub folder of a mod folder
    /// </summary>
    /// <param name="subFolderName">Sub folder to look for in a mod</param>
    /// <param name="callbackWithPath">The action called when a mod is found with the provided sub folder, with path of mod.</param>
    public static void Register(string subFolderName, Action<string> callbackWithPath)
    {
        if (subFoldersToLookFor.ContainsKey(subFolderName))
        {
            subFoldersToLookFor[subFolderName] += callbackWithPath;
            return;
        }

        subFoldersToLookFor.Add(subFolderName, callbackWithPath);   
    }

    public static void Register(string subFolderName, Func<string, IEnumerator> callbackWithPath)
    {
        if (subFoldersToLookForCoroutines.ContainsKey(subFolderName))
        {
            subFoldersToLookForCoroutines[subFolderName].Add(callbackWithPath);
            return;
        }

        subFoldersToLookForCoroutines[subFolderName] = new List<Func<string, IEnumerator>> { callbackWithPath };
    }

    /// <summary>
    /// Deregisters a sub folder, if possible
    /// </summary>
    /// <param name="subFolderName">Sub folder to derigister</param>
    [Obsolete("There is no need for this method, it is now obsolete", true)]
    public static void Deregister(string subFolderName, Action<string> callbackWithPath)
    {
        if (!subFoldersToLookFor.ContainsKey(subFolderName))
        {
            return;
        }
        subFoldersToLookFor[subFolderName] -= callbackWithPath;
    }
}

