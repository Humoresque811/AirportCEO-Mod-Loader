using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.Core;

[HarmonyPatch]
public static class EventDispatcher
{
    /// <summary> Called when a new game is started.</summary>
    public static event Action<SaveLoadGameDataController> NewGameStarted;

    /// <summary> Called when the games load cycle begins.</summary>
    public static event Action<SaveLoadGameDataController> LoadStarted;

    /// <summary> Called when the games load cycle ends, but before the IEnumerator returns.</summary>
    public static event Action<SaveLoadGameDataController> EndOfLoad;

    /// <summary> Called when the games save cycle begins.</summary>
    public static event Action<SaveLoadGameDataController> SaveStarted;

    /// <summary> Called when the games save cycle ends, but before the IEnumerator returns.</summary>
    public static event Action<SaveLoadGameDataController> EndOfSave;

    // The ugly stuff below...

    [HarmonyPatch(typeof(SaveLoadGameDataController), "StartNewGame")]
    [HarmonyPrefix]
    public static void NewGamePatch(SaveLoadGameDataController __instance)
    {
        if (NewGameStarted == null)
        {
            return;
        }

        NewGameStarted(__instance);
    }
    [HarmonyPatch(typeof(SaveLoadGameDataController), "LoadGameDataCoroutine")]
    [HarmonyPrefix]
    public static void LoadGamePatch(SaveLoadGameDataController __instance)
    {
        if (LoadStarted == null)
        {
            return;
        }
        LoadStarted(__instance);
    }
    [HarmonyPatch(typeof(SaveLoadGameDataController), "LoadGameDataCoroutine", MethodType.Enumerator)]
    [HarmonyPostfix]
    public static void LoadGameAfterPatch(SaveLoadGameDataController __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (EndOfLoad == null)
        {
            return;
        }

        EndOfLoad(__instance);
    }
    [HarmonyPatch(typeof(SaveLoadGameDataController), "SaveGameData")]
    [HarmonyPrefix]
    public static void SaveGamePatch(SaveLoadGameDataController __instance)
    {
        if (SaveStarted == null)
        {
            return;
        }

        SaveStarted(__instance);
    }
    [HarmonyPatch(typeof(SaveLoadGameDataController), "SaveGameData", MethodType.Enumerator)]
    [HarmonyPostfix]
    public static void SaveGameAfterPatch(SaveLoadGameDataController __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (EndOfSave == null)
        {
            return;
        }

        EndOfSave(__instance);
    }
}
