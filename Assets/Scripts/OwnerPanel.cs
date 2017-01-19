using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DOPopup))]
public class OwnerPanel : MonoBehaviour {
	public Text PauseText;

	void Awake()
	{
		if (GConf.Paused) {
			PauseText.text = "继续牌局";
		} else {
			PauseText.text = "暂停牌局";
		}
	}

	public void Stop() {
		// @TODO: 二次确定
		Connect.shared.Emit(new Dictionary<string, object>() {
			{"f", "pause"},
			{"args", "3"}
		});		
	}

	public void Pause() {
		var f = "pause";
		if (GConf.Paused) {
			f = "start";
		}

		Connect.shared.Emit(new Dictionary<string, object>() {
			{"f", f},
			{"args", "0"}
		});		
	}
}
