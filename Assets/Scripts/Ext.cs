using UnityEngine;
using System.Collections.Generic;
using System;

public class Ext {
    static public Texture2D Circular(Texture2D sourceTex)
    {
        Color[] c = sourceTex.GetPixels(0, 0, sourceTex.width, sourceTex.height);
		Texture2D b = new Texture2D(sourceTex.width, sourceTex.height);
		float r = sourceTex.height / 2;
		float h = sourceTex.height;
		float w = sourceTex.width;
		float cx = sourceTex.width / 2;
		float cy = sourceTex.height / 2;

        for (int i = 0; i < (h * w); i++)
        {
            int y = Mathf.FloorToInt(((float)i) / ((float)w));
            int x = Mathf.FloorToInt(((float)i - ((float)(y * w))));
            if (r * r >= (x - cx) * (x - cx) + (y - cy) * (y - cy))
            {
                b.SetPixel(x, y, c[i]);
            }
            else
            {
                b.SetPixel(x, y, Color.clear);
            }
        }
        b.Apply();
        return b;
    }

    static public void Log(Dictionary<string,object> dictionary) {
        foreach (var item in dictionary)
        {
            var valueDict = item.Value as Dictionary<string, object>;

            if (valueDict != null) {
                Debug.Log(string.Format("[{0}] is dictionary", item.Key));
                Ext.Log(valueDict);
            } else {
                Debug.Log(string.Format("Key: {0}, Value: {1}", item.Key, item.Value));
            } 
        }
    }

    static public bool isDict(object dict) {
        Type t = dict.GetType();
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}
