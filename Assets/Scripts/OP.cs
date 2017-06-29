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
	public GameObject R1;
	public GameObject R2;
	public GameObject R3;
	public Button AccurateCacel;
	public Button AccurateYes;
    public GameObject BuyTurnTime;

	public Text CallNumber;
	public Text CallText;
	public Slider Slid;
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

	void OnSpawned()
	{	
		var transform = GetComponent<RectTransform>();
		transform.anchoredPosition = new Vector2(0, 360);
	}

	void OnDespawned()
	{	
		hideModal();
	}

	void OnDestroy()
	{
		hideModal();
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
	
	public void StartWithCmds(Dictionary<string, object> data, int left) {
        var cmds = data.Dict("cmds");
		var check = cmds.Bool("check");
		var callNum = cmds.Int("call");
		var allin = cmds.Bool("all_in");

		range = cmds.IL("raise");

		setAccurateBtns(AccurateType.Default); // 还原精确加注
		hideRaiseSlider(); // 还原加注拉杆
		BuyTurnTime.SetActive(true); // 还原加时按钮

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
		AccurateBtn.GetComponent<CanvasGroup>().alpha = 1;
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

		List<int> values = new List<int>(); 
		List<string> names = new List<string>();

		if (pot < 2 * bb) {
			values.Add(2 * bb);
			values.Add(3 * bb);
			values.Add(4 * bb);

			names.Add("X2\n盲注");
			names.Add("X3\n盲注");
			names.Add("X4\n盲注");
		} else {
			var nextPot = pot + call;
			var value1 = fixBB(Mathf.CeilToInt(nextPot / 2f) + call);
			var value2 = fixBB(Mathf.CeilToInt(nextPot * 2f / 3f) + call);
			var value3 = fixBB(nextPot + call);
			
			values.Add(value1);
			values.Add(value2);
			values.Add(value3);

			names.Add("1/2\n底池");
			names.Add("2/3\n底池");
			names.Add("1倍\n底池");
		}

		addProperty(R1, names[0], values[0]);	
		addProperty(R2, names[1], values[1]);	
		addProperty(R3, names[2], values[2]);

		var max = range[1];
		if (values[0] > max) {
			disableAllBtns();
		} else if (values[1] > max) {
			disableBtn(R2) ;
			disableBtn(R3);
		} else if (values[2] > max) {
			disableBtn(R3);
		}
	}

	private int fixBB(int value) {
		var num = Mathf.CeilToInt(value / (float)GameData.Shared.BB);
		return num * GameData.Shared.BB;
	}

	private void addProperty(GameObject go, string text, int value) {
		go.transform.Find("Text").GetComponent<Text>().text = text;
		go.transform.Find("Number").GetComponent<Text>().text = _.Num2CnDigit(value);
		go.GetComponent<CanvasGroup>().alpha = 1;

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
		Slid.gameObject.SetActive(true);
		Slid.value = Slid.minValue = range[0];
		Slid.maxValue = range[1];
		Slid.wholeNumbers = true;
		MaxText.text = _.Num2CnDigit(range[1]);

		// 未按下时，隐藏加注提示
		RoundTipsGo.SetActive(false);
		AccurateBtn.SetActive(false);

		var pointerDown = false;

		Slid.OnValueChangedAsObservable().Subscribe((value) => {
			int newValue;

			if (value >= range[1]) {
				newValue = (int)value;
			} else {
				newValue = value.StepValue(GameData.Shared.BB);
			}

			Slid.value = newValue;			

			if (newValue < range[1]) {
				Allin.SetActive(false);

				if (pointerDown) {
					RoundTipsGo.SetActive(true);
				}
			} else {
				Allin.SetActive(true);
				RoundTipsGo.SetActive(false);
			}
			
			RaiseNumber.text = _.Num2CnDigit(newValue);
			TipsText.text = _.Num2CnDigit(newValue);
		}).AddTo(this);

		Slid.OnPointerDownAsObservable().Subscribe((pointerEvt) => {
			var rect = Slid.GetComponent<RectTransform>();

			changeTipsPosition(rect, pointerEvt.position, pointerEvt.pressEventCamera);
			pointerDown = true;
			RoundTipsGo.SetActive(true);
		}).AddTo(this);

		Slid.OnDragAsObservable().Subscribe((dragEvt) => {
			var rect = Slid.GetComponent<RectTransform>();
			changeTipsPosition(rect, dragEvt.position, dragEvt.pressEventCamera);
		}).AddTo(this);

		Slid.OnPointerUpAsObservable().Subscribe((_) => {
			pointerDown = false;
			RoundTipsGo.SetActive(false);
		}).AddTo(this);

		// 展示遮罩
		modal = ModalHelper.Create();
		modal.Show(transform.parent, hideRaiseSlider, true);
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
            var display = redata.Int("display");
            if (display == 0)
            {
                BuyTurnTime.SetActive(false);
            }
        });
    }

	public void OnFoldClick() {
		OPS.Fold();
	}

	public void OnSliderOK() {
		var value = (int)Slid.value;

		if (value >= range[1]) {
			OPS.AllIn();
		} else {
			OPS.raise(value);
		}
	}

	private void hideRaiseSlider() {
		hideModal();
		Slid.gameObject.SetActive(false);
		AccurateBtn.SetActive(true);
		setToggle(true);
	}

	private void hideModal() {
		if (modal != null) {
			modal.Despawn();
		}
	}

	private void setToggle(bool active = true) {
		R1.transform.parent.gameObject.SetActive(active);
		RaiseGo.SetActive(active);
	}

	private void disableBtn(GameObject go) {
		var button = go.GetComponent<Button>();
		button.interactable = false;
		go.GetComponent<CanvasGroup>().alpha = disableAlpha;	
	}

	private void disableAllBtns() {
		disableBtn(R1);
		disableBtn(R2);
		disableBtn(R3);

		AccurateBtn.GetComponent<CanvasGroup>().alpha = disableAlpha;
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

			if (flag) {
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
