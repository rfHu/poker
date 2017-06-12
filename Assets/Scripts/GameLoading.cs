using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AVXcKNwnZceS1R3B8EBT2C0o7ED9CXxkS.hgJa5byClQ7Em%2B07%2Bj5I9ov0w%2BPxpuXQMJX7ugcxzUI");
        
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("593e9b4a9bb13c2d82fd2ad1");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
