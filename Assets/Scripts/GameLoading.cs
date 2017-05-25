using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3ACVppjc6uYwD9ZeA7iZ5TuI56mKSpdorg.WZS%2FvO0tWxPOJWS9YwQYiF4uFZKH6o%2FTgIs5IPwbSi4");
        
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5924e24b6836de22840a8111");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
