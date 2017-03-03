using UnityEngine;

public class GameLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AxiNWcBO9CILBbi2VOGhLQtXyVVO56nvS.8iZA5Fz7LeAZ%2B4QS3Lrj6EyOQlR96OREVJ0PqtEGe%2BQa");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58b96a899d20aa01613b1596"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
