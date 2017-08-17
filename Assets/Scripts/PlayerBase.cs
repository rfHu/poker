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

        public static string CurrentUid; 

        public Avatar Avt;
        public SpkTextGo SpkText;
	    public GameObject Volume;
	    public Text ScoreLabel;
        private GameObject ScoreParent {
			get {
				return ScoreLabel.transform.parent.gameObject;
			}
		}
        public PlayerActGo PlayerAct;
        public Text StateLabel;
	    public GameObject HandGo;
        public Transform Circle;
        [SerializeField] private ParticleSystem winParticle;
        private Transform WinCq; 
        public Text WinNumber;
        public Text RankText;
        [SerializeField] private ParticleSystem chipsParticle;
        [SerializeField] ParticleSystem allinParticle;

        public Player player;
        private ActionState lastState;
	    private int actCardsNumber = 0;

        [SerializeField]private Text hunterAward;

        private Seat theSeat;

        private PlayerDelegate myDelegate;

        public void Init(Player player, Seat theSeat, PlayerDelegate myDelegate) {
            this.player = player;
            this.theSeat = theSeat;
            this.myDelegate = myDelegate;
            SpkText.Uid = player.Uid;
		    setScoreText(player.Bankroll.Value);
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            var avatar = Avt.GetComponent<Avatar>();
		    avatar.Uid = player.Uid;
		    avatar.SetImage(player.Avatar);	
            
            PlayerAct.AnimID = AnimID;

            if (GameData.Shared.InGame && !player.InGame) {
		    	SetFolded();
                myDelegate.SetFolded();
		    }

            addEvents();
        }

        public int AnimID {
            get {
                return gameObject.GetInstanceID(); 
            }
        } 

        void Awake() {
            WinCq = WinNumber.transform.parent;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }

        private void playParticle(ParticleSystem particle) {
            particle.gameObject.SetActive(true);
            particle.Play(true);
        }

        private void stopParticle(ParticleSystem particle) {
            particle.gameObject.SetActive(false);
            particle.Stop(true);
        }

        void Update() {
            if (winParticle.gameObject.activeSelf && winParticle.isStopped) {
                winParticle.Play(true);
            }
        }

        void OnDespawned() {
            this.Dispose();

            stopParticle(chipsParticle);
            stopParticle(allinParticle);
            stopParticle(winParticle);

            // 取消所有动画
            DOTween.Pause(AnimID);

            hunterAward.transform.parent.gameObject.SetActive(false);
            lastState = ActionState.None;
            WinCq.gameObject.SetActive(false);
            ScoreParent.SetActive(true);
			ScoreParent.GetComponent<CanvasGroup>().alpha = 1;
            PlayerAct.SetActive(false, false);
            Avt.GetComponent<CanvasGroup>().alpha = 1;
            Circle.gameObject.SetActive(true);
            RankText.transform.parent.gameObject.SetActive(false);

            var cgo = theSeat.GetComponentInChildren<ChipsGo>();
            if (cgo != null) {
                PoolMan.Despawn(cgo.transform);
            }

            Volume.SetActive(false);
            ScoreLabel.gameObject.SetActive(true);
        } 

        private void setScoreText(int number) {
            if (number == 0 && !player.InGame) {
                ScoreLabel.text = "等待";
            } else {
                ScoreLabel.text = _.Num2CnDigit(number);
            }
        }

        private void addEvents() {
            player.HeadValue.Subscribe((value) => {
                if (value > 0) {
                    hunterAward.transform.parent.gameObject.SetActive(true);
                    hunterAward.text = _.Num2CnDigit(value);
                }
            }).AddTo(this);

            RxSubjects.ShowCard.Subscribe((e) => {
                var uid = e.Data.String("uid");
                if (uid != player.Uid) {
                    return ;
                }

                 var cards = e.Data.IL("cards");
                 myDelegate.ShowCard(cards);
            }).AddTo(this);

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

            player.Destroyed.AsObservable().Where((v) => v).Subscribe((_) => {
                myDelegate.Despawn();
            }).AddTo(this);

            player.PlayerStat.Subscribe((state) => {
                HandGo.SetActive(false);

                var stateGo = StateLabel.transform.parent.gameObject;
                stateGo.SetActive(false);

                switch(state) {
                    case PlayerState.Hanging:
                        HandGo.SetActive(true);
                        break;
                    case PlayerState.Reserve:
                        stateGo.SetActive(true);	
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
                    SetFolded();
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

            player.LastAct.Subscribe((act) => {
                var actState = act.ToActionEnum();

                if (actState == ActionState.None) {
                    lastState = actState;    
                    PlayerAct.SetActive(false, false);
                } else {
                    dealAct(actState);	
                }
            }).AddTo(this);

            player.Allin.Subscribe((allin) => {
                if (allin) {
                    player.ActStateTrigger = false;
                    player.ActState.OnNext(ActionState.Allin);
                }
            }).AddTo(this);

            RxSubjects.GainChip.Where((gainChip) => gainChip.Uid == Uid).Subscribe((gainChip) => {
                gainChip.Grp.ToParent(transform, () => {
                    if (!gameObject.activeSelf) {
                        return ;
                    } 

                    if (chipsParticle.isPlaying) {
                        return ;
                    }

                    if (player.OverData.Value.Gain() <= 0) {
                        return ;
                    }

                    playParticle(chipsParticle);
                });
            }).AddTo(this);

            player.Bankroll.Subscribe((value) => {
                setScoreText(value);
            }).AddTo(this);

            RxSubjects.MoveTurn.Subscribe((e) => {
            	var uid = e.Data.String("uid");
            	var dc = e.Data.Int("deal_card");
            
                CurrentUid = uid;

            	if (uid == Uid) {
            		myDelegate.TurnTo(e.Data, GameData.Shared.ThinkTime);
            		setPlayerActive(false, false);
            	} else {
            		myDelegate.MoveOut();

            		// 刚发了牌
            		if (dc == 1 || GameData.Shared.PublicCards.Count != actCardsNumber) {
            			setPlayerActive(false);
            		}
            	}

                if (Uid == GameData.Shared.Uid) {
                    player.SetTrust(e.Data.Dict("trust"));
                }
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
                PlayerAct.SetActive(false);
                stopParticle(allinParticle);
                player.PrChips.Value = 0;
                OP.Despawn();
            }).AddTo(this);

            player.Cards.AsObservable().Subscribe((cards) => {
                if (cards.Count < 2) {
                    return ;
                }
                myDelegate.ShowCard(cards);
            }).AddTo(this);

            IDisposable winEndDisposable = null;
            player.OverData.AsObservable().Where((data) => data != null).Subscribe((data) => {
                var gain = data.Gain();
                if (gain > 0) {
                    playParticle(winParticle);
                }

                // 收回大于0，展示盈亏
                if (data.prize > 0) {
                    var cvg = WinCq.GetComponent<CanvasGroup>();
                    cvg.alpha = 0;
                    cvg.DOFade(1, 0.3f).SetId(AnimID);

                    var rect = WinCq.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(0, -300);
                    rect.DOAnchorPos(new Vector2(0, -224), 0.3f).SetId(AnimID);

                    WinCq.gameObject.SetActive(true); 
                    WinNumber.text = _.Number2Text(gain);
                    ScoreParent.SetActive(false);
                }

                // 重新计算用户的bankroll                
                player.Bankroll.Value = player.Bankroll.Value + data.prize;

                if (player.Uid == GameData.Shared.Uid) {
                    GameData.Shared.Bankroll.Value = player.Bankroll.Value;
                }
                
                myDelegate.HandOver(data);

                // 4s后隐藏动画
                winEndDisposable = Observable.Timer(TimeSpan.FromSeconds(4)).Subscribe((_) => {
                    hideWinAnim();
                }).AddTo(this);
            }).AddTo(this);

            RxSubjects.MatchRank.Subscribe((_) => {
                if (winEndDisposable != null) {
                    winEndDisposable.Dispose();
                }

                hideWinAnim();
            }).AddTo(this);

            player.Rank.Where((rank) => rank > 0 && player.readyState == 0).Subscribe((rank) => {
                RankText.transform.parent.gameObject.SetActive(true);
                RankText.text  = string.Format("第<size=42>{0}</size>名", rank);
            }).AddTo(this);
        }

        private void hideWinAnim() {
            stopParticle(winParticle);
            DoFade(WinCq.gameObject, () => {
                ScoreParent.SetActive(true);
            });
            myDelegate.WinEnd();
        }

        public void DoFade(GameObject go, Action callback = null) {
            if (!go.activeSelf) {
                return ;
            }

            var cvg = go.GetComponent<CanvasGroup>();
            if (cvg == null) {
                return ;
            }
            
            cvg.DOFade(0, 0.3f).SetId(AnimID).OnComplete(() => {
                go.SetActive(false);
                cvg.alpha = 1;

                if (callback != null) {
                    callback();
                }
            });
        }

        private bool isSelfJson(string jsonStr) {
            var N = JSON.Parse(jsonStr);
            var uid = N["uid"].Value;
            return uid == Uid;
	    }

        private void setPrChips(int value) {
            var existChips = theSeat.GetComponentInChildren<ChipsGo>();

            var newChips = PoolMan.Spawn("UpChip");
            newChips.SetParent(theSeat.transform, false);
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

        private void setPlayerActive(bool active, bool anim = true) {
            if (!active && isPersisState()) {
                return ;
            }

            PlayerAct.SetActive(active, anim); 
        }

        private void dealAct(ActionState state) {
            lastState = state;

            setPlayerActive(true);
            PlayerAct.SetAct(state);

            if (state == ActionState.Allin) {
                playParticle(allinParticle);
            }
        }

        public void SetFolded() {
		    Avt.GetComponent<CanvasGroup>().alpha = 0.4f;
			ScoreParent.GetComponent<CanvasGroup>().alpha = 0.4f;
        }

        static public void SetInParent(Transform target, Transform parent) {
            target.SetParent(parent, true);
			var rect = target.GetComponent<RectTransform>(); 
            rect.anchoredPosition = new Vector2(0, 0);
            rect.localScale = new Vector2(1, 1);
        }

        static private GameObject playerBase;

        static public PlayerBase LoadPrefab(Transform parent) {
            if (playerBase == null) {
                playerBase = Resources.Load<GameObject>("Prefab/PlayerBase");
            }

            var go = Instantiate(playerBase);
            var transform = go.GetComponent<Transform>();
           	transform.SetParent(parent);			
			return transform.GetComponent<PlayerBase>();
        }
    }


    public interface PlayerDelegate {
        void MoveOut();
        void TurnTo(Dictionary<string, object> data, int left);
        void Fold();
        void SetFolded();
        void ResetTime(int time);
        void Despawn();
        void ShowCard(List<int> cards);
        void HandOver(GameOverJson data);
        void WinEnd();
    } 
}