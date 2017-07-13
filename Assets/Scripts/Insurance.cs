using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System;
using UnityEngine.UI.ProceduralImage;
using MaterialUI;
using UnityEngine.EventSystems;

public class Insurance : MonoBehaviour {
    public Text Pot;
    public Text CountDown;
    public Text AllinTitle;
    public Transform AllinPlayersParent;
    public List<Card> PublicCards;
    public Text Odds;
    public Text SelectNum;
    public Transform OutsCardsParent;
    public Text SumInsured;
    public Text ClaimAmount;
    public Slider CASlider;
    public VButton BuyButton;
    public Button ExitButton;
    public CButton EqualButton;
    public CButton BreakEventButton;
    public Toggle CheckAllToggle;
    public GameObject BuyTime;
    public Text TotalSupass;
    public List<Card> MyCards;
    public Text CardDesc;
    public Text BuyButtonNum;
    public GameObject BuyerButtons;
    public GameObject WatcherText;
    public EventTrigger CASliderUp;

    private CompositeDisposable disposables = new CompositeDisposable();

    float OddsNum;
    float[] OddsNums = { 30, 16, 10, 8, 6, 5, 4, 3.5f, 3, 2.5f, 2.2f, 2, 1.7f, 1.5f, 1.3f, 1.1f, 1, 0.8f, 0.6f, 0.5f };

    private List<int> outsCardArray;
    private int potValue;
    int cost;
    List<int> scope;
    bool mustBuy = false;
    private bool isFlop = false;
    bool isBuyer;
    HashSet<int> selectedCards; 
    List<Toggle> OUTSCards = new List<Toggle>();
    List<int> isoffToggles = new List<int>();

    IEnumerator myCoroutine;


    public void Init(Dictionary<string, object> data, bool isBuyer = false) 
    {
        this.isBuyer = isBuyer;

        outsCardArray = data.IL("outs");
        potValue = data.Int("pot");
        cost = data.Int("cost");
        scope = data.IL("scope");
        mustBuy = data.Int("must_buy") == 2;
        isFlop = (data.Int("room_state") == 4);
        List<object> allinPlayers = data.List("outs_count");
        selectedCards = new HashSet<int>(outsCardArray.ToList());
        isoffToggles.Clear();

        var time = data.Int("time");
        myCoroutine = Timer(time);
        StartCoroutine(myCoroutine);

        BuyTime.SetActive(true);
        if (mustBuy && isBuyer)
        {
            ExitButton.gameObject.SetActive(false);
        }
        else 
        {
            ExitButton.gameObject.SetActive(true);
        }

        //必须购买设置
        CheckAllToggle.gameObject.SetActive(!mustBuy);

        //区分观看者和购买者
        BuyerButtons.SetActive(isBuyer);
        WatcherText.SetActive(!isBuyer);
        CheckAllToggle.interactable = isBuyer;
        CASlider.interactable = isBuyer;


        SetOdds();
        CASlider.value = CASlider.minValue;

        Pot.text = potValue.ToString();
        TotalSupass.text = "/ " + outsCardArray.Count.ToString();

        setupAllinPlayers(allinPlayers); 
        setupPbCards();
        setupOutsCards();

        addEvents();

        var buyPlayer = GameData.Shared.FindPlayer(data.String("uid"));

        for (int i = 0; i < MyCards.Count; i++)
        {
            MyCards[i].Show(buyPlayer.Cards.Value[i]);
        }

        CardDesc.text = Card.GetCardDesc(data.Int("maxFiveRank"));
    }

    private void setupAllinPlayers(List<object> allinPlayers)
    {

        AllinTitle.text = "落后玩家(" + allinPlayers.Count + ")";

        foreach (var obj in allinPlayers)
        {
            var data = obj as Dictionary<string, object>;
            var player = GameData.Shared.FindPlayer(data.String("uid"));
            var outsNumber = data.Int("ct");

            var playerMes = PoolMan.Spawn("InsureAllInPlayer",AllinPlayersParent.transform);
            playerMes.GetComponent<AllInPlayer>().Init(player.Name, player.Cards.Value, player.Uid, outsNumber);
        }
    }

    private void setupPbCards() {
        var cards = GameData.Shared.PublicCards.ToList();

        for (int i = 0; i < PublicCards.Count; i++)
		{
            var card = PublicCards[i];
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

    private void setupOutsCards() {
        OUTSCards.Clear();

        foreach (var cardNum in outsCardArray)
        {
            var card = PoolMan.Spawn("InsureCard",OutsCardsParent.transform);
            card.GetComponent<Card>().Show(cardNum);
            OUTSCards.Add(card.GetComponent<Toggle>());

            var toggle = card.GetComponent<Toggle>();

            toggle.OnValueChangedAsObservable().Subscribe((bool value) => 
            {
                SelectedChanged(value, cardNum, toggle);
                if (isBuyer)
                {
                    if (!value)
                    {
                        isoffToggles.Add(outsCardArray.IndexOf(cardNum));
                    }
                    else 
                    {
                        isoffToggles.Remove(outsCardArray.IndexOf(cardNum));
                    }
                    RPCRsyncInsurance();
                }
            }).AddTo(disposables);

            if (mustBuy || !isBuyer)
            {
                card.GetComponent<Toggle>().interactable = false;
            }
            else 
            {
                card.GetComponent<Toggle>().interactable = true;
            }
        }
    }

    private void addEvents() {
        RxSubjects.Moretime.Subscribe((e) =>{
            var model = e.Data.ToObject<MoreTimeModel>();

            if (model.IsRound())
            {
                return;
            }

            StopCoroutine(myCoroutine);
            myCoroutine = Timer(model.total);
            StartCoroutine(myCoroutine);
        }).AddTo(disposables);

        RxSubjects.Look.Subscribe((e) => {
            GetComponent<DOPopup>().Close();
        }).AddTo(disposables);

        RxSubjects.RsyncInsurance.Subscribe((e) => {
            if (e.Data.Int("closeflag") == 1)
            {
                Exit();
                return;
            }
            int CASlidernum = e.Data.Int("CASlidernum");
            List<int> isoff = e.Data.IL("isoff");

            var idx = 0;
            foreach(var card in OUTSCards) {
                if (isoff.Contains(idx)) {
                    card.isOn = false;
                } else {
                    card.isOn = true;
                }

                idx++;
            }

            CASlider.value = CASlidernum;
        }).AddTo(disposables);

        ExitButton.OnClickAsObservable().Subscribe((_) => {
            if (isBuyer)
            {
                Connect.Shared.Emit(new Dictionary<string, object>() { 
                    {"f", "noinsurance"},
                });
            }
            GetComponent<DOPopup>().Close();
        }).AddTo(disposables);
    }

    public void ClickUp() 
    {
        if (!isBuyer)
        {
            return;
        }

        RPCRsyncInsurance();
    }

    private void SetCASlider()
    {
        // 最小值向上取整
        var minValue = (int)Math.Ceiling(scope[0] / OddsNum);

        // 最大值向下取整
        var maxValue = (int)Math.Floor(scope[1] / OddsNum);

        if (maxValue < minValue) {
            BuyButton.interactable = false;
        } else {
            BuyButton.interactable = true;
        }

        if (isFlop) {
            var limit = (int)Math.Floor(potValue / 3f); 
            maxValue = Math.Min(limit, maxValue);
        }
            
        CASlider.minValue = minValue;
        CASlider.maxValue = maxValue;

        if (eqValue > maxValue) {
            EqualButton.interactable = false;
        }

        if (beValue > maxValue) {
            BreakEventButton.interactable = false;
        }
    }

    private void SetOdds()
    {
        var selected = selectedCards.Count;

        SelectNum.text = selected.ToString();

        int num = selected - 1;
        if (num > 19)
        {
            num = 19;
        } else if (num < 0) {
            num = 0;
        }

        OddsNum = OddsNums[num];
        Odds.text = OddsNum.ToString();
        BreakEventButton.transform.Find("Text").GetComponent<Text>().text = beValue.ToString();
        EqualButton.transform.Find("Text").GetComponent<Text>().text = eqValue.ToString();
        SetCASlider();
    }


    private void SelectedChanged(bool value, int num, Toggle toggle)
    {
        if (value) {
            selectedCards.Add(num);
        }
        else if (!value) {
            selectedCards.Remove(num);
        }

        if (selectedCards.Count == outsCardArray.Count) {
            SetCheckAllToggle(true);
        }
        else 
        {
            SetCheckAllToggle(false);
        }

        SetOdds();
        DependOnClaimAmount();

        if (selectedCards.Count == 0) {
            toggle.isOn = true;
        }
    }

    public void OnCASliderChange() 
    {
        SumInsured.text = (CASlider.value).ToString();
        ClaimAmount.text = claimAmountValue.ToString();
        BuyButtonNum.text = CASlider.value.ToString();
    }

    private void DependOnClaimAmount() {
        var c = int.Parse(ClaimAmount.text);
        var buyValue = (int)Math.Round(c / OddsNum); 
    
        CASlider.value = buyValue;
        OnCASliderChange();
    }

    private int claimAmountValue {
        get {
            return (int)Math.Ceiling(CASlider.value * OddsNum);
        }
    }
   
    private int beValue {
        get {
            return (int)Math.Ceiling(cost / OddsNum);
        }
    }

    private int eqValue {
        get {
            return (int)Math.Ceiling(potValue / (OddsNum + 1));
        }
    }

    public void OnBEButtonClick() 
    {
        CASlider.value = beValue;
        RPCRsyncInsurance();
    }

    public void OnEPButtonClick() 
    {
        CASlider.value = eqValue;
        RPCRsyncInsurance();
    }

    public void Buy() 
    {
        var data = new Dictionary<string, object>(){
			        {"outs", selectedCards.ToList()},
                    {"amount", int.Parse(SumInsured.text)},
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

    public void OnSLButtonClick() 
    {

        if (mustBuy || !isBuyer) {
            return ;
        }
            
        if (selectedCards.Count == outsCardArray.Count) 
        {
            var count = OUTSCards.Count;

            for (var i = count - 1; i >= 0; i--) {
                OUTSCards[i].isOn = false;
            }
        }
        else 
        {
            foreach (var item in OUTSCards)
            {
                item.isOn = true;
            }
        }

        SetOdds();
    }

    private void SetCheckAllToggle(bool isOn) {
        CheckAllToggle.isOn = isOn;
        
        var img = CheckAllToggle.transform.Find("Background").GetComponent<ProceduralImage>();

        if (isOn) {
            img.color = MaterialUI.MaterialColor.cyanA200;            
        } else {
            img.color = MaterialUI.MaterialColor.grey400;
        }
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
        var data = new Dictionary<string, object>(){
			        {"isoff", isoffToggles},
                    {"CASlidernum", int.Parse(SumInsured.text)},
                    {"closeflag", closeFlag}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "rsyncinsurance"},
				{"args", data}
		});
    }

    void OnDespawned() 
    {
        disposables.Clear();

        for (int i = AllinPlayersParent.childCount - 1; i > -1; i--)
        {
            var child = AllinPlayersParent.GetChild(i);
            if (PoolMan.Contains(child))
            {
                PoolMan.Despawn(child);
            }
        }

        for (int i = OutsCardsParent.childCount - 1; i > -1; i--)
        {
            PoolMan.Despawn(OutsCardsParent.GetChild(i));
        }
    }
}