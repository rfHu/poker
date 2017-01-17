using UnityEngine;
using System;
using UIWidgets;
using Extensions;

public class UIManager : MonoBehaviour {
	public Canvas canvas;
	public GameObject recallPage;

	public void ShowMenu() {
		var popup = (GameObject)Instantiate(Resources.Load("Prefab/MenuPopup"));
		popup.GetComponent<DOPopup>().Show(canvas);	
	}
	
	public void Exit() {
		Application.Quit();
	}

	public void ScorePage() {
		var score = (GameObject)Instantiate(Resources.Load("Prefab/ScorePage"));
		score.GetComponent<DOPopup>().Show(canvas);	
	}
	
	public void OnShowRecalls() {
		var recall = (GameObject)Instantiate(Resources.Load("Prefab/RecallPage"));		
		recall.GetComponent<DOPopup>().Show(canvas);
	}

	void onTakeCoin(object sender, DelegateArgs e) {
		GConf.coins = e.Data.Int("coins");
		GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Supplement"));
		obj.GetComponent<DOPopup>().Show(canvas);
	} 

	void Awake()
	{
		Delegates.shared.TakeCoin += new EventHandler<DelegateArgs>(onTakeCoin);
		Delegates.shared.Exclusion += new EventHandler<DelegateArgs>(onExclusion);
	}

	void OnDestroy()
	{
		Delegates.shared.TakeCoin -= new EventHandler<DelegateArgs>(onTakeCoin);
		Delegates.shared.Exclusion -= new EventHandler<DelegateArgs>(onExclusion);
	}

	void onExclusion(object sender, DelegateArgs e) {
		var type = e.Data.Int("type");
		
		if (type == 2) {
			UIManager.ShowDialog("您的账号已在其他设备登陆");
		}
	}

	// Test Methods
	static public void ShowDialog(string text) {
		var go = (GameObject)Instantiate(Resources.Load("Prefab/DialogTemplate"));
		var canvas = GameObject.FindWithTag("Canvas1").GetComponent<Canvas>();
		go.GetComponent<Dialog>().Show(message: text, modal: true, modalColor: new Color(0, 0, 0, 0.2f), canvas: canvas);
	}
}
