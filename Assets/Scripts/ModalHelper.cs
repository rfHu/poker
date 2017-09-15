using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class ModalHelper: MonoBehaviour {
	private Action onClick;

	static public int ModalCount {
		get {
			return _modalCount;
		}
	}

	static private int _modalCount = 0;

	public static Color DefaultColor = new Color(0, 0, 0, 40 / 255f);

	public void Show(Transform parent, Action onClick) {
		Show(parent, onClick, DefaultColor);
	}

	public void Show(Transform parent, Action onClick, Color modalColor) {
		this.onClick = onClick;
		
		transform.SetParent(parent, false);
		_.FillParent(gameObject);
		
		transform.SetAsLastSibling();
		SetModalColor(modalColor);

#if UNITY_EDITOR
#else
		    Commander.Shared.VoiceIconToggle(false);
#endif
    }

	public void SetModalColor(Color color) {
		GetComponent<Image>().color = color;
	}

	void Awake()
	{
		// 设置点击事件
		GetComponent<Button>().OnClickAsObservable().Subscribe((_) => {
			if (onClick != null) {
				onClick();
			}
		}).AddTo(this);
	}

	void OnSpawned() {
		_modalCount++;
	}

	void OnDespawned() {
		_modalCount--;

		if (_modalCount <= 0 && !GameData.Shared.TalkLimit.Value) {
			_.Log("Show Native Voice Button");
			#if UNITY_EDITOR 
			#else
				Commander.Shared.VoiceIconToggle(true);
			#endif
		}
	}

	public void Despawn() {
		PoolMan.Despawn(transform);
	}

	public static ModalHelper Create() {
		var transform = PoolMan.Spawn("Modal");
		return transform.GetComponent<ModalHelper>();
	}
}
