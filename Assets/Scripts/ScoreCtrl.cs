using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Extensions;

public class ScoreCtrl : MonoBehaviour {
	public GameObject viewport;
	public Text Hands;
	public Text Countdown;

	void Start()
	{
		Connect.shared.Emit(new Dictionary<string, object>(){
        	{"f", "gamerlist"}
        }, (json) =>
        {
			var ret = json.Dict("ret");

			Hands.text = ret.Int("handid").ToString();
			Countdown.text = ret.Int("left_time").ToString();

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

	IEnumerator<WWW> DownloadImage(RawImage img) {
		string url = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=3081053742,1983158129&fm=116&gp=0.jpg";
		WWW www = new WWW(url);
		yield return www;
		img.texture = Ext.Circular(www.texture);
	}
}
