using System;
using UnityEngine;

public static class Lookup
{
    
    public static Color Colour(string key)
    {
        switch (key.ToUpper())
        {
            case "PLAYER": return Color.yellow;
            case "JORDAN": return FuncLib.GetColour(51, 214, 255); // cyan
            case "WAITER": return FuncLib.GetColour(204, 51, 153); // pink
            default: return Color.white;
        }
    }



    public static string File(string key)
    {
        switch (key.ToUpper())
        {
            case "BLACK": return "Assets/_Project/Art/Background/black.jpg";
            default: return string.Empty;
        }
    }

}
