using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Joystick_Controller : MonoBehaviour
{
    [SerializeField] private VirtualJoystick joystick;

    public ManageApiUrl ApiUrl;
    private string baseUrl;

    private const float yawThreshold = 0.1f;
    private const float throttleThreshold = 0.1f;

    private bool isRequestInProgress = false;
    private float requestCooldown = 2f; 
    private float timeSinceLastRequest = 0f;
    private DroneController droneController;

    public Button roll_btn;
    public Button pitch_btn;
    public Button throttle_btn;
    public Button yaw_btn;
    public Button stop_btn;

    private bool isRollActive = false;
    private bool isPitchActive = false;
    private bool isThrottleActive = false;
    private bool isYawActive = false;
    private bool isStopActive = false;

    private void Start()
    {
        baseUrl = ApiUrl.apiUrl;

        // Get the DroneController component on the same GameObject
        droneController = GetComponent<DroneController>();
        droneController.Initialize(baseUrl);

        roll_btn.onClick.AddListener(() => ToggleActiveState(ref isRollActive));
        pitch_btn.onClick.AddListener(() => ToggleActiveState(ref isPitchActive));
        throttle_btn.onClick.AddListener(() => ToggleActiveState(ref isThrottleActive));
        yaw_btn.onClick.AddListener(() => ToggleActiveState(ref isYawActive));
        stop_btn.onClick.AddListener(() => ToggleActiveState(ref isStopActive));
    }

    private void Update()
    {
        Vector3 joystickInput = joystick.Direction;
        float yaw = joystickInput.x; 
        float throttle = joystickInput.z;

        timeSinceLastRequest += Time.deltaTime;

        if (timeSinceLastRequest >= requestCooldown && (Mathf.Abs(yaw) > yawThreshold || Mathf.Abs(throttle) > throttleThreshold))
        {
            timeSinceLastRequest = 0f;
            string direction = DetermineDirection(joystickInput);

            int yawPercent = ConvertToPercentage(yaw);
            int throttlePercent = ConvertToPercentage(throttle);

            if (isRollActive)
            {
                StartCoroutine(ControlRoll(direction, yawPercent));
                isRollActive = false;
            }
            else if (isPitchActive)
            {
                StartCoroutine(ControlPitch(direction, throttlePercent));
                isPitchActive = false; 
            }
            else if (isThrottleActive)
            {
                StartCoroutine(ControlThrottle(direction, throttlePercent));
                isThrottleActive = false; 
            }
            else if (isYawActive)
            {
                StartCoroutine(ControlYaw(direction, yawPercent));
                isYawActive = false; 
            }
            else if (isStopActive)
            {
                StartCoroutine(EmergencyStop());
                isStopActive = false; 
            }
        }
    }

    private string DetermineDirection(Vector3 input)
    {
        if (input.z > 0 && Mathf.Abs(input.z) > Mathf.Abs(input.x))
            return "forward";
        if (input.z < 0 && Mathf.Abs(input.z) > Mathf.Abs(input.x))
            return "backward";
        if (input.x > 0 && Mathf.Abs(input.x) > Mathf.Abs(input.z))
            return "right";
        if (input.x < 0 && Mathf.Abs(input.x) > Mathf.Abs(input.z))
            return "left";
        return "stationary";
    }

    private int ConvertToPercentage(float value)
    {
        int percentage = Mathf.RoundToInt((value + 1) * 50); 
        return percentage;
    }

    private IEnumerator ControlRoll(string direction, int rollPercent)
    {
        isRequestInProgress = true;
        Debug.Log($"Roll action with direction: {direction}, roll percent: {rollPercent}");
        yield return droneController.ControlRoll(direction, rollPercent);
        isRequestInProgress = false;
    }

    private IEnumerator ControlPitch(string direction, int pitchPercent)
    {
        isRequestInProgress = true;
        Debug.Log($"Pitch action with direction: {direction}, pitch percent: {pitchPercent}");
        yield return droneController.ControlPitch(direction, pitchPercent);
        isRequestInProgress = false;
    }

    private IEnumerator ControlThrottle(string direction, int throttlePercent)
    {
        isRequestInProgress = true;
        Debug.Log($"Throttle action with direction: {direction}, throttle percent: {throttlePercent}");
        yield return droneController.ControlThrottle(throttlePercent);
        isRequestInProgress = false;
    }

    private IEnumerator ControlYaw(string direction, int yawPercent)
    {
        isRequestInProgress = true;
        Debug.Log($"Yaw action with direction: {direction}, yaw percent: {yawPercent}");
        yield return droneController.ControlYaw(direction, yawPercent);
        isRequestInProgress = false;
    }

    private IEnumerator EmergencyStop()
    {
        isRequestInProgress = true;
        Debug.Log("Emergency Stop action");
        yield return droneController.EmergencyStop();
        isRequestInProgress = false;
    }

    private void ToggleActiveState(ref bool buttonState)
    {
        isRollActive = false;
        isPitchActive = false;
        isThrottleActive = false;
        isYawActive = false;
        isStopActive = false;
        buttonState = true;
    }
}
