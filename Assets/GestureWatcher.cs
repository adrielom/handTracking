using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandDetection
{

    public static class GestureWatcher
    {

        public static void SetGesture(Hand hand, Gesture gesture, Action callback)
        {
            if (hand.getHandGesture() == gesture) return;

            hand.setHandGesture(gesture);
            callback();
        }
    }

}