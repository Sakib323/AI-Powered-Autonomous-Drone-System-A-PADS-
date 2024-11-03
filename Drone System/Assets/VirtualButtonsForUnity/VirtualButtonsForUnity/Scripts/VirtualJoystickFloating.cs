using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualJoystickFloating : VirtualJoystick
{

    [SerializeField] private bool hideOnPointerUp = false;
    [SerializeField] private bool centralizeOnPointerUp = true;

    protected override void Awake()
    {
        joystickType = VirtualJoystickType.Floating;
        _hideOnPointerUp = hideOnPointerUp;
        _centralizeOnPointerUp = centralizeOnPointerUp;

        base.Awake();
    }

    void Update()
    {
        // Get the joystick movement value as a Vector3
        Vector3 joystickInput = Direction; // Use the property to get the input

        // Use the joystick input value as needed
        if (joystickInput != Vector3.zero)
        {
            //Debug.Log("Joystick Input: " + joystickInput);
            DroneMovement(joystickInput);
        }
    }

    private void DroneMovement(Vector3 input)
    {

    }

}
