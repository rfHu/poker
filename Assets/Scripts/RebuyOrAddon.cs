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

    public GameObject[] ShowInMenuOpen;

    public GameObject LeftTimes;

    private DialogAlert payDialog;
    private int gameType;

    IEnumerator myCoroutine;

    public void Init(bool Rebuy = true, bool isPush = false) 
    {
        gameType = Rebuy ? 1 : 2;

        Title.text = Rebuy? "重购":"增购";

        EnterText.text = Rebuy ? "重购" : "增购";

        foreach (var item in ShowInMenuOpen)
        {
            item.SetActive(!isPush);
        }
        LeftTimes.SetActive(isPush);
        if (!isPush)
        {
 

            Money.text = GameData.Shared.Coins.ToString();

            Fee.text = GameData.MatchData.Data[0].ToString();

            LimitLv.text = GameData.MatchData.LimitLv.ToString();

            Message.text = string.Format("{0}<color=#ffd028>{1}</color>倍的初始记分牌，继续参与赛事", Title.text, Rebuy ? "1" : "1.5");
        }
        else
        {
            LeftTimes.GetComponentInChildren<Text>().text = (Rebuy ? (GameData.MatchData.Rebuy - GameData.Shared.GetMyPlayer().RebuyCount) : 1) + "次";
            myCoroutine = Timer(15);
            StartCoroutine(myCoroutine);
        }
    }

    public void Enter()
    {
        Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", gameType == 1? "rebuy": "addon"}
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

    void OnDespawned() 
    {
        StopCoroutine(myCoroutine);
    }

    private IEnumerator Timer(float time)
    {
        int pay = (int)(GameData.MatchData.Data[0] * (gameType == 1? 1 : 1.5f));
        int get = (int)(GameData.MatchData.Data[1] * (gameType == 1? 1 : 1.5f));

        string str = string.Format("比赛筹码输光了，是否花费${0}重新购买{1}筹码   （", pay, get);

        while (time > 0)
        {
            time = time - Time.deltaTime;
            Message.text = str + (int)time + "秒）";

            yield return new WaitForFixedUpdate();
        }

        GetComponent<DOPopup>().Close();
    }
}
