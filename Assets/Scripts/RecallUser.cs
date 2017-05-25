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


	void setScoreColor(int num) {
		var color = _.GetBgColor(num);
		ProceImage.color = color;
	}

	void ShowAvatar(string url) {
		StartCoroutine(_.LoadImage(url, (texture) => {
			Avatar.texture = _.Circular(texture);
		}));
	}
}
