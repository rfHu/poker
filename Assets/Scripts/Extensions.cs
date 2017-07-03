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

         public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize) 
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static int CnCount(this string source) {
            if (string.IsNullOrEmpty(source)) {
                return 0;
            }

            int len = source.Length;
            int i = 0;
            int count = 0;

            while (i < len) {
                var _char = source[i];
                if (Convert.ToInt32(_char) > 255) {
                    count += 2;
                } else {
                    count += 1;
                }

                i++;
            } 

            return count;
        }

        public static string CnCut(this string source, int length) {
            var cnCount = source.CnCount();

            if (cnCount <= length) {
                return source;
            }
            
            var idx = source.Length;

            while (cnCount > length) {
                var _char = source[idx - 1];
                
                if (Convert.ToInt32(_char) > 255) {
                    cnCount -= 2;
                } else {
                    cnCount -= 1;
                }
                
                idx--;
            }

            return source.Substring(0, idx);
        }
    }