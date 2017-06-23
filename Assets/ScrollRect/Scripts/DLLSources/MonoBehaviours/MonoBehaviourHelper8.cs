using System;
using System.Collections;
using UnityEngine;

namespace frame8.Logic.Core.MonoBehaviours
{
    /// <summary>
    /// Creates a GameObject at runtime via <see cref="CreateInstance(Action, Transform, string)"/> and adds an <see cref="MonoBehaviourHelper8"/> component to it, which can provide utile functionalities.
    /// Don't foget to call Dispose() when done !
    /// </summary>
    public class MonoBehaviourHelper8 : MonoBehaviour
    {
        Action updateAction;


        /// <summary> Don't foget to call Dispose() when done !</summary>
        /// <param name="updateAction">Will be called each frame</param>
        /// <param name="parent">if null, the instance will be at the scene's root</param>
        /// <param name="name"></param>
        /// <returns>the new instance</returns>
        public static MonoBehaviourHelper8 CreateInstance(Action updateAction, Transform parent = null, string name = "MonoBehaviourHelper8")
        {
            var instance = new GameObject(name).AddComponent<MonoBehaviourHelper8>();
            instance.updateAction = updateAction;
            if (parent)
                instance.transform.parent = parent;

            return instance;
        }

        public void CallDelayedByFrames(Action action, int afterFrames)
        {
            StartCoroutine(DelayedCallByFrames(action, afterFrames));
        }

        IEnumerator DelayedCallByFrames(Action action, int afterFrames)
        {
            while (afterFrames-- > 0)
                yield return null;

            if (action != null)
                action();

            yield return null;
            yield break;
        }

        void Update()
        {
            if (updateAction == null)
                return;

            updateAction();
        }

        public void Dispose()
        {
            if (gameObject)
            {
                try
                {
                    try
                    {
                        gameObject.SetActive(false);
                        StopAllCoroutines();
                    }
                    catch { }

                    Destroy(gameObject);
                }
                catch { }
            }
        }
    }
}
