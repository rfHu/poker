using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
public class RecallUser : MonoBehaviour {
    public Text PlayerName;
	public RawImage Avatar;
	public Text Score;
	public GameObject[] Cards;
	public Text MaxFive;
    public Text[] ActionsText;
	public Card[] ComCards;

    private Color bClolor = new Color(5 / 255f, 150 / 255f, 213 / 255f);

	public GameObject Tag;

	private int actCount = -1;

	public enum UserTag: int {
		SmallBlind = 0,
		BigBlind,
		Dealer,
		None
	}
	
	public void Show(Dictionary<string, object> dict) {
		var uid = dict.String("uid");
		Tag.SetActive(false);

		var img = gameObject.GetComponent<Image>();

		// Is current user
		if (uid == GameData.Shared.Uid) {
			img.enabled = true;	
		} else {
			img.enabled = false;
		}

		var cardDesc = Card.GetCardDesc(dict.Int("maxFiveRank"));
		if (!string.IsNullOrEmpty(cardDesc)) {
			MaxFive.gameObject.SetActive(true);
			MaxFive.text = cardDesc;
		} else {
			MaxFive.gameObject.SetActive(false);
		}

        PlayerName.text = dict.String("name");
		ShowAvatar(uid, dict.String("avatar"));

		var earn = dict.Int("coin") - dict.Int("chips");
		Score.text = _.Number2Text(earn);
		setScoreColor(earn);	

		var cards = dict.IL("cards");
		var cardsList = new Card[] {
			Cards[0].GetComponent<Card>(),
			Cards[1].GetComponent<Card>()
		};

		if (cards.Count <= 0) {
			cardsList[0].Turnback();
			cardsList[1].Turnback();
			return ;
		}	

		cardsList[0].Show(cards[0]);
		cardsList[1].Show(cards[1]);

        var actions = dict.List("action");
        actCount = actions.Count;

		foreach(var text in ActionsText) {
			text.gameObject.SetActive(false);
		}

        for (int i = 0; i < actions.Count; i++)
        {
            ActionsText[i].gameObject.SetActive(true);

            var actDict = actions[i] as Dictionary<string, object>;
			var text = actText(actDict);
            ActionsText[i].text = text;

            var insuranceNum = actDict.Int("in_amount");
			var insu = ActionsText[i].transform.Find("InsuranceNum");

            if (insuranceNum > 0 && insu != null)
            {
				insu.gameObject.SetActive(true);
                insu.GetComponent<Text>().text = "投保" + _.Num2CnDigit(insuranceNum);
            } else if (insu != null) {
				insu.gameObject.SetActive(false);
			}
        }
	}

	private string actText(Dictionary<string, object> actDict) {
		var text = actDict.String("act");
        var actNum = actDict.Int("num");
        if (actNum != 0)
        {
			text += _.Num2CnDigit(actNum);
        }

		return text;
	}

	public void SetTag(UserTag tag) {
		if (tag == UserTag.None) {
			return ;
		}

		string text;
		Color color = bClolor;

		if (tag == UserTag.SmallBlind) {
			text = "小盲";
		} else if (tag == UserTag.BigBlind) {
			text = "大盲";
		} else {
			text = "D";
			color = G.Black;
		}

		Tag.SetActive(true);
        Tag.GetComponent<ProceduralImage>().color = bClolor;
        var UText = Tag.transform.Find("Text").GetComponent<Text>();
        UText.text = text;
        UText.color = Color.white;
	}

	public void SetComCard(List<int> list) {
		for (var i = 0; i < 5; i++) {
			ComCards[i].gameObject.SetActive(false);
		}

		for (var i = 0; i < getComCardsCount(actCount) && i < list.Count; i++) {
			var card = ComCards[i];
			card.gameObject.SetActive(true);
			card.Show(list[i]);
		}
	}

	private int getComCardsCount(int number) {
		if (number <= 1) {
			return 0;
		}

		return number + 1;
	}

	void setScoreColor(int num) {
		var color = _.GetTextColor(num);
		Score.color = color;
	}

	void ShowAvatar(string uid, string url) {
		var avt = Avatar.GetComponent<Avatar>();
		avt.Uid = uid;
		avt.SetImage(url);
	}
}
