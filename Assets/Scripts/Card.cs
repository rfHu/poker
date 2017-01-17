using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Card : MonoBehaviour {
	public Sprite[] faces;
	public Sprite cardBack;

	public bool IsBack = true;

	void Awake() {
		GetComponent<Image>().sprite = cardBack;
	}
	public void Show(int index) {	
		gameObject.SetActive(true);
		
		var image = GetComponent<Image>();
		image.sprite = faces[index];
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
		GetComponent<Image>().sprite = cardBack;
		IsBack = true;
		gameObject.SetActive(false);
	}
}
