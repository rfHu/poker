﻿using UnityEngine;
using System.Collections.Generic;
using Extensions;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System;

public class OP : MonoBehaviour {
	public GameObject RaiseGo;
	public GameObject FoldGo;
	public GameObject CallGo;
	public GameObject R1;
	public GameObject R2;
	public GameObject R3;

	public Sprite CheckSpr;
	public Sprite CallSpr;
	public Sprite AllinSpr;
	public Text CallNumber;
	public Text CallText;
	public Slider Slid;
	public GameObject Allin;
	public Text RaiseNumber;
	public Text MaxText;
	public Button SlidOK;

	public GameObject RoundTipsGo;
	public Text TipsText;

	private List<int> range;
	private ModalHelper modal;

	private static OP instance; 

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
	
	public void StartWithCmds(Dictionary<string, object> data, int elaspe, Action onOnlyAllin) {
        var cmds = data.Dict("cmds");
		var check = cmds.Bool("check");
		var callNum = cmds.Int("call");
		var allin = cmds.Bool("all_in");

		range = cmds.IL("raise");

		if (check) { // 看牌
			CallGo.GetComponent<Button>().onClick.AddListener(OPS.check);
			CallGo.GetComponent<Image>().sprite = CheckSpr;
			CallNumber.gameObject.SetActive(false);
			CallText.text = "看牌";
		} else if (callNum > 0) { // 跟注
			CallGo.GetComponent<Button>().onClick.AddListener(() => {
				OPS.call();
			});
			CallGo.GetComponent<Image>().sprite = CallSpr;
			CallNumber.gameObject.SetActive(true);
			CallNumber.text = callNum.ToString();
			CallText.text = "跟注";
		} else if (allin) {
			CallGo.GetComponent<Button>().onClick.AddListener(OPS.allIn);
			CallGo.GetComponent<Image>().sprite = AllinSpr;
			CallNumber.gameObject.SetActive(false);
			CallText.enabled = false;
		} else {
			CallGo.SetActive(false);
		}

		var mask = FoldGo.GetComponent<CircleMask>();
		mask.Enable(elaspe, true);

		if (range.Count >= 2) {
			setRaiseButtons(callNum);
			RaiseGo.GetComponent<Button>().onClick.AddListener(OnRaiseClick);
		} else if(callNum > 0 && allin) {
			RaiseGo.GetComponent<Image>().sprite = AllinSpr;
			RaiseGo.transform.Find("Text").gameObject.SetActive(false);
			RaiseGo.GetComponent<Button>().onClick.AddListener(OPS.allIn);
			setAction(false);
		} else {
			setAction(false);
			RaiseGo.SetActive(false);
			onOnlyAllin();
		}	
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
			
			values.Add(Mathf.CeilToInt(nextPot / 2f) + call);
			values.Add(Mathf.CeilToInt(nextPot * 2f / 3f) + call);
			values.Add(nextPot + call);

			names.Add("1/2\n底池");
			names.Add("2/3\n底池");
			names.Add("1倍\n底池");
		}

		addProperty(R1, names[0], values[0]);	
		addProperty(R2, names[1], values[1]);	
		addProperty(R3, names[2], values[2]);

		var max = range[1];
		if (values[0] > max) {
			setAction(false);
		} else if (values[1] > max) {
			R2.SetActive(false);
			R3.SetActive(false);
		} else if (values[2] > max) {
			R3.SetActive(false);
		}
	}

	private void addProperty(GameObject go, string text, int value) {
		go.transform.Find("Text").GetComponent<Text>().text = text;
		go.transform.Find("Number").GetComponent<Text>().text = value.ToString();
		go.GetComponent<Button>().onClick.AddListener(() => {
			OPS.raise(value);
		});
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
			if (value < range[1]) {
				Allin.SetActive(false);

				if (pointerDown) {
					RoundTipsGo.SetActive(true);
				}
			} else {
				Allin.SetActive(true);
				RoundTipsGo.SetActive(false);
			}
			
			RaiseNumber.text = value.ToString();
			TipsText.text = value.ToString();
		}).AddTo(this);

		Slid.OnPointerDownAsObservable().Subscribe((_) => {
			pointerDown = true;
			RoundTipsGo.SetActive(true);
		}).AddTo(this);

		Slid.OnPointerUpAsObservable().Subscribe((_) => {
			pointerDown = false;
			RoundTipsGo.SetActive(false);
		}).AddTo(this);

		// 展示遮罩
		modal = ModalHelper.Create();
		modal.Show(G.UICvs, close);
		transform.SetAsLastSibling();

		setToggle(false);
	}

	public void OnFoldClick() {
		OPS.fold();
	}

	public void OnSliderOK() {
		var value = (int)Slid.value;

		if (value >= range[1]) {
			OPS.allIn();
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

	class OPS {
		internal static void fold() {
			OPS.invoke("fold");	
		}

		internal static void call() {
			OPS.invoke("call");		
		}

		internal static void check() {
			OPS.invoke("check");		
		}

		internal static void allIn() {
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

		internal static void raise(int number) {
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
