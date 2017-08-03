using MaterialUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class RebuyOrAddon : MonoBehaviour {

    public Text Title;

    public Text Message;

    public Text CueText;

    public Text EnterText;

    public Text RebuyTimes;

    private RAData radata;
    private bool relive;
    private IDisposable disposable;

    public void Rebuy(bool relive = false) {
        radata = new RAData("rebuy"); 
        this.relive = relive;
        setup();       
    }


    public void AddOn(bool relive = false) {
        radata = new RAData("addon");
        this.relive = relive;
        setup();
    }

    private void setup() {
        EnterText.text = Title.text = radata.title;
        CueText.text = radata.cue;
        CueText.gameObject.SetActive(!relive);
        
        var player = GameData.Shared.GetMyPlayer();
        var rbgo = RebuyTimes.transform.parent.gameObject;

        if (radata.type == "rebuy" && player.IsValid()) {
            rbgo.SetActive(true);
            RebuyTimes.text = player.LeftRebuy + "次";
        } else {
           rbgo.SetActive(false); 
        }

        if (relive) {
            CueText.gameObject.SetActive(false);
            setupDispose();
        } else {
            CueText.text = radata.cue;
            Message.text = radata.desc();
        }
    }

    private void setupDispose() {
        if (disposable != null) {
            disposable.Dispose();
        }

        var time = 15;
        setMessage(time);

       disposable = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe((_) => {
            time -= 1;

            if (time == 0) {
                disposable.Dispose();
                Exit();
            } else {
                setMessage(time);
            }
        });
    }

    private void setMessage(int time) {
        Message.text = string.Format("记分牌输光了，{0}（{1}秒）", radata.desc() , time);
    }

    public void Enter()
    {
        // 复活金币不足，不弹出购买框；主动点击弹出购买框
        Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", radata.type},
            {"for_match", 1}
		}, (json, err) =>
        {
            if (err == 0) {
                if (json.ContainsKey("bankroll")) {
                    radata.increase();
                    PokerUI.Toast(radata.successText);
                } else {
                    PokerUI.Toast(string.Format("{0}申请已提交，等待群主审核中", radata.title));
                }

                PokerUI.Toast(radata.successText);
            } else if (err == 1201)
            {
                PokerUI.Toast("金币不足，无法购买记分牌"); 
            } else {
                // PokerUI.Toast("服务器出错了");
            }

            GetComponent<DOPopup>().Close();
        });
    }

    public void Exit() 
    {
        if (relive) {
            Connect.Shared.Emit("nobuy");
        }

        GetComponent<DOPopup>().Close();
    }

    void OnDespawned() 
    {
        if (disposable != null) {
            disposable.Dispose();
        }
    }
}

internal class RAData {
    internal string title;
    internal int cost;
    internal int chips;
    internal string cue;
    internal string type;
    
    internal Action increase;

    internal string successText;


    internal string desc() {
        var cost = this.cost + (int)(this.cost * 0.1);
        return string.Format("是否花费<color=#FFD028FF>${0}</color>{1}<color=#FFD028FF>{2}</color>记分牌", cost, title, chips);
    }
    
    internal RAData(string type) {
        this.type = type;
        var cost = GameData.MatchData.Data[0];
        var chips = GameData.MatchData.Data[1];
        
        if (type == "rebuy") {
            title = "重购";
            this.cost = cost;
            this.chips = chips;
            cue = string.Format("温馨提示：盲注升到第 <color=#FFD028FF>{0}</color> 级别后，将无法再重购", GameData.MatchData.LimitLv);
            increase = () => {
                var player = GameData.Shared.GetMyPlayer();
                if (player.IsValid()) {
                    player.RebuyCount += 1;
                }
            };
            successText = string.Format("成功重购{0}记分牌（下一手生效）", chips);
        } else {
            title = "增购";
            this.cost = (int)(cost * 1.5);
            this.chips = (int)(chips * 1.5);
            cue = "温馨提示：本级别过后将无法再增购";
            increase = () => {
                var player = GameData.Shared.GetMyPlayer();
                if (player.IsValid()) {
                    player.AddonCount += 1;
                }
            };
            successText = string.Format("成功增购{0}记分牌（下一手生效）", chips);
        }

        // successText
    }
}
