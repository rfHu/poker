using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using UniRx;

public class ButtonColor : MonoBehaviour {
	[SerializeField]private ProceduralImage border;
	private ProceduralImage bgImage;

	void Start () {
		bgImage = GetComponent<ProceduralImage>();

		GameSetting.TableSprite.Subscribe((sprite) => {
			if (sprite == 0) {
				bgImage.color = _.HexColor("#1495f7");
				border.color = _.HexColor("#78c4ff");
			} else if (sprite == 1) {
				bgImage.color = _.HexColor("#3fa360");
				border.color = _.HexColor("#5fce83");
			}
		}).AddTo(this);
	}
}
