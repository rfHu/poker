using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System;
using MaterialUI;

public class OP : MonoBehaviour {
	public GameObject RaiseGo;
	public GameObject FoldGo;
	public GameObject CallGo;
	public GameObject CheckGo;
	public GameObject AllinGo;

	// @FIXME：R1-R4代码比较恶心，望有心人优化
	public GameObject R1;
	public GameObject R2;
	public GameObject R3;
	public GameObject R4;
	public Button AccurateCacel;
	public Button AccurateYes;
    public GameObject BuyTurnTime;

	public Text CallNumber;
	public Text CallText;
	public Slider Slid;

	private GameObject slidParent {
		get {
			return Slid.transform.parent.gameObject;
		}
	}

	public GameObject Allin;
	public Text RaiseNumber;
	public Text MaxText;
	public AccurateRaise AccuratePanel;
	public GameObject AccurateBtn;

	public GameObject RoundTipsGo;
	public Text TipsText;

	private List<int> range;
	private ModalHelper modal;

	private static Transform instance; 
	private CircleMask circleMask;
	private int accurateValue;
	private float disableAlpha = 0.4f;

	public Text BuyTimeCost; 

	private CompositeDisposable slidDisposables = new CompositeDisposable();

	void OnSpawned()
	{	
		var transform = GetComponent<RectTransform>();
		transform.anchoredPosition = new Vector2(0, 360);
	}

	void OnDespawned()
	{	
		hideModal();
		slidDisposables.Clear();
	}

	void OnDestroy()
	{
		hideModal();
		slidDisposables.Clear();
	}
	
	void Awake()
	{
		CheckGo.GetComponent<Button>().onClick.AddListener(OPS.Check);
		CallGo.GetComponent<Button>().onClick.AddListener(() => {
			OPS.Call();
		});
		RaiseGo.GetComponent<Button>().onClick.AddListener(OnRaiseClick);
	}

	public static Transform Spawn() {
		if (instance == null) {
			var ins = PoolMan.Spawn("OP", G.UICvs.transform);
			instance = ins;
		} else {
			if (PoolMan.IsSpawned(instance)) {
				// skip
			} else {
				PoolMan.Spawn("OP", G.UICvs.transform);
			}
		}

		return instance;		
	}

	private bool canCheck = false;
	private bool canAllin = false;
	
	public void StartWithCmds(Dictionary<string, object> data, int left, int buyTimeCost = 10) {
		// 设置购买时间按钮
		setBuyCost(buyTimeCost);

        var cmds = data.Dict("cmds");
		var check = cmds.Bool("check");
		var callNum = cmds.Int("call");
		var allin = cmds.Bool("all_in");

		canCheck = check;
		canAllin = allin;

		range = cmds.IL("raise");

		setAccurateBtns(AccurateType.Default); // 还原精确加注
		hideRaiseSlider(); // 还原加注拉杆

		if (check) { // 看牌
			CallGo.SetActive(false);
			toggleEnable(CheckGo, true);	
		} else if (callNum > 0) { // 跟注
			CheckGo.SetActive(false);
			CallGo.SetActive(true);
			CallNumber.text = _.Num2CnDigit(callNum);
		} else { // 不能跟注、不能看牌，展示灰掉的看牌按钮
			CallGo.SetActive(false);
			toggleEnable(CheckGo, false);	
		}		

		circleMask = FoldGo.transform.Find("CD").GetComponent<CircleMask>();
		circleMask.Enable(left, true);

		if (range.Count >= 2) { // 可加注
			AllinGo.SetActive(false);
			toggleEnable(RaiseGo, true);	
			setRaiseButtons(callNum);
		} else if(allin) { // 不可加注、可Allin
			RaiseGo.SetActive(false);
			AllinGo.SetActive(true);
			disableAllBtns();
		} else { // 不可加注、不可Allin
			disableAllBtns();
			toggleEnable(RaiseGo, false);	
		}	
	}

	private void toggleEnable(GameObject go, bool enable) {
		var cvg = go.GetComponent<CanvasGroup>();
		var btn = go.GetComponent<Button>();

		go.SetActive(true);
		btn.interactable = enable;
		cvg.alpha = enable ? 1 : disableAlpha;
	}

	public void Reset(float left) {
		if (circleMask == null) {
			return ;
		}

		circleMask.Enable(left, true);
	}

	public void OnAccurate() {
		AccuratePanel.Show(range);
		AccuratePanel.OnValueChange = (num) => {
			accurateValue = num;

			if (num >= range[1]) { // allin
				setAccurateBtns(AccurateType.Allin);
			} else if (num < range[0]) {
				setAccurateBtns(AccurateType.Cancel);
			} else {
				setAccurateBtns(AccurateType.Yes);
			}
		};
		AccuratePanel.Close = () => {
			setAccurateBtns(AccurateType.Default);
		};

		setAccurateBtns(AccurateType.Cancel);	
	}

	private void setAccurateBtns(AccurateType type) {
		AccurateBtn.GetComponent<OPColor>().ColorEnabled = true;
		AccurateBtn.GetComponent<Button>().interactable = true;

		RaiseGo.SetActive(false);
		AllinGo.SetActive(false);
		AccurateCacel.gameObject.SetActive(false);
		AccurateYes.gameObject.SetActive(false);

		if (type == AccurateType.Cancel) {
			AccurateCacel.gameObject.SetActive(true);
		} else if (type == AccurateType.Yes) {
			AccurateYes.gameObject.SetActive(true);
		} else if (type == AccurateType.Allin) {
			AllinGo.SetActive(true);
		} else {
			RaiseGo.SetActive(true);
			AccuratePanel.gameObject.SetActive(false);
		}
	}

	private void disableAccurateBtns() {
		RaiseGo.SetActive(false);
		AllinGo.SetActive(false);
		AccurateCacel.gameObject.SetActive(false);
		AccurateYes.gameObject.SetActive(false);
	}

	public void OnAllinClick() {
		OPS.AllIn();
	}

	public void OnAccurateYes() {
		OPS.raise(accurateValue);
	}

	public void OnAccurateCancel() {
		setAccurateBtns(AccurateType.Default);
	}
	
	private void setRaiseButtons(int call) {
		var pot = GameData.Shared.Pot.Value;
		var bb = GameData.Shared.BB;
		var minRaise = Mathf.Max(2 * call, bb);

		List<int> values = new List<int>(); 
		List<string> names = new List<string>();

		if (pot < 2 * bb) {
			values.Add(2 * bb);
			values.Add(3 * bb);
			values.Add(4 * bb);
			values.Add(5 * bb);

			names.Add("X2\n盲注");
			names.Add("X3\n盲注");
			names.Add("X4\n盲注");
			names.Add("X5\n盲注");
		} else {
			var nextPot = pot + call;
			var value1 = Mathf.CeilToInt(nextPot / 3f) + call;
			var value2 = Mathf.CeilToInt(nextPot / 2f) + call;
			var value3 = Mathf.CeilToInt(nextPot * 2f / 3f) + call;
			var value4 = nextPot + call;
			
			values.Add(value1);
			values.Add(value2);
			values.Add(value3);
			values.Add(value4);

			names.Add("1/3\n底池");
			names.Add("1/2\n底池");
			names.Add("2/3\n底池");
			names.Add("1倍\n底池");
		}

		addProperty(R1, names[0], values[0]);	
		addProperty(R2, names[1], values[1]);	
		addProperty(R3, names[2], values[2]);
		addProperty(R4, names[3], values[3]);

		var max = range[1];

		if (values[0] > max) {
			disableAllBtns();
		} else if (values[1] > max) {
			disableBtn(R2) ;
			disableBtn(R3);
			disableBtn(R4);
		} else if (values[2] > max) {
			disableBtn(R3);
			disableBtn(R4);
		} else if (values[3] > max) {
			disableBtn(R4);
		}

		if (values[3] < minRaise) {
			disableAllBtns();
		} else if (values[2] < minRaise) {
			disableBtn(R1);
			disableBtn(R2);
			disableBtn(R3);
		} else if (values[1] < minRaise) {
			disableBtn(R1);
			disableBtn(R2);
		} else if (values[0] < minRaise) {
			disableBtn(R1);
		}
	}

	private void addProperty(GameObject go, string text, int value) {
		go.transform.Find("Text").GetComponent<Text>().text = text;
		go.transform.Find("Number").GetComponent<Text>().text = _.Num2CnDigit(value);
		go.GetComponent<OPColor>().ColorEnabled = true;

		var btn = go.GetComponent<Button>();
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(() => {
			OPS.raise(value);
		});
		btn.interactable = true;
	}

	private void changeTipsPosition(RectTransform rect, Vector2 position, Camera camera) {
		Vector2 vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, position, camera, out vector);

		var tipsTransform = RoundTipsGo.GetComponent<RectTransform>();
		var y = tipsTransform.anchoredPosition.y;

		if (vector.x >= 0) {
			tipsTransform.anchoredPosition = new Vector2(-160, y);
		} else {
			tipsTransform.anchoredPosition = new Vector2(160, y);
		}
	}

	public void OnRaiseClick() {
		slidDisposables.Clear();
		
		slidParent.SetActive(true);
		Slid.maxValue = range[1];
		Slid.value = Slid.minValue = range[0];
		Slid.wholeNumbers = true;
		MaxText.text = _.Num2CnDigit(range[1]);

		// 未按下时，隐藏加注提示
		RoundTipsGo.SetActive(false);
		AccurateBtn.SetActive(false);

		var pointerDown = false;

		Slid.OnValueChangedAsObservable().Subscribe((value) => {
			if (value >= range[1] && canAllin) {
				Allin.SetActive(true);
				RoundTipsGo.SetActive(false);
			} else {
				Allin.SetActive(false);

				if (pointerDown) {
					RoundTipsGo.SetActive(true);
				}
			}
			
			RaiseNumber.text = _.Num2CnDigit(value);
			TipsText.text = _.Num2CnDigit(value);
		}).AddTo(slidDisposables);

		Slid.OnPointerDownAsObservable().Subscribe((pointerEvt) => {
			var rect = Slid.GetComponent<RectTransform>();

			changeTipsPosition(rect, pointerEvt.position, pointerEvt.pressEventCamera);
			pointerDown = true;
			RoundTipsGo.SetActive(true);
		}).AddTo(slidDisposables);

		Slid.OnDragAsObservable().Subscribe((dragEvt) => {
			var rect = Slid.GetComponent<RectTransform>();
			changeTipsPosition(rect, dragEvt.position, dragEvt.pressEventCamera);
		}).AddTo(slidDisposables);

		Slid.OnPointerUpAsObservable().Subscribe((_) => {
			pointerDown = false;
			RoundTipsGo.SetActive(false);
		}).AddTo(slidDisposables);

		// 展示遮罩
		modal = ModalHelper.Create();
		modal.Show(transform.parent, hideRaiseSlider);
		transform.SetAsLastSibling();

		setToggle(false);
	}

    public void OnClickBuyTime()
    {
        var data = new Dictionary<string, object>(){
			{"type",112}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() { 
			{"f", "moretime"},
			{"args", data}
        }, (redata) =>
        {
            var cost = redata.Int("show_moretime");
			setBuyCost(cost);
        });
    }

	private void setBuyCost(int cost) {
		 if (cost < 0 || GameData.Shared.Coins < cost)
		{
			BuyTurnTime.SetActive(false);
		} else {
			BuyTimeCost.text = cost.ToString();
			BuyTurnTime.SetActive(true);
		}
	}

	public void OnFoldClick() {
		if (canCheck) {
			PokerUI.Alert("当前可以看牌，您确定要弃牌？", () => {
				OPS.Fold();
			}, null);
		} else {
			OPS.Fold();
		}
	}

	public void OnSliderOK() {
		var value = (int)Slid.value;

		if (value >= range[1] && canAllin) {
			OPS.AllIn();
		} else {
			OPS.raise(value);
		}
	}

	static public void Despawn() {
		if (instance != null) {
			PoolMan.Despawn(instance);
		}
	}

	private void hideRaiseSlider() {
		hideModal();
		slidParent.SetActive(false);
		AccurateBtn.SetActive(true);
		setToggle(true);
	}

	private void hideModal() {
		if (modal != null) {
			modal.Despawn();
		}

		modal = null;
	}

	private void setToggle(bool active = true) {
		R1.transform.parent.gameObject.SetActive(active);
		RaiseGo.SetActive(active);
	}

	private void disableBtn(GameObject go) {
		var button = go.GetComponent<Button>();
		button.interactable = false;
		go.GetComponent<OPColor>().ColorEnabled = false;	
	}

	private void disableAllBtns() {
		disableBtn(R1);
		disableBtn(R2);
		disableBtn(R3);
		disableBtn(R4);

		AccurateBtn.GetComponent<OPColor>().ColorEnabled = false;
		AccurateBtn.GetComponent<Button>().interactable = false;
	}

	public class OPS {
		public static void Fold() {
			OPS.invoke("fold");	
		}

		public static void Call() {
			OPS.invoke("call");		
		}

		public static void Check() {
			OPS.invoke("check");		
		}

		public static void AllIn() {
			OPS.invoke("all_in");
		}

		private static void onerr() {
			setActive(true);		
		}

		private static void hide() {
			setActive(false);
		}

		private static void setActive(bool flag) {
			if (instance == null) {
				return ;
			}

			if (flag && GameData.Shared.Uid == PokerPlayer.PlayerBase.CurrentUid) {
				OP.Spawn();	
			} else {
				PoolMan.Despawn(instance);
			}
		}

		private static void onres(Dictionary<string, object> json, int err) {
			if (err != 0) {
				setActive(true);
			}
		}

		public static void raise(int number) {
			hide();

			Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f" , "raise"},
				{"args", number.ToString()}
			}, onres, onerr);
		}

		internal static void invoke(string f) {
			hide();

			Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f" , f}
			}, onres, onerr);
		}
	}
	
	internal enum AccurateType {
		 Cancel,
		 Yes,
		 Allin,
		 Default
	}
}
