using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {
	public Sprite[] faces;
	public Sprite cardBack;

	public bool IsBack = true;

	Sprite getCardBack() {
		return cardBack;
	}

	Sprite getCardFace(int index) {
		return faces[index];
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
