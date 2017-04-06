using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3AqeiL5PS7YnUYF9aHy0pUv7yY4HS8L6Q_.WpD7%2B2hjFu2%2Fvb4EXIu2uQjS1gB4D79OnZHqGpXlpx8");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58e6289746b17a3b315425d5");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
