using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldCardMsg : MonoBehaviour {

    public Text Name;

    public Text Pot;

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
        Pot.text = dict.Int("pot").ToString();

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
