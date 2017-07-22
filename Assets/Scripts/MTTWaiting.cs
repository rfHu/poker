using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class MTTWaiting : MonoBehaviour {
	[SerializeField]private Avatar avatar;
	[SerializeField]private Text coins;
	[SerializeField]private Text cdMin;
	[SerializeField]private Text cdSec;

	private IDisposable disposable;

	private int ts;

	public void Init(Dictionary<string, object> data) {
		avatar.SetImage(GameData.Shared.Avatar);
		var dt = data.Dict("myself");
		coins.text = _.Num2CnDigit(dt.Int("bankroll"));

		ts = data.Int("left_time");
		setTime();

		Observable.Interval(TimeSpan.FromSeconds(1)).Where((_) => ts > 0).Subscribe((_) => {
			ts--;
			setTime();

			if (ts <= 0) {
				disposable.Dispose();
			}
		}).AddTo(this);
	}

	private void setTime() {
		var dateTime = _.DateTimeFromTimeStamp(ts);
		cdMin.text = fix(dateTime.Minute);
		cdSec.text = fix(dateTime.Second);
	}

	private string fix(int num) {
		if (num < 10) {
			return "0" + num.ToString();
		} 

		return num.ToString();
	}
}
