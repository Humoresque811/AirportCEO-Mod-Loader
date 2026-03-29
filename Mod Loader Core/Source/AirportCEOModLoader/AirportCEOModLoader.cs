using AirportCEOModLoader.Core;
using AirportCEOModLoader.SaveLoadUtils;
using AirportCEOModLoader.WatermarkUtils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Epic.OnlineServices;
using HarmonyLib;
using System;
using System.Collections;
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
    internal static AirportCEOModLoader Instance { get; private set; }

    // Config
    public static ConfigEntry<bool> RestrictMenuActions { get; private set; }
    public static ConfigEntry<bool> ShowWelcomeMessage { get; private set; }
    public static ConfigEntry<string> DevKey { get; private set; }
    public static ConfigEntry<float> AdditionalUIScaleIncrease { get; private set; }
    public static ConfigEntry<bool> UpdateModdingMenu { get; private set; }

    public static bool stopWorkshopRepublishing = true;

    private void Awake()
    {
        // Logging, HarmonyX
        ModLoaderLogger = Logger;
        Instance = this;
        Harmony = new(PluginInfo.PLUGIN_GUID);
        Harmony.PatchAll();

        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        // Config
        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is setting up config.");
        SetUpConfig();
        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} finished setting up config.");

        WatermarkUtils.WatermarkUtils.Awake();
        WorkshopUtils.WorkshopUtils.Awake();
        Core.Loading.UpdateManager.Awake();
        SaveLoadUtils.SaveLoadUtils.Awake();
        Core.Tweaks.StopInGameReload.Awake();
        Core.Tweaks.StopBugReports.Awake();
        EventDispatcher.Awake();
        MemoryNotifier.Awake();
        CoroutineEventDispatcher.Awake();
    }

    private void SetUpConfig()
    {
        RestrictMenuActions = Config.Bind("General", "Restrict Menu Actions", true, MLLocalization.Loc("Config_Restrict-Menu-Actions_Desc"));
        ShowWelcomeMessage = Config.Bind("General", "Show Welcome Message", true, MLLocalization.Loc("Config_Show-Welcome-Message_Desc"));
        DevKey = Config.Bind("General", "Enter dev key", "", "It does something");
        DevKey.SettingChanged += OnDevKeyUpdate;
        AdditionalUIScaleIncrease = Config.Bind("General", "Increase UI Scale", 0f, 
            new ConfigDescription("Increases UI scale even more on very high res screens", new AcceptableValueRange<float>(0f, 2f)));
        UpdateModdingMenu = Config.Bind("General", "Update Modding Menu UI", true, "Update the main menu modding panel UI to be more streamlined. " +
            "Keep enabled please... Reload required to take effect.");

        AdditionalUIScaleIncrease.SettingChanged += Core.Tweaks.UIRescale.SetNewValue;
    }

    private void OnDevKeyUpdate(object sender, EventArgs e)
    {
        if (DevKey.Value.Equals("RandomKey"))
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
        Core.ModUIs.ModUIsManager.Awake();
    }

    private void Update()
    {
        Core.DialogUtils.UpdateText();
    }
}
