using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
        external.SetSid("s%3ApwZms6GFOCGJQZAJdJUDNXp8OOXmnEf9.qohLGzd4rWfddhBfB%2FuIAh4WFVZgTjwsAS3bPmQDyJ0");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58ef4e494164574711d432d9");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
