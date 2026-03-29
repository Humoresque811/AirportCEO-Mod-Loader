using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AirportCEOModLoader.Core.ModUIs;

public class ModUIsContainerComp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool? IsCBM
    {
        get
        {
            return container?.modData.tags.Contains("Code Based");
        }
    }

    private WorkshopModContainer container;

    private void Awake()
    {
        container = gameObject.GetComponent<WorkshopModContainer>();
    }

    public void OnPointerEnter(PointerEventData _)
    {
        ModUIsManager.ExpandMod(container);
        TrySetToCodeBased();
    }

    public void OnPointerExit(PointerEventData _)
    {
        ModUIsManager.DeExtendMod(container);
    }

    internal void TrySetToCodeBased()
    {
        if (IsCBM == true && !ModUIsManager.CBMsAllowingDisabling.Contains(container.modData.id))
        {
            ModManager.ActivateMod(container.modData.id);
            container.activatedToggle.isOn = true;

            container.activatedToggle.interactable = false;
            container.activatedToggle.onValueChanged.RemoveAllListeners();
            container.activatedToggle.transform.GetChild(0).GetComponent<Image>().color = DataPlaceholderColors.Instance.green;

            HoverToolTip toolTip = container.activatedToggle.GetComponent<HoverToolTip>();
            toolTip.headerToDisplay = "Code Based Mod";
            toolTip.textToDisplay = "This Code Based Mod cannot simply be disabled. To disable, you must unsubscribe.";

        }
    }
}
