using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class MTTWaiting : MonoBehaviour {
	[SerializeField]private Avatar avatar;
	[SerializeField]private Text coins;
	[SerializeField]private Text inviteCode;
	[SerializeField]private Text cdText;
	[SerializeField]private Text RoomName;

	private IDisposable disposable;

	private int ts;

	void Awake()
	{
		RxSubjects.MTTMatch.Subscribe((e) => {
			var type = e.Data.Int("type");
			if (type == 2) {
				PokerUI.ToastThenExit("报名人数不足，无法开赛");
			}
		}).AddTo(this);		
	}

	public void Init(Dictionary<string, object> data) {
		avatar.SetImage(GameData.Shared.Avatar);
		var dt = data.Dict("myself");
		coins.text = _.Num2CnDigit(dt.Int("bankroll"));

		ts = data.Int("left_time");
		setTime();

		RoomName.text = GameData.Shared.Room;

		var codeGo = inviteCode.transform.parent.gameObject;
		var code = GameData.Shared.GameCode.Value;

		if (string.IsNullOrEmpty(code)) {
			codeGo.SetActive(false);
		} else {
			codeGo.SetActive(true);
			inviteCode.text = code;
		}

		if (disposable != null) {
			disposable.Dispose();
		}

		disposable = Observable.Interval(TimeSpan.FromSeconds(1)).Where((_) => ts > 0).Subscribe((_) => {
			ts--;
			setTime();

			if (ts <= 0) {
				disposable.Dispose();
			}
		}).AddTo(this);
	}

	private void setTime() {
		var dateTime = _.DateTimeFromTimeStamp(ts);
		cdText.text = string.Format("{0}:{1}", _.Fix(dateTime.Minute), _.Fix(dateTime.Second));
	}
}
