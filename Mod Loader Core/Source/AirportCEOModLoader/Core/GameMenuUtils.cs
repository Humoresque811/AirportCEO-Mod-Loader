using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOModLoader.Core
{

    [HarmonyPatch(typeof(GameMenuPanelUI))]
    [HarmonyPatch("InitializePanel")]
    public class GameMenuInitialize()
    {
        /// <summary>
        /// Initializes the GameMenuUtils instance when the game menu panel is initialized. This ensures that the game menu utilities are ready to be used when adding buttons to the game menu. 
        /// </summary>
        internal static void Postfix(GameMenuPanelUI __instance)
        {
            AirportCEOModLoader.ModLoaderLogger.LogInfo("GameMenuUtils: Initializing");
            GameMenuUtils.Init(__instance);
            //Test();
        }

        /// <summary>
        /// A test method to demonstrate the functionality of adding a button to the game menu. This method adds a button labeled "Test Button" that logs a message when clicked. It is intended for testing purposes and can be removed or commented out in production code.
        /// </summary>
        static void Test()
        {
            GameMenuUtils.AddButton("Test Button", () => { AirportCEOModLoader.ModLoaderLogger.LogInfo("Test Button Clicked!"); }, false);
        }
    }

    internal class GameMenuUtils
    {
        public static GameMenuUtils instance;
        private GameObject gameMenuPanelUI;
        private GameObject buttonPrefab;

        private int numberOfButtons = 0;
        internal static void Init(GameMenuPanelUI gameMenuPanel)
        {
            if (instance != null)
            {
                AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: Already initialized");
                return;
            }
            else
            {
                instance = new GameMenuUtils();
                AirportCEOModLoader.ModLoaderLogger.LogInfo("GameMenuUtils: Initialized");
            }
            instance.gameMenuPanelUI = gameMenuPanel.gameObject;
            if (instance.gameMenuPanelUI == null)
            {
                //throw new Exception("GameMenuUtils: Could not find GameMenuPanelUI");
                AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: Could not find GameMenuPanelUI");

            }
            instance.buttonPrefab = gameMenuPanel.quitGameButton.gameObject;
            if (instance.buttonPrefab == null)
            {
                //throw new Exception("GameMenuUtils: Could not find ButtonPrefab");
                AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: Could not find ButtonPrefab");
            }
        }

        /// <summary>
        /// Adds a button to the game menu. This is not reversible and will be added every time the function is called.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <param name="onClickAction">The action to perform when the button is clicked.</param>
        /// <param name="localized">Indicates whether the button text should be localized.</param>
        /// <exception cref="Exception">Thrown when the game menu panel or button prefab is not found.</exception>
        public static void AddButton(string buttonText, Action onClickAction, bool localized)
        {
            AirportCEOModLoader.ModLoaderLogger.LogInfo($"GameMenuUtils: Adding button with text \"{buttonText}\"");
            if (instance == null)
            {
                AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: Not initialized");
                return;
            } else if (instance.gameMenuPanelUI == null)
            {
                AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: GameMenuPanelUI not found. retrying...");
                //not found, trying search again
                instance.gameMenuPanelUI = GameObject.Find("GameMenuPanel");
                if (instance.gameMenuPanelUI == null)
                {
                    AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: GameMenuPanelUI still not found");
                    return;
                }
            }
            else if (instance.buttonPrefab == null)
            {
                AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: ButtonPrefab not found");
                instance.buttonPrefab = GameObject.Find("GameMenuButton");
                if (instance.buttonPrefab == null)
                {
                    AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: ButtonPrefab still not found");
                    return;
                }
            }
            GameObject newButton = GameObject.Instantiate(instance.buttonPrefab, instance.gameMenuPanelUI.transform);
            instance.numberOfButtons++;
            newButton.SetActive(true);

            newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => onClickAction());
            newButton.name = $"GameMenuButton_{buttonText}";
            newButton.transform.SetSiblingIndex(instance.gameMenuPanelUI.transform.childCount - 1); // Set the button to be the last child
            newButton.transform.Translate(0, -75 * (instance.numberOfButtons) - 10, 0); // Adjust the position based on the number of buttons
            if(localized)
            {
                //TODO: Implement localization for button text. For now, we will log an error and use non-localized text.
                AirportCEOModLoader.ModLoaderLogger.LogError("GameMenuUtils: Localized button text is not yet implemented. Using non-localized text instead.");
                newButton.GetComponent<LocalizedTextMeshPro>().enabled = false; // Disable the LocalizedTextMeshPro component to use non-localized text
                newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = buttonText;
            }
            else
            {
                newButton.GetComponentInChildren<LocalizedTextMeshPro>().enabled = false; // Disable the LocalizedTextMeshPro component to use non-localized text
                newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = buttonText;
            }  
            AirportCEOModLoader.ModLoaderLogger.LogInfo($"GameMenuUtils: Added button with text \"{buttonText}\"");
        }

        /// <summary>
        /// Removes a button from the game menu based on its text. This will remove the first button found with the specified text. If no button is found, nothing will happen.
        /// </summary>
        /// <param name="buttonText">The text of the button to remove.</param>
        public static void RemoveButton(string buttonText)
        {
            foreach (Transform child in instance.gameMenuPanelUI.transform)
            {
                var textComponent = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textComponent != null && textComponent.text == buttonText)
                {
                    GameObject.Destroy(child.gameObject);
                    AirportCEOModLoader.ModLoaderLogger.LogInfo($"GameMenuUtils: Removed button with text \"{buttonText}\"");
                    return;
                }
            }
        }

    }
}
