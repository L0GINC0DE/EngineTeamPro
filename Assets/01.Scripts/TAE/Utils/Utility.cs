using System;
using UnityEngine;

namespace _01.Scripts.TAE.Utils
{
    public class Utility : MonoBehaviour
    {
        private static Camera _mainCam;

        private void Awake()
        {
            _mainCam = Camera.main;
        }

        public static Vector3 GetPos(Vector3 pos)
        {
            return _mainCam.WorldToScreenPoint(pos);
        }

        public static Vector3 GetViewPort(Vector3 screen)
        {
            return _mainCam.WorldToViewportPoint(screen);
        }
    }
}