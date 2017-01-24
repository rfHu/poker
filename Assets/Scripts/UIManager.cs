using UnityEngine;
using System;
using UIWidgets;
using Extensions;
using UniRx;

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

	void Awake()
	{
		Delegates.shared.Exclusion += new EventHandler<DelegateArgs>(onExclusion);

		RxSubjects.Audit.Subscribe((e) => {
			var array = e.Data.List("ids");

            if (!auditMsg)
            {
                auditMsg = (GameObject)Instantiate(Resources.Load("Prefab/AuditMsg"));
            }

            if (array.Count == 0)
            {
                auditMsg.GetComponent<DOPopup>().Close();
            }
            else
            {
                auditMsg.GetComponent<DOPopup>().Show(canvas);
            }
		});

		RxSubjects.Audit.Subscribe((e) => {
			if (e.Data != null) {
				GameData.Shared.Coins = e.Data.Int("coins");
            }

            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Supplement"));
            obj.GetComponent<DOPopup>().Show(canvas);
		});
	}

	void onExclusion(object sender, DelegateArgs e) {
		var type = e.Data.Int("type");
		
		if (type == 2) {
			PokerUI.ShowDialog("您的账号已在其他设备登陆");
		}
	}
}
