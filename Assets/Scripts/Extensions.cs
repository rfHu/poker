using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using BestHTTP.JSON;
using SimpleJSON;
using System.IO;
using System.Collections;
using UniRx;
using BestHTTP;
using System.Text.RegularExpressions;

public class TexturePool {
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private int max = 150;

    private TexturePool() {}

    static public TexturePool Shared = new TexturePool();

   public bool Exists(string url) {
        return textures.ContainsKey(url);
    }

    public void Store(string url, Texture2D texture) {
        textures[url] = texture;
    }

    public void Despawn(string url) {
        if (textures.Count > max && Exists(url)) {
            var tex = textures[url];
            textures.Remove(url);
            GameObject.Destroy(tex); 

            _.Log("回收Texture");
        }
    } 

    public void FetchTexture(string url, Action<Texture2D> cb) {
        string pattern = @"/w/(\d)+/h/(\d)+/format/jpg";
		Regex rgx = new Regex(pattern);
		url = rgx.Replace(url, "/w/124/h/124/format/png");

        Despawn(url);

        if (Exists(url)) {
            cb(textures[url]);
        } else {
            _.LoadTexture(url, (texture) => {
                Store(url, texture);
                cb(texture);
            });
        }
    }
}

    public static class CShapeExtensions
    {
        public static int Int<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return Convert.ToInt32(value);
        }

         public static int StepValue(this float value, int step)
        {
            int temp1 = Mathf.FloorToInt(value / step) * step;
            int temp2 = Mathf.CeilToInt(value / step) * step;
            int newValue;

            if (value - temp1 >= temp2 - value) {
                newValue = temp2;
            } else {
                newValue = temp1;
            }

            return newValue;            
        }

         public static long Long<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return Convert.ToInt64(value);
        }

        public static double Double<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return Convert.ToDouble(value);
        }

        public static bool Bool<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return Convert.ToBoolean(value);
        }

        public static float Float<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return Convert.ToSingle(value);
        }

        public static string String<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return Convert.ToString(value); 
        }

        public static Dictionary<string, object> Dict<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            var result = value as Dictionary<string, object> ?? new Dictionary<string, object>();
            return result;
        }

         public static List<string> SL<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            var result = value as List<object> ?? new List<object>();
            return result.Select(o => Convert.ToString(o)).ToList();
        }

        public static List<int> IL<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            var result = value as List<object> ?? new List<object>();
            return result.Select(o => Convert.ToInt32(o)).ToList();
        }

        public static List<int> GetIL(this object obj)
        {
            var result = obj as List<object> ?? new List<object>();
            return result.Select(o => Convert.ToInt32(o)).ToList();
        }

        public static List<object> List<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            var result = value as List<object> ?? new List<object>();
            return result;
        }

        public static List<Dictionary<string, object>> DL<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
            TValue value;
            dictionary.TryGetValue(key, out value);
            var result = value as List<object> ?? new List<object>();
            return result.Select(o => o as Dictionary<string, object> ?? new Dictionary<string, object>()).ToList();
        }

        public static Transform Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            return transform;
        }

        public static T ToObject<T>(this Dictionary<string, object> source)
          where T : class, new()
        {
            var json = Json.Encode(source);
            return JsonUtility.FromJson<T>(json);
        }

        public static String Serialize(this Dictionary<string, object> source)
        {
            return Json.Encode(source);
        }

        public static JSONNode ToJSON(this Dictionary<string, object> source) {
            return JSON.Parse(source.Serialize());
        }

        public static float GetThinkTime(this float num) {
            return num > GameData.Shared.ThinkTime ? num : GameData.Shared.ThinkTime;
        }

        public static Dictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }
    }