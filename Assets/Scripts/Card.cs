using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {
	public Sprite[] faces;
	public Sprite cardBack;

	public Sprite[] sfaces;
	public Sprite scardBack;

	public bool IsBack = true;
	public bool IsSmall = false;

	Sprite getCardBack() {
		if (IsSmall) {
			return scardBack;
		} else {
			return cardBack;
		}
	}

	Sprite getCardFace(int index) {
		if (IsSmall) {
			return sfaces[index];
		} else {
			return faces[index];
		}
	}

	void Awake() {
		var img = GetComponent<Image>();
		img.sprite = getCardBack();
	}

	public void Show(int index) {	
		gameObject.SetActive(true);
		
		var image = GetComponent<Image>();
		image.sprite = getCardFace(index);
		
		IsBack = false;
	}

	public void ShowServer(int index) {
		if (index == 0) {
			return ;
		}

		var realIndex = Controller.CardIndex(index);
		Show(realIndex);
	}

	public void Turnback() {
		GetComponent<Image>().sprite = getCardBack();
		IsBack = true;
		gameObject.SetActive(false);
	}
}
