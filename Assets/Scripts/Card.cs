using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {
	public Sprite[] faces;
	public Sprite cardBack;
	public int cardIndex;

	void Awake() {
		GetComponent<SpriteRenderer>().sprite = cardBack;
	}
	public void Show() {	
		GetComponent<Image>().sprite = faces[cardIndex];
	}

	public void Turnback() {
		GetComponent<Image>().sprite = cardBack;
	}
}
