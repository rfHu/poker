using UnityEngine;
using UnityEngine.UI;
using UniRx;
using SimpleJSON;
using DG.Tweening;
using System.Linq;
using System;
using System.Collections.Generic;

namespace PokerPlayer {
    public class PlayerBase: MonoBehaviour {
        public string Uid {
            get {
                return player.Uid;
            }   
        } 

        public Avatar Avt;
        public SpkTextGo SpkText;
	    public GameObject Volume;
	    public Text ScoreLabel;
        public PlayerActGo PlayerAct;
        public Text StateLabel;
	    public GameObject BackGameBtn;
	    public GameObject HandGo;
        public GameObject AllinGo;
        public Transform Circle;

        public Player player;
        private ActionState lastState;
	    private int actCardsNumber = 0;
        private ChipsGo chipsGo;

        private Seat theSeat;

        private PlayerDelegate myDelegate;

        public void Init(Player player, Seat theSeat, PlayerDelegate myDelegate) {
            this.player = player;
            this.theSeat = theSeat;
            this.myDelegate = myDelegate;
            SpkText.Uid = player.Uid;
		    ScoreLabel.text = _.Num2CnDigit<int>(player.Bankroll.Value);
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            var avatar = Avt.GetComponent<Avatar>();
		    avatar.Uid = player.Uid;
		    avatar.SetImage(player.Avatar);	

            if (GameData.Shared.InGame && !player.InGame) {
		    	SetFolded();
		    }

            addEvents();
        }

        public void BackGame() {
            Connect.Shared.Emit(new Dictionary<string, object>{
                {"f", "ready"}
            });
        }

        private void addEvents() {
            RxSubjects.ShowAudio.Where(isSelfJson).Subscribe((jsonStr) => {
                Volume.SetActive(true);
                ScoreLabel.gameObject.SetActive(false);
            }).AddTo(this);

            RxSubjects.HideAudio.Where(isSelfJson).Subscribe((_) => {
                Volume.SetActive(false);
                ScoreLabel.gameObject.SetActive(true);
            }).AddTo(this);

            RxSubjects.SendChat.Where(isSelfJson).Subscribe((jsonStr) => {
                var N = JSON.Parse(jsonStr);
                var text = N["text"].Value;
                SpkText.ShowMessage(text);
            }).AddTo(this);

            theSeat.SeatPos.Subscribe((pos) => {
                SpkText.ChangePos(pos);
                PlayerAct.ChangePos(pos);
            }).AddTo(this);

            player.PrChips.AsObservable().Subscribe((value) => {
                if (value == 0) {
                    return ;
                }

                setPrChips(value);
            }).AddTo(this);

             RxSubjects.Award27.Subscribe((e) => {
                if (Uid == e.Data.String("uid"))
                {
                    CancelInvoke("hideAnim");
                    Invoke("hideAnim", 4);
                }
            }).AddTo(this);

            player.PlayerStat.Subscribe((state) => {
                HandGo.SetActive(false);
                BackGameBtn.SetActive(false);

                var stateGo = StateLabel.transform.parent.gameObject;
                stateGo.SetActive(false);

                switch(state) {
                    case PlayerState.Waiting: case PlayerState.Auditing:
                        ScoreLabel.text = "<size=28>等待</size>";
                        break;
                    case PlayerState.Hanging:
                        HandGo.SetActive(true);
                        // if (isSelf) {
                        //     BackGameBtn.SetActive(true);
                        //     if (OPTransform != null) {
                        //         PoolMan.Despawn(OPTransform);
                        //     }
                        // }
                        break;
                    case PlayerState.Reserve:
                        stateGo.SetActive(true);	
                        // if (isSelf) {
                        //     BackGameBtn.SetActive(true);
                        // }
                        break;
                    default: 
                        break;
                }
            }).AddTo(this);

            player.ActState.AsObservable().Subscribe((e) => {
                if (e == ActionState.None) {
                    return ;
                }

                switch(e) {
                    case ActionState.Check:
                        G.PlaySound("check");
                        break;
                    case ActionState.Fold:
                        G.PlaySound("foldpai");
                        break;
                    case ActionState.Call:
                        break;
                    case ActionState.Allin:
                        if (player.ActStateTrigger) {
                            G.PlaySound("allin");
                        }
                        break;
                    case ActionState.Raise:
                        break;
                    default:
                        break;
                }

                if (e == ActionState.Fold) {
                    myDelegate.Fold();
                } else {
                    myDelegate.MoveOut();
                }

                dealAct(e);
                actCardsNumber = GameData.Shared.PublicCards.Count;	
            }).AddTo(this);

            IDisposable reserveCd = null; 
            player.ReservedCD.Subscribe((value) => {
                if (reserveCd != null) {
                    reserveCd.Dispose();
                }

                if (value > 0) {
                    setReserveCd(value);

                    reserveCd = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe((_) => {
                        value = Mathf.Max(value - 1, 1);
                        setReserveCd(value);
                    }).AddTo(this);
                }
            }).AddTo(this);

            player.LastAct.Where((act) => !String.IsNullOrEmpty(act)).Subscribe((act) => {
                dealAct(act.ToActionEnum());	
            }).AddTo(this);

            player.Allin.Subscribe((allin) => {
                if (allin) {
                    player.ActStateTrigger = false;
                    player.ActState.OnNext(ActionState.Allin);
                }
            }).AddTo(this);

            RxSubjects.GainChip.Where((gainChip) => gainChip.Uid == Uid).Subscribe((gainChip) => {
                gainChip.Grp.ToParent(transform);
            }).AddTo(this);

            player.Bankroll.Subscribe((value) => {
                ScoreLabel.text = _.Num2CnDigit(value);
            }).AddTo(this);

            RxSubjects.MoveTurn.Subscribe((e) => {
            	var uid = e.Data.String("uid");
            	var dc = e.Data.Int("deal_card");
            
            	if (uid == Uid) {
            		myDelegate.TurnTo(e.Data, GameData.Shared.ThinkTime);
            		setPlayerAct(false, false);
            	} else {
            		myDelegate.MoveOut();

            		// 刚发了牌
            		if (dc == 1 || GameData.Shared.PublicCards.Count != actCardsNumber) {
            			setPlayerAct(false);
            		}
            	}

            	// // 自动托管
            	// if (isSelf) {
            	// 	player.SetTrust(e.Data.Dict("trust"));
            	// }
            }).AddTo(this);

            // 思考延时
            RxSubjects.Moretime.Subscribe((e) => {
                var model = e.Data.ToObject<MoreTimeModel>();

                if (model.uid != Uid) {
                    return ;
                }

                if (!model.IsRound())
                {
                    return;
                }

                myDelegate.ResetTime(model.total); 
            }).AddTo(this);

            RxSubjects.GameOver.Subscribe((e) => {
                myDelegate.MoveOut();
                PlayerAct.gameObject.SetActive(false);
                AllinGo.SetActive(false);

                if (chipsGo != null) {
                    chipsGo.Hide();
                }
            }).AddTo(this);
        }

        private bool isSelfJson(string jsonStr) {
            var N = JSON.Parse(jsonStr);
            var uid = N["uid"].Value;
            return uid == Uid;
	    }

        private void setPrChips(int value) {
            var chips = (GameObject)Instantiate(Resources.Load("Prefab/UpChip"));
            chips.transform.SetParent(transform, false);
            chips.transform.SetAsLastSibling();

            if (chipsGo == null) {
                chipsGo = chips.GetComponent<ChipsGo>();
                chipsGo.Create(value, theSeat, player);
            } else {
                chips.GetComponent<ChipsGo>().AddMore(() => {
                    chipsGo.SetChips(value);
                }, theSeat, player);	
            }	
        }

        private void setReserveCd(int number) {
            var text = string.Format("留座<b>{0}s</b>", number);
            StateLabel.text = text;
        }

        private bool isPersisState() {
            return lastState == ActionState.Allin || lastState == ActionState.Fold;
        }

        private void setPlayerAct(bool active, bool anim = true) {
            if (!active && isPersisState()) {
                return ;
            }

            var cvg = PlayerAct.GetComponent<CanvasGroup>();
            var targetValue = active ? 1 : 0;
            var duration = 0.1f;

            if (anim) {
                cvg.DOFade(targetValue, duration);
            } else {
                cvg.alpha = targetValue;
            }
        }

        private void dealAct(ActionState state) {
            lastState = state;

            setPlayerAct(true);
            PlayerAct.SetAct(state);

            if (state == ActionState.Allin) {
                AllinGo.SetActive(true);
            }
        }

        public void SetFolded() {
		    Avt.GetComponent<CanvasGroup>().alpha = 0.6f;
        }

        static public void SetInParent(Transform target, Transform parent) {
            target.SetParent(parent, true);
			var rect = target.GetComponent<RectTransform>(); 
            rect.anchoredPosition = new Vector2(0, 0);
            rect.localScale = new Vector2(1, 1);
        }
    }


    public interface PlayerDelegate {
        void MoveOut();
        void TurnTo(Dictionary<string, object> data, int left);
        void Fold();
        void ResetTime(int time);
    } 
}