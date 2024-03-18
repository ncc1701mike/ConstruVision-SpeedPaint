using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UserInput : MonoBehaviour
{
    public UnityEvent OnTouch = new UnityEvent();
    public UnityEvent OnDoubleTap = new UnityEvent();
    private void Update()
    {
        // when the player taps the screen fire the OnTouch event
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            OnTouch.Invoke();
        }

        //when the player double taps the screen fire the onDoubleTap event
        if (Input.touchCount > 0 && Input.GetTouch(0).tapCount == 2)
        {
            OnDoubleTap.Invoke();
        }
    }
}
