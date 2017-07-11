using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class ModalHelper: MonoBehaviour {
	private Action onClick;

	public void Show(Transform parent, Action onClick, bool modalColor = false) {
		this.onClick = onClick;
		
		transform.SetParent(parent);
		_.FillParent(gameObject);
		
		transform.SetAsLastSibling();

		if (modalColor) {
			GetComponent<Image>().color = new Color(0, 0, 0, 40 / 255f);
        }

#if UNITY_EDITOR
#else
		    Commander.Shared.VoiceIconToggle(false);
#endif
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

	void OnDespawned() {
		var modals = GameObject.FindObjectsOfType<ModalHelper>();
		var hasModal = false;

		for (var i  = 0; i < modals.Length; i++) {
			if (modals[i].gameObject.activeSelf) {
				hasModal = true;
				break;
			}
		}

		if (hasModal && !GameData.Shared.TalkLimit.Value) {
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
