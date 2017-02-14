using UnityEngine;

public class G {
	public static Canvas Cvs {
		get {
			if (G.canvas == null) {
				G.canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
			}

			return G.canvas;
		}
	} 

	private static Canvas canvas;
}