using UnityEngine;
using System.Collections.Generic;
using System;
using Extensions;
using UnityEngine.UI;

public class OP : MonoBehaviour {
	public GameObject RaiseGo;
	public GameObject FoldGo;
	public GameObject CallGo;
	public GameObject R1;
	public GameObject R2;
	public GameObject R3;

	public Sprite CheckSpr;
	public Sprite CallSpr;
	public Text CallNumber;

	Action r1Act;
	Action r2Act;
	Action r3Act;
	Action callAct;

	public void StartWithCmds(Dictionary<string, object> data) {
        var cmds = data.Dict("cmds");
		var check = data.Bool("check");
		var callNum = "1";

		if (check) {
			callAct = OPS.check;	
			CallGo.GetComponent<Image>().sprite = CheckSpr;
			CallNumber.gameObject.SetActive(false);
		} else {
			callAct = OPS.call;
			CallGo.GetComponent<Image>().sprite = CallSpr;
			CallNumber.gameObject.SetActive(true);
			CallNumber.text = callNum;
		}

		FoldGo.GetComponent<CircleMask>().Enable();
	}

	public void OnRaiseClick() {
		Debug.Log("OnClick");
	}

	public void OnCallClick() {
		callAct();
	}

	public void OnFoldClick() {
		OPS.fold();
	}

	public void OnR1Click() {
		r1Act();
	}

	public void OnR2Click() {
		r2Act();
	}

	public void OnR3Click() {
		r3Act();
	}

	class OPS {
		internal static void fold() {
			OPS.callFunc("fold");	
		}

		internal static void call() {
			OPS.callFunc("call");		
		}

		internal static void check() {
			OPS.callFunc("check");		
		}

		internal static void allIn() {
			OPS.callFunc("all_in");
		}

		internal static void raise(int number) {
			Connect.shared.Emit(new Dictionary<string, object>() {
				{"f" , "raise"},
				{"args", number.ToString()}
			});
		}

		internal static void callFunc(string f) {
			Connect.shared.Emit(new Dictionary<string, object>() {
				{"f" , f}
			});
		}
	}
	
}
