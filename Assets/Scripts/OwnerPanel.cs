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
		if (isGamePause()) {
			PauseText.text = continueStr;
		} else {
			PauseText.text = pauseStr;
		}
	}

	public void Stop() {
		GetComponent<DOPopup>().Close();

		// 二次确定
		PokerUI.ShowDialog("确定提前结束牌局", new DialogActions() {
			{"取消", Dialog.Close},
			{"确定", endTheGame}
		});
	}

	public void Pause() {
		string f;

		if (isGamePause()) {
			f = "start";
		} else {
			f = "pause";
		}

		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", f},
			{"args", "0"}
		}, (data) => {
			var msg = data.String("msg");
			if (!string.IsNullOrEmpty(msg)) {
				PokerUI.Alert(msg);	
			}	

			var err = data.Int("err");
			if (err == 0) {
				if (f == "start") {
					PauseText.text = pauseStr;
				} else {
					PauseText.text = continueStr;
				}
			}
		});		
	}

	bool isGamePause() {
		return GameData.Shared.Paused && GameData.Shared.GameStarted;
	}

	bool endTheGame() {
		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "pause"},
			{"args", "3"}
		});

		return true;		
	}
}
