using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3ArMFysYpiTazn-D4lr5rIUbYTwWHkTit0.tOW5pJjHiRHB7mUWglptkUvX98cVD%2B52B6IGQ1cgtnI");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5941600554119b0f2bf1870f");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
