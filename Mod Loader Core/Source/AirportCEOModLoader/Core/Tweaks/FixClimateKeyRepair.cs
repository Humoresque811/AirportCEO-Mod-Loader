using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal static class FixClimateKeyRepair
{
    private const string ClimateFolder = "DataFiles/GameData/JSON/Climate/";
    private const string ShippedKey = "\"1h\"";
    private const string SerializedKey = "\"_1h\"";

    [HarmonyPatch(typeof(WeatherController), "GetWeatherSource")]
    [HarmonyPrefix]
    private static bool LoadOne(string dataPath, string weatherSourceName, ref WeatherSource[] __result)
    {
        string folder = Path.Combine(dataPath, ClimateFolder);
        if (!Directory.Exists(folder))
        {
            UnityEngine.Debug.LogError("[WEATHER] ERROR: Could not find climate data folder!");
            __result = null;
            return false;
        }

        __result = Load(Path.Combine(folder, weatherSourceName + ".json"));
        return false;
    }

    [HarmonyPatch(typeof(WeatherController), "GetWeatherSources")]
    [HarmonyPrefix]
    private static bool LoadAll(string dataPath, ref List<WeatherSource[]> __result)
    {
        string folder = Path.Combine(dataPath, ClimateFolder);
        if (!Directory.Exists(folder))
        {
            UnityEngine.Debug.LogError("[WEATHER] ERROR: Could not find climate data folder!");
            __result = null;
            return false;
        }

        try
        {
            string[] files = Directory.GetFiles(folder, "*.json");
            List<WeatherSource[]> sources = new List<WeatherSource[]>(files.Length);
            for (int i = 0; i < files.Length; i++)
            {
                sources.Add(Load(files[i]));
            }

            __result = sources;
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError("[WEATHER] ERROR: Could not load climate data profiles! Stack: " + exception);
            __result = null;
        }

        return false;
    }

    private static WeatherSource[] Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            if (json.Contains(ShippedKey))
            {
                json = json.Replace(ShippedKey, SerializedKey);
            }

            return Utils.CreateFromJSON<Wrapper<WeatherSource>>(json).array;
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError("[WEATHER] ERROR: Could not load climate data profiles! Stack: " + exception);
            return null;
        }
    }
}
