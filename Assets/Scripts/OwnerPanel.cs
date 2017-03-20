using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using UniRx;

[RequireComponent(typeof(DOPopup))]
public class OwnerPanel : MonoBehaviour {
	public Text PauseText;

	private string pauseStr = "暂停牌局";
	private string continueStr = "继续牌局";

	void Awake()
	{
		GameData.Shared.Paused.Subscribe((pause) => {
			if (pause && GameData.Shared.GameStarted) {
				PauseText.text = continueStr;
			} else {
				PauseText.text = pauseStr;
			}
		}).AddTo(this);
	}

	public void Stop() {
		GetComponent<DOPopup>().Close();

		// 二次确定
		PokerUI.Alert("确定提前结束牌局", () => {
			Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "pause"},
				{"args", "3"}
			});
		}, null);
	}

	public void Pause() {
		string f;

		if (GameData.Shared.Paused.Value && GameData.Shared.GameStarted) {
			f = "start";
		} else {
			f = "pause";
		}

		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", f},
			{"args", "0"}
		}, (data) => {
			var err = data.Int("err");
			
			if (err == 0) {
				if (f == "start") {
					PauseText.text = pauseStr;
				} else {
					PauseText.text = continueStr;
				}
			} else {
				var msg = data.String("ret");
				GetComponent<DOPopup>().Close();
				PokerUI.Alert(msg);	
			}
		});		
	}
}
