using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
	public int numberOfPlayers = 9;
	public Button seat;
	public Canvas canvas;

	void Start () {
		for (int i = 0; i < numberOfPlayers; i++) {
			Button copySeat = Instantiate (seat);
			copySeat.transform.SetParent (canvas.transform, false);
			copySeat.transform.position = new Vector2 (0, 0);
		}
	}
}
