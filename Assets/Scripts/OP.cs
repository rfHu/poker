using UnityEngine;
using System.Collections.Generic;

public class OP : MonoBehaviour {
	public GameObject RaiseGo;
	public GameObject FoldGo;
	public GameObject CallGo;
	public GameObject R1;
	public GameObject R2;
	public GameObject R3;


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
