using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class PlayerPrefsUtility
{

    public static void setString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public static void setInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static void setFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public static void setBool(string key, bool value)
    {
        int intValue = 0; //Default is FALSE
        if (value) intValue = 1; //If value is TRUE, then it's a 1.
        PlayerPrefs.SetInt(key, intValue);
        PlayerPrefs.Save();
    }


    //The methods below are silly, but the above ones have a bit more validity, so I wanted to have everything wrapped at that point.
    public static string getString(string key)
    {
        return PlayerPrefs.GetString(key);
    }

    public static float getFloat(string key)
    {
        return PlayerPrefs.GetFloat(key);
    }

    public static int getInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public static bool getBool(string key)
    {
        int intValue = PlayerPrefs.GetInt(key);
        if (intValue >= 1) return true; //If it's 1, then TRUE. The greater than is just a sanity check.
        return false; //Default is FALSE which should be 0.
    }

    public static bool hasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }


}
