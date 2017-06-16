using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AVKWQnnTDI8UyG8Ja_ODdjSMQARR9nVB_.AB6RoUeq8UpRUrucKUMa62%2BvPM05puErDWHy6gpbXn4");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5941f2db54119b0f2bf18993");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
