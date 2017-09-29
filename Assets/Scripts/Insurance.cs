﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System;
using UnityEngine.UI.ProceduralImage;
using MaterialUI;
using UnityEngine.EventSystems;

public interface InsuranceStruct {
	bool isBuyer {get; set;}
	bool mustBuy {get; set;}

	void OnSelectedChange(InsuranceOuts insOuts);

	int SelectedCount{get;}
} 

public class Insurance : MonoBehaviour, InsuranceStruct {
    public Text Pot;
    public Text CountDown;
    public Text AllinTitle;
    public Transform AllinPlayersParent;
    public Text Odds;
    public Text SumInsured;
    public Text ClaimAmount;
    public Slider CASlider;
    public VButton BuyButton;
    public Button ExitButton;
    public CButton EqualButton;
    public CButton BreakEventButton;
    public GameObject BuyTime;
    public Text CardDesc;
    public Text BuyButtonNum;
    public GameObject BuyerButtons;
    public GameObject WatcherText;
    public Text WinRate;
  
    [SerializeField]
    private List<CardContainer> PublicCardContainers;

	private IDisposable throttle;

	public void OnSelectedChange(InsuranceOuts insOuts) {
		// 节流，防止执行太频繁
		if (throttle != null) {
			throttle.Dispose();
		}

		throttle = Observable.Timer(TimeSpan.FromMilliseconds(50)).Subscribe((_) => {
			SetOdds();

			if (isBuyer) {
        		DependOnClaimAmount();
			}
        	RPCRsyncInsurance();
		}).AddTo(this);
	}

	public int SelectedCount  {
		get {
			return insuranceOutsList[0].SelectedOuts.Count + insuranceOutsList[1].SelectedOuts.Count;
		}
	}

    public List<Transform> MyCards
    {
        get
        {
            return MyCardContainers.Select(o => o.CardInstance.transform).ToList();
        }
    }
    [SerializeField]
    private List<CardContainer> MyCardContainers; 

    float odds;
    float[] OddsNums = { 30, 16, 10, 8, 6, 5, 4, 3.5f, 3, 2.5f, 2.2f, 2, 1.7f, 1.5f, 1.3f, 1.1f, 1, 0.8f, 0.6f, 0.5f };

    private int potValue;
    int cost;

    // scope是赔付额
    List<int> scope;

	[SerializeField] private List<InsuranceOuts> insuranceOutsList;

    public bool mustBuy {get; set;}
    public bool isBuyer {get; set;}
    private bool isFlop = false;

    private List<int> SelectedOuts {
		get {
			var list = insuranceOutsList[0].SelectedOuts.Union(insuranceOutsList[1].SelectedOuts);
			return list.ToList();
		}
	}

    IEnumerator myCoroutine;


    public void Init(Dictionary<string, object> data, bool isBuyer = false) 
    {
        GetComponent<DOPopup>().Show();

        this.isBuyer = isBuyer;

        List<int> loseOuts = data.IL("lose_outs");
        List<int> tieOuts = data.IL("tie_outs");

        potValue = data.Int("pot");
        cost = data.Int("cost");
        scope = data.IL("scope");
        mustBuy = data.Int("must_buy") == 2;
        isFlop = (data.Int("room_state") == 4);
        List<object> allinPlayers = data.List("outs_count");
        int myRate = data.Int("win_rate");

		insuranceOutsList[0].SetOuts(loseOuts, this);
		insuranceOutsList[1].SetOuts(tieOuts, this);

        //设置倒计时动画
        var time = data.Int("time");
        myCoroutine = Timer(time);
        StartCoroutine(myCoroutine);
        BuyTime.SetActive(true);

        ExitButton.gameObject.SetActive(!(mustBuy && isBuyer));

        //区分观看者和购买者
        BuyerButtons.SetActive(isBuyer);
        WatcherText.SetActive(!isBuyer);
        CASlider.interactable = isBuyer;

        SetOdds();
        CASlider.value = CASlider.minValue;

        Pot.text = potValue.ToString();

        int maxPercent = -1;
		if (data.ContainsKey("win_rate")) {
			maxPercent = getMaxPercent(allinPlayers, myRate);
		}
        setupAllinPlayers(allinPlayers, maxPercent); 

        setupPbCards();

        addEvents();

        //底部个人信息
        var buyPlayer = GameData.Shared.FindPlayer(data.String("uid"));
        for (int i = 0; i < MyCards.Count; i++)
        {
            MyCards[i].GetComponent<Card>().Show(buyPlayer.Cards.Value[i]);
        }
        CardDesc.text = Card.GetCardDesc(data.Int("maxFiveRank"));
        // WinRate.text = myRate + "%";
        // string color = myRate == maxPercent ? "#ff1744" : "#868d94";
        // WinRate.transform.parent.gameObject.SetActive(maxPercent != -1);
        // WinRate.transform.parent.GetComponent<ProceduralImage>().color = _.HexColor(color);
    }

    private int getMaxPercent(List<object> allinPlayers ,int rate) 
    {
        foreach (var obj in allinPlayers)
        {
            var data = obj as Dictionary<string, object>;
			var other = data.Int("win_rate");
			rate = rate > other ? rate : other;
        }
        return rate;
    }

    private void setupAllinPlayers(List<object> allinPlayers, int maxPercent)
    {

        AllinTitle.text = "落后玩家(" + allinPlayers.Count + ")";

        foreach (var obj in allinPlayers)
        {
            var data = obj as Dictionary<string, object>;
            var playerMes = PoolMan.Spawn("InsureAllInPlayer",AllinPlayersParent.transform);
            playerMes.GetComponent<AllInPlayer>().Init(data, maxPercent);
        }
    }

    private void setupPbCards() {
        var cards = GameData.Shared.PublicCards.ToList();
		var pbCards = PublicCardContainers.Select(o => o.CardInstance).ToList();

        for (int i = 0; i < pbCards.Count; i++)
		{
            var card = pbCards[i];
            if (i < cards.Count)
            {
                card.gameObject.SetActive(true);
                card.Show(cards[i]);
            }
            else 
            {
                card.gameObject.SetActive(false);
            }
		}
    }

    private void addEvents() {
        //加时
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

        //中途进入房间逻辑
        RxSubjects.Look.Subscribe((e) => {
            GetComponent<DOPopup>().Close();
        }).AddTo(this);

        //直接退出不购买保险
        ExitButton.OnClickAsObservable().Subscribe((_) => {
            if (isBuyer)
            {
                Connect.Shared.Emit(new Dictionary<string, object>() { 
                    {"f", "noinsurance"},
                });
            }
            GetComponent<DOPopup>().Close();
        }).AddTo(this);

		 RxSubjects.RsyncInsurance.Subscribe((e) => {
            if (e.Data.Int("closeflag") == 1)
            {
                Exit();
                return;
            }

            int CASlidernum = e.Data.Int("CASlidernum");
            CASlider.value = CASlidernum;
        }).AddTo(this);
    }

    private void SetCASlider()
    {
		if (scope.Count < 2) {
			BuyButton.interactable = false;
			CASlider.minValue = CASlider.maxValue = 0;
			EqualButton.interactable = false;
			BreakEventButton.interactable = false;
			return ;
		}	

        // 最小值向上取整
        var minValue = Mathf.CeilToInt(scope[0] / odds);

        // 最大值向下取整且不能超过底池
        var maxValue = Mathf.Min(Mathf.FloorToInt(scope[1] / odds), scope[1]);

        if (maxValue < minValue) {
            BuyButton.interactable = false;
        } else {
            BuyButton.interactable = true;
        }

        //翻牌圈限制
        if (isFlop) {
            var limit = (int)Math.Floor(potValue / 3f); 
            maxValue = Math.Min(limit, maxValue);
        }
            
        CASlider.minValue = minValue;
        CASlider.maxValue = maxValue;

        EqualButton.interactable = !(eqValue > maxValue) ;
        BreakEventButton.interactable = !(beValue > maxValue);
    }

    /// <summary>
    /// 选牌数改变导致赔率和显示的改变
    /// </summary>
    private void SetOdds()
    {

        int num = SelectedCount - 1;
        if (num > 19)
        {
            num = 19;
        } else if (num < 0) {
            num = 0;
        }

        odds = OddsNums[num];
        Odds.text = odds.ToString();
        BreakEventButton.transform.Find("Text").GetComponent<Text>().text = beValue.ToString();
        EqualButton.transform.Find("Text").GetComponent<Text>().text = eqValue.ToString();
        SetCASlider();
    }

    public void OnCASliderChange() 
    {
		sliderChange();
		RPCRsyncInsurance();
    }

	private void sliderChange() {
		SumInsured.text = (CASlider.value).ToString();
        ClaimAmount.text = claimAmountValue.ToString();
        BuyButtonNum.text = CASlider.value.ToString();
	}

    private void DependOnClaimAmount() {
        var c = int.Parse(ClaimAmount.text);
        var buyValue = (int)Math.Round(c / odds); 
    
        CASlider.value = buyValue;
        sliderChange();
    }

    private int claimAmountValue {
        get {
            return (int)Math.Ceiling(CASlider.value * odds);
        }
    }
   
    private int beValue {
        get {
            return (int)Math.Ceiling(cost / odds);
        }
    }

    private int eqValue {
        get {
            return (int)Math.Ceiling(potValue / (odds + 1));
        }
    }

    //保本按钮
    public void OnBEButtonClick() 
    {
        CASlider.value = beValue;
        RPCRsyncInsurance();
    }

    //等利按钮
    public void OnEPButtonClick() 
    {
        CASlider.value = eqValue;
        RPCRsyncInsurance();
    }

    public void Buy() 
    {
        var data = new Dictionary<string, object>(){
			        {"outs", SelectedOuts},
                    {"amount", (int)(CASlider.value)},
		        };

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "insurance"},
				{"args", data}

			});

        RPCRsyncInsurance(1);
        GetComponent<DOPopup>().Close();
    }

    public void Exit() 
    {
        if (isBuyer)
        {
            Connect.Shared.Emit(new Dictionary<string, object>() { 
                {"f", "noinsurance"},
            });

            RPCRsyncInsurance(1);
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

    private IEnumerator Timer(float time) 
    {
        while (time > 0)
        {
            time = time - Time.deltaTime;
            CountDown.text = ((int)time).ToString() + "s";

            yield return new WaitForFixedUpdate();
        }

        GetComponent<DOPopup>().Close();
    }

    void RPCRsyncInsurance(int closeFlag = 0)
    {
		if (!isBuyer) {
			return ;
		}

        var data = new Dictionary<string, object>(){
			        {"selectedOuts", SelectedOuts},
                    {"CASlidernum", (int)(CASlider.value)},
                    {"closeflag", closeFlag}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "rsyncinsurance"},
				{"args", data}
		});
    }

    void OnDespawned() 
    {
        this.Dispose();

        for (int i = AllinPlayersParent.childCount - 1; i > -1; i--)
        {
            var child = AllinPlayersParent.GetChild(i);
            if (PoolMan.Contains(child))
            {
                PoolMan.Despawn(child);
            }
        }
    }
}