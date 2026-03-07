using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace AirportCEOModLoader.SaveLoadUtils;

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

    /// <summary> Called when the games save cycle begins, provides path as well</summary>
    public static event Action<SaveLoadGameDataController, string> SaveStartedPath;

    /// <summary> Called when the games save cycle ends, but before the IEnumerator returns.</summary>
    public static event Action<SaveLoadGameDataController> EndOfSave;

    // The ugly stuff below...
    
    internal static void Awake()
    {
        CoroutineEventDispatcher.RegisterToLoadGamePhase(OnLoadStarted, CoroutineEventDispatcher.CoroutineAttachmentType.Before);
        CoroutineEventDispatcher.RegisterToLoadGamePhase(OnLoadEnded, CoroutineEventDispatcher.CoroutineAttachmentType.After);
    }

    internal static IEnumerator OnLoadStarted()
    {
        LoadStarted(SaveLoadGameDataController.Instance);
        yield break;
    }
    internal static IEnumerator OnLoadEnded()
    {
        EndOfLoad(SaveLoadGameDataController.Instance);
        yield break;
    }

    [HarmonyPatch(typeof(SaveLoadGameDataController), "StartNewGame")]
    [HarmonyPrefix]
    internal static void NewGamePatch(SaveLoadGameDataController __instance)
    {
        if (NewGameStarted == null)
        {
            return;
        }

        NewGameStarted(__instance);
    }
    //[HarmonyPatch(typeof(SaveLoadGameDataController), "LoadGameDataCoroutine")]
    //[HarmonyPrefix]
    //internal static void LoadGamePatch(SaveLoadGameDataController __instance)
    //{
    //    if (LoadStarted == null)
    //    {
    //        return;
    //    }
    //    LoadStarted(__instance);
    //}
    //[HarmonyPatch(typeof(SaveLoadGameDataController), "LoadGameDataCoroutine", MethodType.Enumerator)]
    //[HarmonyPostfix]
    //internal static void LoadGameAfterPatch(SaveLoadGameDataController __instance, ref bool __result)
    //{
    //    if (__result)
    //    {
    //        return;
    //    }

    //    if (EndOfLoad == null)
    //    {
    //        return;
    //    }

    //    EndOfLoad(__instance);
    //}
    [HarmonyPatch(typeof(SaveLoadGameDataController), "SaveGameData")]
    [HarmonyPrefix]
    internal static void SaveGamePatch(SaveLoadGameDataController __instance, string savePath)
    {
        if (SaveStarted != null)
        {
            SaveStarted(__instance);
        }
        
        if (SaveStartedPath != null)
        {
            SaveStartedPath(__instance, savePath);
        }
    }
    [HarmonyPatch(typeof(SaveLoadGameDataController), "SaveGameData", MethodType.Enumerator)]
    [HarmonyPostfix]
    internal static void SaveGameAfterPatch(SaveLoadGameDataController __instance, ref bool __result)
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
