using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Insurance : MonoBehaviour {
    public Text Pot;
    public Text CountDown;
    public GameObject AllinPlayer;
    public List<Card> PublicCards;
    public Text Odds;
    public Text SelectNum;
    public GameObject Card;
    public Text SumInsured;
    public Text ClaimAmount;
    public Text InputValue;
    public Text AutoPurchase;
    public Slider CASlider;
    public Button ExitButton;
    public Button SelectAll;

    int cost;
    float OddsNum;
    float[] OddsNums = { 30, 16, 10, 8, 6, 5, 4, 3.5f, 3, 2.5f, 2.2f, 2, 1.7f, 1.5f, 1.3f, 1.1f, 1, 0.8f, 0.7f, 0.6f, 0.5f };
    int selected;
    int WholeOUTSNum;
    List<Toggle> OUTSCards;
    List<int> selectedCards;
    bool isBuy = false;

    RectTransform _rectTransform;

    public void Init(List<int> outCards, int pot,int cost, List<int> scope, bool mustBuy, int time) 
    {
        _rectTransform = GetComponent<RectTransform>();
        if (mustBuy)
            ExitButton.interactable = false;


        this.cost = cost;
        WholeOUTSNum = outCards.Count;
        selected = outCards.Count;
        OddsNum = OddsNums[selected - 1];
        Odds.text = OddsNum.ToString();
        OUTSCards = new List<Toggle>();
        CASlider.minValue = scope[0];
        CASlider.maxValue = scope[1];
        CASlider.value = CASlider.maxValue;

        //主池数字
        Pot.text = pot.ToString();

        //allin用户
        foreach (var player in GameData.Shared.Players)
        {
            if (player.Value.InGame)
            {
                if (true)
                {
                    if (player.Value.Cards != null)
                    {
                        var playerMes = Instantiate(AllinPlayer);
                        playerMes.transform.SetParent(AllinPlayer.transform.parent,false);
                        playerMes.SetActive(true);
                        playerMes.GetComponent<AllInPlayer>().Init(player.Value.Name, player.Value.Cards.Value, false);
                    }
                }
            }
            
        }

        //公共牌展示
        var cards = GameData.Shared.PublicCards.ToList();

        for (int i = 0; i < cards.Count; i++)
		{
            PublicCards[i].Show(cards[i]);
		}

        //OUTS牌展示
        _rectTransform.sizeDelta += new Vector2(0, 130 * ((outCards.Count - 1) / 7));
        selectedCards = outCards;
        foreach (var cardNum in outCards)
        {
            var card = Instantiate(Card);
            card.SetActive(true);
            card.transform.SetParent(Card.transform.parent,false);
            card.GetComponent<Card>().Show(cardNum);
            OUTSCards.Add(card.GetComponent<Toggle>());
            card.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => 
            {
                SelectedChanged(value, cardNum);
            });
        }
    }

    private void SelectedChanged(bool value, int num)
    {
        selected += value ? 1 : -1;
        if (selected == 0)
        {
            PokerUI.Toast("至少选择一张");
            OUTSCards[0].isOn = true;
            return;
        }

        if (value)
            selectedCards.Add(num);
        else
            selectedCards.Remove(num);

        OddsNum = OddsNums[selected - 1];
        Odds.text = OddsNum.ToString();
        DependentValue();
    }

    public void OnCASliderChange() 
    {
        ClaimAmount.text = (CASlider.value).ToString();
        DependentValue();
    }

    private void DependentValue()
    {
        int auto = 0;
        AutoPurchase.text = "无自动投保额";
        SumInsured.text = ((int)(CASlider.value / OddsNum)).ToString();
        int left = WholeOUTSNum - selected;
        if (left != 0)
        {
            auto = (int)((CASlider.value / OddsNum) / OddsNums[left - 1]) + 1;
            AutoPurchase.text = "剩余" + left + "张，赔率1-" + OddsNums[left - 1] + "，自动投保额" + auto;
        }

    }

    public void OnBEButtonClick() 
    {
        CASlider.value = cost;
    }

    public void OnEPButtonClick() 
    {
        CASlider.value = int.Parse(Pot.text);
    }

    public void Buy() 
    {
        if (CASlider.value == 0)
        {
            PokerUI.Toast("您的投保额为0");
            return;
        }

        isBuy = true;

        var data = new Dictionary<string, object>(){
			        {"outs", selectedCards},
                    {"amount", int.Parse(SumInsured.text)},
		        };

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "insurance"},
				{"args", data}

			});

        GetComponent<DOPopup>().Close();
    }

    public void Exit() 
    {
        GetComponent<DOPopup>().Close();
    }

    public void OnBTButton() 
    {
        var data = new Dictionary<string, object>(){
                    {"type",112}
		        };

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "moretime"},
                {"args", data}
			});
    }

    public void OnSLButtonClick() 
    {
        if (selected == WholeOUTSNum)
        {
            foreach (var item in OUTSCards)
            {
                item.isOn = false;
            }
        }
        else
        {
            foreach (var item in OUTSCards)
            {
                if (!item.isOn)
                {
                    item.isOn = true;
                }
            }
        }
    }

    void OnDestroy() 
    {
        if (isBuy)
            return;

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "noinsurance"}
			});
    }
}
