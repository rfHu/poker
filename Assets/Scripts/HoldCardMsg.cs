using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldCardMsg : MonoBehaviour {

    public Text Name;

    public Text Pot;

	public Text Score;

	public Text CardType;

    private Card card1
    {
        get
        {
            return CardContainers[0].CardInstance;
        }
    }

    private Card card2
    {
        get
        {
            return CardContainers[1].CardInstance;
        }
    }

    [SerializeField]
    private List<CardContainer> CardContainers;

    public void Show(Dictionary<string, object> dict) 
    {
        Name.text = dict.String("winner");
        if (dict.String("winnerid") == GameData.Shared.Uid)
        {
            Name.color = MaterialUI.MaterialColor.cyanA200;
        }
        else 
        {
            Name.color = Color.white;
        }

		// 分数
		var score = dict.Int("score");
        Score.text = _.Num2CnDigit(score, true);
		Score.color = _.GetTextColor(score);

		// 底池
		Pot.text = _.Num2CnDigit(dict.Int("pot"));

		// 牌型
		var desc = Card.GetCardDesc(dict.Int("maxFiveRank"));
		var cardTypeParent = CardType.transform.parent.gameObject;
		if (string.IsNullOrEmpty(desc)) {
			cardTypeParent.SetActive(false);
		} else {
			cardTypeParent.SetActive(true);
			CardType.text = desc;
		}

		// 手牌
        var cards = dict.IL("cards");
        if (cards.Count <= 0)
        {
            card1.Turnback();
            card2.Turnback();
            return;
        }

        card1.Show(cards[0]);
        card2.Show(cards[1]);
    }
}
