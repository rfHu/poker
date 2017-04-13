using UnityEngine;
using System;
using UnityEngine.UI;
using UniRx;
using UnityEngine.UI.ProceduralImage;

public class StatusBar : MonoBehaviour {
	public Text timeText;
	public Text power;
	public ProceduralImage rect;
	public Text CountDown;
	public GameObject AuditGo;
	private IDisposable disposable;

	private float width;
	private float height;

	void Start () {
		var size = rect.GetComponent<RectTransform>().sizeDelta;
		width = size.x;
		height = size.y;

		addEvents();
	}

	void Update()
	{
		DateTime time = System.DateTime.Now;	
		String timeStr = time.ToString("hh:mm");	
		timeText.text = timeStr;

		int battery = GetBatteryLevel();		
		power.text = battery.ToString() + "%";

		rect.GetComponent<RectTransform>().sizeDelta = new Vector2(width * battery / 100, height);
	}

	public static int GetBatteryLevel()
    {
        return Commander.Shared.Power();
    }

	private void addEvents() {
		GameData.Shared.AuditCD.Subscribe((secs) => {
			Debug.Log("审核倒计时: " + secs.ToString());

			if (secs <= 0) {
				AuditGo.SetActive(false);
				return ;
			}

			enableCD(secs);
			AuditGo.SetActive(true);
		}).AddTo(this);

		RxSubjects.Pass.Subscribe((e) => {
			hideWithMsg("带入成功");

			if (GameData.Shared.InGame) {
				PokerUI.Toast("记分牌带入成功，将在下局游戏生效");
			} else {
				PokerUI.Toast("记分牌带入成功");
			}
		}).AddTo(this);

		RxSubjects.UnPass.Subscribe((e) => {
			hideWithMsg("拒绝带入");			
		}).AddTo(this);
	}

	private void hideWithMsg(string msg) {
		if (disposable != null) {
			disposable.Dispose();
		}
		CountDown.text = msg;
		
		disposable = Observable.Timer(TimeSpan.FromSeconds(2)).AsObservable().Subscribe((_) => {
			AuditGo.SetActive(false);
		});
	}

	private void enableCD(int time) {
		CountDown.text = time.ToString();

		if (disposable != null) {
			disposable.Dispose();
		}

		disposable = Observable.Interval(TimeSpan.FromSeconds(1)).AsObservable().Subscribe((_) => {
			time = time - 1;
			if (time < 0) {
				time = 0;
			}

			CountDown.text = time.ToString();
		}).AddTo(this);
	}
}
