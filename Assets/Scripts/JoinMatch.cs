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

    private int[][] data = { new int[] { 200, 2000, 20 }, new int[] { 500, 4000, 50 }, new int[] { 1000, 4000, 100 }, new int[] { 2000, 8000, 200 } };

    void OnSpawned() 
    {
        int type = GameData.Shared.SNGType - 1;

        EntryFee.text = data[type][0].ToString();
        InitialScoreboard.text = "初始记分牌：" + data[type][1];
        Charge.text = data[type][2].ToString();

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
