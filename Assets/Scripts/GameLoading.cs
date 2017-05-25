using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
		        external.SetSid("s%3AZoVzZ1bXPmVWSiW3Fwb5cGBH3TkMrcWY.a3lMQS%2FzDAdSu9MTnJBhq%2B6olQgS1DOEo85uDfeB5vc");
        
			external.SetProxy("http://localhost:8888");
                        external.SetRoomID("5926ac1d6e777918f4bfc69e");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
