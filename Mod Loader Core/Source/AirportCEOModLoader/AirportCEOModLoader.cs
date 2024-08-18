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

[BepInPlugin("org.airportceomodloader.humoresque", Name, PluginInfo.PLUGIN_VERSION)]
public class AirportCEOModLoader : BaseUnityPlugin
{
    // Major Consts
    public static string Version => PluginInfo.PLUGIN_VERSION;
    public const string Name = "AirportCEO Mod Loader";

    // Logging, HarmonyX
    public static ManualLogSource ModLoaderLogger { get; private set; }
    public static Harmony Harmony { get; private set; }

    // Config
    public static ConfigEntry<bool> RestrictMenuActions { get; private set; }
    public static ConfigEntry<bool> showWelcomeMessage { get; private set; }

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
        Core.Tweaks.StopInGameReload.Awake();
        Core.Tweaks.StopBugReports.Awake();
        Core.ModUIs.ModUIsManager.Awake();
    }

    private void SetUpConfig()
    {
        RestrictMenuActions = Config.Bind("General", "Restrict Menu Actions", true, MLLocalization.Loc("Config_Restrict-Menu-Actions_Desc"));
        showWelcomeMessage = Config.Bind("General", "Show Welcome Message", true, MLLocalization.Loc("Config_Show-Welcome-Message_Desc"));
    }

    private void Start()
    {
        WatermarkUtils.WatermarkUtils.Register(new WatermarkInfo("ML", PluginInfo.PLUGIN_VERSION, true));
    }

    private void Update()
    {
        Core.DialogUtils.UpdateText();
    }
}
