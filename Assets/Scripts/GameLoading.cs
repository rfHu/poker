using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  

        external.SetSid("s%3ASAbXEJECbr6EIhP-VFSnAKfbgpHkWM-4.7ZJkZ23kXVZA1F7s%2FirnHteuWcBTma%2BIii5OhqmUKyQ");
			external.SetProxy("http://localhost:8888");
            external.SetRoomID("58ecbda28e5fb610794746c2");


		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
