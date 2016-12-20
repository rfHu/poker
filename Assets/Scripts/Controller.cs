﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
	public int numberOfPlayers = 2;
	public Button seat;
	public Canvas canvas;

	void Start () {
		List<Button> buttons = new List<Button>();

		for (int i = 0; i < numberOfPlayers; i++) {
			Button copySeat = Instantiate (seat);
			copySeat.transform.SetParent (canvas.transform, false);
			buttons.Add (copySeat);
		}

		Vector2[] vectors = getVectors (numberOfPlayers);
		int iter = 0;

		foreach(Button button in buttons) {
			button.GetComponent<RectTransform> ().localPosition = vectors[iter] ;
			iter++;
		}
	}

	// 逆时针生成位置信息
	Vector2[] getVectors(int total) {
		float width = canvas.GetComponent<RectTransform>().rect.width;
		float height = canvas.GetComponent<RectTransform>().rect.height;

		float top = height / 2 - 150;
		float bottom = -height / 2 + 180;
		float right = width / 2 - 100;
		float left = -width / 2 + 100; 

		Vector2 number1 = new Vector2 (0, bottom);

		if (total == 2) {
			return new Vector2 []{
				number1,
				new Vector2(0, top)
			};
		}

		float hh = Mathf.Abs (top) + Mathf.Abs (bottom);
		float ww = Mathf.Abs (left) + Mathf.Abs (right);

		float h3 = hh / 2 - hh / 3;
		float h4 = hh / 4;
		float w3 = ww / 2 - ww / 3;

		if (total == 6) {
			return new Vector2[] {
				number1, 
				new Vector2(right, 0 - h3),
				new Vector2(right, 0 + h3),
				new Vector2(0, top),
				new Vector2(left, 0 + h3),
				new Vector2(left, 0 - h3)
			};
		}

		if (total == 7) {
			return new Vector2[] {
				number1, 
				new Vector2(right, 0 - h3),
				new Vector2(right, 0 + h3),
				new Vector2(w3, top),
				new Vector2(-w3, top),
				new Vector2(left, 0 + h3),
				new Vector2(left, 0 - h3)
			};
		}

		if (total == 8) {
			return new Vector2[] {
				number1, 
				new Vector2(right, 0 - h4),
				new Vector2(right, 0),
				new Vector2(right, 0 + h4),
				new Vector2(0, top),
				new Vector2(left, 0 + h4),
				new Vector2(left, 0),
				new Vector2(left, 0 - h4)
			};
		}

		if (total == 9) {
			return new Vector2[] {
				number1, 
				new Vector2(right, 0 - h4),
				new Vector2(right, 0),
				new Vector2(right, 0 + h4),
				new Vector2(w3, top),
				new Vector2(-w3, top),
				new Vector2(left, 0 + h4),
				new Vector2(left, 0),
				new Vector2(left, 0 - h4)
			};
		}

		return new Vector2[]{};
	}
}
