using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Extensions;
using UniRx;
using System;

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
    public RectTransform PlayerList;
    public RectTransform MidPart;
    public Text TotalSupass;

    int cost;
    List<int> scope;

    float OddsNum;
    float[] OddsNums = { 30, 16, 10, 8, 6, 5, 4, 3.5f, 3, 2.5f, 2.2f, 2, 1.7f, 1.5f, 1.3f, 1.1f, 1, 0.8f, 0.6f, 0.5f };

    int selected;
    int WholeOUTSNum;
    List<Toggle> OUTSCards;
    List<int> selectedCards;

    bool mustBuy = false;
    private bool isFlop = false;
    IEnumerator myCoroutine;

    RectTransform _rectTransform;

    public void Init(Dictionary<string, object> data) 
    {
        var outsCard = data.IL("outs");
        var pot = data.Int("pot");
        var cost = data.Int("cost");
        var scope = data.IL("scope");
        var mustBuy = data.Int("must_buy") == 2 ? true : false;
        var time = data.Int("time");
        var uids = data.List("uids");

        isFlop = data.Int("room_state") == 4;

        _rectTransform = GetComponent<RectTransform>();
        if (mustBuy)
        {
            ExitButton.interactable = false;
            CheckAll.gameObject.SetActive(false);
            this.mustBuy = mustBuy; 
        }

        //投入金额
        this.cost = cost;
        this.scope = scope;

        // InputValue.text = cost.ToString();

        WholeOUTSNum = outsCard.Count;
        selected = outsCard.Count;
        SetOdds();



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
            PlayerList.sizeDelta += new Vector2(230, 0);
        }

        //公共牌展示
        var cards = GameData.Shared.PublicCards.ToList();

        for (int i = 0; i < cards.Count; i++)
		{
            PublicCards[i].Show(cards[i]);
		}

        //OUTS牌展示
        OUTSCards = new List<Toggle>();
        MidPart.sizeDelta += new Vector2(0, 124 * ((outsCard.Count - 1) / 7));
        selectedCards = outsCard;
        foreach (var cardNum in outsCard)
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

    private void SetCASlider()
    {
        // 最小值向上取整
        var minValue = (int)Math.Ceiling(scope[0] / OddsNum);

        // 最大值向下取整
        var maxValue = (int)Math.Floor(scope[1] / OddsNum);

        if (isFlop) {
            var limit = (int)Math.Floor(scope[1] / 3f); 
            maxValue = Math.Min(limit, maxValue);
        }
            
        CASlider.minValue = minValue;
        CASlider.maxValue = maxValue;
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

        SetCASlider();
    }


    private void SelectedChanged(bool value, int num)
    {
        selected += value ? 1 : -1;
        if (selected == 0)
        {
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
        SumInsured.text = (CASlider.value).ToString();
        DependentValue();
    }

    private void DependentValue()
    {
        int num = (int)(CASlider.value * OddsNum);
      
        ClaimAmount.text = num.ToString();
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