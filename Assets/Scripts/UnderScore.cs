using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;
using System.IO;

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

    static public string Num2CnDigit<T>(T num, bool symbol = false) {
        var value = (float)Convert.ToDouble(num);

        if (float.IsInfinity(value)) {
            return "";
        }

        float w = 100 * 100;
        float y = w * w; 
        var digits = 2;
        var text = "";

        if (Mathf.Abs(value) >= y) {
            var v = value / y;
            if (Mathf.Abs(v) >= 100) {
               digits = 0;
            } 

            text = Math.Round(v, digits).ToString() + "亿";   
        } else if (Mathf.Abs(value) >= w) {
            var v = value / w;
            if (Mathf.Abs(v) >= 100) {
                digits = 0;
            }

            text = Math.Round(v, digits).ToString() + "万";
        } else {
            text = value.ToString();
        }

        if (symbol && value > 0) {
            return "+" + text;
        }

        return text;
    }

    static public Color HexColor(string hex) {
        var color = new Color();
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    static public bool IsNumeric(string number) {
        int n;
		return int.TryParse(number, out n);
    }

    static public Color GetBgColor(int num) {
        string[] colors = new string[]{
		    "#ff1744",
		    "#646464",
		    "#00c853"
	    };
        
        return getColor(colors, num);
    }

    static public Color GetTextColor(int num) {
        string[] colors = new string[]{
		    "#ff1744",
		    "#ffffff",
		    "#00c853"
	    };

        return getColor(colors, num);
    }

    static private Color getColor(string[] colors, int num) {
        var color = new Color();
		string c;

		if (num > 0) {
			c = colors[0];
		} else if (num < 0) {
			c = colors[2];
		} else {
			c = colors[1];
		}

		ColorUtility.TryParseHtmlString(c, out color);
		return color;
    }

    static public string Number2Text(int num) {
        return Num2CnDigit(num, true);
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

    static public void Log(object obj) {
        if (Debug.isDebugBuild) {
            Debug.Log(String.Format("Unity3D: {0}", obj));
        }
    }
}
