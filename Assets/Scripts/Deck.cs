using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {
	// Use this for initialization
	void Start () {
		SpriteRenderer sprite = GetComponent<SpriteRenderer> ();
		float x = sprite.bounds.size.x;
		float y = sprite.bounds.size.y;
		float scale = (y / x) * ((float)Screen.width / (float)Screen.height);

		Vector2 localScale = sprite.transform.localScale;
		localScale.x = scale;
		sprite.transform.localScale = localScale;
	}
}
