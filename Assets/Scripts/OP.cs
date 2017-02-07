using UnityEngine;
using System.Collections.Generic;
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
	public Text CallText;

	public void StartWithCmds(Dictionary<string, object> data) {
        var cmds = data.Dict("cmds");
		var check = cmds.Bool("check");
		var callNum = cmds.Int("call");
		var raise = cmds.IL("raise");

		if (check) { // 看牌
			CallGo.GetComponent<Button>().onClick.AddListener(OPS.check);
			CallGo.GetComponent<Image>().sprite = CheckSpr;
			CallNumber.gameObject.SetActive(false);
			CallText.text = "看牌";
		} else { // 跟注
			CallGo.GetComponent<Button>().onClick.AddListener(() => {
				OPS.call();
			});
			CallGo.GetComponent<Image>().sprite = CallSpr;
			CallNumber.gameObject.SetActive(true);
			CallNumber.text = callNum.ToString();
			CallText.text = "跟注";
		}

		FoldGo.GetComponent<CircleMask>().Enable();
		setRaiseButtons(raise, callNum);
	}

	private void setRaiseButtons(List<int> range, int call) {
		// 底池小于二倍	
		var pot = GameData.Shared.Pot.Value;
		var bb = GameData.Shared.BB;

		if (pot < 2 * bb) {
			addProperty(R1, string.Format("X2\n盲注"), 2 * bb);	
			addProperty(R1, string.Format("X3\n盲注"), 3 * bb);	
			addProperty(R1, string.Format("X4\n盲注"), 4 * bb);	
		} else {
			var nextPot = pot + call;
			addProperty(R1, string.Format("1/2\n底池"), nextPot / 2 + call);
			addProperty(R1, string.Format("2/3\n底池"), nextPot * 2 / 3 + call);
			addProperty(R1, string.Format("1倍\n底池"), nextPot + call);
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
		Debug.Log("OnClick");
	}

	public void OnFoldClick() {
		OPS.fold();
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

		internal static void raise(int number) {
			Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f" , "raise"},
				{"args", number.ToString()}
			});
		}

		internal static void invoke(string f) {
			Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f" , f}
			});
		}
	}
	
}
