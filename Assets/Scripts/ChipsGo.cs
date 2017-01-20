using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;
	int chips;

	public void SetChips(int chips) {
		if (this.chips == chips) {
			return ;
		}
		
		this.chips = chips;
		TextNumber.text = chips.ToString();
		TextNumber.enabled = true;
	}

	public void HideChips() {
		var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		transform.SetParent(canvas.transform, true);
		
		var rect = GetComponent<RectTransform>();
		rect.DOAnchorPos(new Vector2(0, 0), 0.4f)
		.OnComplete(() => {
			Destroy(gameObject);
		});
	}

	public bool Same(int chips) {
		return this.chips == chips;
	}
}
