using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AirportCEOModLoader.Core.ModUIs;

public class ModUIsContainerComp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsExpaneded { get; protected set; }
    public bool IsCBM { get; protected set; }

    private WorkshopModContainer container;

    private void Awake()
    {
        container = gameObject.GetComponent<WorkshopModContainer>();
    }

    public void OnPointerEnter(PointerEventData _)
    {
        ModUIsManager.ExpandMod(container);
        LocalExpandEvent();
    }

    public void OnPointerExit(PointerEventData _)
    {
        ModUIsManager.DeExtendMod(container);
    }

    protected void LocalExpandEvent()
    {
        foreach (string tag in container.modData.tags)
        {
            AirportCEOModLoader.ModLoaderLogger.LogInfo(tag);
        }

        if (container.modData.tags.Contains("Code Based"))
        {
            container.activatedToggle.gameObject.SetActive(false);
        }
    }
}
