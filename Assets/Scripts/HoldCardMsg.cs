using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldCardMsg : MonoBehaviour {

    public Text Name;

    public Text Pot;

	public Text Score;

	public Text CardType;

    public HorizontalLayoutGroup HandCardsParent;

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

        //omaha特殊处理
        bool isOmaha = GameData.Shared.Type.Value == GameType.Omaha;
        CardContainers[2].gameObject.SetActive(isOmaha);
        CardContainers[3].gameObject.SetActive(isOmaha);
        HandCardsParent.spacing = isOmaha ? -46 : 6;


		// 手牌
        var cards = dict.IL("cards");

        if (cards.Count <= 0)
        {
            foreach (var card in CardContainers)
            {
                card.CardInstance.Turnback();
            }
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            CardContainers[i].CardInstance.Show(cards[i]);
        }
    }
}
