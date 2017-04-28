using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3Ac4nO8vQYtgQlaxH0pYTLROureNNb8SUd.NQinWXHREKnLVXKYPrY%2FFRgygXZqRM1uDLrTYIfHPQo");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("59016dcffc558f70a10dd82b");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
