﻿using UnityEngine;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIManager : MonoBehaviour {
	public void SelectMenu() {

	}

	public GameObject menu;
	public Canvas canvas;
	public GameObject maskPrefab;
	public GameObject cardTipPanel;
	public GameObject scorePage;
	public Canvas popupCanvas;
	public GameObject recallPage;
	public GameObject userPopup;
	public GameObject startButton;

	GameObject ShowMask(Action callback) {
		GameObject mask = Instantiate(maskPrefab);
		mask.transform.SetParent(canvas.transform, false);
		EventTrigger trigger = mask.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener((eventData) => {
			callback();
			Destroy(mask);
		}); 
		trigger.triggers.Add(entry);

		return mask;
	}

	public void ShowMenu() {
		DOTweenAnimation anim = menu.GetComponent<DOTweenAnimation>();
		ShowTween(anim);
	}

	public void Standup() {
		Debug.Log("My God");
	}

	public void Exit() {
		Application.Quit();
	}

	public void UserInfo() {
		DOTweenAnimation anim = userPopup.GetComponent<DOTweenAnimation>();
		ShowTween(anim);
		HideMenu();	
	}

	public void ScorePage() {
		DOTweenAnimation anim = scorePage.GetComponent<DOTweenAnimation>();
		ShowTween(anim);
	}

	public void CardTip() {
		DOTweenAnimation anim = cardTipPanel.GetComponent<DOTweenAnimation>();
		ShowTween(anim);
		HideMenu();
	}

	void HideMenu() {
		// 隐藏menu和上一个遮罩
		menu.GetComponent<DOTweenAnimation>().DORestartById("Hide");
		GameObject obj = GameObject.Find("Mask(Clone)");		
		Destroy(obj);
	}

	void ShowPage(Animator anim, Action callback = null) {
		anim.SetBool("Show", true);

		ShowMask(() => {
			anim.SetBool("Show", false);
			if (callback != null) {
				callback();
			}
		});		
	}

	public void ToggleMute() {
		if (AudioListener.volume > 0) {
			AudioListener.volume = 0;
		} else {
			AudioListener.volume = 1;
		}
	}

	public void ShowRecalls() {
		DOTweenAnimation anim = recallPage.GetComponent<DOTweenAnimation>();
		ShowTween(anim);
	}

	void ShowTween(DOTweenAnimation anim) {
		anim.DORestartById("Show") ;

		ShowMask(() => {
			anim.DORestartById("Hide");			
		});
	}

	public void Supplement() {

	}
}
