using System.Collections.Generic;
using UnityEngine;
using System;

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

		public static List<int> ListInt<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as List<int> ?? new List<int>();
			return result;
		} 

		public static List<string> ListStr<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as List<string> ?? new List<string>();
			return result;
		} 

		public static List<object> ListObj<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
			TValue value;
			dictionary.TryGetValue(key, out value);
			var result = value as List<object> ?? new List<object>();
			return result;
		} 
	}
}