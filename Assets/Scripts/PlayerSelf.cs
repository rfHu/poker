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
        
        private Transform OPTransform;
        private Player player; 
        private bool gameover = false;

        public void Init(Player player, Transform parent) {
            this.player = player;
            Base.Init(player, parent.GetComponent<Seat>(), this);
            transform.SetParent(parent, true);
            addEvents();

            RxSubjects.ChangeVectorsByIndex.OnNext(GameData.Shared.MySeat);
            RxSubjects.Seating.OnNext(true);
        }

        private void addEvents() {
            GameData.Shared.MaxFiveRank.Subscribe((value) => {
                // var parent = CardDesc.transform.parent.gameObject;

                // if (value == 0)
                // {
                //     parent.SetActive(false);
                //     return;
                // }

                // parent.SetActive(true);
                // CardDesc.text = Card.GetCardDesc(value);
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
		}).AddTo(this);

		RxSubjects.Deal.Subscribe((_) => {
			player.Trust.Hide();
		}).AddTo(this);

        RxSubjects.GameOver.Subscribe((_) => {
            AutoArea.SetActive(false);
            gameover = true;
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



        // ===== Delegate =====

        public void Fold() {
            MoveOut();
            darkenCards();
        }

        public void MoveOut() {
            if (OPTransform != null) {
                PoolMan.Despawn(OPTransform);
            }

            // Base.Circle.SetActive(true); // 显示头像
        }

        public void TurnTo(Dictionary<string, object> data, int left) {
        
        }

        public void ResetTime(int total) {
            OPTransform.GetComponent<OP>().Reset(total);
        }
    }
}