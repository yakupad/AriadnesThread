using System.Collections.Generic;
using UnityEngine;

namespace AriadnesThread.View
{
    /// <summary>
    /// OnGUI touch buttons and GridPlayerController's tap-to-move read the same physical
    /// touch through two independent input paths (IMGUI vs. the Input System directly) —
    /// neither knows the other exists, so tapping a button would also raycast-move the
    /// player underneath it. Callers that draw a touch control claim its screen rect here;
    /// tap-to-move checks it first and bails out if the touch landed inside one.
    /// </summary>
    public static class TouchUIGuard
    {
        private static readonly List<Rect> ClaimedRects = new List<Rect>();

        /// <summary>Call once at the start of the frame's OnGUI, before drawing any controls.</summary>
        public static void ClearFrame() => ClaimedRects.Clear();

        /// <summary>Reserve a rect (in GUI space — Y down from the top) as UI, not world.</summary>
        public static void ClaimRect(Rect rect) => ClaimedRects.Add(rect);

        /// <summary>screenPoint is in screen space (Y up from the bottom), matching Touch/mouse position.</summary>
        public static bool IsPointerOverUI(Vector2 screenPoint)
        {
            var guiPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            foreach (var rect in ClaimedRects)
                if (rect.Contains(guiPoint)) return true;
            return false;
        }
    }
}
