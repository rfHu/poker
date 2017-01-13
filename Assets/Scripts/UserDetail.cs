using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DOPopup))]
public class UserDetail : MonoBehaviour {
	public Image Avatar;
	public Text Name;
	public Text Coins;
	public Text Hands;
	public Text ShowHand;
	public Text Join;
	public Text JoinWin; 

	void ShowById(string id) {
		var d = new Dictionary<string, object>(){
			{"uid", id}
		};

		Connect.shared.Emit(new Dictionary<string, object>() {
			{"f", "gamerdetail"},
			{"args", d}
		}, (json) => {
			
		});
	}	
}
