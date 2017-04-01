using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class ScoreCtrl : MonoBehaviour {
	public GameObject viewport;
	public Text Hands;

	public GameObject PlayerScore;

	public GameObject GuestHeader;

	public GameObject GridLayout;

	public GameObject Guest;
	
	void Awake()
	{
		Connect.Shared.Emit(new Dictionary<string, object>(){
        	{"f", "gamerlist"}
        }, (json) =>
        {
			var ret = json.Dict("ret");
			Hands.text = string.Format("第{0}手", ret.Int("handid")); 
			
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

			playerList.Sort((a, b) => {
				var ar = a.Int("bankroll") - a.Int("takecoin");
				var br = b.Int("bankroll") - b.Int("takecoin");

				return br - ar;
			});

			foreach(Dictionary<string, object> player in playerList) {
				GameObject  entry = Instantiate(PlayerScore);
				entry.SetActive(true);
				var all = player.Int("takecoin");

				entry.transform.Find("Name").GetComponent<Text>().text = player.String("name");
				entry.transform.Find("Total").GetComponent<Text>().text = all.ToString(); 
				entry.transform.Find("Score").GetComponent<Text>().text = (player.Int("bankroll") - all).ToString();
				entry.transform.SetParent(viewport.transform, false);
        	}

			// 游客
			var header = (GameObject)Instantiate(GuestHeader);
			header.SetActive(true);
			header.transform.Find("Text").GetComponent<Text>().text = string.Format("游客（{0}）", guestList.Count);
        	header.transform.SetParent(viewport.transform, false);

			if (guestList.Count < 1) {
				return ;
			}

			GameObject grid = Instantiate(GridLayout);
			grid.SetActive(true);
			grid.transform.SetParent(viewport.transform, false);

			foreach(Dictionary<string, object> guest in guestList) {
				var guestObj = Instantiate(Guest);
				guestObj.SetActive(true);
				Avatar avatar = guestObj.transform.Find("Avatar").GetComponent<Avatar>();
				avatar.Uid = guest.String("uid");
				avatar.SetImage(guest.String("avatar"));
				avatar.BeforeClick = () => {
					GetComponent<DOPopup>().Close();
				};

				guestObj.transform.Find("Text").GetComponent<Text>().text = guest.String("name");
				guestObj.transform.SetParent(grid.transform, false);
			} 
        });
    }
}
