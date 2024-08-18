using LapinerTools.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AirportCEOModLoader.Core.ModUIs;

public class ModUIsManager
{
    // Found stuff
    public static GameObject ModsPanel;
    internal static GameObject SteamModsBox;
    internal static GameObject TogglesBox;

    internal static NativeModsPanel NativeModsPanel;

    // Prefabs
    internal static GameObject PREFAB_WorkshopContainer;

    // Created stuff


    internal static void Awake()
    {
        return;

        ModsPanel = GameObject.Find("UI").transform.GetChild(3).GetChild(2).GetChild(7).gameObject;
        SteamModsBox = ModsPanel.transform.GetChild(1).gameObject;
        TogglesBox = ModsPanel.transform.GetChild(2).gameObject;

        NativeModsPanel = ModsPanel.GetComponent<NativeModsPanel>();

        PREFAB_WorkshopContainer = GameObject.Instantiate(NativeModsPanel.workshopContainer);
        UpdatePrefab();
        NativeModsPanel.workshopContainer = PREFAB_WorkshopContainer;
    }

    internal static void UpdatePrefab()
    {
        WorkshopModContainer container = PREFAB_WorkshopContainer.GetComponent<WorkshopModContainer>();

        DeExtendMod(container);

        container.activatedToggle.transform.localScale = new Vector2(1.5f, 1.5f);
        container.activatedToggle.transform.Translate(new Vector3(-25, -10));

        container.unSubscribeButton.transform.localScale = new Vector2(1.5f, 1.5f);
        container.unSubscribeButton.transform.Translate(new Vector3(-5, -10));

        container.playButton.transform.localScale = new Vector2(1.5f, 1.5f);
        container.playButton.transform.Translate(new Vector3(-25, -10));

        container.modDescText.GetComponent<RectTransform>().sizeDelta -= new Vector2(100, 0);
        container.modDescText.transform.Translate(new Vector3(-50, 0));

        PREFAB_WorkshopContainer.AddComponent<ModUIsContainerComp>();
    }

    internal static void DeExtendMod(WorkshopModContainer container)
    {
        LayoutElement layoutElement = container.gameObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 50;

        container.modDescText.gameObject.SetActive(false);
        container.modNameText.fontSize = 24;
        container.modNameText.verticalOverflow = VerticalWrapMode.Overflow;
        container.modNameText.horizontalOverflow = HorizontalWrapMode.Overflow;
        container.modNameText.transform.Translate(new Vector3(-1f, -9f));
        container.image.transform.localScale = new Vector2(0.75f, 0.75f);

    }

    internal static void ExpandMod(WorkshopModContainer container)
    {
        LayoutElement layoutElement = container.gameObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 80;

        container.modDescText.gameObject.SetActive(true);

        container.modNameText.fontSize = 20;
        container.modNameText.verticalOverflow = VerticalWrapMode.Truncate;
        container.modNameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        container.modNameText.transform.Translate(new Vector3(1f, 9f));
        container.image.transform.localScale = new Vector2(1f, 1f);
    }
}
