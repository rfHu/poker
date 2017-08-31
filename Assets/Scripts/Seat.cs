using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine.UI;

public enum SeatPosition {
	Top,
	TopLeft,
	TopRight,
	Left,
	Right,
	Bottom
}

public class Seat : MonoBehaviour {
	public int Index {
		get {
			return _index;
		}
	}

	private int _index = -1;

	public ReactiveProperty<SeatPosition> SeatPos  = new ReactiveProperty<SeatPosition>();

	public Text ButtonText;

	public GameObject Container;

	private Vector2 realVector;

	private Transform chip;

	public Transform Chip {
		get {
			return chip;
		}
	}

	public void AddChip(Transform chip) {
		chip.SetParent(transform, false);
		chip.SetAsLastSibling();
		this.chip = chip;
	}

	public void RemoveChip() {
		if (chip != null) {
			PoolMan.Despawn(chip);
		}

		chip = null;
	}

	public void Hide() {
		Container.SetActive(false);
	}

	public void Show() {
		if (GameData.Shared.Type == GameType.MTT) {
			return ;
		}

		Container.SetActive(true);
	}

	public void OnClick() {
		if (GameData.MyCmd.Unseat) {
			return ;
		}

		if (GameData.Shared.Players.ContainsKey(Index)) {
			return ;
		}

		if (GameData.Shared.MySeat >= 0) {
			return ;
		}

		if (GameData.Shared.GPSLimit.Value) {
			StartCoroutine(Commander.Shared.Location(takeSeat, () => {
				PokerUI.Alert("为保证公平竞技，请在设置—隐私—定位服务中开启位置权限");	
			}));	 
		} else {
			takeSeat(new float[]{0, 0});
		}

		RxSubjects.ChangeVectorsByIndexAnimate.OnNext(Index);
	}

	void OnDespawned() {
		_index = -1;	
	}

	public SeatPosition GetPos() {
		var h = G.UICvs.GetComponent<RectTransform>().rect.height;
		var x = realVector.x;
		var y = realVector.y;

		// y轴距顶部不超过TopMargin，则认为是顶部
		if (h - y <= Controller.TopMargin) {
			if (x < 0) {
				return SeatPosition.TopLeft;
			}

			if (x > 0) {
				return SeatPosition.TopRight;
			}

			return SeatPosition.Top;
		}
		
		if (x < 0) {
			return SeatPosition.Left;
		}

		if (x > 0) {
			return SeatPosition.Right;
		}

		return SeatPosition.Bottom;
	}

	public void ChgPos(Vector2 vector, bool anim) {
		realVector = vector;

		if (anim) {
			GetComponent<RectTransform>().DOAnchorPos(vector, 0.15f).OnComplete(() => {
				SeatPos.Value = GetPos();
			});	
		} else {
			GetComponent<RectTransform>().anchoredPosition = vector;
			SeatPos.Value = GetPos();
		}
	}

	public void Init(int index, Vector2 vector) {
		realVector = vector;
		_index = index;
		transform.SetParent (G.UICvs.transform, false);
		GetComponent<RectTransform>().anchoredPosition = vector;

		SeatPos.Value = GetPos();

		if (GameData.Shared.IsMatch()) {
			ButtonText.text = "参赛";
		} else {
			ButtonText.text = "坐下";
		}

		if (GameData.Shared.Type == GameType.MTT) {
			Hide();
 		} else {
			Show();
		 }
	}

	private void takeSeat(float[] pos) {
		// 坐下
		Connect.Shared.Emit(
			new Dictionary<string, object>(){
				{"f", "takeseat"},
				{"args", new Dictionary<string, object>{
					{"seat", Index}, 
					{"position_x", pos[0]}, 
					{"position_y", pos[1]} 
				}}
			}, (data, e) => {
				if (e != 0) {
					PokerUI.Alert("无法入座");
					return ;
				}

				var err  = data.Int("err");
				if (err == 0) {
					return ;
				}

				string text = "";

				if (err == 1103) {
					text = "您与某玩家IP地址相同，不能参与本牌局";
				} else if(err == 1104) {
					text = "您与某玩家距离过近，不能参与本牌局";
				} else {
					return ;
				}

				PokerUI.Alert(text);
			}
		);		
	}
}
