using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace AirportCEOModLoader.Performance;

[HarmonyPatch]
internal class PerformanceTests
{
    static Stopwatch stopwatch = new Stopwatch();

    public static void DoTests()
    {
        //Vector2 vector2 = new Vector2();
        //Vector2 test1 = Vector2.left;
        //int test2 = 123;
        //int test3 = 92;

        //stopwatch.Start();
        //for (int i = 0; i < 10_000_000; i++)
        //{
        //    vector2 = test1 * test2 * test3;
        //}
        //stopwatch.Stop();
        //AirportCEOModLoader.ModLoaderLogger.LogInfo("Length normal" + stopwatch.ElapsedMilliseconds);
        //stopwatch.Restart();
        //for (int i = 0; i < 10_000_000; i++)
        //{
        //    vector2 = test1 * (test2 * test3);
        //}
        //stopwatch.Stop();
        //AirportCEOModLoader.ModLoaderLogger.LogInfo("Length edit" + stopwatch.ElapsedMilliseconds);
    }

    [HarmonyPatch(typeof(AssetRenderManager), "RenderSingleDataBuffers")]
    public static bool Prefix()
    {
        return false;
    }
}
