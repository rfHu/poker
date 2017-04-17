using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AVB2F2o5VxVOhGrxGkB650otjcgp12tvo.3dAgX20ZhXojqNwHBPN8V1VVGQJ%2F0yj2jCKghKS5cyM");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58f469024164574711d45e7a");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
