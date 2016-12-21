using UnityEngine;

public class UIManager : MonoBehaviour {
	public void SelectMenu() {

	}

	public GameObject menu;
	public Canvas canvas;
	public GameObject mask;

	public void HideMenu() {
		menu.GetComponent<Animator>().SetBool("Show", false);
	}

	public void ShowMenu() {
		menu.GetComponent<Animator>().SetBool("Show", true);
		mask.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
	}

	public void ClickMask() {
		Debug.Log("Click mask");
	}
}
