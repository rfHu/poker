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
		GetComponent<Image>().enabled = true;
		
		var image = GetComponent<Image>();
		image.sprite = getCardFace(index);
		
		IsBack = false;
	}

	public void SetSize(Vector2 size) {
		var rectTrans = GetComponent<RectTransform>();
		rectTrans.sizeDelta = size;
	}

	public void ShowServer(int index) {
		if (index == 0) {
			return ;
		}

		var realIndex = Card.CardIndex(index);
		Show(realIndex);
	}

	public void Turnback() {
		GetComponent<Image>().sprite = getCardBack();
		IsBack = true;
	}

	public void Hide() {
		GetComponent<Image>().enabled = false;
	}

	public static int CardIndex(int number) {
		var pairs = Card.CardValues(number);
		int index;

		// 服务器数值为2~14
		if (pairs[1] ==  14) {
			index = 0;
		} else {
			index = pairs[1] - 1;
		}

		index = index + (4 - pairs[0]) * 13;

		return index;
	}

	public static int[] CardValues(int number) {
		var a = number >> 4;
		var b = number & 0x0f;

		// 第一个花色、第二个数值
		return new int[]{a, b};
	}
}
