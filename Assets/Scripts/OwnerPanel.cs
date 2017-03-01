using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using UIWidgets;

[RequireComponent(typeof(DOPopup))]
public class OwnerPanel : MonoBehaviour {
	public Text PauseText;

	private string pauseStr = "暂停牌局";
	private string continueStr = "继续牌局";

	void Awake()
	{
		if (GameData.Shared.Paused) {
			PauseText.text = continueStr;
		} else {
			PauseText.text = pauseStr;
		}
	}

	public void Stop() {
		// 二次确定
		PokerUI.ShowDialog("确定提前结束牌局", new DialogActions() {
			{"取消", Dialog.Close},
			{"确定", endTheGame}
		});
	}

	public void Pause() {
		string f;

		if (GameData.Shared.Paused) {
			f = "start";
			PauseText.text = pauseStr;
		} else {
			f = "pause";
			PauseText.text = continueStr;
		}

		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", f},
			{"args", "0"}
		}, (data) => {
			var msg = data.String("msg");

			if (string.IsNullOrEmpty(msg)) {
				PokerUI.Alert(msg);	
			}	
		});		
	}

	bool endTheGame() {
		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "pause"},
			{"args", "3"}
		});

		return true;		
	}
}
