using UnityEngine;
using System;
using UIWidgets;
using Extensions;

public class UIManager : MonoBehaviour {
	public Canvas canvas;
	public GameObject recallPage;
	GameObject auditMsg;

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
		if (e.Data != null) {
			GConf.coins = e.Data.Int("coins");
		}

		GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Supplement"));
		obj.GetComponent<DOPopup>().Show(canvas);
	} 

	void Awake()
	{
		Delegates.shared.TakeCoin += new EventHandler<DelegateArgs>(onTakeCoin);
		Delegates.shared.Exclusion += new EventHandler<DelegateArgs>(onExclusion);
		Delegates.shared.Audit += new EventHandler<DelegateArgs>(onAudit);
	}

	void OnDestroy()
	{
		Delegates.shared.TakeCoin -= new EventHandler<DelegateArgs>(onTakeCoin);
		Delegates.shared.Exclusion -= new EventHandler<DelegateArgs>(onExclusion);
		Delegates.shared.Audit -= new EventHandler<DelegateArgs>(onAudit);
	}

	void onExclusion(object sender, DelegateArgs e) {
		var type = e.Data.Int("type");
		
		if (type == 2) {
			PokerUI.ShowDialog("您的账号已在其他设备登陆");
		}
	}

	void onAudit(object sender, DelegateArgs e) {
		var array = e.Data.List("ids");

		if (!auditMsg) {
			auditMsg = (GameObject)Instantiate(Resources.Load("Prefab/AuditMsg"));
		}

		if (array.Count == 0) {
			auditMsg.GetComponent<DOPopup>().Close();
		} else {
			auditMsg.GetComponent<DOPopup>().Show(canvas);
		}
	}
}
