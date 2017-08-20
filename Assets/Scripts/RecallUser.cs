using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Linq;

public class RecallUser : MonoBehaviour {
    public Text PlayerName;
	public RawImage Avatar;
	public Text Score;
	public Text MaxFive;
    public Text[] ActionsText;

    private Card card1
    {
        get
        {
            return CardContainers[0].CardInstance;
        }
    }

	private Card card2 {
		get {
			return CardContainers[1].CardInstance;
		}
	}

	private List<Card> _comCards;

    public List<Card> ComCards
    {
        get
        {
			if (_comCards == null) {
				_comCards = ComCardContainers.Select(o => o.CardInstance).ToList();
			}

            return _comCards;
        }
    }

    [SerializeField]
    private List<CardContainer> ComCardContainers;
    [SerializeField]
    private List<CardContainer> CardContainers; 

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

		if (cards.Count <= 0) {
			card1.Turnback();
			card2.Turnback();
			return ;
		}	

		card1.Show(cards[0]);
		card2.Show(cards[1]);

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
		Color bgColor = bClolor;
		Color textColor = Color.white;

		if (tag == UserTag.SmallBlind) {
			text = "小盲";
		} else if (tag == UserTag.BigBlind) {
			text = "大盲";
		} else {
			text = "D";
			bgColor = Color.white;
			textColor = Color.black;
		}

		Tag.SetActive(true);
        Tag.GetComponent<ProceduralImage>().color = bgColor;
        var UText = Tag.transform.Find("Text").GetComponent<Text>();
        UText.text = text;
        UText.color = textColor;
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
