using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOModLoader.Core;

public static class ExceptionHelper
{
    public static string ProccessException(Exception ex)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Error: {ex.Message}");
        sb.AppendLine($"\tInnerException: {ProccessInnerException(ex.InnerException)}");

        var st = new StackTrace(ex, true);
        var frame = st.GetFrame(0);
        var line = frame.GetFileLineNumber();
        var column = frame.GetFileColumnNumber();
        var method = frame.GetMethod();
        var file = frame.GetFileName();

        sb.AppendLine($"\tAdditional Info:  Line: {ProccessNull(line.ToString())}, Column: {ProccessNull(column.ToString())}, Method: {ProccessNull(method.Name)}, File: {ProccessNull(file)}");

        return sb.ToString();
    }

    private static string ProccessNull(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "N/A";
        }

        return message;
    }

    private static string ProccessInnerException(Exception ex)
    {
        if (ex == null)
        {
            return "N/A";
        }

        return ex.Message;
    }
}
