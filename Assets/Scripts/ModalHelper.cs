using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class ModalHelper: MonoBehaviour {
	private bool hasShow = false;

	public void Show(Canvas canvas, Action onClick = null) {
		if (hasShow) {
			return ;
		}

		hasShow = true;
		transform.SetParent(canvas.transform);

		_.FillParent(gameObject);
		
		// 设置点击事件
		GetComponent<Button>().OnClickAsObservable().Subscribe((_) => {
			if (onClick != null) {
				onClick();
			}
			Hide();	
		}).AddTo(this);
		transform.SetAsLastSibling();
	}

	public void Hide() {
		if (gameObject == null) {
			return ;
		}

		Destroy(gameObject);
	}

	public static ModalHelper Create() {
		var go = Instantiate((GameObject)Resources.Load("Prefab/Modal"));
		return go.GetComponent<ModalHelper>();
	}
}
