using UnityEngine;

public class AuditMsg : MonoBehaviour {
	public void OnClick() {
		Commander.Shared.Audit();
	}
}
