using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirportCEOModLoader.SaveLoadUtils;
using HarmonyLib;
using UnityEngine.UIElements;

namespace AirportCEOModLoader.Core.Tweaks;

[Harmony]
internal class FixTutorial
{
    [HarmonyPatch(typeof(TutorialController), nameof(TutorialController.InitializeTutorials))]
    [HarmonyPrefix]
    internal static bool PreventEarlyTutorialLoading()
    {
        if (Singleton<SaveLoadGameDataController>.Instance.inputGameLoadSetting != Enums.GameLoadSetting.ContinueGame)
        {
            return true;
        }

        if (CoroutineEventDispatcher.IsGameDataDeserialized)
        {
            return true;
        }

        CoroutineEventDispatcher.OnGameDataDeserialized += InitializeTutorialNew;
        return false;
    }

    private static void InitializeTutorialNew()
    {
        TutorialController.Instance.InitializeTutorials(); // This will call the above patch, however, it will let it through as the second condition is true
		if (Singleton<TutorialController>.Instance.currentBranchType == TutorialController.TutorialBranch.Introduction)
		{
			FrontTutorialUI.Instance.ShowHideFrontTutorialPanel(true);
			FrontTutorialUI.Instance.StartIntroduction();
		}
		else
		{
			FrontTutorialUI.Instance.SetIntroductionCompleted(true);
		}
    }
}
