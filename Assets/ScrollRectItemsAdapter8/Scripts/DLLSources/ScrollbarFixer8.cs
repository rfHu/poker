using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace frame8.Logic.Misc.Visual.UI.MonoBehaviours
{
    /// <summary>
    /// <para>Fixes ScrollView inertia when the content grows too big. The default method cuts off the inertia in most cases.</para>
    /// <para>Make sure no scrollbars are assigned to the ScrollRect</para>
    /// </summary>
    [RequireComponent(typeof(Scrollbar))]
    public class ScrollbarFixer8 : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool hideWhenNotNeeded = true;
        public bool autoHide = true;

        [Tooltip("Used if autoHide is on. Duration in seconds")]
        public float autoHideTime = 1f;

        [Range(0.01f, 1f)]
        public float minSize = .1f;

        [Range(0.1f, 2f)]
        public float sizeUpdateInterval = .2f;

        [Tooltip("If not assigned, will try yo find one in the parent")]
        public ScrollRect scrollRect;

        [Tooltip("If not assigned, will use the resolved scrollRect")]
        public RectTransform viewport;

        RectTransform _ScrollRectRT, _ViewPortRT;
        Scrollbar _Scrollbar;
        bool _HorizontalScrollBar;
        Vector3 _ScaleBeforeHide = Vector3.one;
        bool _Hidden, _AutoHidden, _HiddenNotNeeded;
        float _LastValue;
        float _TimeOnLastValueChange;
        bool _Dragging;
        Coroutine _SlowUpdateCoroutine;

        // Use this for initialization
        void Awake()
        {
            _Scrollbar = GetComponent<Scrollbar>();
            _LastValue = _Scrollbar.value;
            _TimeOnLastValueChange = Time.time;
            _HorizontalScrollBar = _Scrollbar.direction == Scrollbar.Direction.LeftToRight || _Scrollbar.direction == Scrollbar.Direction.RightToLeft;
            if (!scrollRect)
            {
                scrollRect = GetComponentInParent<ScrollRect>();
                //if (!scrollRect)
                //    throw new UnityException("Please provide a ScrollRect for ScrollbarFixer8 to work");
            }

            if (scrollRect)
            {
                _ScrollRectRT = scrollRect.transform as RectTransform;
                if (!viewport)
                    viewport = _ScrollRectRT;

                if (_HorizontalScrollBar)
                {
                    if (!scrollRect.horizontal)
                        throw new UnityException("Can't use horizontal scrollbar with non-horizontal scrollRect");

                    if (scrollRect.horizontalScrollbar)
                    {
                        Debug.Log("ScrollbarFixer8: setting scrollRect.horizontalScrollbar to null (the whole point of using ScrollbarFixer8 is to NOT have any scrollbars assigned)");
                        scrollRect.horizontalScrollbar = null;
                    }
                    if (scrollRect.verticalScrollbar == _Scrollbar)
                    {
                        Debug.Log("ScrollbarFixer8: Can't use the same scrollbar for both vert and hor");
                        scrollRect.verticalScrollbar = null;
                    }
                }
                else
                {
                    if (!scrollRect.vertical)
                        throw new UnityException("Can't use vertical scrollbar with non-vertical scrollRect");

                    if (scrollRect.verticalScrollbar)
                    {
                        Debug.Log("ScrollbarFixer8: setting scrollRect.verticalScrollbar to null (the whole point of using ScrollbarFixer8 is to NOT have any scrollbars assigned)");
                        scrollRect.verticalScrollbar = null;
                    }
                    if (scrollRect.horizontalScrollbar == _Scrollbar)
                    {
                        Debug.Log("ScrollbarFixer8: Can't use the same scrollbar for both vert and hor");
                        scrollRect.horizontalScrollbar = null;
                    }
                }

            }
            else
                Debug.LogError("No ScrollRect assigned!");
        }

        void OnEnable()
        {
            _Dragging = false; // just in case dragging was stuck in true and the object was disabled
            _SlowUpdateCoroutine = StartCoroutine(SlowUpdate());
        }

        void OnDisable()
        {
            StopCoroutine(_SlowUpdateCoroutine);
        }

        IEnumerator SlowUpdate()
        {
            var wait1Sec = new WaitForSeconds(sizeUpdateInterval);

            while (true)
            {
                yield return wait1Sec;

                if (!enabled)
                    break;

                if (_ScrollRectRT && scrollRect.content)
                {
                    float size, contentSize, viewportSize;
                    if (_HorizontalScrollBar)
                    {
                        contentSize = scrollRect.content.rect.width;
                        viewportSize = viewport.rect.width;
                    }
                    else
                    {
                        contentSize = scrollRect.content.rect.height;
                        viewportSize = viewport.rect.height;
                    }

                    if (contentSize <= 0f || contentSize == float.NaN || contentSize == float.Epsilon || contentSize == float.NegativeInfinity || contentSize == float.PositiveInfinity)
                        size = 1f;
                    else
                        size = Mathf.Clamp(viewportSize / contentSize, minSize, 1f);

                    _Scrollbar.size = size;
                    if (hideWhenNotNeeded)
                    {
                        if (size == 1f)
                        {
                            if (!_Hidden)
                            {
                                _HiddenNotNeeded = true;
                                Hide();
                            }
                        }
                        else
                        {
                            if (_Hidden && !_AutoHidden) // if autohidden, we don't interfere with the process
                            {

                                Show();
                            }
                        }
                    }
                    // Handling the case when the scrollbar was hidden but its hideWhenNotNeeded property was set to false afterwards
                    // and autoHide is also false, meaning the scrollbar won't ever be shown
                    else if (!autoHide)
                    {
                        if (_Hidden)
                            Show();
                    }
                }
            }
        }

        void Hide()
        {
            _Hidden = true;
            _ScaleBeforeHide = gameObject.transform.localScale;
            gameObject.transform.localScale = Vector3.zero;
        }

        void Show()
        {
            gameObject.transform.localScale = _ScaleBeforeHide;
            _HiddenNotNeeded = _AutoHidden = _Hidden = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (scrollRect)
            {
                // Don't override when dragging
                if (_Dragging)
                {
                    _TimeOnLastValueChange = Time.time;
                    return;
                }
                float value;
                if (scrollRect.vertical)
                    value = scrollRect.normalizedPosition.y;
                else
                    value = scrollRect.normalizedPosition.x;

                _Scrollbar.value = value;
                if (autoHide)
                {
                    if (value == _LastValue)
                    {
                        if (!_Hidden)
                        {
                            if (Time.time - _TimeOnLastValueChange >= autoHideTime)
                            {
                                _AutoHidden = true;
                                Hide();
                            }
                        }
                    }
                    else
                    {
                        _TimeOnLastValueChange = Time.time;
                        _LastValue = value;

                        if (_Hidden && !_HiddenNotNeeded)
                            Show();
                    }
                }
                // Handling the case when the scrollbar was hidden but its autoHide property was set to false afterwards 
                // and hideWhenNotNeeded is also false, meaning the scrollbar won't ever be shown
                else if (!hideWhenNotNeeded)
                {
                    if (_Hidden)
                        Show();
                }
            }
        }

        #region Unity UI event callbacks
        public void OnBeginDrag(PointerEventData eventData) { _Dragging = true; }
        public void OnEndDrag(PointerEventData eventData) { _Dragging = false; }
        public void OnDrag(PointerEventData eventData)
        {
            scrollRect.StopMovement();
            var normPos = scrollRect.normalizedPosition;
            if (_HorizontalScrollBar)
                normPos.x = _Scrollbar.value;
            else
                normPos.y = _Scrollbar.value;

            scrollRect.normalizedPosition = normPos;
        }
        #endregion
    }
}