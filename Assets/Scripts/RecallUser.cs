using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;

public class RecallUser : MonoBehaviour {
	public RawImage Avatar;
	public Text Name;
	public Text Score;
	public GameObject[] Cards;

	// 三种不同颜色
	private string[] colors = new string[]{
		"#ff1744",
		"#ffffff",
		"00c853"
	};

	public void Show(Dictionary<string, object> dict) {
		ShowAvatar(dict.String("avatar"));
		Name.text = dict.String("name");

		var earn = dict.Int("coin") - dict.Int("chips");
		Score.text = earnStr(earn);
		setScoreColor(earn);	

		var cards = dict.IL("cards");
		if (cards.Count <= 0) {
			return ;
		}	

		Cards[0].GetComponent<Card>().ShowServer(cards[0]);
		Cards[1].GetComponent<Card>().ShowServer(cards[1]);
	}

	private string earnStr(int earn) {
		if (earn > 0) {
			return "+" + earn.ToString();
		}

		return earn.ToString();
	}


	void setScoreColor(int earn) {
		var color = new Color();
		string str;

		if (earn > 0) {
			str = colors[0];
		} else if (earn < 0) {
			str = colors[2];
		} else {
			str = colors[1];
		}

		ColorUtility.TryParseHtmlString(str, out color);
		Score.color = color;
	}

	void ShowAvatar(string avatar) {
		StartCoroutine(downloadImage(avatar));
	}

	IEnumerator<WWW>  downloadImage(string url) {
		WWW www = new WWW(url);
		yield return www;
		Avatar.texture = _.Circular(www.texture);	
	}
}
