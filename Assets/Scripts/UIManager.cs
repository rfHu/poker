using UnityEngine;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;
using UIWidgets;
using Extensions;

public class UIManager : MonoBehaviour {
	public Canvas canvas;
	public GameObject maskPrefab;
	public Canvas popupCanvas;
	public GameObject recallPage;
	public GameObject userPopup;
	public GameObject startButton;

	void Start()
	{
		ShowDialog();
	}

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
	}
	
	public void Exit() {
		Application.Quit();
	}

	public void UserInfo() {
		DOTweenAnimation anim = userPopup.GetComponent<DOTweenAnimation>();
		ShowTween(anim);
	}

	public void ScorePage() {
		var score = (GameObject)Instantiate(Resources.Load("Prefab/ScorePage"));
		score.GetComponent<DOPopup>().Show(popupCanvas);	
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
		GConf.coins = e.Data.Dict("args").Int("coins");
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

	// Test Methods
	void ShowDialog() {
		// var go = (GameObject)Instantiate(Resources.Load("Prefab/DialogTemplate"));
		// go.GetComponent<Dialog>().Show(message: "金币不足，请购买", modal: true, modalColor: new Color(0, 0, 0, 0.2f), canvas: popupCanvas);
	}
}
