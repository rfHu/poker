using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class ScoreCtrl : MonoBehaviour {
	public GameObject viewport;

    public GameObject InGameViewport;

    public GameObject OutGameViewport;

	public Text Hands;

    public Text Pot;

    public Text Time;

    public Text Buy;

	public GameObject PlayerScore;

	public GameObject GuestHeader;

	public GameObject GridLayout;

	public GameObject Guest;

    public GameObject Insurance;

    public Text InsuranceData;

	private Color offlineColor = new Color(1, 1, 1, 0.4f);
	
	void Awake()
	{
		Connect.Shared.Emit(new Dictionary<string, object>(){
        	{"f", "gamerlist"}
        }, (json) =>
        {
			var ret = json.Dict("ret");
            var insurance = ret.Dict("insurance");
			Hands.text = ret.Int("handid").ToString();
            Pot.text = ret.Int("avg_pot").ToString();
            Time.text = ret.Int("hand_time").ToString();
            Buy.text = ret.Int("avg_buy").ToString();

            if (GameData.Shared.NeedInsurance)
            {
                Insurance.SetActive(true);

				var insuValue = insurance.Int("pay");

                InsuranceData.text = _.Number2Text(insuValue);
				InsuranceData.color = _.GetTextColor(insuValue);
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
                    if (dict.Int("seat") < 0)
                    {
                        guestList.Add(dict);
                    }
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

				var profit = player.Int("bankroll") - all; 

                if (player.Int("seat") < 0)
                {
                    name.color = offlineColor;
                    total.color = offlineColor;
                    score.color = offlineColor;
                    entry.transform.SetParent(OutGameViewport.transform, false);
                }
                else 
                {
				    score.text = _.Number2Text(profit);
				    score.color = _.GetTextColor(profit);
				    entry.transform.SetParent(InGameViewport.transform, false);
                }
        	}

            if (InGameViewport.transform.childCount == 0)
            {
                InGameViewport.SetActive(false);
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
                    name.color = offlineColor;
                    avatar.GetComponent<RawImage>().color = offlineColor;
                }
			} 
        });
    }
}
