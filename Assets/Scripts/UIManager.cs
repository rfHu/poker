using UnityEngine;
using Extensions;
using UniRx;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
	public GameObject recallPage;
	GameObject auditMsg;

	private DOPopup menuPopup;

	public void ShowMenu() {
		var popup = (GameObject)Instantiate(Resources.Load("Prefab/MenuPopup"));
		menuPopup = popup.GetComponent<DOPopup>();
		menuPopup.Show(G.Cvs);	
	}

	public void ScorePage() {
		var score = (GameObject)Instantiate(Resources.Load("Prefab/ScorePage"));
		score.GetComponent<DOPopup>().Show(G.Cvs);	
	}
	
	public void OnShowRecalls() {
		var recall = (GameObject)Instantiate(Resources.Load("Prefab/RecallPage"));		
		recall.GetComponent<DOPopup>().Show(G.Cvs);
	}

	void Awake()
	{
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
                auditMsg.GetComponent<DOPopup>().Show(G.Cvs);
            }
		}).AddTo(this);

		RxSubjects.TakeCoin.Subscribe((e) => {
			if (e.Data != null) {
				GameData.Shared.Coins = e.Data.Int("coins");
            }

			if (menuPopup != null) {
				menuPopup.Close();
			}

            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Supplement"));
            obj.GetComponent<DOPopup>().Show(G.Cvs, () => {
				Connect.Shared.Emit(new Dictionary<string, object>() {
					{"f", "unseat"}
				});
			});
		}).AddTo(this);

		RxSubjects.Exclusion.Subscribe((e) => {
			var type = e.Data.Int("type");
		
			if (type == 2)
            {
                PokerUI.Alert("您的账号已在其他设备登陆");
            }
		}).AddTo(this);
	}
}
