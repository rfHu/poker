using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {
	public void SelectMenu() {

	}

	public GameObject menu;
	public Canvas canvas;
	public GameObject maskPrefab;
	public GameObject cardTipPanel;
	public GameObject scorePage;
	public Canvas frontCanvas;

	void showMask(Action callback) {
		GameObject mask = Instantiate(maskPrefab);
		mask.transform.SetParent(frontCanvas.transform, false);
		EventTrigger trigger = mask.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener((eventData) => {
			callback();
			Destroy(mask);
		}); 
		trigger.triggers.Add(entry);
	}

	public void ShowMenu() {
		Animator anim = menu.GetComponent<Animator>();
		anim.SetBool("Show", true);
		showMask(() => {
			anim.SetBool("Show", false);
		});
	}

	public void Standup() {
		Debug.Log("My God");
	}

	public void Exit() {
		Application.Quit();
	}

	public void Supplement() {
		Debug.Log("Supplement");
	}

	public void ScorePage() {
		scorePage.GetComponent<Animator>().SetBool("Show", true);
	}

	public void CardTip() {
		menu.GetComponent<Animator>().SetBool("Show", false);
		cardTipPanel.GetComponent<Animator>().SetBool("ShowCard", true);
	}

	public void ToggleMute() {
		if (AudioListener.volume > 0) {
			AudioListener.volume = 0;
		} else {
			AudioListener.volume = 1;
		}
	}
}
