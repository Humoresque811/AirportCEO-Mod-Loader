using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.Core;

public static class DialogUtils
{
    private static Queue<string> messages = new Queue<string>();

    public static void QueueDialog(string message)
    {
        messages.Enqueue(message);
        UpdateText();
    }

    public static void UpdateText()
    {
        if (messages.Count == 0)
        {
            return;
        }

        if (DialogPanel.Instance.isDisplayed)
        {
            return;
        }

        string messageNew = $"[{messages.Count - 1} Messages Left]\n{messages.Dequeue()}";

        DialogPanel.Instance.ShowMessagePanel(messageNew);
    }
}
