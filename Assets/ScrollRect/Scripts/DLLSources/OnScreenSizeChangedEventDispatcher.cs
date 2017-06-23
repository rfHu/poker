using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Misc.Visual.UI.MonoBehaviours
{
    /// <summary>
    /// <para>Collects all components on the same GameObject of type <see cref="IOnScreenSizeChangedListener"/> and calls <see cref="IOnScreenSizeChangedListener.OnScreenSizeChanged(float, float, float, float)"/> every time this event happens</para>
    /// <para>The listeners can also be registered manually via <see cref="RegisterListenerManually(IOnScreenSizeChangedListener)"/></para>
    /// </summary>
    public class OnScreenSizeChangedEventDispatcher : MonoBehaviour
    {
        float _LastScreenW, _LastScreenH;
        IOnScreenSizeChangedListener[] _Listeners = new IOnScreenSizeChangedListener[0];
        List<IOnScreenSizeChangedListener> _ManuallyRegisteredListeners = new List<IOnScreenSizeChangedListener>();

        void Start()
        {
            _LastScreenW = Screen.width;
            _LastScreenH = Screen.height;

            _Listeners = Array.ConvertAll(GetComponents(typeof(IOnScreenSizeChangedListener)), c => (IOnScreenSizeChangedListener)c);
        }

        void Update()
        {
            float curW = Screen.width, curH = Screen.height;
            if (curW != _LastScreenW || curH != _LastScreenH)
            {
                foreach (var listener in _Listeners)
                {
                    if (listener == null)
                        continue;

                    listener.OnScreenSizeChanged(_LastScreenW, _LastScreenH, curW, curH);
                }
                _LastScreenW = curW;
                _LastScreenH = curH;
            }

            // Registering the manually given ones by RegisterListenerManually(IOnScreenSizeChangedListener)
            if (_ManuallyRegisteredListeners.Count > 0)
            {
                _ManuallyRegisteredListeners.AddRange(_Listeners);
                _Listeners = _ManuallyRegisteredListeners.ToArray();
                _ManuallyRegisteredListeners.Clear();
            }
        }

        /// <summary> The listeners can also be registered manually via this method</summary>
        public void RegisterListenerManually(IOnScreenSizeChangedListener listener)
        {
            _ManuallyRegisteredListeners.Add(listener);
        }

        public interface IOnScreenSizeChangedListener
        {
            void OnScreenSizeChanged(float lastWidth, float lastHeight, float width, float height);
        }
    }
}
