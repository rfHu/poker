using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class _ {
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

    public static DateTime DateTimeFromTimeStamp(double timeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(timeStamp).ToLocalTime();
        return dtDateTime;
    }

    static public bool isDict(object dict) {
        Type t = dict.GetType();
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }

    static  public IEnumerator<WWW> DownloadImage(RawImage img, string url) {
		WWW www = new WWW(url);
		yield return www;
		img.texture = _.Circular(www.texture);
	}
}
