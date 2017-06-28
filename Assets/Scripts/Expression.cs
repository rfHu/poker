using UnityEngine;

public class Expression: MonoBehaviour {
    public Transform Face;

    public void SetTrigger(string name) {
        foreach(Transform child in Face) {
            child.gameObject.SetActive(false);
        }

        Face.GetComponent<Animator>().SetTrigger(name);
    }
}