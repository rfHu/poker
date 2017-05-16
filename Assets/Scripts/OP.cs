using UnityEngine;
using System.Collections.Generic;
using Extensions;
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

	public Text CallNumber;
	public Text CallText;
	public Slider Slid;
	public GameObject Allin;
	public Text RaiseNumber;
	public Text MaxText;

	public GameObject RoundTipsGo;
	public Text TipsText;

	private List<int> range;
	private ModalHelper modal;

	private static OP instance; 
	private CircleMask circleMask;

	void Awake()
	{
		if (instance != null) {
			Destroy(instance.gameObject);
		}

		instance = this;

		// 设置位置等信息
		transform.SetParent(G.UICvs.transform, false);
	}

	void OnDestroy()
	{
		hideModal();
	}
	
	public void StartWithCmds(Dictionary<string, object> data, int left) {
        var cmds = data.Dict("cmds");
		var check = cmds.Bool("check");
		var callNum = cmds.Int("call");
		var allin = cmds.Bool("all_in");

		range = cmds.IL("raise");

		if (check) { // 看牌
			CallGo.SetActive(false);
			CheckGo.SetActive(true);
			CheckGo.GetComponent<Button>().onClick.AddListener(OPS.Check);
		} else if (callNum > 0) { // 跟注
			CheckGo.SetActive(false);
			CallGo.SetActive(true);
			CallGo.GetComponent<Button>().onClick.AddListener(() => {
				OPS.Call();
			});
			CallNumber.text = callNum.ToString();
		} else { // 不能跟注、不能看牌，展示灰掉的看牌按钮
			CallGo.SetActive(false);
			CheckGo.SetActive(true);
			CheckGo.GetComponent<CanvasGroup>().alpha = 0.4f;
		}

		circleMask = FoldGo.transform.Find("CD").GetComponent<CircleMask>();
		circleMask.Enable(left, true);

		if (range.Count >= 2) { // 可加注
			AllinGo.SetActive(false);
			RaiseGo.SetActive(true);
			setRaiseButtons(callNum);
			RaiseGo.GetComponent<Button>().onClick.AddListener(OnRaiseClick);
		} else if(allin) { // 不可加注、可Allin
			RaiseGo.SetActive(false);
			AllinGo.SetActive(true);
			AllinGo.GetComponent<Button>().onClick.AddListener(OPS.AllIn);
			disableAllBtns();
		} else { // 不可加注、不可Allin
			disableAllBtns();
			RaiseGo.SetActive(true);
			RaiseGo.GetComponent<CanvasGroup>().alpha = 0.4f;
		}	
	}

	public void Reset(float left) {
		if (circleMask == null) {
			return ;
		}

		circleMask.Reset(left);
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
		go.transform.Find("Number").GetComponent<Text>().text = value.ToString();
		go.GetComponent<Button>().onClick.AddListener(() => {
			OPS.raise(value);
		});
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
		MaxText.text = range[1].ToString();

		// 未按下时，隐藏加注提示
		RoundTipsGo.SetActive(false);

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
			
			RaiseNumber.text = newValue.ToString();
			TipsText.text = newValue.ToString();
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
		modal.Show(G.UICvs, close, true);
		transform.SetAsLastSibling();

		setToggle(false);
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

	private void close() {
		hideModal();
		Slid.gameObject.SetActive(false);
		setToggle(true);
	}

	private void hideModal() {
		if (modal != null) {
			modal.Hide();
		}
	}

	private void setToggle(bool active = true) {
		setAction(active);
		RaiseGo.SetActive(active);
	}
	
	private void setAction(bool active = true) {
		R1.transform.parent.gameObject.SetActive(active);
	}

	private void disableBtn(GameObject go) {
		var button = go.GetComponent<Button>();
		button.onClick.RemoveAllListeners();
		go.GetComponent<CanvasGroup>().alpha = 0.4f;	
	}

	private void disableAllBtns() {
		disableBtn(R1);
		disableBtn(R2);
		disableBtn(R3);
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

			instance.gameObject.SetActive(flag);
		}

		private static void onres(Dictionary<string, object> json) {
			var err = json.Int("err");
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
	
}
