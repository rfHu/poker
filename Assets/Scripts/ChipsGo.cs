using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class ChipsGo : MonoBehaviour {
	public Text TextNumber;
	int chips;

	void Awake() {
		RxSubjects.Deal.Subscribe((e) => {
			hideChips();
		}).AddTo(this);
	}

	void setChips(int chips) {
		if (this.chips == chips) {
			return ;
		}
		
		this.chips = chips;
		TextNumber.text = chips.ToString();
		TextNumber.gameObject.SetActive(true);
	}

	public void Create(int value) {
		create(value);
	}

	public void AddMore(int value) {
		create(value, true);
	}

	private void create(int value, bool add = false) {
		GetComponent<RectTransform>()
		.DOAnchorPos(new Vector2(80, 0), 0.4f)
		.OnComplete(() => {
			setChips(value);

			if (add) {
				Destroy(gameObject);	
			}
		});
	}

	void hideChips() {
		var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		transform.SetParent(canvas.transform, true);
		
		var rect = GetComponent<RectTransform>();
		rect.DOAnchorPos(new Vector2(0, 0), 0.4f)
		.OnComplete(() => {
			Destroy(gameObject);
		});
	}
}
