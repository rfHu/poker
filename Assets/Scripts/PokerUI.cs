using UnityEngine;
using UIWidgets;

public class PokerUI: MonoBehaviour {
	static public void ShowDialog(string text) {
		var go = (GameObject)Instantiate(Resources.Load("Prefab/DialogTemplate"));
		go.GetComponent<Dialog>().Show(message: text, modal: true, modalColor: new Color(0, 0, 0, 0.2f), canvas: G.Cvs);
	}
}