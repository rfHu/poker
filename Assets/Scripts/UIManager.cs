using UnityEngine;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using Extensions;

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
	
	public List<Sprite> muteSprites;
    public GameObject muteObj;

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
		var mySeat = GConf.MySeat;

		if (mySeat < 0) {
			return ;
		}

		Connect.shared.Emit(new Dictionary<string, object>(){
			{"f", "unseat"}
		});
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
			muteObj.GetComponent<Image>().sprite = muteSprites[1];
		} else {
			AudioListener.volume = 1;
		    muteObj.GetComponent<Image>().sprite = muteSprites[0];
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

	void onTakeCoin(object sender, DelegateArgs e) {
		GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Supplement"));
		ShowPopup(obj, () => {
			Connect.shared.Emit(new Dictionary<string, object>() {
				{"f", "unseat"}
			});
		});
	} 

	public void ShowPopup(GameObject obj, Action callback = null) {
		obj.transform.SetParent(popupCanvas.transform, false);
		Animations.ShowPopup(obj);
		ShowMask(() => {
			Animations.HidePopup(obj);	

			if (callback != null) {
				callback();
			}
		});
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
