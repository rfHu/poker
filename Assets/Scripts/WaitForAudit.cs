using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;

public class WaitForAudit : MonoBehaviour {
    public Text CountDown;
    private IDisposable disposable;

	void Awake()
	{
		addEvents();
	}

    private void addEvents() {
		RxSubjects.Pass.Subscribe((e) => {
			Hide();
		}).AddTo(this);

		RxSubjects.UnPass.Subscribe((e) => {
			Hide();
			
			string type;

			switch(GameData.Shared.Type) {
				case GameType.MTT:
					type = "购买";
					break;
				case GameType.SNG:
					type = "报名";
					break;
				default:
					type = "带入";
					break;
			}
			PokerUI.Toast(string.Format("房主拒绝您的{0}申请", type));
		}).AddTo(this);

		GameData.Shared.AuditCD.Subscribe((secs) => {
			if (secs > 0) {
				Show(secs);
			} else {
				Hide();
			}
		}).AddTo(this);
	}
	
	public void Show(int secs) {
		GetComponent<DOPopup>().Show(null, false, false);
		enableCD(secs);
	}

	public void Hide() {
		if (disposable != null) {
			disposable.Dispose();
		}

		GetComponent<DOPopup>().Hide();
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
