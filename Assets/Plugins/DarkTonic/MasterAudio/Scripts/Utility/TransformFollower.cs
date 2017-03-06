using DarkTonic.MasterAudio;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class TransformFollower : MonoBehaviour {
    [Tooltip("This is for diagnostic purposes only. Do not change or assign this field.")]
    public Transform RuntimeFollowingTransform;

    private GameObject _goToFollow;
    private Transform _trans;
    private GameObject _go;
    private SphereCollider _collider;
    private string _soundType;
    private bool _willFollowSource;

    // ReSharper disable once UnusedMember.Local
    void Awake() {
        var trig = Trigger;
        if (trig == null) { } // get rid of warning
    }

    public void StartFollowing(Transform transToFollow, string soundType, float trigRadius, bool willFollowSource) {
        RuntimeFollowingTransform = transToFollow;
        _goToFollow = transToFollow.gameObject;
        Trigger.radius = trigRadius;
        _soundType = soundType;
        _willFollowSource = willFollowSource;
    }

    private void StopFollowing() {
        RuntimeFollowingTransform = null;
        GameObject.Destroy(GameObj);
    }

    // ReSharper disable once UnusedMember.Local
    private void OnTriggerEnter(Collider other) {
        if (RuntimeFollowingTransform == null) {
            return;
        }

        if (other == null || name == AmbientUtil.ListenerFollowerName || other.name != AmbientUtil.ListenerFollowerName) {
            return; // abort if this is the Listener or if not colliding with Listener.
        }

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (_willFollowSource) {
            MasterAudio.PlaySound3DFollowTransformAndForget(_soundType, RuntimeFollowingTransform);
        } else {
            MasterAudio.PlaySound3DAtTransformAndForget(_soundType, RuntimeFollowingTransform);
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void OnTriggerExit(Collider other) {
        if (RuntimeFollowingTransform == null) {
            return;
        }

        if (other == null || other.name != AmbientUtil.ListenerFollowerName) {
            return; // abort if not colliding with Listener.
        }

        MasterAudio.StopSoundGroupOfTransform(RuntimeFollowingTransform, _soundType);
    }

    // ReSharper disable once UnusedMember.Local
    void LateUpdate() {
        if (RuntimeFollowingTransform == null || !DTMonoHelper.IsActive(_goToFollow)) {
            StopFollowing();
            return;
        }

        Trans.position = RuntimeFollowingTransform.position;
    }

    public SphereCollider Trigger {
        get {
            if (_collider != null) {
                return _collider;
            }

            _collider = GameObj.AddComponent<SphereCollider>();
            _collider.isTrigger = true;

            return _collider;
        }
    }

    public GameObject GameObj {
        get {
            if (_go != null) {
                return _go;
            }

            _go = gameObject;
            return _go;
        }
    }

    public Transform Trans {
        get {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_trans == null) {
                _trans = transform;
            }

            return _trans;
        }
    }
}
