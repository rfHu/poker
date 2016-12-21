using UnityEngine;

public class Menu : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Animation animation = GetComponent<Animation>();	
		animation.wrapMode = WrapMode.Once;

		animation.Play("Dropdown");		
	}
}
