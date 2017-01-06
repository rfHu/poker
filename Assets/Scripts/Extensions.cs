using System.Collections.Generic;
using UnityEngine;
using System;

namespace Extensions {
	public static class CShapeExtensions {
		public static int IntValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			return Convert.ToInt32(value);
		}

		public static double DoubleValue<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			return Convert.ToDouble(value);
		}	

		public static float FloatValue<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			return Convert.ToSingle(value);
		}

		public static string StringValue<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as string ?? "";
			return result;
		}		

		public static Dictionary<string, object> DictionaryValue<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as Dictionary<string, object> ?? new Dictionary<string, object>();
			return result;
		}

		public static List<object> ListValue<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as List<object> ?? new List<object>();
			return result;
		} 
	}
}