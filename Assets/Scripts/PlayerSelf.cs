using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UniRx;
using UnityEngine.UI.ProceduralImage;

namespace PokerPlayer {
    public class PlayerSelf: MonoBehaviour, PlayerDelegate {
        public PlayerBase Base;
        public GameObject[] Eyes; 
        public GameObject AutoArea;
        public GameObject[] AutoOperas; 
        public List<Transform> MyCards;
		public Text CardDesc;
		public GameObject YouWin;
		public GameObject BackGameBtn;
        
        private Transform OPTransform;
		private CompositeDisposable disposables = new CompositeDisposable();

        private Player player {
			get {
				return Base.player;
			}
		} 
        private bool gameover = false;

        public void Init(Player player, Transform parent) {
            Base.Init(player, parent.GetComponent<Seat>(), this);
			PlayerBase.SetInParent(transform, parent);

			gameover = false;
            addEvents();

            RxSubjects.ChangeVectorsByIndex.OnNext(GameData.Shared.MySeat);
            RxSubjects.Seating.OnNext(true);
        }

		void OnDespawned() {
			disposables.Clear();	

			YouWin.SetActive(false);
			CardDesc.transform.parent.gameObject.SetActive(false);
			MyCards[0].parent.gameObject.SetActive(false);
			MyCards[0].GetComponent<Card>().Turnback();
			MyCards[1].GetComponent<Card>().Turnback();
			OP.Despawn();
		}

        private void addEvents() {
            GameData.Shared.MaxFiveRank.Subscribe((value) => {
                var parent = CardDesc.transform.parent.gameObject;

                if (value == 0)
                {
                    parent.SetActive(false);
                    return;
                }

                parent.SetActive(true);
                CardDesc.text = Card.GetCardDesc(value);
            }).AddTo(disposables);

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
		}).AddTo(disposables);

		player.Trust.ShouldShow.Subscribe((show) => {
			AutoArea.SetActive(show);
		}).AddTo(disposables);

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
		}).AddTo(disposables);

		player.Trust.SelectedFlag.Where((flags) => { return flags != null; }).Subscribe((flags) => {
			var ncolor = _.HexColor("#2196F300");
			var scolor = _.HexColor("#2196F3");
			
			var img0 = AutoOperas[0].GetComponent<ProceduralImage>();
			var img1 = AutoOperas[1].GetComponent<ProceduralImage>();

			if (flags[0] == '0') {
				img0.color = ncolor;
			} else {
				img0.color = scolor;
			}

			if (flags[1] == '0') {
				img1.color = ncolor;
			} else {
				img1.color = scolor;
			}
		}).AddTo(disposables);

		RxSubjects.Deal.Subscribe((_) => {
			player.Trust.Hide();
		}).AddTo(disposables);

        RxSubjects.GameOver.Subscribe((_) => {
            AutoArea.SetActive(false);
            gameover = true;
        }).AddTo(disposables);

		  // 中途复原行动
            player.Countdown.AsObservable().Subscribe((obj) => {
				if (obj.seconds > 0) {
                	turnTo(obj.data, obj.seconds, true, obj.BuyTimeCost);
				} else {
					OP.Despawn();
				}
            }).AddTo(disposables);

			 player.PlayerStat.Subscribe((state) => {
				switch(state) {
                    case PlayerState.Hanging:
						BackGameBtn.SetActive(true);
						if (OPTransform != null) {
							PoolMan.Despawn(OPTransform);
						}
                        break;
                    case PlayerState.Reserve:
                        BackGameBtn.SetActive(true);
                        break;
                    default:
						BackGameBtn.SetActive(false);
                        break;
                }
			 }).AddTo(disposables);
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

    public void AutoCheckOrFold() {
		toggleAutoBtns(0);	
	}

	public void AutoCall() {
		toggleAutoBtns(1);	
	}

    private void darkenCards() {
		MyCards[0].GetComponent<Card>().Darken();
		MyCards[1].GetComponent<Card>().Darken();
	}

	private OP showOP(Dictionary<string, object> data, int left, int buyTimeCost = 10) {
		if (this == null) {
			return null;
		}

		OPTransform = OP.Spawn();
		var op = OPTransform.GetComponent<OP>();
		op.StartWithCmds(data, left, buyTimeCost);
		Base.Circle.gameObject.SetActive(false);

		return op;
	}

	private void turnTo(Dictionary<string, object> dict, int left, bool restore = false,int buyTimeCost = 10) {
			showOP(dict, left, buyTimeCost);

			var flag = player.Trust.FlagString();
			var callNum = player.Trust.CallNumber.Value;

			if (flag == "10") { // 选中左边
				PoolMan.Despawn(OPTransform);	
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
					PoolMan.Despawn(OPTransform);

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



        // ===== Delegate =====

        public void Fold() {
            MoveOut();
            darkenCards();
        }

        public void MoveOut() {
            if (OPTransform != null) {
                PoolMan.Despawn(OPTransform);
            }

            Base.Circle.gameObject.SetActive(true); // 显示头像
        }

        public void TurnTo(Dictionary<string, object> data, int left) {
			turnTo(data, left);
        }

        public void ResetTime(int total) {
            OPTransform.GetComponent<OP>().Reset(total);
        }

		public void Despawn() {
			PoolMan.Despawn(transform);
        }

		public void SeeCard(List<int> cards) {
			MyCards[0].parent.gameObject.SetActive(true);

			var state = player.SeeCardAnim;

			if (state) {
				MyCards[0].GetComponent<Card>().ShowWithSound(cards[0], state);

				Observable.Timer(TimeSpan.FromSeconds(0.3)).Subscribe((_) => {
					MyCards[1].GetComponent<Card>().ShowWithSound(cards[1], state);
				}).AddTo(disposables);
			} else {
				MyCards[0].GetComponent<Card>().Show(cards[0], state);
				MyCards[1].GetComponent<Card>().Show(cards[1], state);
			}
		}	

		public void HandOver(GameOverJson data) {
			if (data.Gain() > 0) {
				YouWin.SetActive(true);
			}
		}
    }
}