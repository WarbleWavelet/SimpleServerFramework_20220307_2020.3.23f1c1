using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>UI工具</summary>
public static class UITool
{
    /// <summary>得到UI的画布</summary>
    public static GameObject GetCanvas(string name="Canvas")
    {
        return GameObject.Find(name);
    }
    /// <summary>查找子节点(父节点,子节点)</summary>
    public static T FindChild<T>(GameObject parent, string childName)
    {
        GameObject uiGO = UnityTool.FindChild(parent, childName);
        if (uiGO == null)
        {
            Debug.LogError("在游戏物体" + parent + "下面查找不到" + childName);
            return default(T);
        }
        return uiGO.GetComponent<T>();
    }
}
