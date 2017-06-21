using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DarkTonic.MasterAudio;

public class Card : MonoBehaviour {
	public Sprite[] Faces;
	public Sprite CardBack;
	public int Index = -1;


	public AnimationCurve scaleCurve;

	public static float TurnCardDuration = 0.3f;

	void Awake() {
		var img = GetComponent<Image>();
		img.sprite = CardBack;

		scaleCurve = new AnimationCurve();
		scaleCurve.AddKey(0, 1);
		scaleCurve.AddKey(0.5f, 0);
		scaleCurve.AddKey(1, 1);
	}

	private void show(int index, bool anim = false, Action complete = null) {	
		Index = index;

		gameObject.SetActive(true);
		GetComponent<Image>().enabled = true;
		var image = GetComponent<Image>();
		
		if (anim) {
			StartCoroutine(flipCard(index, complete));
		} else {
			image.sprite = Faces[index];
		}
	}

	public void SetSize(Vector2 size) {
		var rectTrans = GetComponent<RectTransform>();
		rectTrans.sizeDelta = size;
	}

	public void Darken() {
		GetComponent<Image>().color = new Color(150 / 255f ,150 / 255f, 150 / 255f, 1);
	}

	public void ReColor() {
		GetComponent<Image>().color = new Color(1, 1, 1, 1);
	}

	public void Show(int index, bool anim = false, Action complete = null) {
		if (index == 0) {
			return ;
		}

		ReColor();

		var realIndex = Card.CardIndex(index);
		show(realIndex, anim, complete);
	}

	public void ShowWithSound(int index, bool anim = false) {
		if (index == 0) {
			return ;
		}

		Show(index, anim);
		G.PlaySound("fapai_1");
	}

	IEnumerator flipCard(int index, Action complete = null) {
		float time = 0f;
		var image = GetComponent<Image>();
		var rectTrans = GetComponent<RectTransform>();
		
		while(time < 1f) {
			time = Mathf.Min(time + Time.deltaTime / TurnCardDuration, 1);
			float scale = scaleCurve.Evaluate(time);

			Vector2 vector = rectTrans.localScale;
			vector.x = scale;
			rectTrans.localScale = vector;

			if (time >= 0.5) {
				image.sprite = Faces[index];
			}

			yield return new WaitForFixedUpdate();
		}

		rectTrans.localScale = new Vector2(1, 1);

		if (complete != null) {
			complete();
		}
	}

	public void Turnback() {
		GetComponent<Image>().sprite = CardBack;
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

	static public string GetCardDesc(int val) {
		var value = val >> 20;

		var map = new Dictionary<int, string> {
			{1, "高牌"},
			{2, "一对"},
			{3, "两对"},
			{4, "三条"},
			{5, "顺子"},
			{6, "同花"},
			{7, "葫芦"},
			{8, "四条"},
			{9, "同花顺"},
			{10, "皇家同花顺"},
		};

		if (!map.ContainsKey(value)) {
			return "";
		}

		return map[value];
	}
}
