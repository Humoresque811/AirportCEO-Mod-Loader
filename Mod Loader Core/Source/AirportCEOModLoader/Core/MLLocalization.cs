using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.Core;

internal class MLLocalization
{
    internal static string Loc(string key) => GetLocalizedString(key);

    internal static string GetLocalizedString(string key)
    {
        if (!localizationValues.ContainsKey(key))
        {
            return "";
        }


        if (!localizationValues[key].ContainsKey(GameSettingManager.GameLanguage))
        {
            return localizationValues[key]["en"];
        }

        return localizationValues[key][GameSettingManager.GameLanguage];
    }

    internal static Dictionary<string, Dictionary<string, string>> localizationValues = new Dictionary<string, Dictionary<string, string>>()
    {
        { "Config_Restrict-Menu-Actions_Desc", new() { 
            { "en", "Prevents you from making bug reports or reloading in game with mods. Please only disable if you know what you are doing. These restrictions are put in place to increase stability and reduces spam for the ACEO devs" },
            { "zh-CN", "这是防止你制造错误报告或因为该模组无反应而感到疑惑，然后去强制重启游戏。如果你不知道该模组的用途请禁用它，除非你知道你在做什么。这些限制和提示是为了提高ACEO开发者的稳定性和减少无用的邮件" },
            { "it", "Impedisce di fare segnalazioni di bug o ricaricare il gioco con mcd. Si prega di disabilitare solo se si sa cosa si sta facendo. Queste limitazioni sono state introdotte per aumentare la stabilità e ridurre lo Spal per gli ACEO dev" }
        }},
        { "Config_Show-Welcome-Message_Desc", new() {
            { "en", "Show the welcome message on reload again. This will always be automatically disabled on start." },
            { "zh-CN", "重新启动一次游戏时显示欢迎信息。在这之后启动游戏时自动禁用弹窗提示。" },
            { "it", "Mostra nuovamente il messaggio di benvenuto al momento del ricaricamento. Questo verrà sempre disabilitato automaticamente all'avvio." }
        }},
        { "General_Messages-Left", new() {
            { "en", "Message(s) Left"},
            { "zh-CN", "留言(s)" },
            { "it", "Messaggi rimasti" }
        }},
        { "General_Workshop-Republish", new() {
            { "en", "Please do not re-upload mods found on the workshop back to the workshop. This invalidates the amount of time creators put into their mods! If this seems incorrect, please contact Humoresque about how to bypass."},
            { "zh-CN", "请不要在创意工坊重新上传在创意工坊上找到的模组。这会让作者投入到模组中的一切变得没有意义！如果这看起来不正确，请联系Humorresque作者了解如何绕过。" },
            { "it", "Si prega di non ricaricare le mcd trovate sul workshop nel workshop. Questo invalida il tempo che i creatori hanno dedicato alle rispettive mcd! Se questo aspetto non sembra corretto, contattare Umoresche per sapere come evitarlo." }
        }},
        { "Welcome_M1", new() {
            { "en", "Thank you for installing the AirportCEO Mod Loader! Configuration for mods can be accessed via the F1 key."},
            { "zh-CN", "感谢您安装AirportCEO Mod Loader！各个模组设定可以通过按下键盘F1键访问。" },
            { "it", "Grazie per aver installato AirportCEO Mod Loader! La configurazione delle mod è accessibile tramite il tasto F1." }
        }},
        { "Welcome_M2", new() {
            { "en", "Please note that to disable a mod, you must unsubscribe from it. Disabling it will not do anything."},
            { "zh-CN", "请注意，要禁用一个模组，您必须前往创意工坊取消订阅它。游戏内禁用它不会有任何用。" },
            { "it", "Si prega di notare che per disabilitare una mod, è necessario annullare la sottoscrizione. Disattivarla non ha alcun effetto." }
        }},
        { "Welcome_M3", new() {
            { "en", "This message will not show up again. To show it again, you must renable the option in the configuration."},
            { "zh-CN", "此消息将不再显示。要再次显示它，必须在配置中启用该选项。" },
            { "it", "Questo messaggio non verrà più visualizzato. Per visualizzarlo di nuovo, è necessario rinominare l'opzione nella configurazione." }
        }},
        { "Tooltip_Stop-Bug_Header", new() {
            { "en", "Bug Reporting Not Permitted"},
            { "zh-CN", "错误报告不可用" },
            { "it", "Segnalazione errori non consentita" }
        }},
        { "Tooltip_Stop-Bug_Text", new() {
            { "en", "Please do not report bugs with code based mods enabled! You can report mod bugs to their respective developers on Airport CEO Forum. To report bugs with the game, please disable mods to make sure they are not interfereing."},
            { "zh-CN", "请不要报告基于代码模组启用的bug错误！您可以在机场CEO官方论坛上向各自的开发人员报告模组bug报告。要报告游戏错误，请先禁用模组以确保它们不会被干扰。" },
            { "it", "Si prega di non segnalare bug con mod basate su codici abilitati! È possibile segnalare i bug delle mod ai rispettivi sviluppatori sul forum Airport CEO. Per segnalare bug del gioco, disabilitare le mod per assicurarsi che non interferiscano." }
        }},
        { "Tooltip_Stop-Load_Header", new() {
            { "en", "In Game Reload Not Permitted!"},
            { "zh-CN", "游戏中不允许重新加载！" },
            { "it", "La ricarica durante il gioco non è consentita!" }
        }},
        { "Tooltip_Stop-Load_Text", new() {
            { "en", "Reloading in game with Code Based Mods is not supported. To get the same result, please quit the game and load from a fresh instance of the game."},
            { "zh-CN", "在游戏中重新加载基于代码模组是不支持的。要想得到模组真正的效果，请退出游戏并从游戏中新开一个存档。" },
            { "it", "La ricarica durante il gioco con le Mcd basate su codice non è supportata. Per ottenere lo stesso risultato, chiudere il gioco e caricarlo da una nuova istanza del gioco." }
        }},
    };
}