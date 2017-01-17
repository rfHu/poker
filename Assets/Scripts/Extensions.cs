using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace Extensions {
	public static class CShapeExtensions {
		public static int Int<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			return Convert.ToInt32(value);
		}

		public static double Double<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			return Convert.ToDouble(value);
		}	

		public static bool Bool<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			return Convert.ToBoolean(value);
		}

		public static float Float<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			return Convert.ToSingle(value);
		}

		public static string String<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			return Convert.ToString(value);
		}		

		public static Dictionary<string, object> Dict<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as Dictionary<string, object> ?? new Dictionary<string, object>();
			return result;
		}

		public static List<int> IL<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as List<object> ?? new List<object>();
			return result.Select(o => Convert.ToInt32(o)).ToList();
		} 

		public static List<object> List<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as List<object> ?? new List<object>();
			return result;
		}

		public static Transform Clear(this Transform transform)
     {
         foreach (Transform child in transform) {
             GameObject.Destroy(child.gameObject);
         }
         return transform;
     }

	  public static T ToObject<T>(this Dictionary<string, object> source)
        where T : class, new()
    {
            T someObject = new T();
            Type someObjectType = someObject.GetType();

            foreach (KeyValuePair<string, object> item in source)
            {
				PropertyInfo property = someObjectType.GetProperty(item.Key);

				if (property == null) {
					continue;
				}

				Type propType = property.PropertyType;
                someObjectType.GetProperty(item.Key).SetValue(someObject, Convert.ChangeType(item.Value, propType), null);
            }

            return someObject;
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
}