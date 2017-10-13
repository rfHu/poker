using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UniRx;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;
using System.Linq;

namespace PokerPlayer {
    public class PlayerSelf: MonoBehaviour, PlayerDelegate {
		public GameObject BaseObject;
        public PlayerBase Base;
        public GameObject[] Eyes; 
        public GameObject AutoArea;
        public GameObject[] AutoOperas; 

		private Card  card1 {
			get {
				return cardContainers[0].CardInstance;
			}
		}

		private Card card2 {
			get {
				return cardContainers[1].CardInstance;
			}
		}

		private GameObject cardParent {
			get {
				return cardContainers[0].transform.parent.gameObject;
			}
		}

		[SerializeField]private Transform ccParent;

		[SerializeField] private List<CardContainer> cardContainers; 

		public Text CardDesc;
		public GameObject YouWin;
		public ParticleSystem WinParticle;
		public GameObject BackGameBtn;
        
        private OP OPMono;

		private bool hasShowCard = false;

        private Player player {
			get {
				return Base.player;
			}
		} 
        private bool gameover = false;


		static public void Init(Player player, Seat seat) {
			var transform = PoolMan.Spawn("PlayerSelf", seat.transform);
			transform.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
			transform.GetComponent<PlayerSelf>().init(player, seat);
		}

        private void init(Player player, Seat seat) {
            Base.Init(player, seat, this);

			gameover = false;
            addEvents();

            RxSubjects.ChangeVectorsByIndex.OnNext(GameData.Shared.MySeat);
            RxSubjects.Seating.OnNext(true);
        }

		void Awake()
		{
			Base = PlayerBase.Load(BaseObject, transform);	
		}

		void OnDespawned() {
			this.Dispose();	
			RxSubjects.Seating.OnNext(false);

			hasShowCard = false;
			YouWin.SetActive(false);
			YouWin.GetComponent<CanvasGroup>().alpha = 1;
			WinParticle.Stop(true);
			CardDesc.gameObject.SetActive(false);

			cardParent.SetActive(false);
			resetCards();
			card1.Turnback();
			card2.Turnback();

			OP.Despawn();
		}

        private void addEvents() {
            GameData.Shared.MaxFiveRank.Subscribe((value) => {
                var parent = CardDesc.gameObject;

                if (value == 0)
                {
                    parent.SetActive(false);
                    return;
                }

                parent.SetActive(true);
                CardDesc.text = Card.GetCardDesc(value);
            }).AddTo(this);

		    player.ShowCard.Subscribe((value) => {
			    if (value[0] == '1') {
				    Eyes[0].SetActive(true);
			    } else {
				    Eyes[0].SetActive(false);
			    }

			    if (value[1] == '1') {
				    Eyes[1].SetActive(true);
			    } else {
				    Eyes[1].SetActive(false);
			    }
		    }).AddTo(this);

		    player.Trust.ShouldShow.Subscribe((show) => {
			    AutoArea.SetActive(show);
		    }).AddTo(this);

			GameData.Shared.HighlightIndex.Subscribe((list) => {
				Card.HighlightCards(new List<Card>{card1, card2}, list);
			}).AddTo(this);

		    player.Trust.CallNumber.Subscribe((num) => {
			    var text = AutoOperas[1].transform.Find("Text").GetComponent<Text>();

			    if (num == 0) {
				    text.text = "自动让牌";
			    } else if (num == -1) {
				    text.text = "全下";
			    } else if (num > 0) {
				    text.text = String.Format("跟注\n<size=40>{0}</size>", _.Num2CnDigit<int>(num));
			    }

			    var flag = player.Trust.FlagString();
			    if (flag == "01") {
				    player.Trust.SelectedFlag.Value = "00";	
			    }
		    }).AddTo(this);

		    player.Trust.SelectedFlag.Where((flags) => { return flags != null; }).Subscribe((flags) => {
			    var img0 = AutoOperas[0].GetComponent<ProceduralImage>();
			    var img1 = AutoOperas[1].GetComponent<ProceduralImage>();

			    if (flags[0] == '0') {
				    img0.enabled = false;
			    } else {
				    img0.enabled = true;
			    }

			    if (flags[1] == '0') {
				    img1.enabled = false;
			    } else {
				    img1.enabled = true;
			    }
		    }).AddTo(this);

            RxSubjects.Deal.Subscribe((_) => {
                player.Trust.Hide();
            }).AddTo(this);

            RxSubjects.GameOver.Subscribe((_) => {
                AutoArea.SetActive(false);
                gameover = true;
            }).AddTo(this);

		  // 中途复原行动
            player.Countdown.AsObservable().Subscribe((obj) => {
				if (obj.seconds > 0) {
                	turnTo(obj.data, obj.seconds, true, obj.BuyTimeCost);
				} else {
					OP.Despawn();
				}
            }).AddTo(this);

			 player.PlayerStat.Subscribe((state) => {
				switch(state) {
                    case PlayerState.Hanging:
						BackGameBtn.SetActive(true);
						OP.Despawn();
						Base.Circle.gameObject.SetActive(true);
                        break;
                    case PlayerState.Reserve:
                        BackGameBtn.SetActive(true);
                        break;
                    default:
						BackGameBtn.SetActive(false);
                        break;
                }
			 }).AddTo(this);
        }


        private void toggleAutoBtns(int index) {
		var valStr = player.Trust.SelectedFlag.Value;

		if (String.IsNullOrEmpty(valStr)) {
			valStr = "00";
		}
		
		var value = new System.Text.StringBuilder(valStr);

		value[index] = value[index] == '0' ? '1' : '0';
		value[index ^ 1] = '0';	

		player.Trust.SelectedFlag.Value = value.ToString();	
	}
	
	private void toggleEye(int index) {
		var value = new System.Text.StringBuilder(player.ShowCard.Value);

		// 这一手结束后，只能亮牌，不能关闭亮牌
		if (value[index] == '1' && gameover) {
			return ;
		}

		value[index] =  value[index] == '0' ? '1' : '0';

		player.ShowCard.Value = value.ToString();

		// 转换10进制
		var num = Convert.ToInt16(value.ToString(), 2); 

		// 发送请求
		Connect.Shared.Emit(new Dictionary<string, object> {
			{"f", "showcard"},
			{"args", new Dictionary<string, object> {
				{"showcard", num}
			}}
		});
	}

	public void ShowFirstCard() {
		toggleEye(0);	
	}

	public void ShowSecondCard() {
		toggleEye(1);
	}

	public void BackGame() {
		Connect.Shared.Emit(new Dictionary<string, object>{
			{"f", "ready"}
		}, (_) => {
			var player = GameData.Shared.GetMyPlayer();
			if (player.IsValid()) {
				player.SetState((int)PlayerState.Normal);
			}
		});
	}

    public void AutoCheckOrFold() {
		toggleAutoBtns(0);	
	}

	public void AutoCall() {
		toggleAutoBtns(1);	
	}

    private void darkenCards() {
		card1.Darken();
		card2.Darken();
	}

	private void showOP(Dictionary<string, object> data, int left, int buyTimeCost = 10) {
		if (!PoolMan.IsSpawned(transform)) {
			return ;
		}

		OPMono = OP.Spawn().GetComponent<OP>();
		OPMono.StartWithCmds(data, left, buyTimeCost);
		Base.Circle.gameObject.SetActive(false);
	}

	 void OnDestroy()
	{
	}

	private void turnTo(Dictionary<string, object> dict, int left, bool restore = false,int buyTimeCost = 10) {
		PlayerBase.CurrentUid = player.Uid;

			showOP(dict, left, buyTimeCost);

			var flag = player.Trust.FlagString();
			var callNum = player.Trust.CallNumber.Value;

			if (flag == "10") { // 选中左边
				OP.Despawn();
				var check = dict.Dict("cmds").Bool("check");

				if (check) {
					OP.OPS.Check();
 				} else {
					OP.OPS.Fold();
				 }
			} else if (flag == "01") { // 选中右边
				var data = dict.Dict("cmds");
				var call = data.Int("call");
				var check = data.Bool("check");

				if (callNum == -1 || (callNum == 0 && check) || (callNum == call && call != 0)) {
					OP.Despawn();

					if (callNum == 0) {
						OP.OPS.Check();
					} else if (callNum == -1) {
						OP.OPS.AllIn();
					} else {
						OP.OPS.Call();
					}
				} else {
					if (!restore) {
						G.PlaySound("on_turn");
					}
				}
			} else {
				if (!restore) {
					G.PlaySound("on_turn");
				}
			}
			
			player.Trust.SelectedFlag.Value = "00";
	}

	private void reShow(Card card, int index) {
		if (index <= 0) {
			return ;
		}

		if (!hasShowCard) {
			card.ShowWithSound(index, player.SeeCardAnim);
		} else if (index > 0) {
			card.ReShow();
		}

		if (!player.InGame) {
			card.Darken();
		}
	}

	private void resetCards() {
		var transform = cardParent.GetComponent<RectTransform>();
		
		transform.anchoredPosition3D = new Vector3(0, -124, 0);
		transform.GetComponent<CanvasGroup>().alpha = 1;
		transform.localScale = Vector3.one;
		transform.SetParent(ccParent, false);
	}

        // ===== Delegate =====

        public void Fold() {
            MoveOut();

			// 弃牌动画
			var transform = cardParent.GetComponent<RectTransform>();
			transform.SetParent(G.UICvs.transform, true);

			var duration = 0.5f;
			
			Ease ease = Ease.Flash;
			
			transform.DOScale(new Vector2(0.4f, 0.4f), duration).SetEase(ease).SetId(Base.AnimID);
			transform.GetComponent<CanvasGroup>().DOFade(0.4f, duration).SetEase(ease).SetId(Base.AnimID);
			transform.DOMove(Controller.LogoVector, duration).SetEase(ease).SetId(Base.AnimID).OnComplete(() => {
				resetCards();
            	darkenCards();
			});
        }

		public void SetFolded() {
			darkenCards();
		}

        public void MoveOut() {
			OP.Despawn();
            Base.Circle.gameObject.SetActive(true); // 显示头像
        }

        public void TurnTo(Dictionary<string, object> data, int left) {
			turnTo(data, left);
        }

        public void ResetTime(int total) {
			if (OPMono == null) {
				return ;
			}
            OPMono.Reset(total);
        }

		void OnSpawned() {
		}

		public void Despawn() {
			PoolMan.Despawn(transform);
        }

		public void ShowCard(List<int> cards) {
			cardParent.SetActive(true);

			if (cards[0] == 0) {
				cards[0] = -1;
			}

			if (cards[1] == 0) {
				cards[1] = -1;
			}

			if (player.SeeCardAnim) {
				if (hasShowCard) { // 同时开牌
					reShow(card1, cards[0]);
					reShow(card2, cards[1]);
				} else { // 间隔开牌
					reShow(card1, cards[0]);
					Observable.Timer(TimeSpan.FromSeconds(0.3)).Subscribe((_) => {
						reShow(card2, cards[1]);	
						hasShowCard = true;
					}).AddTo(this);
				}
			} else {
				card1.ShowIfDarken(cards[0], player.InGame);
				card2.ShowIfDarken(cards[1], player.InGame);

				hasShowCard = true;
			}
		}	

		public void HandOver(GameOverJson data) {
			player.SeeCardAnim = true;

			ShowCard(data.cards);

			if (data.Gain() <= 0) {
				return ;
			}

			var rect = YouWin.GetComponent<RectTransform>();
			rect.localScale = Vector3.zero;
			YouWin.SetActive(true);

			var ease = Ease.OutBounce;
			rect.DOScale(Vector3.one, 0.5f).SetEase(ease).SetId(Base.AnimID);

			WinParticle.Play(true);				
		}

		public void WinEnd() {
			Base.DoFade(YouWin);
		}
    }
}