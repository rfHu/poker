using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3A94E2jw7W86kyd8V1THXp4u5eQHqbI8o0.CqFe9R17ZrnrLzUS9N6ejFkPV7vpJDKaggQ%2BNJnYXYw");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("5907f0a318b8563ecc8f4397");

		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
