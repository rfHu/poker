﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using UnityEngine.UI.ProceduralImage;

public class RecallUser : MonoBehaviour {
    public Text PlayerName;
	public RawImage Avatar;
	public Text Score;
	public GameObject[] Cards;
	public Text MaxFive;
    public Text[] ActionsText;
    private int publicCardNum;

    public int PublicCardNum
    {
        get { return publicCardNum; }
    }
	
	public void Show(Dictionary<string, object> dict) {
		var uid = dict.String("uid");

		// Is current user
		if (uid == GameData.Shared.Uid) {
			gameObject.GetComponent<Image>().enabled = true;	
		}

		var cardDesc = Card.GetCardDesc(dict.Int("maxFiveRank"));
		if (!string.IsNullOrEmpty(cardDesc)) {
			MaxFive.gameObject.SetActive(true);
			MaxFive.text = cardDesc;
		} else {
			MaxFive.gameObject.SetActive(false);
		}

        PlayerName.text = dict.String("name");
		ShowAvatar(dict.String("avatar"));

		var earn = dict.Int("coin") - dict.Int("chips");
		Score.text = _.Number2Text(earn);
		setScoreColor(earn);	

		var cards = dict.IL("cards");
		if (cards.Count <= 0) {
			return ;
		}	

		Cards[0].GetComponent<Card>().Show(cards[0]);
		Cards[1].GetComponent<Card>().Show(cards[1]);


        var actions = dict.List("action");
        publicCardNum = actions.Count;

        for (int i = 0; i < actions.Count; i++)
        {
            ActionsText[i].gameObject.SetActive(true);

            var actDict = actions[i] as Dictionary<string, object>;
            ActionsText[i].text = actDict.String("act");
            var actNum = actDict.Int("num");
            if (ActionsText[i].text != "" && actNum != 0)
            {
                ActionsText[i].text += _.Number2Text(actNum);
            }

            var insuranceNum = actDict.Int("in_amount");

            if (insuranceNum > 0)
            {
                ActionsText[i].transform.FindChild("InsuranceNum").gameObject.SetActive(true);
                ActionsText[i].transform.FindChild("InsuranceNum").GetComponent<Text>().text = "投保" + _.Number2Text(insuranceNum);
            } else {
			}
        }
	}

	void setScoreColor(int num) {
		var color = _.GetTextColor(num);
		Score.color = color;
	}

	void ShowAvatar(string url) {
		StartCoroutine(_.LoadImage(url, (texture) => {
			Avatar.texture = _.Circular(texture);
		}));
	}
}
