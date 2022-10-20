using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchInputManager : MonoBehaviour
{
    public Action tap;

    private void Update()
    {
#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.Space))
            tap?.Invoke();
#else
         if (Input.touchCount > 0 && Input.touchCount == 1)
            tap?.Invoke();
#endif
    }
}