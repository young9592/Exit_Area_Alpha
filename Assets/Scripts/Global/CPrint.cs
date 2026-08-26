using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CPrint
{
    private static readonly HashSet<string> _once = new HashSet<string>();

    public static void Log(string msg)
    {
        Debug.Log(msg);
    }

    public static void Warn(string msg)
    {
        Debug.LogWarning($"경고 : [{msg}]");
    }

    public static void Error(string msg)
    {
        if (!_once.Contains(msg))
        {
            _once.Add(msg);
            Debug.LogError($"오류 : [{msg}]");
        }
    }

    public static void KV(string key, string value)
    {
        Debug.Log($"{key} : {value}");
    }
}
