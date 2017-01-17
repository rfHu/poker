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
	// public Color[] Colors = new Color[]{
	// 	new Color(),
	// 	new Color(),
	// 	new Color(1, 1, 1, 1)
	// };

	public void Show(Dictionary<string, object> dict) {
		ShowAvatar(dict.String("avatar"));
		Name.text = dict.String("name");

		var earn = dict.Int("coin") - dict.Int("chips");
		Score.text = earn.ToString();

		// setScoreColor();	

		var cards = dict.IL("cards");

		if (cards.Count <= 0) {
			return ;
		}	

		Cards[0].GetComponent<Card>().ShowServer(cards[0]);
		Cards[0].GetComponent<Card>().ShowServer(cards[1]);
	}


	// void setScoreColor(int earn) {
	// 	if (earn > 0) {
	// 		Score.color = Colors[0];
	// 	} else if (earn < 0) {
	// 		Score.color = Colors[1];
	// 	} else {
	// 		Score.color = Colors[2];
	// 	}
	// }

	void ShowAvatar(string avatar) {
		StartCoroutine(downloadImage(avatar));
	}

	IEnumerator<WWW>  downloadImage(string url) {
		WWW www = new WWW(url);
		yield return www;
		Avatar.texture = _.Circular(www.texture);	
	}
}
