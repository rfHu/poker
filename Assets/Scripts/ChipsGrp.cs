using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChipsGrp : MonoBehaviour {
	public List<GameObject> Chips;

	private PlayerObject player;

	public void ToPlayer(PlayerObject player) {
		this.player = player;

		doAnim(Chips[0], 0);	
		doAnim(Chips[1], 0.05f);	
		doAnim(Chips[2], 0.1f);	

		Destroy(gameObject);
	} 

	private void doAnim(GameObject go, float delay) {
		go.transform.SetParent(player.transform, true);
		go.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 0), 0.3f).SetDelay(delay).OnComplete(() => {
			Destroy(go);
		});	
	}
}
