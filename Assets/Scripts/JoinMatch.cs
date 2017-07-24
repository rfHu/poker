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

    private DialogAlert payDialog;

    void OnSpawned() 
    {

        EntryFee.text = GameData.MatchData.Data[0].ToString();
        InitialScoreboard.text = "初始记分牌：" + GameData.MatchData.Data[1];
        Charge.text = GameData.MatchData.Data[2].ToString();

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
				payDialog = PokerUI.Alert("金币不足，请购买", () => {
					Commander.Shared.PayFor();

					// 隐藏购买按钮
					payDialog.Hide();

					// 购买记分牌弹框
					RxSubjects.TakeCoin.OnNext(new RxData());
				}, null);
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
}
