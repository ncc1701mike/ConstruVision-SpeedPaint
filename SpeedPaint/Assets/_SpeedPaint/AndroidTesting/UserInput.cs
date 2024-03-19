using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UserInput : MonoBehaviour
{
    public static UnityEvent OnTouch = new UnityEvent();
    public static UnityEvent OnDoubleTap = new UnityEvent();

    public static int totalTouchCount = 0;
    public static Vector2 touchPosition;

    [SerializeField] private int touchCount = 0;

    private void Update()
    {
        // when the player taps the screen fire the OnTouch event
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            OnTouch.Invoke();
            touchCount++;
            touchPosition = Input.GetTouch(0).position;
        }

        //when the player double taps the screen fire the onDoubleTap event
        if (Input.touchCount > 0 && Input.GetTouch(0).tapCount == 2)
        {
            OnDoubleTap.Invoke();
            touchCount += 2;
            touchPosition = Input.GetTouch(0).position;
        }

        totalTouchCount = touchCount;
    }
}
