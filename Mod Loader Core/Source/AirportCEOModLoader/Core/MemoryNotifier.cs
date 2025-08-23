using AirportCEOModLoader.SaveLoadUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOModLoader.Core;

internal class MemoryNotifier
{
    internal static void Awake()
    {
        //CoroutineEventDispatcher.RegisterToWorkshopPhase(UpdateMemoryText, CoroutineEventDispatcher.CorountineAttachmentType.AfterIteration);
    }

    /*
    internal static IEnumerator UpdateMemoryText()
    {
        float availibleMB = 0;

        try
        {
            using (PerformanceCounter availibleMemory = new PerformanceCounter("Memory", "Available MBytes"))
            {
                availibleMB = availibleMemory.NextValue();
            }

            Singleton<SceneMessagePanelUI>.Instance.SetConsoleText($"You currently have {Math.Round(availibleMB / 1000, 2)}gbs of memory left.");
        }
        catch (Exception ex)
        {
            AirportCEOModLoader.ModLoaderLogger.LogError($"Failed to get memory available. {ExceptionUtils.ProccessException(ex)}");
        }

        yield return null;
    }*/
}
