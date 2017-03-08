using UnityEngine;

public class G {
	public static Canvas UICvs {
		get {
			if (uiCanvas == null) {
				uiCanvas = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<Canvas>();
			}

			return uiCanvas;
		}
	} 

	public static Canvas DialogCvs {
		get {
			if (dialogCanvas == null) {
				dialogCanvas = GameObject.FindGameObjectWithTag("DialogCanvas").GetComponent<Canvas>();
			}

			return dialogCanvas;
		}
	} 

	private static Canvas uiCanvas;
	private static Canvas dialogCanvas;
}