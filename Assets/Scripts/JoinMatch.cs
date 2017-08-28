using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MaterialUI;

public class JoinMatch : MonoBehaviour {

    public Text EntryFee;
    public Text InitialScoreboard;
    public Text Coins;
    public Text Charge;

    void OnSpawned() 
    {

        EntryFee.text = GameData.MatchData.SNGJoinFee.ToString();
        InitialScoreboard.text = string.Format("初始记分牌：<color=#fff>{0}</color>", GameData.MatchData.BankrollNum);
        Charge.text = GameData.MatchData.SNGServerFee.ToString();

        Coins.text = _.Num2CnDigit(GameData.Shared.Coins);
    }

    public void TakeCoin()
    {
        Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "takecoin"},
			{"args", new Dictionary<string, object>{
				{"multiple", int.Parse(EntryFee.text)}
			}}
		}, (json, err) =>
        {
            if (err == 1201)
            {
				_.PayFor(() => {
                    RxSubjects.TakeCoin.OnNext(new RxData());
                });
			} 

            GetComponent<DOPopup>().Close();
        });
    }

    public void Exit() 
    {
        if (!GameData.MyCmd.Unseat)
        {
            return;
        }

        Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "unseat"}
		});

        GetComponent<DOPopup>().Close();
    }

    void OnDespawned() 
    {
        Exit();
    }
}
