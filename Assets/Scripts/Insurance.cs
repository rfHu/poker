﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Extensions;
using UniRx;

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
    public Button CheckAll;
    public Toggle CheckAllToggle;
    public GameObject BuyTime;
    public RectTransform MidPart;

    int cost;

    float OddsNum;
    float[] OddsNums = { 30, 16, 10, 8, 6, 5, 4, 3.5f, 3, 2.5f, 2.2f, 2, 1.7f, 1.5f, 1.3f, 1.1f, 1, 0.8f, 0.6f, 0.5f };
    int selected;
    int WholeOUTSNum;
    List<Toggle> OUTSCards;
    List<int> selectedCards;
    bool mustBuy = false;
    IEnumerator myCoroutine;

    RectTransform _rectTransform;

    public void Init(List<int> outCards, int pot,int cost, List<int> scope, bool mustBuy, int time, List<object> uids) 
    {

        _rectTransform = GetComponent<RectTransform>();
        if (mustBuy)
        {
            ExitButton.interactable = false;
            CheckAll.interactable = false;
            this.mustBuy = mustBuy; 
        }

        this.cost = cost;
        InputValue.text = cost.ToString();

        WholeOUTSNum = outCards.Count;
        selected = outCards.Count;
        SetOdds();

        CASlider.minValue = scope[0];
        CASlider.maxValue = scope[1];
        CASlider.value = CASlider.maxValue;

        //主池数字
        Pot.text = pot.ToString();

        myCoroutine = Timer(time);
        StartCoroutine(myCoroutine);

        //allin用户
        foreach (var uid in uids)
        {
            var player = GameData.Shared.FindPlayer((string)uid);

            var playerMes = Instantiate(AllinPlayer);
            playerMes.transform.SetParent(AllinPlayer.transform.parent,false);
            playerMes.SetActive(true);
            playerMes.GetComponent<AllInPlayer>().Init(player.Name, player.Cards.Value, player.Uid);
            MidPart.sizeDelta += new Vector2(230, 0);
            
        }

        //公共牌展示
        var cards = GameData.Shared.PublicCards.ToList();

        for (int i = 0; i < cards.Count; i++)
		{
            PublicCards[i].Show(cards[i]);
		}

        //OUTS牌展示
        OUTSCards = new List<Toggle>();
        _rectTransform.sizeDelta += new Vector2(0, 125 * ((outCards.Count - 1) / 7));
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
            if (mustBuy)
                card.GetComponent<Toggle>().interactable = false;
        }

        RxSubjects.Moretime.Subscribe((e) =>{
            var model = e.Data.ToObject<MoreTimeModel>();

            if (model.IsRound())
            {
                return;
            }

            StopCoroutine(myCoroutine);
            myCoroutine = Timer(model.total);
            StartCoroutine(myCoroutine);
        }).AddTo(this);

    }

    private void SetOdds()
    {
        SelectNum.text = selected.ToString();

        int num = selected - 1;
        if (num > 19)
        {
            num = 19;
        }
        OddsNum = OddsNums[num];
        Odds.text = OddsNum.ToString();
    }


    private void SelectedChanged(bool value, int num)
    {
        selected += value ? 1 : -1;
        if (selected == 0)
        {
            OUTSCards[0].isOn = true;
            return;
        }


        if (selected == WholeOUTSNum)
            CheckAllToggle.isOn = true;
        else 
        {
            CheckAllToggle.isOn = false;
        }

        if (value)
            selectedCards.Add(num);
        else
            selectedCards.Remove(num);

        SetOdds();
        DependentValue();
    }

    public void OnCASliderChange() 
    {
        DependentValue();
        ClaimAmount.text = (CASlider.value).ToString();
    }

    private void DependentValue()
    {
        int auto = 0;
        AutoPurchase.text = "无自动投保额";
        int sumNum = ((int)(CASlider.value / OddsNum));
        //if (CASlider.value % OddsNum != 0)
        //    sumNum++;

        if (sumNum > int.Parse(Pot.text)/3)
        {
            CASlider.value = int.Parse(Pot.text) / 3 * OddsNum;
            return;
        }
      
        SumInsured.text = sumNum.ToString();

        int left = WholeOUTSNum - selected;
        if (left != 0)
        {
            if (left > 20)
                left = 20;

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
        CASlider.value = int.Parse(Pot.text) * OddsNum / (1 + OddsNum);
    }

    public void Buy() 
    {
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
        if (!mustBuy)
        {
            Connect.Shared.Emit(new Dictionary<string, object>() { 
                {"f", "noinsurance"},
            });
        } else {
            var data = new Dictionary<string, object>(){
                {"outs", selectedCards},
                {"amount", CASlider.minValue},
            };

            Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "insurance"},
                {"args", data}
			});
        }

        GetComponent<DOPopup>().Close();
    }

    public void OnBTButton() 
    {
        var data = new Dictionary<string, object>(){
                    {"type",111}
		        };

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "moretime"},
                {"args", data}
			});

        BuyTime.SetActive(false);
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

    private IEnumerator Timer(float time) 
    {
        while (time > 0)
        {
            time = time - Time.deltaTime;
            CountDown.text = ((int)time).ToString();

            yield return new WaitForFixedUpdate();
        }

        Exit();
    }
}