using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AQLPqB5kxP6tTGfkAH1VE32QZiXnQGpoz.P9hI7kO6rcg6Iz6jPRs0%2B5JhUJGUdJyJyCKtRfQcQto");
        
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5937587a32870a6523cbf8ee");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
