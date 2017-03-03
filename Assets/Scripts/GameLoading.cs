using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3Aktug-V34cKn8EejwXNU0zXXZ_HHz7rbP.2aWNQkaGxPAN6FtAJ3BVy4xOm72zANWRT7xG7g%2BpX1U");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58b9574327a365ba0523e565"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
