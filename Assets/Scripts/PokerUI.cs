using UnityEngine;
using UIWidgets;

public class PokerUI: MonoBehaviour {
	static private Dialog dialogSample;

	static public void ShowDialog(string text, DialogActions buttons) {
		if (dialogSample == null) {
			var go = (GameObject)Instantiate(Resources.Load("Prefab/DialogTemplate"));
			dialogSample = go.GetComponent<Dialog>();
		}

		dialogSample.GetComponent<Dialog>().Template().Show(message: text, modal: true, buttons: buttons, modalColor: new Color(0, 0, 0, 0.2f), canvas: G.Cvs);
	}

	static public void Alert(string text) {
		ShowDialog(text, new DialogActions() {
			{"确定", Dialog.Close},
		});
	}

	static public void ExitAlert() {
		PokerUI.ShowDialog("您的账号已在其他设备登陆", new DialogActions() {
			{"确定", Exit} 
		});
	}

	static private bool Exit() {
		Commander.Shared.Exit();
		return false;
	} 
}