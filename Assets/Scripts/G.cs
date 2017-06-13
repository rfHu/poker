using UnityEngine;
using System.Linq;
using DarkTonic.MasterAudio;

public class G {
	public static Canvas UICvs {
		get {
			if (uiCanvas == null) {
				var cvs = GameObject.FindGameObjectWithTag("UICanvas");

				if (cvs == null) {
					return null;
				}

				uiCanvas = cvs.GetComponent<Canvas>();
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

	// public static void WaitSound(SoundGroupVariation.SoundFinishedEventHandler  cb) {
	// 	var sounds = MasterAudio.GetAllPlayingVariationsInBus("Wait");

	// 	if (sounds.Count > 0) {
	// 		sounds.Last().SoundFinished += cb;	
	// 	} else {
	// 		cb();
	// 	}		
	// }

	public static void PlaySound(string name) {
		if (GameData.Shared.muted) {
			return ;
		}

		MasterAudio.PlaySound(name);		
	}
}