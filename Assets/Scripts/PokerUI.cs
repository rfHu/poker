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
}