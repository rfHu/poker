using UnityEngine;

public class UIManager : MonoBehaviour {
	public void selectMenu() {

	}

	public GameObject menu;
	public Canvas canvas;

	public void hideMenu() {
		menu.GetComponent<Animator>().SetBool("Show", false);
	}

	public void showMenu() {
		menu.GetComponent<Animator>().SetBool("Show", true);
	}
}
