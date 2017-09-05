using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UniRx;
using System;

public class InsureToast: MonoBehaviour {
    public Text MsgText;
    public RawImage Avatar;
    public Text NameText;
    public GameObject SeePage;
    public GameObject ChatButton;

    private CanvasGroup cvg;
    private float duration = 0.2f;
    private Tween tween;

    private ReactiveProperty<int> cd = new ReactiveProperty<int>(); 
    private IDisposable disposable;

    void Awake()
    {
        cvg = GetComponent<CanvasGroup>();
        addEvents();
    }

    private void addEvents() {
        RxSubjects.Insurance.Subscribe((e) =>
        {
            var uid = e.Data.String("uid");
            var ct = e.Data.Int("ct");
            var pot = e.Data.Int("pot");
            var amount = e.Data.Int("amount");
            var time = e.Data.Int("time");
            var pay = e.Data.Int("pay");
            var type = e.Data.Int("type");
            var touid = e.Data.String("touid");

            var player = GameData.Shared.FindPlayer(uid);
            
            var name = player.Name;
            var text = "";
            var url = player.Avatar;
            var toName = GameData.Shared.FindPlayer(touid).Name;

            cd.Value = time;

            if (type != 3)
            {
                if (SeePage.activeInHierarchy)
                {
                    SeePage.SetActive(false);
                }


            }

            switch (type)
            {
                case 1: case 2: case 4:
                    Hide(() =>
                    {
                        var dt = new Dictionary<int, string>()
                        {
                            {1, "多名领先玩家，将直接发牌"},
                            {2, "无需风险控制，将直接发牌"},
                            {4, "领先玩家选择直接发牌"}
                        };

                        PokerUI.Toast(dt[type]);
                    });
                    return;
                case 20:
                    text = String.Format(
                        "已购买<color=#18FFFFFF>{0}</color>张OUTS\n保费<color=#FFAB40FF>{1}</color>，预计赔付<color=#FFAB40FF>{2}</color>", 
                     ct, _.Num2CnDigit(amount), _.Num2CnDigit(pay));
                    break;
                case 3:
                    SeePage.SetActive(true);
                    text = cdText(time);
                    break;
                case 11:
                    if (amount == 0) {
                        text = String.Format("获得保险赔付：<color=#FFAB40FF>{0}</color>", _.Num2CnDigit(pay));
                    } else {
                        text = String.Format(
                            "获得保险赔付：<color=#FFAB40FF>{0}</color>\n扣除保费：<color=#FFAB40FF>{1}</color>",
                            _.Num2CnDigit(pay),
                            _.Num2CnDigit(amount)
                        );
                    }
                    
                    break;
                case 12:
                    text = "未买中保险，牌面打平";
                    break;
                case 10:
                    text = "<color=#FFAB40FF>继续领先</color>\n结算时将扣除保费";
                    break;
                case 13:
                    text = string.Format("获得底池：<color=#FFAB40FF>{0}</color>\n扣除保费：<color=#FFAB40FF>{1}</color>", _.Num2CnDigit(pot), _.Num2CnDigit(amount));
                    break;
                case 14:
                    text = string.Format("未买中保险\n被<color=#4FC3F7FF>{0}</color>反超", toName);
                    break;
                default:
                    Hide();
                    return ;
            }

            MsgText.text = text;
            NameText.text = name;
            Avatar.GetComponent<Avatar>().SetImage(url);
           
            Show();
        }).AddTo(this);

        cd.Subscribe((time) => {
            if (time < 0) {
                if (disposable != null) {
                    disposable.Dispose();
                }
                return ;
            }

            MsgText.text = cdText(time);
        }).AddTo(this);

        RxSubjects.Moretime.Subscribe((e) => {
            var model = e.Data.ToObject<MoreTimeModel>();

            if (model.IsRound()) {
                return ;
            }

            cd.Value = model.total;
        }).AddTo(this);

        RxSubjects.GameOver.Subscribe((_) => {
            Hide();
        }).AddTo(this);

        RxSubjects.ToInsurance.Subscribe((_) => {
            Hide();
        }).AddTo(this);

        RxSubjects.Look.Subscribe((_) => {
            Hide();
        }).AddTo(this);
    }

    private string cdText(int time) {
        return String.Format("玩家购买保险中<color=#18FFFFFF>{0}s</color>", time);
    }

    private void startCd() {
        if (disposable != null) {
            disposable.Dispose();
        }

        if (cd.Value == 0) {
            return ;
        }

        disposable = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe((_) => {
            cd.Value -= 1;
        }).AddTo(this);
    }

    public void Show() {
        if (tween != null) {
            tween.Kill();
        }

        if (disposable != null) {
            disposable.Dispose();
        }

        tween = cvg.DOFade(1, duration).OnComplete(startCd);
    }

    public void Hide(TweenCallback complete = null) {
        if (tween != null) {
            tween.Kill();
        }

        if (disposable != null) {
            disposable.Dispose();
        }


        tween = cvg.DOFade(0, duration).OnComplete(complete);
    }
}