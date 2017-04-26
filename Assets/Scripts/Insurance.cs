using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Insurance : MonoBehaviour {

    public Text Pot;
    public Text CountDown;
    public GameObject AllinPlayer;
    public List<Image> PublicCards;
    public Text Odds;
    public Text SelectNum;
    public GameObject Card;
    public Text SumInsured;
    public Text ClaimAmount;
    public Text InputValue;
    public Text AutoPurchase;

    public void Init(List<int> outCards, int pot,int cost, List<int> scope, bool mustBuy) 
    {
        RectTransform _rectTransform = GetComponent<RectTransform>();
        _rectTransform.sizeDelta += new Vector2(0, 130 * ((outCards.Count - 1) / 7));
        foreach (var cardNum in outCards)
        {
            var card = Instantiate(Card);
            card.SetActive(true);
        }
            
    }
}
