using UnityEngine;

/*! \cond PRIVATE */

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    public static class AmbientUtil {
        public const string FollowerHolderName = "_Followers";
        public const string ListenerFollowerName = "~ListenerFollower~";
        public const float ListenerFollowerTrigRadius = 1f;

        private static Transform _followerHolder;
        private static ListenerFollower _listenerFollower;

        public static void InitFollowerHolder() {
            var h = FollowerHolder;
            if (h != null) {
                h.DestroyAllChildren();
            }
        }

        public static bool InitListenerFollower() {
            var listener = MasterAudio.ListenerTrans;
            if (listener == null) {
                return false;
            }

            var follower = ListenerFollower;
            if (follower == null) {
                return false;
            }

            follower.StartFollowing(listener, MasterAudio.NoGroupName, ListenerFollowerTrigRadius);
            return true;
        }

        public static Transform InitAudioSourceFollower(Transform transToFollow, string followerName, string soundGroupName, bool willFollowSource) {
            if (ListenerFollower == null || FollowerHolder == null) {
                return null;
            }

            var grp = MasterAudio.GrabGroup(soundGroupName);
            if (grp == null) {
                return null;
            }

            if (grp.groupVariations.Count == 0) {
                return null;
            }

            var triggerRadius = grp.groupVariations[0].VarAudio.maxDistance;

            var follower = new GameObject(followerName);
            var existingDupe = FollowerHolder.FindChild(followerName);
            if (existingDupe != null) {
                GameObject.Destroy(existingDupe.gameObject);
            }

            follower.transform.parent = FollowerHolder;
            var followerScript = follower.gameObject.AddComponent<TransformFollower>();

            followerScript.StartFollowing(transToFollow, soundGroupName, triggerRadius, willFollowSource);
            return follower.transform;
        }

        public static ListenerFollower ListenerFollower {
            get {
                if (_listenerFollower != null) {
                    return _listenerFollower;
                }

                if (FollowerHolder == null) {
                    return null;
                }

                var follower = FollowerHolder.FindChild(ListenerFollowerName);
                if (follower == null) {
                    follower = new GameObject(ListenerFollowerName).transform;
                    follower.parent = FollowerHolder;
                }

                _listenerFollower = follower.GetComponent<ListenerFollower>();
                if (_listenerFollower == null) {
                    _listenerFollower = follower.gameObject.AddComponent<ListenerFollower>();
                }

                var rb = follower.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;

                return _listenerFollower;
            }
        }

        public static Transform FollowerHolder {
            get {
                if (!Application.isPlaying || MasterAudio.SafeInstance == null) {
                    return null;
                }

                if (_followerHolder != null) {
                    return _followerHolder;
                }

                var ma = MasterAudio.SafeInstance.Trans;
                _followerHolder = ma.FindChild(FollowerHolderName);

                if (_followerHolder != null) {
                    return _followerHolder;
                }

                _followerHolder = new GameObject(FollowerHolderName).transform;
                _followerHolder.parent = ma;

                return _followerHolder;
            }
        }
    }
}

/*! \endcond */
