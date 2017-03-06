using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using UnityEngine.UI.ProceduralImage;

public class RecallUser : MonoBehaviour {
	public RawImage Avatar;
	public Text Name;
	public Text Score;
	public GameObject[] Cards;
	public ProceduralImage ProceImage;
	public GameObject MaxFive;

	// 三种不同颜色
	private string[] colors = new string[]{
		"#ff1744",
		"#646464",
		"#00c853"
	};

	public void Show(Dictionary<string, object> dict) {
		var uid = dict.String("uid");

		// Is current user
		if (uid == GameData.Shared.Uid) {
			gameObject.GetComponent<Image>().enabled = true;	
		}

		var cardDesc = Card.GetCardDesc(dict.Int("maxFiveRank"));
		if (!string.IsNullOrEmpty(cardDesc)) {
			MaxFive.SetActive(true);
			MaxFive.transform.Find("Text").GetComponent<Text>().text = cardDesc;
		} else {
			MaxFive.SetActive(false);
		}
		
		ShowAvatar(dict.String("avatar"));
		Name.text = dict.String("name");

		var earn = dict.Int("coin") - dict.Int("chips");
		Score.text = earnStr(earn);
		setScoreColor(earn);	

		var cards = dict.IL("cards");
		if (cards.Count <= 0) {
			return ;
		}	

		Cards[0].GetComponent<Card>().Show(cards[0]);
		Cards[1].GetComponent<Card>().Show(cards[1]);
	}

	private string earnStr(int earn) {
		if (earn > 0) {
			return "+" + earn.ToString();
		}

		return earn.ToString();
	}


	void setScoreColor(int earn) {
		var color = new Color();
		string c;

		if (earn > 0) {
			c = colors[0];
		} else if (earn < 0) {
			c = colors[2];
		} else {
			c = colors[1];
		}

		ColorUtility.TryParseHtmlString(c, out color);
		ProceImage.color = color;
	}

	void ShowAvatar(string url) {
		_.DownloadImage(Avatar, url);
	}
}
