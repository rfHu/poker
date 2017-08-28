#if UNITY_5 || UNITY_2017
using UnityEngine;

/*! \cond PRIVATE */
// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    // ReSharper disable once CheckNamespace
    public class MechanimStateCustomEvents : StateMachineBehaviour {
        [MasterCustomEvent]
        // ReSharper disable once InconsistentNaming
		public string enterCustomEvent = MasterAudio.NoGroupName;

        [MasterCustomEvent]
        // ReSharper disable once InconsistentNaming
		public string exitCustomEvent = MasterAudio.NoGroupName;

        [Header("Custom Event Timed To Animation")]
        //Make this true if you want to use the option to play a sound at a specific time in your animation
        //This will be set to false automatically once you have played your sound so it will not accidentally run again since the update loop
        //will continue to run until the animation state has completed
        public bool fireEventUsingAnimTime = false;

        //This value represents when you want to start playing your sound. 
        //This value will be compared to the normalizedTime of the animation you are playing.  
        //normalizedTime is just a fancy way of saying, the length in time of your animation.  Your animation goes from 0% - 100% or 0f - 1f when represented by a float value.  
        //normalizedTime is represented as a float so 0 is the beginning, 1 is the end and .5f would be the middle etc. etc
        //If you want your sound to play after 20% of your animation has run, make this float equal to .2f
        [Range(0f, 1f)]
        public float whenToFireEvent;
        private bool fireTimedEvent = true;

        [MasterCustomEvent]
        // ReSharper disable once InconsistentNaming
        public string timedCustomEvent = MasterAudio.NoGroupName;

        private Transform _actorTrans;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _actorTrans = ActorTrans(animator);

            MasterAudio.FireCustomEvent(enterCustomEvent, _actorTrans);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (!fireEventUsingAnimTime) {
                return;
            }

            if (!fireTimedEvent) {
                return;
            }

            if (stateInfo.normalizedTime <= whenToFireEvent) {
                return;
            }

            fireTimedEvent = false;

            MasterAudio.FireCustomEvent(timedCustomEvent, _actorTrans);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            MasterAudio.FireCustomEvent(exitCustomEvent, _actorTrans);

            if (fireEventUsingAnimTime) {
                fireTimedEvent = true;
            }
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

    }
}
/*! \endcond */
#endif