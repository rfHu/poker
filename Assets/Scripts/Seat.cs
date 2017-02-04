using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

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

	public void SetDealer(GameObject dealer) {
		var position = gameObject.GetComponent<RectTransform>().anchoredPosition;
		var newPos = new Vector2(position.x + 90, position.y);
		dealer.GetComponent<RectTransform>().DOAnchorPos(newPos, duration);
	}
}
