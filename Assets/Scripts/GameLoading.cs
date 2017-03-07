using UnityEngine;

public class GameLoading : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Time.timeScale = 0;

		var external = External.Instance;

		#if UNITY_EDITOR  
			external.SetSid("s%3AOccmZ2Z97CAzQXHWHALoOST5PL-LYnDJ.qDKgPBNiAKOZXSIIcT8Qq3OhAHsgo5eKwuz4x8a%2Bk4o");
			external.SetProxy("http://localhost:8888");
			external.SetRoomID("58be36904181879384ecc635"); // 无需审核
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitGame() {
		External.Instance.Exit();
	}
}
