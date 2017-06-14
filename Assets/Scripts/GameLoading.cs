using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        	external.SetSid("s%3Ahau4OVJbvBll9ajABQ8OAQlxQM5V8VEe.w%2FpG24Xb8JtG8i7sHGV6W7BvBuTZwF%2B3Z8AD%2BIhJsMc");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("59409f80758b865c4bb37d95");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
