﻿using UnityEngine;
using MaterialUI;
using System;

public class PokerUI: MonoBehaviour {
	static private DialogAlert dialogAlert;

	static public DialogAlert ShowDialog(string text, Action yesAction, string yesText, Action cancelAction, string cancelText) {
		if (dialogAlert != null) {
			dialogAlert.Hide();
		}

		DialogAlert dialog = DialogManager.CreateAlert();
		dialog.Initialize(text, yesAction, yesText, null, null, cancelAction, cancelText);

		var animator = new DialogAnimatorSlide(0.5f, DialogAnimatorSlide.SlideDirection.Bottom, DialogAnimatorSlide.SlideDirection.Bottom);
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

	static public DialogAlert DisAlert(string msg) {
		if (Connect.Shared == null) {
			return null;
		}
		Connect.Shared.CloseImmediate();
		return PokerUI.Alert(msg, External.Instance.ExitCb);
	}

	static public DialogAlert ConflictAlert() {
		return DisAlert("登录已过期，请重新登录");
	}

	static public void Toast(string msg, float seconds = 2) {
		ToastManager.Show(msg, seconds, _.HexColor("#212932"), new Color(1, 1, 1, 1), 26, null);	
	}
}