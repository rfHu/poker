using UnityEngine.UI;
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

	void Awake()
	{
		Connect.Shared.Emit(new Dictionary<string, object>(){
        	{"f", "gamerlist"}
        }, (json) =>
        {
			var ret = json.Dict("ret");
			var secs = ret.Int("left_time");

			Hands.text = string.Format("第{0}手", ret.Int("handid")); 
			Countdown.text = secToStr(secs);
			
			// 保存当前时间
			seconds = secs;
			installTimer();

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
			var header = (GameObject)Instantiate(Resources.Load("Prefab/Score/GuestHeader"));
			header.transform.Find("Text").GetComponent<Text>().text = string.Format("游客（{0}）", guestList.Count);
        	header.transform.SetParent(viewport.transform, false);

			if (guestList.Count < 1) {
				return ;
			}

			GameObject grid = (GameObject)Instantiate(Resources.Load("Prefab/Score/GridLayout"));
			grid.transform.SetParent(viewport.transform, false);

			foreach(Dictionary<string, object> guest in guestList) {
				var guestObj = (GameObject)Instantiate(Resources.Load("Prefab/Score/Guest"));
				Avatar avatar = guestObj.transform.Find("Avatar").GetComponent<Avatar>();
				avatar.SetImage(guest.String("avatar"));
				avatar.BeforeClick = () => {
					GetComponent<DOPopup>().Close();
				};

				guestObj.transform.Find("Text").GetComponent<Text>().text = guest.String("name");
				guestObj.transform.SetParent(grid.transform, false);
			} 
        });
    }

	void installTimer() {
		if (!GameData.Shared.GameStarted) {
			return ;
		}
		InvokeRepeating("updateSecs", 1.0f, 1.0f);
	}

	void OnDestroy()
	{
		CancelInvoke();
	}
}
