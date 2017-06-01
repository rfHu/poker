using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DarkTonic.MasterAudio;

public class ChipsGrp : MonoBehaviour {
	public List<GameObject> Chips;

	private PlayerObject player;

	public void ToPlayer(PlayerObject player) {
		this.player = player;

		doAnim(Chips[0], 0);	
		doAnim(Chips[1], 0.05f);	
		doAnim(Chips[2], 0.1f);	
		doAnim(Chips[3], 0.15f);	
		doAnim(Chips[4], 0.2f);	

		G.PlaySound("chipfly");

		Destroy(gameObject);
	} 

	private void doAnim(GameObject go, float delay) {
		go.transform.SetParent(player.transform, true);
		go.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 0), 0.3f).SetDelay(delay).OnComplete(() => {
			Destroy(go);
		});	
	}
}
