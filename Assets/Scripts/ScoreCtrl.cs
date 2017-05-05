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

    public GameObject Insurance;

    public Text InsuranceData;
	
	void Awake()
	{
		Connect.Shared.Emit(new Dictionary<string, object>(){
        	{"f", "gamerlist"}
        }, (json) =>
        {
			var ret = json.Dict("ret");
            var insurance = ret.Dict("insurance");
			Hands.text = string.Format("第{0}手", ret.Int("handid"));

            if (GameData.Shared.NeedInsurance)
            {
                Insurance.SetActive(true);
                InsuranceData.text = insurance.Int("pay").ToString();
            }

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

                Text name = entry.transform.Find("Name").GetComponent<Text>();
                Text total = entry.transform.Find("Total").GetComponent<Text>();
                Text score = entry.transform.Find("Score").GetComponent<Text>();

				name.text = player.String("name");
				total.text = all.ToString(); 
				score.text = (player.Int("bankroll") - all).ToString();
				entry.transform.SetParent(viewport.transform, false);

                if (!player.Bool("in_room"))
                {
                    name.color = new Color(1, 1, 1, 0.4f);
                    total.color = new Color(1, 1, 1, 0.4f);
                    score.color = new Color(1, 1, 1, 0.4f);
                }
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

                Text name = guestObj.transform.Find("Text").GetComponent<Text>();
				name.text = guest.String("name");
				guestObj.transform.SetParent(grid.transform, false);

                if (!guest.Bool("in_room"))
                {
                    name.color = new Color(1, 1, 1, 0.4f);
                    avatar.GetComponent<RawImage>().color = new Color(1, 1, 1, 0.5f);
                }
			} 
        });
    }
}
