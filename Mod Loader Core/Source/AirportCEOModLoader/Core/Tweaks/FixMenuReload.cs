using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace AirportCEOModLoader.Core.Tweaks;

[HarmonyPatch]
internal static class FixMenuReload
{
    private class SingletonFields
    {
        internal string Name;
        internal FieldInfo Initialized;
        internal FieldInfo Instance;
    }

    private static List<SingletonFields> singletons;

    [HarmonyPatch(typeof(BusinessController), nameof(BusinessController.ResetForMainMenu))]
    [HarmonyPostfix]
    internal static void ForgetPreviousLoad()
    {
        SaveLoadGameDataController.loadComplete = false;
    }

    [HarmonyPatch(typeof(GameController), "Awake")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    internal static void ResetStaleSingletons()
    {
        singletons ??= CollectSingletons();
        List<string> reset = new();
        foreach (SingletonFields singleton in singletons)
        {
            if (singleton.Initialized.GetValue(null) is not true)
            {
                continue;
            }
            if (singleton.Instance.GetValue(null) is UnityEngine.Object instance && instance != null)
            {
                continue;
            }
            singleton.Instance.SetValue(null, null);
            singleton.Initialized.SetValue(null, false);
            reset.Add(singleton.Name);
        }
        if (reset.Count > 0)
        {
            AirportCEOModLoader.ModLoaderLogger.LogInfo($"Reset singleton cache for controllers that no longer exist: {string.Join(", ", reset)}");
        }
    }

    private static List<SingletonFields> CollectSingletons()
    {
        List<SingletonFields> found = new();
        HashSet<Type> seen = new();
        foreach (Type type in AccessTools.GetTypesFromAssembly(typeof(GameController).Assembly))
        {
            Type owner = type.BaseType;
            if (owner == null || !owner.IsGenericType || owner.GetGenericTypeDefinition() != typeof(Singleton<>) || !seen.Add(owner))
            {
                continue;
            }
            FieldInfo initialized = AccessTools.Field(owner, "initialized");
            FieldInfo instance = AccessTools.Field(owner, "instance");
            if (initialized == null || instance == null)
            {
                continue;
            }
            found.Add(new SingletonFields { Name = type.Name, Initialized = initialized, Instance = instance });
        }
        return found;
    }
}
