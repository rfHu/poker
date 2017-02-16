using UnityEngine;
using System.Collections.Generic;
using Extensions;
using UnityEngine.UI;
using UniRx;

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
	public Slider Slid;
	public GameObject Allin;
	public Text RaiseNumber;

	private List<int> range;

	public void StartWithCmds(Dictionary<string, object> data) {
        var cmds = data.Dict("cmds");
		var check = cmds.Bool("check");
		var callNum = cmds.Int("call");

		this.range = cmds.IL("raise");

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
		setRaiseButtons(callNum);
	}

	private void setRaiseButtons(int call) {
		// 底池小于二倍	
		var pot = GameData.Shared.Pot.Value;
		var bb = GameData.Shared.BB;

		if (pot < 2 * bb) {
			addProperty(R1, string.Format("X2\n盲注"), 2 * bb);	
			addProperty(R2, string.Format("X3\n盲注"), 3 * bb);	
			addProperty(R3, string.Format("X4\n盲注"), 4 * bb);	
		} else {
			var nextPot = pot + call;
			addProperty(R1, string.Format("1/2\n底池"), Mathf.CeilToInt(nextPot / 2f) + call);
			addProperty(R2, string.Format("2/3\n底池"), Mathf.CeilToInt(nextPot * 2f / 3f) + call);
			addProperty(R3, string.Format("1倍\n底池"), nextPot + call);
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

		Slid.OnValueChangedAsObservable().Subscribe((value) => {
			if (value < range[1]) {
				Allin.SetActive(false);
			} else {
				Allin.SetActive(true);
			}
			
			RaiseNumber.text = value.ToString();
		}).AddTo(this);

		set3Acts(false);
	}

	public void OnFoldClick() {
		OPS.fold();
	}

	private void set3Acts(bool active = true) {
		R1.SetActive(active);	
		R2.SetActive(active);	
		R3.SetActive(active);	
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
