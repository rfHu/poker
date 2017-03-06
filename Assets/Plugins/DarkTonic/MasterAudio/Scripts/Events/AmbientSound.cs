/*! \cond PRIVATE */
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    [AddComponentMenu("Dark Tonic/Master Audio/Ambient Sound")]
    // ReSharper disable once CheckNamespace
    [AudioScriptOrder(-20)]
    public class AmbientSound : MonoBehaviour {
        [SoundGroup] public string AmbientSoundGroup = MasterAudio.NoGroupName;
        public bool FollowCaller;

        [Tooltip("This is for diagnostic purposes only. Do not change or assign this field.")]
        public Transform RuntimeFollower;

        private Transform _trans;

        // ReSharper disable once UnusedMember.Local
        void OnEnable() {
            StartTrackers();
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable() {
            if (MasterAudio.AppIsShuttingDown) {
                return; // do nothing
            }

            if (!IsValidSoundGroup) {
                return;
            }

            if (MasterAudio.SafeInstance == null) {
                return;
            }

            MasterAudio.StopSoundGroupOfTransform(Trans, AmbientSoundGroup);
            RuntimeFollower = null;
        }

        private void StartTrackers() {
            if (!IsValidSoundGroup) {
                return;
            }

            var isListenerFollowerAvailable = AmbientUtil.InitListenerFollower();
            if (!isListenerFollowerAvailable) {
                return; // don't bother creating the follower because there's no Listener to collide with.
            }

            var followerName = name + "_" + AmbientSoundGroup + "_" + Random.Range(0, 9) + "_Follower";
            RuntimeFollower = AmbientUtil.InitAudioSourceFollower(Trans, followerName, AmbientSoundGroup, FollowCaller);
        }

        public bool IsValidSoundGroup {
            get {
                return !MasterAudio.SoundGroupHardCodedNames.Contains(AmbientSoundGroup);
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
}
/*! \endcond */
