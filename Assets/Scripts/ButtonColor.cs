using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using UniRx;

public class ButtonColor : MonoBehaviour {
	[SerializeField]private ProceduralImage border;
	private ProceduralImage bgImage;

	static public Color BgColor {
		get {
			var sprite = GameSetting.TableSprite.Value;
				
			if (sprite == 1) {
				return _.HexColor("#3fa360");
			}

			return _.HexColor("#2196F3");
		}
	}

	static public Color BgColorA0 {
		get {
			var color = BgColor;
			color.a = 0;
			return color;
		}
	}

	static public Color BorderColor {
		get {
			var sprite = GameSetting.TableSprite.Value;

			if (sprite == 1) {
				return _.HexColor("#5fce83");	
			}

			return _.HexColor("#4FC3F7");	
		}
	}

	void Start () {
		bgImage = GetComponent<ProceduralImage>();

		GameSetting.TableSprite.Subscribe((sprite) => {
			bgImage.color = BgColor;
			border.color = BorderColor;	
		}).AddTo(this);
	}
}
