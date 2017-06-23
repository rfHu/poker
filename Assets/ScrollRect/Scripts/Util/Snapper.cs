using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using frame8.Logic.Misc.Other.Extensions;

namespace frame8.ScrollRectItemsAdapter.Util
{
    // Incomplete implementation
    //public class Snapper : MonoBehaviour
    //{
        //public float snapWhenSpeedFallsBelow = 50f;
        //public float snapOnlyIfSpeedIsAbove = 20f;
        //public float snapDuration = .3f;
        //public float snapAllowedError = .01f;

        //ScrollRect _ScrollRect;
        //SimpleLoopingSpinnerExample.MyScrollRectAdapter _Adapter;
        //bool _ItemsRefreshed;
        //bool _SnappingInProgress;
        //bool _ScrolledAfterSnappingFinished;


        //IEnumerator Start()
        //{
        //    // Wait for any delayed inits
        //    yield return null;
        //    yield return null;
        //    yield return null;

        //    (_ScrollRect = GetComponent<ScrollRect>()).onValueChanged.AddListener(OnScrolled);
        //    _Adapter = SimpleLoopingSpinnerExample.adapter;
        //    _Adapter.ItemsRefreshed += OnItemsRefreshed;

        //}

        //void OnScrolled(Vector2 value) { if (!_SnappingInProgress) _ScrolledAfterSnappingFinished = true; }

        //int lastItemIndex = -1;
        //float lastItemInsetFromParentStart;

        //void Update()
        //{
        //    // Wait for init
        //    if (!_ScrollRect)
        //        return;

        //    if (Input.GetMouseButtonDown(2))
        //        _Adapter.SmoothScrollTo(3, 1f, .3f);

        //    if (true) return;

        //    var speed = _ScrollRect.velocity.magnitude;
        //    Debug.Log(speed);
        //    if (speed == 0f && !_SnappingInProgress && _ScrolledAfterSnappingFinished)
        //    {
        //        var middle = _Adapter.GetItemViewsHolder(Mathf.Max(0, _Adapter.VisibleItemsCount / 2 - 1));

        //        Debug.Log(middle);
        //        if (middle != null)
        //        {
        //            if (middle.itemIndex == lastItemIndex)
        //            {
        //                float curInset = middle.root.GetInsetFromParentEdge(_ScrollRect.content, _ScrollRect.horizontal ? RectTransform.Edge.Left : RectTransform.Edge.Top);

        //                if (Mathf.Abs(curInset - lastItemInsetFromParentStart) <= snapAllowedError)
        //                    return;
        //            }
        //            else
        //                lastItemIndex = middle.itemIndex;

        //            _ItemsRefreshed = false;
        //            _ScrolledAfterSnappingFinished = false;
        //            _SnappingInProgress = _Adapter.SmoothScrollTo(middle.itemIndex, snapDuration, .5f, progress =>
        //            {
        //                if (progress == 1f || _ItemsRefreshed) // done. last iteration
        //                {
        //                    _SnappingInProgress = false;
        //                    lastItemInsetFromParentStart = middle.root.GetInsetFromParentEdge(_ScrollRect.content, _ScrollRect.horizontal ? RectTransform.Edge.Left : RectTransform.Edge.Top);
        //                }

        //                // If the items were refreshed while the snap animation was playing, don't continue;
        //                return !_ItemsRefreshed;
        //            });
        //        }
        //    }
        //}

        //void OnItemsRefreshed(int oldCount, int newCount)
        //{
        //    _ItemsRefreshed = true;
        //}
    //}
}
