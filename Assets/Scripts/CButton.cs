using MaterialUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

[RequireComponent(typeof(Button)), RequireComponent(typeof(ProceduralImage))]
public class CButton : MonoBehaviour {
	private Button button;
	private ProceduralImage image;
    public List<Text> Texts;

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
                foreach (var text in Texts)
                {
                    text.color = NormalColor;
                }
				image.color = NormalColor;
            }
            else
            {
                foreach (var text in Texts)
                {
                    text.color = DisableColor;
                }
				image.color = DisableColor;
			}
		}
	}
}
