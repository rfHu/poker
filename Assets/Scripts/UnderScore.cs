using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using BestHTTP;

public class _ {
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

    private static int countDigits(float num) {
        num = Mathf.Abs(num);

        if (num >= 100) {
            return 0;
        } else if (num >= 10) {
            return 1;
        } else {
            return 2;
        }
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
            digits = countDigits(v); 
            text = Math.Round(v, digits).ToString() + "亿";   
        } else if (Mathf.Abs(value) >= w) {
            var v = value / w;
            digits = countDigits(v);
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
		    "#00c853",
	    };
        
        return getColor(colors, num);
    }

    static public Color GetTextColor(int num) {
        string[] colors = new string[]{
		    "#ff1744",
		    "#ffffff",
		    "#00c853",
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

    static public string url2Filename(string url) {
         var uri = new Uri(url);
         var filename = "images/" + Path.GetFileName(uri.LocalPath);
         return filename;
    }

    static public void LoadTexture(string url, Action<Texture2D> cb) {
        var filename = url2Filename(url);

        if (ES2.Exists(filename)) {
            var texture = ES2.Load<Texture2D>(filename);
            cb(texture);
        } else {
            new HTTPRequest(new Uri(url), (request, response) => {
                var texture = new Texture2D(0, 0);
                texture.LoadImage(response.Data);
                cb(texture);
                ES2.Save(texture, filename);
            }).Send();                
        }
    }

    static public string SecondStr(int seconds) {
		var hs = 3600;
		var ms = 60;

		var h = Mathf.FloorToInt(seconds / hs);		
		var m = Mathf.FloorToInt(seconds % hs / ms);
		var s = (seconds % ms);

		return string.Format("{0}:{1}:{2}", Fix(h), Fix(m), Fix(s));	
	}

	static public string Fix<T>(T num) {
		var str = num.ToString();
		if (str.Length < 2) {
			return "0" + str;
		}
		return str;
	}

    public static void SetMsgText(bool isOpen, Text text)
    {
        if (isOpen)
        {
            text.text = "开启";
            Color col = new Color();
            ColorUtility.TryParseHtmlString("#18ffff", out col);
            text.color = col;
        }
        else
        {
            text.text = "关闭";
            text.color = Color.white;
        }
    }

    public static void PayFor(Action cb) {
        PokerUI.Alert("金币不足，请购买", () => {
            Commander.Shared.PayFor();
            PokerUI.RemoveDialog();
            cb();
        }, null);
    }
}
