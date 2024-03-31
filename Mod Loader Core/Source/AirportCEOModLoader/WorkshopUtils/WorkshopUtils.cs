using LapinerTools.Steam.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.WorkshopUtils;

public static class WorkshopUtils
{
    public static Dictionary<string, Action<string>> subFoldersToLookFor { get; private set; }
    public static WorkshopItemList workshopItems;

    public static void Awake()
    {
        subFoldersToLookFor = new Dictionary<string, Action<string>>();
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
        }

        subFoldersToLookFor.Add(subFolderName, callbackWithPath);   
    }
    
    /// <summary>
    /// Deregisters a sub folder, if possible
    /// </summary>
    /// <param name="subFolderName">Sub folder to derigister</param>
    public static void Deregister(string subFolderName, Action<string> callbackWithPath)
    {
        if (!subFoldersToLookFor.ContainsKey(subFolderName))
        {
            return;
        }
        subFoldersToLookFor[subFolderName] -= callbackWithPath;
    }
}

