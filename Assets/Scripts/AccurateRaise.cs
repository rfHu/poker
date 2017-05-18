using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UniRx;

public class AccurateRaise : MonoBehaviour {
	public Text InputText;
	public Action<int> OnValueChange;
	public Action Close;
	private List<int> range;

	void Awake() {
		InputText.ObserveEveryValueChanged((x) => x.text).Subscribe((number) => {
			if (!_.IsNumeric(number)) {
				InputText.color = _.HexColor("#FFFFFFB2");
				return ;
			}

			var num = int.Parse(number);

			if (num > range[1]) {
				return ;
			}

			InputText.color = Color.white;
			OnValueChange(num);
		}).AddTo(this);
	}

	public void Show(List<int> range) {
		this.range = range;
		InputText.text = "最小为" + range[0].ToString();
		gameObject.SetActive(true);
	}

	public void Hide() {
		gameObject.SetActive(false);
	}
	
	public void OnClick(int number) {
		string text;

		if (_.IsNumeric(InputText.text) && int.Parse(InputText.text) != 0) {
			text = InputText.text + number.ToString();
		} else {
			text = number.ToString();
		}

		var num = int.Parse(text);
		num = Mathf.Min(num, range[1]);

		InputText.text = num.ToString(); 
	}

	public void OnDelete() {
		var text = InputText.text;

		if (text.Length == 1 || !_.IsNumeric(text)) {
			text = "0";
		} else {
			text = text.Remove(text.Length - 1);
		}

		InputText.text = text;
	}

	public void OnClose() {
		if (Close != null) {
			Close();
		}
	}
}
