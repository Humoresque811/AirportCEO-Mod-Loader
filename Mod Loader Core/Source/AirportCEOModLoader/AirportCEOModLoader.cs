using AirportCEOModLoader.Core;
using AirportCEOModLoader.WatermarkUtils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Epic.OnlineServices;
using HarmonyLib;
using System;
using UnityEngine.PlayerLoop;

namespace AirportCEOModLoader;

[BepInPlugin("org.airportceomodloader.humoresque", Name, Version)]
public class AirportCEOModLoader : BaseUnityPlugin
{
    // Major Consts
    public const string Version = "1.0.0";
    public const string Name = "AirportCEO Mod Loader";

    // Logging, HarmonyX
    public static ManualLogSource ModLoaderLogger { get; private set; }
    public static Harmony Harmony { get; private set; }

    // Config
    public static ConfigEntry<bool> stopBugReporting { get; private set; }

    public static bool stopWorkshopRepublishing = true; // Can be turned off via the AirportCEO Dev Tools mod, which requires a some random string to work

    private void Awake()
    {
        // Logging, HarmonyX
        ModLoaderLogger = Logger;
        Harmony = new(PluginInfo.PLUGIN_GUID);
        Harmony.PatchAll();

        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        // Config
        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is setting up config.");
        SetUpConfig();
        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} finished setting up config.");

        WorkshopUtils.WorkshopUtils.Awake();
        Core.Loading.UpdateManager.Awake();
        WatermarkUtils.WatermarkUtils.Awake();
        SaveLoadUtils.SaveLoadUtils.Awake();
    }

    private void SetUpConfig()
    {
        stopBugReporting = Config.Bind<bool>("General", "StopBugReports", true, "Prevents you from making bug reports with mods. " +
            "Only report bugs if they occur when mods are NOT installed");
    }

    private void Start()
    {
        WatermarkUtils.WatermarkUtils.Register(new WatermarkInfo(Name, Version, false));
    }

    private void Update()
    {
        DialogUtils.UpdateText();
    }
}
