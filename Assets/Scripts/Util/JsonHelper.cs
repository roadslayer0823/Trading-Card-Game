using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
   public static T[] FromJson<T>(string json)
    {
        string wrapped = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
