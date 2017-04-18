using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AiFqN9BjS4CQSjycVNBiIC7HQL7KseSVl.0AYu12stPj%2FJukALDl7Eaqgoz4HzfBgucf8TY2ED2us");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58f589765630a67fe7d2450d");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
