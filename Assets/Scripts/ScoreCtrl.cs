﻿using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class ScoreCtrl : MonoBehaviour {
	public GameObject viewport;
	public Text Hands;
	public Text Countdown;

	int seconds;

	private string secToStr(int seconds) {
		var hs = 3600;
		var ms = 60;

		var h = Mathf.FloorToInt(seconds / hs);		
		var m = Mathf.FloorToInt(seconds % hs / ms);
		var s = (seconds % ms);

		return string.Format("{0}:{1}:{2}", fix(h), fix(m), fix(s));	
	}

	private string fix(int num) {
		var str = num.ToString();
		if (str.Length < 2) {
			return "0" + str;
		}
		return str;
	}

	private void updateSecs() {
		seconds -= 1;
		Countdown.text = secToStr(seconds);
	}

	void Start()
	{
		Connect.shared.Emit(new Dictionary<string, object>(){
        	{"f", "gamerlist"}
        }, (json) =>
        {
			var ret = json.Dict("ret");
			var secs = ret.Int("left_time");

			Hands.text = string.Format("第{0}手", ret.Int("handid")); 
			Countdown.text = secToStr(secs);
			
			// 保存当前时间
			seconds = secs;

			var list = ret.List("list");
			var guestList = new List<Dictionary<string, object>>();
			var playerList = new List<Dictionary<string, object>>();

			foreach(object item in list) {
				var dict = item as Dictionary<string, object>;
				if (dict == null) {
					continue;
				}

				if (dict.Int("takecoin") > 0) {
					playerList.Add(dict);
				} else {
					guestList.Add(dict);
				}
			}

			foreach(Dictionary<string, object> player in playerList) {
				GameObject  entry = (GameObject)Instantiate(Resources.Load("Prefab/Score/PlayerScore"));
				var all = player.Int("takecoin");

				entry.transform.Find("Name").GetComponent<Text>().text = player.String("name");
				entry.transform.Find("Total").GetComponent<Text>().text = all.ToString(); 
				entry.transform.Find("Score").GetComponent<Text>().text = (player.Int("bankroll") - all).ToString();
				entry.transform.SetParent(viewport.transform, false);
        	}

			// 游客
			var header = (GameObject)Instantiate(Resources.Load("Prefab/Score/LookerHeader"));
        	header.transform.SetParent(viewport.transform, false);
			header.transform.Find("Text").GetComponent<Text>().text = string.Format("游客（{0}）", guestList.Count);

			installTimer();
        });

        // // 每个item相距30，两边留20
        // float width = 150;
        // float height = 170;
        // GridLayoutGroup gridLayout = Instantiate(lookerGridLayout).GetComponent<GridLayoutGroup>();
        // gridLayout.cellSize = new Vector2(width, height);
        // gridLayout.transform.SetParent(viewport.transform, false);

        // for (var i = 0; i < 10; i++) {
        // 	GameObject obj = Instantiate(lookerPrefab);

        // 	RawImage img = obj.transform.Find("RawImage").GetComponent<RawImage>();
        // 	StartCoroutine(DownloadImage(img));
        // 	obj.transform.Find("Text").GetComponent<Text>().text = "我是大番薯";

        // 	obj.transform.SetParent(gridLayout.transform, false);
        // }
    }

	void installTimer() {
		InvokeRepeating("updateSecs", 1.0f, 1.0f);
	}

	IEnumerator<WWW> DownloadImage(RawImage img) {
		string url = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=3081053742,1983158129&fm=116&gp=0.jpg";
		WWW www = new WWW(url);
		yield return www;
		img.texture = Ext.Circular(www.texture);
	}

	void OnDestroy()
	{
		CancelInvoke();
	}
}
