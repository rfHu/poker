using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class ModalHelper: MonoBehaviour {
	private Action onClick;

	public void Show(Canvas canvas, Action onClick = null, bool modalColor = false) {
		this.onClick = onClick;
		
		transform.SetParent(canvas.transform);
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
			
			Despawn();	
		}).AddTo(this);
	}

	private bool despawned = false;

	void OnDespawned() {
		despawned = true;

		#if UNITY_EDITOR
		#else
            var modal = GameObject.FindObjectOfType<ModalHelper>();

            if (modal == null && !GameData.Shared.TalkLimit.Value) {
				Commander.Shared.VoiceIconToggle(true);
            }
		#endif	
	}

	void OnSpawned() {
		despawned = false;
	}

	public void Despawn() {
		if (!despawned) {
			G.Despawn(transform);
		}
	}

	public static ModalHelper Create() {
		var transform = G.Spawn("Modal");
		return transform.GetComponent<ModalHelper>();
	}
}
