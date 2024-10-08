﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirportCEOModLoader.Core;
using HarmonyLib;

namespace AirportCEOModLoader.SaveLoadUtils;

[HarmonyPatch]
public static class SaveLoadUtils
{
    private static string loadPath;
    private static string savePath;
    private static float gameTimeWhenSaving = 1f;
     
    internal static void Awake()
    {
        EventDispatcher.NewGameStarted += OnLoadStart;
        EventDispatcher.SaveStartedPath += OnSaveStart;
        EventDispatcher.LoadStarted += LoadProccess;
    }
    private static void OnLoadStart(SaveLoadGameDataController instance)    
    {
        loadPath = instance.savePath;
        AirportCEOModLoader.ModLoaderLogger.LogInfo($"Found loadPath. It is {loadPath}");
    }
    private static void OnSaveStart(SaveLoadGameDataController instance, string path)
    {
        AirportCEOModLoader.ModLoaderLogger.LogInfo($"Got save path! Its \"{path}\"");
        savePath = path;
    }


    // JSON file name, Save Function
    private static readonly Dictionary<string, Func<string>> saveRegistries = new Dictionary<string, Func<string>>();

    // JSON file name, JSON file content
    private static readonly Dictionary<string, string> saveJSONS = [];
    public static void RegisterSave(in string JSONFileName, Func<string> saveFunction)
    {
        saveRegistries.Add(JSONFileName, saveFunction);
    }

    [HarmonyPatch(typeof(SaveLoadGameDataController), "SaveGameData", MethodType.Enumerator)]
    [HarmonyPrefix]
    public static void SaveProccess(SaveLoadGameDataController __instance, ref bool __result)
    {
        try
        {
            // This makes sure there are no more elements to go through (makes sure its a true postfix!)
            if (__result)
            {
                return;
            }
            SaveLoadUtils.savePath = Path.Combine(Utils.GetDefaultSavePath() + "/", savePath);
            AirportCEOModLoader.ModLoaderLogger.LogInfo($"Starting save process. {saveRegistries.Count} mods registered!");

            if (saveRegistries.Count <= 0)
            {
                return; // nothing to do
            }

            // Set
            Singleton<MainInteractionPanelUI>.Instance.EnableDispableSavingTextPanel(true);
            gameTimeWhenSaving = Singleton<TimeController>.Instance.currentSpeed;
            PlayerInputController.SetPlayerControlAllowed(false);
            if (gameTimeWhenSaving != 0f)
            {
                Singleton<TimeController>.Instance.TogglePauseTime();
            }

            // Do
            int counter = 1;
            foreach (var key in saveRegistries.Keys)
            {
                AirportCEOModLoader.ModLoaderLogger.LogMessage($"Getting JSON File for file \"{key}\" ({counter}/{saveRegistries.Count})");
                saveJSONS[key] = saveRegistries[key]();
                AirportCEOModLoader.ModLoaderLogger.LogMessage($"Successfully got JSON File for file \"{key}\" ({counter}/{saveRegistries.Count})");
                counter++;
            }
            CreateAllJSONFiles();

            // Revert
            if (gameTimeWhenSaving != 0f)
            {
                Singleton<TimeController>.Instance.TogglePauseTime();
            }
            PlayerInputController.SetPlayerControlAllowed(true);
            if (gameTimeWhenSaving == 100f)
            {
                Singleton<TimeController>.Instance.InvokeSkipToNextDay();
            }
            Singleton<MainInteractionPanelUI>.Instance.EnableDispableSavingTextPanel(false);
        }
        catch (Exception ex)
        {
            AirportCEOModLoader.ModLoaderLogger.LogError($"An general error occurred while trying to save! {ExceptionUtils.ProccessException(ex)}");
            return;
        }
    }

    private static void CreateAllJSONFiles()
    {
        AirportCEOModLoader.ModLoaderLogger.LogMessage($"Starting creation process for {saveJSONS.Count} JSON file(s)");
        //if (string.IsNullOrEmpty(savePath))
        //{
        //    savePath = Singleton<SaveLoadGameDataController>.Instance.saveName;
        //}
        //if (string.IsNullOrEmpty(savePath))
        //{
        //    AirportCEOModLoader.ModLoaderLogger.LogError("No save path available to create JSON...");
        //    return;
        //}

        string basepath = Singleton<SaveLoadGameDataController>.Instance.GetUserSavedDataSearchPath();
        //if (!string.Equals(savePath.SafeSubstring(0, 2), "C:"))
        //{
        //    savePath = Path.Combine(basepath.Remove(basepath.Length - 1), savePath);
        //}

        if (!Directory.Exists(savePath))
        {
            AirportCEOModLoader.ModLoaderLogger.LogError($"The directory for saving does not exist! The path was \"{savePath}\".");
            return;
        }

        int counter = 1;
        foreach (var JSONFileName in saveJSONS.Keys)
        {
            try
            {
                string filePath = Path.Combine(savePath, $"{JSONFileName}.json");
                AirportCEOModLoader.ModLoaderLogger.LogInfo($"Saving JSON file {JSONFileName} ({counter}/{saveJSONS.Count}) with full path of \"{filePath}\"");

                if (string.IsNullOrEmpty(saveJSONS[JSONFileName]))
                {
                    AirportCEOModLoader.ModLoaderLogger.LogError($"A save file (for mod file name \"{JSONFileName}\") is null or empty... Can't save!");
                    counter++;
                    continue;
                }

                if (File.Exists(filePath))
                {
                    AirportCEOModLoader.ModLoaderLogger.LogInfo("The save file trying to be created already exists! Deleting it!");
                    File.Delete(filePath);
                    AirportCEOModLoader.ModLoaderLogger.LogInfo($"Deleted previous file for mod file name \"{JSONFileName}\"");
                }

                Utils.TryWriteFile(saveJSONS[JSONFileName], filePath, out _);
                AirportCEOModLoader.ModLoaderLogger.LogInfo($"Saved JSON file {JSONFileName} successfully! ({counter}/{saveJSONS.Count})");
            }
            catch (Exception ex)
            {
                AirportCEOModLoader.ModLoaderLogger.LogError($"Failed to save the file. Key: {JSONFileName}, {ExceptionUtils.ProccessException(ex)}");
                continue;
            }
        }
    }


    private static List<string> JSONFileNames = new List<string>();
    private static readonly Dictionary<string, string> JSONFiles = [];
    public static void RegisterLoad(in string JSONFileName)
    {
        JSONFileNames.Add(JSONFileName);
    }

    private static void LoadProccess(SaveLoadGameDataController instance)
    {
        if (string.IsNullOrEmpty(loadPath))
        {
            loadPath = instance.savePath;
        }

        if (!Directory.Exists(loadPath))
        {
            AirportCEOModLoader.ModLoaderLogger.LogError("The save directory does not exist!");
            return;
        }

        if (JSONFileNames.Count <= 0)
        {
            return;
        }

        foreach (var fileName in JSONFileNames)
        {
            string path = Path.Combine(loadPath, $"{fileName}.json");
            if (!File.Exists(path))
            {
                AirportCEOModLoader.ModLoaderLogger.LogWarning($"The {fileName}.json file does not exist. Skipped loading of that JSON file");
                continue;
            }

            try
            {
                JSONFiles[fileName] = Utils.ReadFile(path);
            }
            catch (Exception ex)
            {
                AirportCEOModLoader.ModLoaderLogger.LogError($"Failed to read JSON file. {ExceptionUtils.ProccessException(ex)}");
                continue;
            }
        }
    }

    public static bool TryGetJSONFile(in string JSONFileName, out string content)
    {
        if (JSONFiles.Count == 0 && JSONFileNames.Count > 0)
        {
            LoadProccess(null);
        }

        if (!JSONFiles.TryGetValue(JSONFileName, out string value))
        {
            content = null;
            return false;
        }
        content = value;
        return true;
    }
}
