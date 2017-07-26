using MaterialUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RebuyOrAddon : MonoBehaviour {

    public Text Title;

    public Text Message;

    public Text Money;

    public Text Fee;

    public Text LimitLv;

    public Text EnterText;

    private DialogAlert payDialog;
    private int Type;

    public void Init(bool Rebuy = true) 
    {
        Type = Rebuy ? 1 : 2;

        Title.text = Rebuy? "重购":"增购";

        EnterText.text = Rebuy ? "重购" : "增购";

        Message.text = string.Format("{0}<color=#ffd028>{1}</color>倍的初始记分牌，继续参与赛事", Title.text, Rebuy ? "1" : "1.5");

        Money.text = GameData.Shared.Coins.ToString();

        Fee.text = GameData.MatchData.Data[0].ToString();

        LimitLv.text = GameData.MatchData.LimitLv.ToString();
    }

    public void Enter()
    {
        Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", Type == 1? "rebuy": "addon"}
		}, (json, err) =>
        {
            if (err == 1201)
            {
                payDialog = PokerUI.Alert("金币不足，请购买", () =>
                {
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
        GetComponent<DOPopup>().Close();
    }
}
