using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3A8cJA87Gi5D_coKOFAln6DRfT1qvZ3__A.ZQhz5zMCfIFp4XQOk8o6ZSI1MCfhfmrz4ngqH5LgZaE");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58d8927c82e99eab930a49e5");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
