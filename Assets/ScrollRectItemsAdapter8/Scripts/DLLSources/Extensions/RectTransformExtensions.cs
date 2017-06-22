using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
    public static class RectTransformExtensions
    {
        /// <summary> GetTop(), GetRight() etc. only work if there's no canvas scaling</summary>
        public static float GetWorldTop(this RectTransform rt)
        { return rt.position.y + (1f - rt.pivot.y) * rt.rect.height; }

        public static float GetWorldBottom(this RectTransform rt)
        { return rt.position.y - rt.pivot.y * rt.rect.height; }

        public static float GetWorldLeft(this RectTransform rt)
        { return rt.position.x - rt.pivot.x * rt.rect.width; }

        public static float GetWorldRight(this RectTransform rt)
        { return rt.position.x + (1f - rt.pivot.x) * rt.rect.width; }

        /// <summary>
        /// It assumes the transform has a parent
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parentHint"> the parent of child. used in order to prevent casting, in case the caller already has the parent stored in a variable</param>
        /// <returns></returns>
        public static float GetInsetFromParentTopEdge(this RectTransform child, RectTransform parentHint)
        {
            float parentPivotYDistToParentTop = (1f - parentHint.pivot.y) * parentHint.rect.height;
            float childLocPosY = child.localPosition.y;

            return parentPivotYDistToParentTop - child.rect.yMax - childLocPosY;
        }

        public static float GetInsetFromParentBottomEdge(this RectTransform child, RectTransform parentHint)
        {
            float parentPivotYDistToParentBottom = parentHint.pivot.y * parentHint.rect.height;
            float childLocPosY = child.localPosition.y;

            return parentPivotYDistToParentBottom + child.rect.yMin + childLocPosY;
        }

        public static float GetInsetFromParentLeftEdge(this RectTransform child, RectTransform parentHint)
        {
            float parentPivotXDistToParentLeft = parentHint.pivot.x * parentHint.rect.width;
            float childLocPosX = child.localPosition.x;

            return parentPivotXDistToParentLeft + child.rect.xMin + childLocPosX;
        }

        public static float GetInsetFromParentRightEdge(this RectTransform child, RectTransform parentHint)
        {
            float parentPivotXDistToParentRight = (1f - parentHint.pivot.x) * parentHint.rect.width;
            float childLocPosX = child.localPosition.x;

            return parentPivotXDistToParentRight - child.rect.xMax - childLocPosX;
        }


        public static float GetInsetFromParentEdge(this RectTransform child, RectTransform parentHint, RectTransform.Edge parentEdge)
        {
            switch (parentEdge)
            {
                case RectTransform.Edge.Top:
                    return child.GetInsetFromParentTopEdge(parentHint);
                case RectTransform.Edge.Bottom:
                    return child.GetInsetFromParentBottomEdge(parentHint);
                case RectTransform.Edge.Left:
                    return child.GetInsetFromParentLeftEdge(parentHint);
                case RectTransform.Edge.Right:
                    return child.GetInsetFromParentRightEdge(parentHint);
            }

            return 0f; // shouldn't happen if the caller is sane
        }

        // Assumes the child has a parent
        //public static void SetSizeWithCurrentAnchorsAndFixedEdge(this RectTransform child, RectTransform.Edge fixedEdge, float newSize)
        //{
        //    child.SetInsetAndSizeWithCurrentAnchorsAndFixedEdge(fixedEdge, child.GetInsetFromParentEdge(child.parent as RectTransform, fixedEdge), newSize);
        //}

        //public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float inset, float newSize)
        //{
        //    Vector2 anchorMin = child.anchorMin;
        //    Vector2 anchorMax = child.anchorMax;
        //    child.SetInsetAndSizeFromParentEdge(fixedEdge, inset, newSize);
        //    child.anchorMin = anchorMin;
        //    child.anchorMax = anchorMax;
        //}

        /// <summary> NOTE: Use the optimized version if parent is known </summary>
        public static void SetSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newSize)
        {
            var par = child.parent as RectTransform;
            child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(par, fixedEdge, child.GetInsetFromParentEdge(par, fixedEdge), newSize);
        }

        /// <summary> Optimized version of SetSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge fixedEdge, float newSize) when parent is known </summary>
        /// <param name="parentHint"></param>
        /// <param name="fixedEdge"></param>
        /// <param name="newSize"></param>
        public static void SetSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform parentHint, RectTransform.Edge fixedEdge, float newSize)
        {
            child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(parentHint, fixedEdge, child.GetInsetFromParentEdge(parentHint, fixedEdge), newSize);
        }

        /// <summary> NOTE: Use the optimized version if parent is known </summary>
        /// <param name="fixedEdge"></param>
        /// <param name="newInset"></param>
        /// <param name="newSize"></param>
        public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newInset, float newSize)
        {
            child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(child.parent as RectTransform, fixedEdge, newInset, newSize);
        }

        /// <summary> Optimized version of SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge fixedEdge, float newInset, float newSize) when parent is known </summary>
        /// <param name="parentHint"></param>
        /// <param name="fixedEdge"></param>
        /// <param name="newInset"></param>
        /// <param name="newSize"></param>
        public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform parentHint, RectTransform.Edge fixedEdge, float newInset, float newSize)
        {
            Vector2 offsetMin = child.offsetMin;
            Vector2 offsetMax = child.offsetMax;

            float currentOffset, offsetChange;

            switch (fixedEdge)
            {
                case RectTransform.Edge.Bottom:
                    currentOffset = child.GetInsetFromParentBottomEdge(parentHint);
                    offsetChange = newInset - currentOffset;
                    offsetMax.y += (newSize - child.rect.height) + offsetChange;
                    offsetMin.y += offsetChange;
                    break;

                case RectTransform.Edge.Top:
                    currentOffset = child.GetInsetFromParentTopEdge(parentHint);
                    offsetChange = newInset - currentOffset;
                    offsetMin.y -= (newSize - child.rect.height) + offsetChange;
                    offsetMax.y -= offsetChange;
                    break;

                case RectTransform.Edge.Left:
                    currentOffset = child.GetInsetFromParentLeftEdge(parentHint);
                    offsetChange = newInset - currentOffset;
                    offsetMax.x += (newSize - child.rect.width) + offsetChange;
                    offsetMin.x += offsetChange;
                    break;

                case RectTransform.Edge.Right:
                    currentOffset = child.GetInsetFromParentRightEdge(parentHint);
                    offsetChange = newInset - currentOffset;
                    offsetMin.x -= (newSize - child.rect.width) + offsetChange;
                    offsetMax.x -= offsetChange;
                    break;
            }

            child.offsetMin = offsetMin;
            child.offsetMax = offsetMax;
        }


    }
}
