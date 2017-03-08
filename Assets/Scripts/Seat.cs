using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using Extensions;

public enum SeatPosition {
	Top,
	Left,
	Right
}

public class Seat : MonoBehaviour {
	public int Index;
	public ReactiveProperty<SeatPosition> SeatPos  = new ReactiveProperty<SeatPosition>();

	private Vector2 realVector;

	public void OnClick() {
		if (GameData.MyCmd.Unseat) {
			return ;
		}

		if (GameData.Shared.GPSLimit) {
			StartCoroutine(Commander.Shared.Location(takeSeat, () => {
				PokerUI.Alert("为保证公平竞技，请在设置—隐私—定位服务中开启位置权限");	
			}));	 
		}

		takeSeat(new float[]{0, 0});
	}

	public SeatPosition GetPos() {
		var h = G.UICvs.GetComponent<RectTransform>().rect.height;
		var x = realVector.x;
		var y = realVector.y;

		// y轴距顶部不超过210，则认为是顶部
		if (h - y < 210) {
			return SeatPosition.Top;
		}
		
		if (x <= 0) {
			return SeatPosition.Left;
		}	

		return SeatPosition.Right;
	}

	public void ChgPos(Vector2 vector) {
		realVector = vector;
		GetComponent<RectTransform>().DOAnchorPos(vector, 0.15f).OnComplete(() => {
			SeatPos.Value = GetPos();
		});
	}

	public void Init(int index, Vector2 vector) {
		realVector = vector;
		Index = index;
		transform.SetParent (G.UICvs.transform, false);
		GetComponent<RectTransform>().anchoredPosition = vector;

		SeatPos.Value = GetPos();
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
			}, (data) => {
				var err = data.Dict("ret").Int("err");
				
				if (err == 0) {
					return ;
				}

				string text = "";

				if (err == 1103) {
					text = "您与某玩家IP地址相同，不能参与本牌局";
				} else if(err == 1104) {
					text = "您与某玩家距离过近，不能参与本牌局";
				} else {
					text = data.Dict("ret").String("msg");
				}

				PokerUI.Alert(text);
			}
		);		
	}
}
