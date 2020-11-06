using UnityEngine;

public static class Lookup
{
    
    public static Color Colour(string key)
    {
        switch (key.ToUpper())
        {
            case "PLAYER": return Color.yellow;
            case "JORDAN": return Color.cyan;
            case "WAITER": return Color.magenta;
            default: return Color.white;
        }
    }

}
