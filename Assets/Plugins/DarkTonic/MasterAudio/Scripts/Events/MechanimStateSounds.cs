#if UNITY_5 || UNITY_2017
using UnityEngine;

/*! \cond PRIVATE */
// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    // ReSharper disable once CheckNamespace
    public class MechanimStateSounds : StateMachineBehaviour {
        [Header("Select for sounds to follow Object")]
        public bool SoundFollowsObject = false;

        [Tooltip("Play a Sound Group when state is Entered")]
        [Header("Enter Sound Group")]
        public bool playEnterSound = true;
        public bool stopEnterSoundOnExit = false;
        [SoundGroup]
        public string enterSoundGroup = MasterAudio.NoGroupName;
        [Tooltip("Random Variation plays if blank, otherwise name a Variation from the above Sound Group to play.")]
        public string enterVariationName; //User inputs name of variation to play
        private bool wasEnterSoundPlayed;

        [Tooltip("Play a Sound Group when state is Exited")]
        [Header("Exit Sound Group")]
        public bool playExitSound = true;
        [SoundGroup]
        public string exitSoundGroup = MasterAudio.NoGroupName;
        [Tooltip("Random Variation plays if blank, otherwise name a Variation from the above Sound Group to play.")]
        public string exitVariationName; //User inputs name of variation to play

        [Tooltip("Play a Sound Group (Normal or Looped Chain Variation Mode) timed to the animation state's normalized time.  " +
            "Normalized time is simply the length in time of the animation.  " +
            "Time is represented as a float from 0f - 1f.  0f is the beginning, .5f is the middle, 1f is the end...etc.etc.  " +
            "Select a Start time from 0 - 1.  Select a stop time greater than the start time or leave stop time equals to zero and " +
            "select Stop Anim Time Sound On Exit.  This can be used for Looped Chain Sound Groups since you have to define a stop time.")]
        [Header("Play Sound Group Timed to Animation")]
        public bool playAnimTimeSound = false;             //Play a sound at a speccific time in your animation
        public bool stopAnimTimeSoundOnExit = false;       //Stop sound upon state exit instead of using Time
        [Tooltip("If selected, When To Stop Sound (below) will be used. Otherwise the sound will not be stopped unless you have Stop Anim Time Sound On Exit selected above.")]
        public bool useStopTime;

        [Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
        [Range(0f, 1f)]
        public float whenToStartSound;          //Based upon normalizedTime
        [Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
        [Range(0f, 1f)]
        public float whenToStopSound;           //Based upon normalizedTime
        [SoundGroup]
        public string TimedSoundGroup = MasterAudio.NoGroupName;
        [Tooltip("Random Variation plays if blank, otherwise name a Variation from the above Sound Group to play.")]
        public string timedVariationName; //User inputs name of variation to play
        private bool playSoundStart = true;
        private bool playSoundStop = true;


        [Tooltip("Play a Sound Group with each variation timed to the animation.  This allows you to " +
            "time your sounds to the actions in you animation.  Example: A sword swing combo where you want swoosh sounds" +
            "or character grunts timed to the acceleration phase of the sword swing.  Select the number of sounds to be played, up to 4.  " +
            "Then set the time you want each sound to start with each subsequent time greater than the previous time.")]

        [Header("Play Multiple Sounds Timed to Anim")]
        public bool playMultiAnimTimeSounds = false;

        public bool StopMultiAnimTimeSoundsOnExit;

        [Range(0, 4)]
        public int numOfMultiSoundsToPlay;
        [Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
        [Range(0f, 1f)]
        public float whenToStartMultiSound1;           //Based upon normalizedTime
        [Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
        [Range(0f, 1f)]
        public float whenToStartMultiSound2;           //Based upon normalizedTime
        [Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
        [Range(0f, 1f)]
        public float whenToStartMultiSound3;           //Based upon normalizedTime
        [Tooltip("This value will be compared to the normalizedTime of the animation you are playing. NormalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc.")]
        [Range(0f, 1f)]
        public float whenToStartMultiSound4;           //Based upon normalizedTime
        [SoundGroup]
        public string MultiSoundsTimedGroup = MasterAudio.NoGroupName;
        [Tooltip("Random Variation plays if blank, otherwise name a Variation from the above Sound Group to play.")]
        public string multiTimedVariationName; //User inputs name of variation to play

        private bool playMultiSound1 = true;
        private bool playMultiSound2 = true;
        private bool playMultiSound3 = true;
        private bool playMultiSound4 = true;
        private Transform _actorTrans;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _actorTrans = ActorTrans(animator);

            if (!playEnterSound) {
                return;
            }

            var varName = GetVariationName(enterVariationName);
            if (SoundFollowsObject) {
                if (varName == null) {
                    MasterAudio.PlaySound3DFollowTransformAndForget(enterSoundGroup, _actorTrans);
                } else {
                    MasterAudio.PlaySound3DFollowTransformAndForget(enterSoundGroup, _actorTrans, 1f, null, 0f, varName);
                }
            } else {
                if (varName == null) {
                    MasterAudio.PlaySound3DAtTransformAndForget(enterSoundGroup, _actorTrans);
                } else {
                    MasterAudio.PlaySound3DAtTransformAndForget(enterSoundGroup, _actorTrans, 1f, null, 0f, varName);
                }
            }

            wasEnterSoundPlayed = true;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            #region Timed to Anim

            if (playAnimTimeSound) {
                if (playSoundStart) {
                    if (stateInfo.normalizedTime > whenToStartSound) {
                        playSoundStart = false;

                        //If user selects useStopTime and the stop time is less then start time, they will hear no sound
                        if (useStopTime && whenToStopSound < whenToStartSound) {
                            Debug.LogError("Stop time must be greater than start time when Use Stop Time is selected.");
                            goto outside;
                        }

                        var varName = GetVariationName(timedVariationName);
                        if (SoundFollowsObject) {
                            if (varName == null) {
                                MasterAudio.PlaySound3DFollowTransformAndForget(TimedSoundGroup, _actorTrans);
                            } else {
                                MasterAudio.PlaySound3DFollowTransformAndForget(TimedSoundGroup, _actorTrans, 1f, null, 0f, varName);
                            }
                        } else {
                            if (varName == null) {
                                MasterAudio.PlaySound3DAtTransformAndForget(TimedSoundGroup, _actorTrans);
                            } else {
                                MasterAudio.PlaySound3DAtTransformAndForget(TimedSoundGroup, _actorTrans, 1f, null, 0f, varName);
                            }
                        }
                    }
                }
            }

            outside:

			if (playAnimTimeSound && useStopTime) {
				if (playSoundStop) {
                    if (stateInfo.normalizedTime > whenToStartSound) {
                        if (!stopAnimTimeSoundOnExit) {
                            //Sound will stop upon exit instead of relying on animation time
                            if (stateInfo.normalizedTime > whenToStopSound) {
                                playSoundStop = false;
                                MasterAudio.StopSoundGroupOfTransform(_actorTrans, TimedSoundGroup);
                            }
                        }
                    }
                }
            }
            #endregion

            #region Play Multiple Sounds Timed To Anim

            if (!playMultiAnimTimeSounds) {
                return;
            }

            var multiVarName = GetVariationName(multiTimedVariationName);

            if (playMultiSound1) {
                if (stateInfo.normalizedTime > whenToStartMultiSound1 && numOfMultiSoundsToPlay >= 1) {
                    playMultiSound1 = false;
                    if (SoundFollowsObject) {
                        if (multiVarName == null) {
                            MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup,
                                _actorTrans);
                        } else {
                            MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup,
                                _actorTrans, 1f, null, 0f, multiVarName);
                        }
                    } else {
                        if (multiVarName == null) {
                            MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
                        } else {
                            MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, multiVarName);
                        }
                    }
                }
            }

            if (playMultiSound2) {
                if (stateInfo.normalizedTime > whenToStartMultiSound2 && numOfMultiSoundsToPlay >= 2) {
                    playMultiSound2 = false;
                    if (SoundFollowsObject) {
                        if (multiVarName == null) {
                            MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
                        } else {
                            MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, multiVarName);
                        }
                    } else {
                        if (multiVarName == null) {
                            MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
                        } else {
                            MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, multiVarName);
                        }
                    }
                }
            }

            if (playMultiSound3) {
                if (stateInfo.normalizedTime > whenToStartMultiSound3 && numOfMultiSoundsToPlay >= 3) {
                    playMultiSound3 = false;
                    if (SoundFollowsObject) {
                        if (multiVarName == null) {
                            MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
                        } else {
                            MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, multiVarName);
                        }
                    } else {
                        if (multiVarName == null) {
                            MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
                        } else {
                            MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, multiVarName);
                        }
                    }
                }
            }

            if (playMultiSound4) {
                if (stateInfo.normalizedTime > whenToStartMultiSound4 && numOfMultiSoundsToPlay >= 4) {
                    playMultiSound4 = false;
                    if (SoundFollowsObject) {
                        if (multiVarName == null) {
                            MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
                        } else {
                            MasterAudio.PlaySound3DFollowTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, multiVarName);
                        }
                    } else {
                        if (multiVarName == null) {
                            MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans);
                        } else {
                            MasterAudio.PlaySound3DAtTransformAndForget(MultiSoundsTimedGroup, _actorTrans, 1f, null, 0f, multiVarName);
                        }
                    }
                }
            }

            #endregion
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (wasEnterSoundPlayed && stopEnterSoundOnExit) {
                MasterAudio.StopSoundGroupOfTransform(_actorTrans, enterSoundGroup);
            }
            wasEnterSoundPlayed = false;

            if (playExitSound) {
                var varName = GetVariationName(exitVariationName);

                if (SoundFollowsObject) {
                    if (varName == null) {
                        MasterAudio.PlaySound3DFollowTransformAndForget(exitSoundGroup, _actorTrans);
                    } else {
                        MasterAudio.PlaySound3DFollowTransformAndForget(exitSoundGroup, _actorTrans, 1f, null, 0f, varName);
                    }
                } else {
                    if (varName == null) {
                        MasterAudio.PlaySound3DAtTransformAndForget(exitSoundGroup, _actorTrans);
                    } else {
                        MasterAudio.PlaySound3DAtTransformAndForget(exitSoundGroup, _actorTrans, 1f, null, 0f, varName);
                    }
                }
            }

            #region Timed to Anim
            if (playAnimTimeSound) {
                if (stopAnimTimeSoundOnExit) {
                    MasterAudio.StopSoundGroupOfTransform(_actorTrans, TimedSoundGroup);
                }

                playSoundStart = true;
                playSoundStop = true;
            }
            #endregion

            #region Play Multiple Sounds Timed To Anim
            if (playMultiAnimTimeSounds) {
                if (StopMultiAnimTimeSoundsOnExit) {
                    MasterAudio.StopSoundGroupOfTransform(_actorTrans, MultiSoundsTimedGroup);
                }
                playMultiSound1 = true;
                playMultiSound2 = true;
                playMultiSound3 = true;
                playMultiSound4 = true;
            }
            #endregion
        }

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        private Transform ActorTrans(Animator anim) {
            if (_actorTrans != null) {
                return _actorTrans;
            }

            _actorTrans = anim.transform;

            return _actorTrans;
        }

        private static string GetVariationName(string varName) {
            if (string.IsNullOrEmpty(varName)) {
                return null;
            }

            varName = varName.Trim();

            if (string.IsNullOrEmpty(varName)) {
                return null;
            }

            return varName;
        }
    }
}
/*! \endcond */
#endif