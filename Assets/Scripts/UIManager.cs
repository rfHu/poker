using UnityEngine;
using Extensions;
using UniRx;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
	public GameObject recallPage;
	private GameObject auditMsg;

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
		GameData.Shared.AuditList.Subscribe((list) => {
			if (!auditMsg)
            {
                auditMsg = (GameObject)Instantiate(Resources.Load("Prefab/AuditMsg"));
            }

            if (list.Count == 0)
            {
                auditMsg.GetComponent<DOPopup>().Close();
            }
            else
            {
                auditMsg.GetComponent<DOPopup>().Show(G.Cvs, null, false);
            }
		});	

		RxSubjects.TakeCoin.Subscribe((e) => {
			if (e.Data != null) {
				GameData.Shared.Coins = e.Data.Int("coins");
            }

			if (menuPopup != null) {
				menuPopup.Close();
			}

            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Supplement"));
            obj.GetComponent<DOPopup>().Show(G.Cvs, () => {
				var player = GameData.Shared.GetMyPlayer();

				if (player == null || player.Bankroll.Value > 0) {
					return ;
				}

				Connect.Shared.Emit(new Dictionary<string, object>() {
					{"f", "unseat"}
				});
			});
		}).AddTo(this);

		RxSubjects.Exclusion.Subscribe((e) => {
			var type = e.Data.Int("type");
		
			if (type == 2)
            {
				PokerUI.ExitAlert();
            }
		}).AddTo(this);
	}
}
