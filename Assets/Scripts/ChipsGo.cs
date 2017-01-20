using UnityEngine;
using UnityEngine.UI;

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

	public bool Same(int chips) {
		return this.chips == chips;
	}
}
