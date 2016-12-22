using UnityEngine;

public class UIManager : MonoBehaviour {
	public void SelectMenu() {

	}

	public GameObject menu;
	public Canvas canvas;
	public GameObject mask;

	public void ShowMenu() {
		menu.GetComponent<Animator>().SetBool("Show", true);
		mask.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
	}

	public void ClickMask() {
		mask.GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
		menu.GetComponent<Animator>().SetBool("Show", false);
	}

	public void Standup() {
		Debug.Log("My God");
	}

	public void Exit() {
		Application.Quit();
	}

	public void CardTip() {
		Debug.Log("Tip");
	}

	public void Supplement() {
		Debug.Log("Supplement");
	}
}
