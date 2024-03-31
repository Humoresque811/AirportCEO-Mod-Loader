using BepInEx;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOModLoader.Core.Loading;

public static class UpdateManager
{
    public static readonly string PluginFolderName = "plugins";
    public static readonly string PluginFolderAltName = $"{PluginFolderName}1";
    public static readonly string PluginFolderTempName = $"{PluginFolderName}TEMP";

    public static readonly string PatcherFolderName = "patchers";
    public static readonly string PatcherFolderAltName = $"{PatcherFolderName}1";
    public static readonly string PatcherFolderTempName = $"{PatcherFolderName}TEMP";

    public static readonly string VersionFolderName = "version";

    private static bool modNeededUpdate = false;
     
    internal static void Awake()
    {
        WorkshopUtils.WorkshopUtils.Register(PluginFolderName, CheckPluginOnMod);
        //WorkshopUtils.WorkshopUtils.Register(PatcherFolderName, CheckPatcherOnMod);
        //WorkshopUtils.WorkshopUtils.Register(VersionFolderName, CheckVersionOnMod);
    }


    private static void CheckPluginOnMod(string loadedPluginPath)
    {
        MoveBlacklist(loadedPluginPath);
        //string altPluginPath = Path.Combine(modRoot, PluginFolderAltName);

        //if (!Directory.Exists(loadedPluginPath) || !Directory.Exists(altPluginPath))
        //{
        //    AirportCEOModLoader.ModLoaderLogger.LogWarning($"A mods plugin path or alt path does not exist... Paths: {loadedPluginPath}, {altPluginPath}");
        //    return;
        //}

        //if (!RequiresUpdate(loadedPluginPath, altPluginPath))
        //{
        //    AirportCEOModLoader.ModLoaderLogger.LogInfo($"No update needed for mod with path {modRoot}.");
        //    return;
        //}

        //UpdatePlugin(loadedPluginPath, altPluginPath, modRoot);
    }

    private static void MoveBlacklist(string loadedPluginPath)
    {
        if (!SteamManager.Initialized)
        {
            // Not steam instance of game
            return;
        }

        string modRoot = Path.GetFullPath(Path.Combine(loadedPluginPath, ".."));

        string[] files = Directory.GetFiles(modRoot, "*blacklist.txt");

        if (files.Length == 0 || files.Length > 1)
        {
            return;
        }

        AirportCEOModLoader.ModLoaderLogger.LogInfo("Found blacklist.txt! Moving");
        AirportCEOModLoader.ModLoaderLogger.LogInfo($"Moving from {files[0]}");

        try
        {
            if (!File.Exists(files[0]))
            {
                return;
            }

            string existingBlacklist = Path.GetFullPath(Path.Combine(Paths.ExecutablePath, "..", "blacklist.txt"));
            AirportCEOModLoader.ModLoaderLogger.LogInfo($"Moving to {existingBlacklist}");
            if (File.Exists(existingBlacklist))
            {
                File.Delete(existingBlacklist);
            }

            File.Move(files[0], existingBlacklist);
        }
        catch (Exception ex)
        {
            AirportCEOModLoader.ModLoaderLogger.LogError($"Failed to move new blacklist over. {ExceptionUtils.ProccessException(ex)}");
        }

        return;
    }


    //private static void CheckPatcherOnMod(string path) // Planned impl at some point
    //{
    //    return;
    //}   

    //private static void CheckVersionOnMod(string path) // Planned impl at some point
    //{
    //    return;
    //}

    //private static void UpdatePlugin(string loadedPluginPath, string altPluginPath, string modRootPath)
    //{
    //    string tempPath = Path.Combine(modRootPath, PluginFolderTempName);

    //    try
    //    {
    //        //Directory.Move(altPluginPath, tempPath);
    //        //Directory.Move(loadedPluginPath, altPluginPath);
    //        //Directory.Move(tempPath, loadedPluginPath);
    //    }
    //    catch (Exception ex)
    //    {
    //        AirportCEOModLoader.ModLoaderLogger.LogError($"CRITICAL: An error occured when moving in the update system. This could cuase some issues. Please notify Humoresque. " +
    //            $"It is recomended to unsubscribe and resubscribe to the mod with path {modRootPath}. {ExceptionHelper.ProccessException(ex)}");
    //        DialogUtility.QueueDialog("An error occured when moving directories in the update system. Please notify Humoresque. Check logs for details!");
    //        return;
    //    }

    //    AirportCEOModLoader.ModLoaderLogger.LogInfo($"Update succeeded for a mod, please restart game!");
    //    if (!modNeededUpdate)
    //    {
    //        DialogUtility.QueueDialog("One (or more) mods have a new update available. Please restart the game to use the new version!");
    //        modNeededUpdate = true;
    //    }

    //    if (Application.platform != RuntimePlatform.WindowsPlayer)
    //    {
    //    }
    //}

    //private static bool RequiresUpdate(string loadedPluginPath, string altPluginPath)
    //{
    //    string[] filesLoaded = Directory.GetFiles(loadedPluginPath, "*.dll");
    //    string[] filesAlt = Directory.GetFiles(altPluginPath, "*.dll");

    //    if (filesLoaded.Length != filesAlt.Length)
    //    {
    //        return true;
    //    }

    //    FileVersionInfo[] versionLoaded = new FileVersionInfo[filesLoaded.Length];
    //    FileVersionInfo[] versionAlt = new FileVersionInfo[filesAlt.Length];

    //    for (int i = 0; i < filesLoaded.Length; i++)
    //    {
    //        try
    //        {
    //            versionLoaded[i] = FileVersionInfo.GetVersionInfo(filesLoaded[i]);
    //        }
    //        catch (Exception ex)
    //        {
    //            AirportCEOModLoader.ModLoaderLogger.LogError($"Failed to get file version for a dll file. {ExceptionHelper.ProccessException(ex)}");
    //        }
    //    }

    //    for (int i = 0; i < filesAlt.Length; i++)
    //    {
    //        try
    //        {
    //            versionAlt[i] = FileVersionInfo.GetVersionInfo(filesAlt[i]);
    //        }
    //        catch (Exception ex)
    //        {
    //            AirportCEOModLoader.ModLoaderLogger.LogError($"Failed to get file version for a dll file. {ExceptionHelper.ProccessException(ex)}");
    //        }
    //    }

    //    for (int k = 0; k < versionLoaded.Length; k++)
    //    {
    //        if (versionLoaded[k].FileVersion == null || versionAlt[k].FileVersion == null)
    //        {
    //            continue;
    //        }

    //        if (new Version(versionAlt[k].FileVersion) > new Version(versionLoaded[k].FileVersion))
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}
}
