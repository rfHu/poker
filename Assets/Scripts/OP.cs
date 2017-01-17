using UnityEngine;
using System.Collections.Generic;
using System;

public class OP : MonoBehaviour {
	public GameObject RaiseGo;
	public GameObject FoldGo;
	public GameObject CallGo;
	public GameObject R1;
	public GameObject R2;
	public GameObject R3;

	public Action R1Act;
	public Action R2Act;
	public Action R3Act;
	public Action CallAct;


	public void OnRaiseClick() {
		Debug.Log("OnClick");
	}

	public void OnCallClick() {
		CallAct();
	}

	public void OnFoldClick() {
		Fold();
	}

	public void OnR1Click() {
		R1Act();
	}

	public void OnR2Click() {
		R2Act();
	}

	public void OnR3Click() {
		R3Act();
	}

	public void Fold() {
		CallFunc("fold");	
	}

	public void Call() {
		CallFunc("call");		
	}

	public void Check() {
		CallFunc("check");		
	}

	public void AllIn() {
		CallFunc("all_in");
	}

	public void Raise(int number) {
		Connect.shared.Emit(new Dictionary<string, object>() {
			{"f" , "raise"},
			{"args", number.ToString()}
		});
	}

	private void CallFunc(string f) {
		Connect.shared.Emit(new Dictionary<string, object>() {
			{"f" , f}
		});
	}
}
