using UnityEngine;

public class Owner : MonoBehaviour {
	public void OnClick() {
        var go = (GameObject)Instantiate(Resources.Load("Prefab/OwnerPanel"));
        go.transform.position = Vector3.zero;
		go.GetComponent<DOPopup>().Show();
	}
}
