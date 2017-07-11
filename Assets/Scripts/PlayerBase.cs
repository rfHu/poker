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
	    public GameObject HandGo;
        public GameObject AllinGo;
        public Transform Circle;
        public GameObject WinStars;
        public Text WinNumber;
        public Text RankText;

        public Player player;
        private ActionState lastState;
	    private int actCardsNumber = 0;

        private Seat theSeat;
        CompositeDisposable disposables = new CompositeDisposable();

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
                myDelegate.SetFolded();
		    }

            addEvents();
        }

        void OnDespawned() {
            disposables.Clear();

            WinStars.SetActive(false);
            WinNumber.transform.parent.gameObject.SetActive(false);
            ScoreLabel.transform.parent.gameObject.SetActive(true);
            setPlayerAct(false, false);
            AllinGo.SetActive(false);
            Avt.GetComponent<CanvasGroup>().alpha = 1;
            Circle.gameObject.SetActive(true);
            RankText.transform.parent.gameObject.SetActive(false);

            var go = GetComponentInChildren<ChipsGo>();
            if (go != null) {
                go.Hide();
            }
        } 

        private void addEvents() {
            RxSubjects.ShowAudio.Where(isSelfJson).Subscribe((jsonStr) => {
                Volume.SetActive(true);
                ScoreLabel.gameObject.SetActive(false);
            }).AddTo(disposables);

            RxSubjects.HideAudio.Where(isSelfJson).Subscribe((_) => {
                Volume.SetActive(false);
                ScoreLabel.gameObject.SetActive(true);
            }).AddTo(disposables);

            RxSubjects.SendChat.Where(isSelfJson).Subscribe((jsonStr) => {
                var N = JSON.Parse(jsonStr);
                var text = N["text"].Value;
                SpkText.ShowMessage(text);
            }).AddTo(disposables);

            theSeat.SeatPos.Subscribe((pos) => {
                SpkText.ChangePos(pos);
                PlayerAct.ChangePos(pos);
            }).AddTo(disposables);

            player.PrChips.AsObservable().Subscribe((value) => {
                if (value == 0) {
                    return ;
                }

                setPrChips(value);
            }).AddTo(disposables);

            player.Destroyed.AsObservable().Where((v) => v).Subscribe((_) => {
                myDelegate.Despawn();
            }).AddTo(disposables);

            player.PlayerStat.Subscribe((state) => {
                HandGo.SetActive(false);

                var stateGo = StateLabel.transform.parent.gameObject;
                stateGo.SetActive(false);

                switch(state) {
                    case PlayerState.Waiting: case PlayerState.Auditing:
                        ScoreLabel.text = "<size=28>等待</size>";
                        break;
                    case PlayerState.Hanging:
                        HandGo.SetActive(true);
                        break;
                    case PlayerState.Reserve:
                        stateGo.SetActive(true);	
                        break;
                    default: 
                        break;
                }
            }).AddTo(disposables);

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
                    SetFolded();
                    myDelegate.Fold();
                } else {
                    myDelegate.MoveOut();
                }

                dealAct(e);
                actCardsNumber = GameData.Shared.PublicCards.Count;	
            }).AddTo(disposables);

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
                    }).AddTo(disposables);
                }
            }).AddTo(disposables);

            player.LastAct.Where((act) => !String.IsNullOrEmpty(act)).Subscribe((act) => {
                dealAct(act.ToActionEnum());	
            }).AddTo(disposables);

            player.Allin.Subscribe((allin) => {
                if (allin) {
                    player.ActStateTrigger = false;
                    player.ActState.OnNext(ActionState.Allin);
                }
            }).AddTo(disposables);

            RxSubjects.GainChip.Where((gainChip) => gainChip.Uid == Uid).Subscribe((gainChip) => {
                gainChip.Grp.ToParent(transform);
            }).AddTo(disposables);

            player.Bankroll.Subscribe((value) => {
                ScoreLabel.text = _.Num2CnDigit(value);
            }).AddTo(disposables);

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

                if (Uid == GameData.Shared.Uid) {
                    player.SetTrust(e.Data.Dict("trust"));
                }
            }).AddTo(disposables);

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
            }).AddTo(disposables);

            RxSubjects.GameOver.Subscribe((e) => {
                myDelegate.MoveOut();
                setPlayerActForce(false);
                AllinGo.SetActive(false);
                player.PrChips.Value = 0;
            }).AddTo(disposables);

            player.Cards.AsObservable().Where((cards) => {
                if (cards != null && cards.Count == 2) {
                    return cards[0] > 0 && cards[1] > 0;
                }

                return false;
            }).Subscribe((cards) => {
                myDelegate.SeeCard(cards);
            }).AddTo(disposables);

            player.OverData.AsObservable().Where((data) => data != null).Subscribe((data) => {
                var gain = data.Gain();
                if (gain > 0) {
                    WinStars.SetActive(true);
                }

                // 收回大于0，展示盈亏
                if (data.prize > 0) {
                    WinNumber.transform.parent.gameObject.SetActive(true); 
                    WinNumber.text = _.Number2Text(gain);
                    ScoreLabel.transform.parent.gameObject.SetActive(false);
                }

                myDelegate.HandOver(data);
            }).AddTo(disposables);

            player.Rank.Where((rank) => rank > 0 && player.readyState == 0).Subscribe((rank) => {
                RankText.transform.parent.gameObject.SetActive(true);
                RankText.text  = string.Format("第<size=42>{0}</size>名", rank);
            }).AddTo(this);
        }

        private bool isSelfJson(string jsonStr) {
            var N = JSON.Parse(jsonStr);
            var uid = N["uid"].Value;
            return uid == Uid;
	    }

        private void setPrChips(int value) {
            var existChips = GetComponentInChildren<ChipsGo>();

            var newChips = PoolMan.Spawn("UpChip");
            newChips.SetParent(transform, false);
            newChips.SetAsLastSibling();

            if (existChips == null) {
                newChips.GetComponent<ChipsGo>().Create(value, theSeat, player);
            } else {
                newChips.GetComponent<ChipsGo>().AddMore(() => {
                    existChips.SetChips(value);
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

            PlayerAct.SetActive(active, anim); 
        }

        private void setPlayerActForce(bool active) {
            PlayerAct.SetActive(active, true);
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
        void SetFolded();
        void ResetTime(int time);
        void Despawn();
        void SeeCard(List<int> cards);
        void HandOver(GameOverJson data);
    } 
}