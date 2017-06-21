using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class ModalHelper: MonoBehaviour {
	private bool hasShow = false;

	public void Show(Canvas canvas, Action onClick = null, bool modalColor = false) {
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
			Destroy();	
		}).AddTo(this);
		transform.SetAsLastSibling();

		if (modalColor) {
			GetComponent<Image>().color = new Color(0, 0, 0, 40 / 255f);
        }

#if UNITY_EDITOR
#else
		    Commander.Shared.VoiceIconToggle(false);
#endif
    }

	public void Destroy() {
		if (gameObject == null) {
			return ;
        }

        Destroy(gameObject);
	}

	void OnDestroy()
	{
		#if UNITY_EDITOR
		#else
            var modal = GameObject.FindObjectOfType<ModalHelper>();

            if (modal == null && !GameData.Shared.TalkLimit.Value) {
				Commander.Shared.VoiceIconToggle(true);
            }
		#endif	
	}

	public static ModalHelper Create() {
		var go = Instantiate((GameObject)Resources.Load("Prefab/Modal"));
		return go.GetComponent<ModalHelper>();
	}
}
