using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AgbIInquahVvvOlo0DGsOwXST-0b2TJG_.2YFkj3UFqCkAwO7OPd7OZEXmzJRuYLQeDvxwL0Y7o5A");
        
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("592e2476bc3f41317a33b540");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
