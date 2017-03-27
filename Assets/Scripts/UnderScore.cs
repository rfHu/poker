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

    static  public void DownloadImage(RawImage img, string url) {
        var options =  new LLOptions().setOnLoad((Texture2D texture) => {
            if (img == null) {
                return ;
            }
            img.texture = Circular(texture);
        }).setUseCache(true).setCacheLife(60 * 60 * 24 * 30);
        LeanLoader.load(url, options);
	}

    static public string Num2Text<T>(T num) {
        var value = Convert.ToDouble(num);

        if (Double.IsInfinity(value)) {
            return "";
        }

        double w = 100 * 100;
        double y = w * w; 
        var digits = 2;

        if (value > y) {
            var v = value / y;
            if (v > 100) {
               digits = 0;
            } 

            return Math.Round(v, digits).ToString() + "亿";   
        } else if (value > w) {
            var v = value / w;
            if (v > 100) {
                digits = 0;
            }

            return Math.Round(v, digits).ToString() + "万";
        }

        return value.ToString();
    }

    static public Color HexColor(string hex) {
        var color = new Color();
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    static public string PercentStr(float number) {
        if (float.IsNaN(number) || float.IsInfinity(number)) { 
			number = 0;
		}

		return Math.Round(number * 100, 1).ToString() + "%"; 
    }

    static public void FillParent(GameObject go) {
        var rect = go.GetComponent<RectTransform>();

        if (rect == null) {
            return ;
        }

		rect.localScale = new Vector3(1, 1, 1);
		rect.sizeDelta = new Vector2(0, 0);
		rect.anchorMin = new Vector2(0, 0);
		rect.anchorMax = new Vector2(1, 1);
		rect.anchoredPosition = new Vector2(0, 0);
    }

    static public void Log(string text) {
        if (Debug.isDebugBuild) {
            Debug.Log(text);
        }
    }
}
