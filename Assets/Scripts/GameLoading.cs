using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3Ajx5Nlt5_0arXHMvLzJp7MoyoK26zVyoe.2J5B2yabA7I99eVolsgFtU9mgTpP2WXP%2FyQJPcdOLro");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58eefe7e21202036fccf05b2");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
