using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;

public enum SeatPosition {
	Top,
	Left,
	Right
}

public class Seat : MonoBehaviour {
	public int Index;
	public ReactiveProperty<SeatPosition> SeatPos  = new ReactiveProperty<SeatPosition>();

	public void OnClick() {
		if (GameData.MyCmd.Unseat) {
			return ;
		}

		// 坐下
		Connect.Shared.Emit(
			new Dictionary<string, object>(){
				{"f", "takeseat"},
				{"args", Index}
			}
		);		
	}

	public SeatPosition GetPos() {
		var h = G.Cvs.GetComponent<RectTransform>().rect.height;
		var trans = transform.GetComponent<RectTransform>();
		var x = trans.anchoredPosition.x;
		var y = trans.anchoredPosition.y;

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
		GetComponent<RectTransform>().DOAnchorPos(vector, 0.15f).OnComplete(() => {
			SeatPos.Value = GetPos();
		});
	}

	public void Init(int index, Vector2 vector) {
		Index = index;
		transform.SetParent (G.Cvs.transform, false);
		GetComponent<RectTransform>().anchoredPosition = vector;

		SeatPos.Value = GetPos();
	}
}
