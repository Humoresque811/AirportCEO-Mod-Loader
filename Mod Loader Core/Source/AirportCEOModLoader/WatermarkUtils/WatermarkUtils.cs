using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace AirportCEOModLoader.WatermarkUtils;

public static class WatermarkUtils
{
    static List<string> normalText = new List<string>();
    static List<string> abbrevText = new List<string>();
    static string defaultText = null;

    /// <summary>
    /// Adds the watermark of your mod to the top of the screen. This is not reversible
    /// </summary>
    /// <param name="watermarkInfo"><see cref="WatermarkInfo"/> to make the watermark with</param>
    public static void Register(in WatermarkInfo watermarkInfo)
    {
        if (defaultText == null)
        {
            defaultText = WatermarkUtilsPatches.WatermarkText.text.TrimEnd('\n');
            WatermarkUtilsPatches.WatermarkText.lineSpacing = 0.5f;
        }

        if (watermarkInfo._isAbbreviated)
        {
            abbrevText.Add(watermarkInfo.GetWatermark());
        }
        else
        {
            normalText.Add(watermarkInfo.GetWatermark());
        }

        RegenerateText();
    }

    private static void RegenerateText()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(defaultText);

        foreach (string normalWatermark in normalText)
        {
            stringBuilder.Append(normalWatermark);
        }

        foreach (string abbrevWatermark in abbrevText)
        {
            stringBuilder.Append(abbrevWatermark);
        }

        WatermarkUtilsPatches.WatermarkText.text = stringBuilder.ToString();
    }
}

