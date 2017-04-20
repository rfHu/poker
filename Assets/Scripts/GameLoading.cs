using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AaKO446984Zhoh8sbLrUzLTbTQ6Lp2r9p.yAdua3Ygx8BGuL%2FWmOcAqiM15JizO9Fb%2F%2FmKstZCG2A");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58f88f055630a67fe7d26546");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
