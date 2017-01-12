using UnityEngine;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
	public Canvas canvas;
	public GameObject maskPrefab;
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
		var popup = (GameObject)Instantiate(Resources.Load("Prefab/MenuPopup"));
		popup.GetComponent<DOPopup>().Show(popupCanvas);	
		// DOTweenAnimation anim = menu.GetComponent<DOTweenAnimation>();
		// ShowTween(anim);
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
	
	void HideMenu() {
		// 隐藏menu和上一个遮罩
		// menu.GetComponent<DOTweenAnimation>().DORestartById("Hide");
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

	void onTakeCoin(object sender, DelegateArgs e) {
		GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Supplement"));
		obj.GetComponent<DOPopup>().Show(popupCanvas);
	} 

	void Awake()
	{
		Delegates.shared.TakeCoin += new EventHandler<DelegateArgs>(onTakeCoin);
	}

	void OnDestroy()
	{
		Delegates.shared.TakeCoin -= new EventHandler<DelegateArgs>(onTakeCoin);
	}
}
