using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3Atq6ez6nYgOCsUB6MGCKUjzjvDLjFl_Xf.tW4O6HfPxYIWoL0fi9pvrgSlUmMN%2FGInZpitQe2HmwM");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5909473318b8563ecc8f5961");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
