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

		// 翻卡片
		image.DOFlip();
		image.sprite = faces[index];
		IsBack = false;
	}

	public void Turnback() {
		GetComponent<Image>().sprite = cardBack;
		IsBack = true;
		gameObject.SetActive(false);
	}
}
