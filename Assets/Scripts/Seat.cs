using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public enum SeatPosition {
	Top,
	Left,
	Right
}

public class Seat : MonoBehaviour {
	public int Index;
	float duration = 0.15f;

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

	public SeatPosition Pos() {
		var h = G.Cvs.GetComponent<RectTransform>().rect.height;
		var trans = transform.GetComponent<RectTransform>();
		var x = trans.anchoredPosition.x;
		var y = trans.anchoredPosition3D.y;

		// y轴距顶部不超过210，则认为是顶部
		if (h - y < 210) {
			return SeatPosition.Top;
		}
		
		if (x <= 0) {
			return SeatPosition.Left;
		}	

		return SeatPosition.Right;
	}

	public void SetDealer(GameObject dealer) {
		var pos = Pos();
		var position = gameObject.GetComponent<RectTransform>().anchoredPosition;
		var y = position.y - 45;
		var x = position.x + 70;

		if (pos == SeatPosition.Right) {
			x =  position.x - 70;
		}

		dealer.GetComponent<RectTransform>().DOAnchorPos(new Vector2(x, y), duration);
	}
}
