using UnityEngine;
using UnityEngine.UI;
using System;

public class Modal {
	private GameObject gameObject;
	private bool hasShow = false;

	public void Show(Canvas canvas, Action onClick = null) {
		if (hasShow) {
			return ;
		}

		hasShow = true;

		gameObject = new GameObject();
		var rect = gameObject.AddComponent<RectTransform>();
		var button = gameObject.AddComponent<Button>();

		gameObject.transform.SetParent(canvas.transform);

		rect.sizeDelta = new Vector2(0, 0);
		rect.anchorMin = new Vector2(0, 0);
		rect.anchorMax = new Vector2(1, 1);
		rect.anchoredPosition = new Vector2(0, 0);

		button.onClick.AddListener(() => {
			onClick();
			Hide();
		});

		gameObject.transform.SetAsLastSibling();
	}

	public void Hide() {
		if (gameObject == null) {
			return ;
		}

		gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
		MonoBehaviour.Destroy(gameObject);
	}
}
