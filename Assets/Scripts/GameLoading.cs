using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
		external.SetSid("s%3AaNtwI1hJ4M1fdiJ9pfT5ObwEQqIt90HE.WAT%2FdyrzJY0RqNcEG%2FVd6gZp7Fru55vh5MMH443dDi0");
        // external.SetSid("s%3AW49LZlOGmF4qE2NVn0wrAJRHs8G_tZwF.Dlv36sqF4Ua%2FRPW00FpXtY0vUJE2y%2FwSVVL6%2BDB%2FIdI");
        
			external.SetProxy("http://localhost:8888");
            // external.SetRoomID("592e332a99b6f4322c891a4e");
			external.SetRoomID("592e7d55f664e34171ecbb4c");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
