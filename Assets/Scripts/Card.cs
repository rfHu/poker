using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour {
	public Sprite[] faces;
	public Sprite cardBack;

	public bool IsBack = true;

	public AnimationCurve scaleCurve;

	public static float TurnCardDuration = 0.5f;

	void Awake() {
		var img = GetComponent<Image>();
		img.sprite = cardBack;

		scaleCurve = new AnimationCurve();
		scaleCurve.AddKey(0, 1);
		scaleCurve.AddKey(0.5f, 0);
		scaleCurve.AddKey(1, 1);
	}

	private void show(int index, bool anim = false) {	
		gameObject.SetActive(true);
		GetComponent<Image>().enabled = true;
		var image = GetComponent<Image>();
		
		if (anim) {
			StartCoroutine(flipCard(index));
		} else {
			image.sprite = faces[index];
		}
		
		IsBack = false;
	}

	public void SetSize(Vector2 size) {
		var rectTrans = GetComponent<RectTransform>();
		rectTrans.sizeDelta = size;
	}

	public void Show(int index, bool anim = false) {
		if (index == 0) {
			return ;
		}

		if (!IsBack) {
			return ;
		}

		var realIndex = Card.CardIndex(index);
		show(realIndex, anim);
	}

	IEnumerator flipCard(int index) {
		float time = 0f;
		var image = GetComponent<Image>();
		var rectTrans = GetComponent<RectTransform>();
		
		while(time < 1f) {
			float scale = scaleCurve.Evaluate(time);
			time = Mathf.Min(time + Time.deltaTime / TurnCardDuration, 1);

			Vector2 vector = rectTrans.localScale;
			vector.x = scale;
			rectTrans.localScale = vector;

			if (time >= 0.5) {
				image.sprite = faces[index];
			}

			yield return new WaitForFixedUpdate();
		}
	}

	public void Turnback() {
		GetComponent<Image>().sprite = cardBack;
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
