using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace AirportCEOModLoader.SaveLoadUtils;

[HarmonyPatch]
public static class CoroutineEventDispatcher
{
    public enum CorountineAttachmentType
    {
        Before,
        After,
        BeforeIteration,
        AfterIteration,
    }

    private class AttachmentListWrapper
    {
        internal Queue<Func<IEnumerator>> _beforeList = new();
        internal Queue<Func<IEnumerator>> _afterList = new();
        internal Queue<Func<IEnumerator>> _beforeIterationList = new();
        internal Queue<Func<IEnumerator>> _afterIterationList = new();
    }


    private static bool useNewTextUpdater = false;
    public static Action<string, int> GetTextUpdater()
    {
        if (!useNewTextUpdater)
        {
            return BusinessController.Instance.businessLoader.setLoadingSlider;
        }

        return Singleton<SceneMessagePanelUI>.Instance.SetLoadingText;
    }

    public static void RegisterToWorkshopPhase(Func<IEnumerator> enumerator, CorountineAttachmentType attachmentType)
    {
        switch (attachmentType)
        {
            case CorountineAttachmentType.Before:
                mainmMenuWrapper._beforeList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.After:
                mainmMenuWrapper._afterList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.BeforeIteration:
                mainmMenuWrapper._beforeIterationList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.AfterIteration:
                mainmMenuWrapper._afterIterationList.Enqueue(enumerator);
                return;
        }
    }
    public static void RegisterToLaunchGamePhase(Func<IEnumerator> enumerator, CorountineAttachmentType attachmentType)
    {
        switch (attachmentType)
        {
            case CorountineAttachmentType.Before:
                launchGameWrapper._beforeList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.After:
                launchGameWrapper._afterList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.BeforeIteration:
                launchGameWrapper._beforeIterationList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.AfterIteration:
                launchGameWrapper._afterIterationList.Enqueue(enumerator);
                return;
        }
    }
    public static void RegisterToLoadGamePhase(Func<IEnumerator> enumerator, CorountineAttachmentType attachmentType)
    {
        switch (attachmentType)
        {
            case CorountineAttachmentType.Before:
                loadGameWrapper._beforeList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.After:
                loadGameWrapper._afterList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.BeforeIteration:
                loadGameWrapper._beforeIterationList.Enqueue(enumerator);
                return;
            case CorountineAttachmentType.AfterIteration:
                loadGameWrapper._afterIterationList.Enqueue(enumerator);
                return;
        }
    }

    private static AttachmentListWrapper mainmMenuWrapper = new();
    [HarmonyPatch(typeof(BusinessController), nameof(BusinessController.LoadQueuedObjects))]
    [HarmonyPostfix]
    internal static void MainMenuOverridePatch(ref IEnumerator __result)
    {
        __result = EnumeratorCreator(mainmMenuWrapper, __result);
    }


    private static AttachmentListWrapper launchGameWrapper = new();
    [HarmonyPatch(typeof(GameController), nameof(GameController.LaunchGame))]
    [HarmonyPostfix]
    internal static void LaunchGameOverridePatch(ref IEnumerator __result)
    {
        useNewTextUpdater = true;
        Singleton<SceneMessagePanelUI>.Instance.EnableSliderAndText();
        __result = EnumeratorCreator(launchGameWrapper, __result);
    }


    private static AttachmentListWrapper loadGameWrapper = new();
    [HarmonyPatch(typeof(SaveLoadGameDataController), nameof(SaveLoadGameDataController.LoadGameDataCoroutine))]
    [HarmonyPostfix]
    internal static void LoadGameOverridePatch(ref IEnumerator __result)
    {
        __result = EnumeratorCreator(loadGameWrapper, __result);
    }

    private static IEnumerator EnumeratorCreator(AttachmentListWrapper attachmentListWrapper, IEnumerator originalEnumerator)
    {
        foreach (Func<IEnumerator> beforeEnumerator in attachmentListWrapper._beforeList)
        {
            foreach (Func<IEnumerator> beforeIterationEnumerator in attachmentListWrapper._beforeIterationList)
            {
                yield return AirportCEOModLoader.Instance.StartCoroutine(beforeIterationEnumerator());
            }
            yield return AirportCEOModLoader.Instance.StartCoroutine(beforeEnumerator());
            foreach (Func<IEnumerator> afterIterationEnumerator in attachmentListWrapper._afterIterationList)
            {
                yield return AirportCEOModLoader.Instance.StartCoroutine(afterIterationEnumerator());
            }
        }
        while (originalEnumerator.MoveNext())
        {
            foreach (Func<IEnumerator> beforeIterationEnumerator in attachmentListWrapper._beforeIterationList)
            {
                yield return AirportCEOModLoader.Instance.StartCoroutine(beforeIterationEnumerator());
            }

            yield return originalEnumerator.Current;

            foreach (Func<IEnumerator> afterIterationEnumerator in attachmentListWrapper._afterIterationList)
            {
                yield return AirportCEOModLoader.Instance.StartCoroutine(afterIterationEnumerator());
            }
        }
        foreach (Func<IEnumerator> afterEnumerator in attachmentListWrapper._afterList)
        {
            foreach (Func<IEnumerator> beforeIterationEnumerator in attachmentListWrapper._beforeIterationList)
            {
                yield return AirportCEOModLoader.Instance.StartCoroutine(beforeIterationEnumerator());
            }
            yield return AirportCEOModLoader.Instance.StartCoroutine(afterEnumerator());
            foreach (Func<IEnumerator> afterIterationEnumerator in attachmentListWrapper._afterIterationList)
            {
                yield return AirportCEOModLoader.Instance.StartCoroutine(afterIterationEnumerator());
            }
        }
    }
}
