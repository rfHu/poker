using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

[RequireComponent(typeof(Button)), RequireComponent(typeof(ProceduralImage))]
public class CButton : MonoBehaviour {
	private Button button;
	private ProceduralImage image;
	public Text text;

	public Color NormalColor = MaterialUI.MaterialColor.cyanA200;
	public Color DisableColor = MaterialUI.MaterialColor.grey400;

	void Awake() {
		button = GetComponent<Button>();
		image = GetComponent<ProceduralImage>();
	}

	public bool interactable {
		get {
			return button.interactable;
		}

		set {
			button.interactable = value;

			if (value) {
				if (text != null) {
					text.color = NormalColor;
				}
				image.color = NormalColor;
			} else {
				if (text != null) {
					text.color = DisableColor;
				}
				image.color = DisableColor;
			}
		}
	}
}
