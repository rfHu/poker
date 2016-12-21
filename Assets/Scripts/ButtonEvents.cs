using UnityEngine;

public class ButtonEvents : MonoBehaviour {
	public void selectMenu() {

	}

	bool menuShow = false;

	public GameObject menu;

	public void toggleMenu() {
		Animation anim = menu.GetComponent<Animation>();

		if (menuShow) {
			anim.Play("Dropup");			
		} else {
			anim.Play("Dropdown");
		}

		menuShow = !menuShow;		
	}
}
