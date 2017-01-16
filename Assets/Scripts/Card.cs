using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {
	public Sprite[] faces;
	public Sprite cardBack;
	public int cardIndex;

	void Awake() {
		GetComponent<Image>().sprite = cardBack;
	}
	public void Show(int index) {	
		cardIndex = index;
		GetComponent<Image>().sprite = faces[cardIndex];
	}

	public void Turnback() {
		GetComponent<Image>().sprite = cardBack;
	}
}
