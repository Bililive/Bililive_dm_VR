using UnityEngine.Events;

public static class Logger4UIScripts
{
    public class LogEvent : UnityEvent<string, LogColor> { }
    public static LogEvent Log = new LogEvent();

    public enum LogColor
    {
        Green,
        Red,
        Blue,
        Yellow,
        Purple,
        White
    }

    public static string LogColor2Hex(this LogColor color)
    {
        switch (color)
        {
            case LogColor.Green:
                return "00FF00";
            case LogColor.Red:
                return "FF0000";
            case LogColor.Blue:
                return "0066FF";
            case LogColor.Yellow:
                return "FFFF00";
            case LogColor.Purple:
                return "FF00FF";
            case LogColor.White:
            default:
                return "FFFFFF";
        }
    }
}
