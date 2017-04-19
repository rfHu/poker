using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3AFln3KAeWCV0Pm8laOOazXkpWo00-XhHy.3abjvvJ1FnQF1kahy%2FAqwqyQxAbjo%2B77YptgGgB0y4c");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58f728ca5630a67fe7d2606a");
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
