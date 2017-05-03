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
			hideWithMsg("带入成功");
		}).AddTo(this);

		RxSubjects.UnPass.Subscribe((e) => {
			hideWithMsg("拒绝带入");			
		}).AddTo(this);

		GameData.Shared.AuditCD.Subscribe((secs) => {
			if (secs > 0) {
				GetComponent<DOPopup>().Show(null, false, false);
				Show(secs);
			} else {
				Hide();
			}
		}).AddTo(this);
	}

	private void hideWithMsg(string msg) {
		if (disposable != null) {
			disposable.Dispose();
		}
		CountDown.text = msg;
		
		disposable = Observable.Timer(TimeSpan.FromSeconds(2)).AsObservable().Subscribe((_) => {
			Hide();
		});
	}

	public void Show(int secs) {
		GetComponent<DOPopup>().Show(null, false, false);
		enableCD(secs);
	}

	public void Hide() {
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
