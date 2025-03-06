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
    public static ConfigEntry<string> devKey { get; private set; }
    public static ConfigEntry<float> additionalUIScaleIncrease { get; private set; }

    public static bool stopWorkshopRepublishing = true;

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
        devKey = Config.Bind("General", "Enter dev key", "", "It does something");
        devKey.SettingChanged += OnDevKeyUpdate;
        additionalUIScaleIncrease = Config.Bind("General", "Increase UI Scale", 0f, 
            new ConfigDescription("Increases UI scale even more on very high res screens", new AcceptableValueRange<float>(0f, 2f)));
        additionalUIScaleIncrease.SettingChanged += Core.Tweaks.UIRescale.SetNewValue;
    }

    private void OnDevKeyUpdate(object sender, EventArgs e)
    {
        if (devKey.Value.Equals("RandomKey"))
        {
            ModLoaderLogger.LogInfo("Disabled workshop republish blocker");
            stopWorkshopRepublishing = false;
        }
        else
        {
            stopWorkshopRepublishing = true;
        }
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
