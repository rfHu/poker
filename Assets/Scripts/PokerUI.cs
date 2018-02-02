using UnityEngine;
using MaterialUI;
using System;
using UniRx;

public class PokerUI: MonoBehaviour {
	static private DialogAlert dialogAlert;

	static public void RemoveDialog() {
		if (dialogAlert != null) {
			dialogAlert.Hide();
		}
	}

	static public void DestroyElements() {
		var cvs = G.MaterialCvs;
		var toast = cvs.GetComponentInChildren<ToastManager>();

		if (toast) {
			Destroy(toast.gameObject);
		}

		var dialog = cvs.GetComponentInChildren<DialogManager>();

		if (dialog) {
			Destroy(dialog.gameObject);
		}
	}

	static public DialogAlert ShowDialog(string text, Action yesAction, string yesText, Action cancelAction, string cancelText) {
		if (dialogAlert != null) {
			dialogAlert.Hide();
		}

		DialogAlert dialog = DialogManager.CreateAlert();
		dialog.Initialize(text, yesAction, yesText, null, null, cancelAction, cancelText);

		var animator = new DialogAnimatorSlide(0.2f, DialogAnimatorSlide.SlideDirection.Left, DialogAnimatorSlide.SlideDirection.Right);
		animator.backgroundAlpha = 0f;

		dialog.dialogAnimator = animator;
		dialog.ShowModal();

		// 保证单实例
		dialogAlert = dialog; 

		return dialogAlert;
	}

	static public DialogAlert Alert(string text) {
		return ShowDialog(text, null, "确定", null, null);
	}

	static public DialogAlert Alert(string text, Action yesAction) {
		return ShowDialog(text, yesAction, "确定", null, null);
	}

	static public DialogAlert Alert(string text, Action yesAction, Action cancelAction) {
		return ShowDialog(text, yesAction, "确定", cancelAction, "取消");
	}

    static public DialogAlert Alert(string text, string yesText, Action yesAction, Action cancelAction)
    {
        return ShowDialog(text, yesAction, yesText, cancelAction, "取消");
    }

    static public DialogAlert DisAlert(string msg) {
		if (Connect.Shared == null) {
			return null;
		}

		var alert = PokerUI.Alert(msg, External.Instance.ExitCb);
		Connect.Shared.CloseImmediate();

		return alert;
	}

	static public DialogAlert ConflictAlert() {
		return DisAlert("登录已过期，请重新登录");
	}

	static public void Toast(string msg, float seconds = 2) {
		var canvasHierarchy = G.MaterialCvs.transform.Find("MaterialUI");
		ToastManager.Show(msg, seconds, MaterialColor.grey900, new Color(1, 1, 1, 1), 39, canvasHierarchy);
		// ToastManager.Hide();
	}

	static public void ToastThenExit(string msg) {
		Toast(msg);
		Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe((_) => {
			External.Instance.Exit();					
		});
	}
}