using UnityEngine;
using MaterialUI;
using System;

public class PokerUI: MonoBehaviour {
	static private DialogAlert dialogAlert;

	static public void ShowDialog(string text, Action yesAction, string yesText, Action cancelAction, string cancelText) {
		if (dialogAlert != null) {
			dialogAlert.Hide();
		}

		DialogAlert dialog = DialogManager.CreateAlert();
		dialog.Initialize(text, yesAction, yesText, null, null, cancelAction, cancelText);

		var animator = new DialogAnimatorSlide(0.5f, DialogAnimatorSlide.SlideDirection.Bottom, DialogAnimatorSlide.SlideDirection.Bottom);
		animator.backgroundAlpha = 0f;

		dialog.dialogAnimator = animator;
		dialog.Show();

		// 保证单实例
		dialogAlert = dialog; 
	}

	static public void Alert(string text) {
		ShowDialog(text, null, "确定", null, null);
	}

	static public void Alert(string text, Action yesAction) {
		ShowDialog(text, yesAction, "确定", null, null);
	}

	static public void Alert(string text, Action yesAction, Action cancelAction) {
		ShowDialog(text, yesAction, "确定", cancelAction, "取消");
	}

	static public void ExitAlert() {
		PokerUI.Alert("您的账号已在其他设备登陆", () => {
			External.Instance.Exit();
		});
	}
}