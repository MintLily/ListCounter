using UnityEngine;

namespace ListCounter; 

public static class Ext {
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
        var component = gameObject.GetComponent<T>();
        return component == null ? gameObject.AddComponent<T>() : component;
    }
}