using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class DroneController : MonoBehaviour
{
    private string baseUrl;

    [System.Serializable]
    private class ThrottleData
    {
        public int value;
    }

    [System.Serializable]
    private class ControlData
    {
        public string direction;
        public int amount;
    }

    public void Initialize(string url)
    {
        baseUrl = url;
    }

    public IEnumerator ControlThrottle(int value)
    {
        string url = $"{baseUrl}/throttle";
        var data = new ThrottleData { value = value };
        string jsonPayload = JsonUtility.ToJson(data);
        yield return SendPostRequest(url, jsonPayload);
    }

    public IEnumerator ControlRoll(string direction, int amount)
    {
        string url = $"{baseUrl}/roll";
        var data = new ControlData { direction = direction, amount = amount };
        string jsonPayload = JsonUtility.ToJson(data);
        yield return SendPostRequest(url, jsonPayload);
    }

    public IEnumerator ControlPitch(string direction, int amount)
    {
        string url = $"{baseUrl}/pitch";
        var data = new ControlData { direction = direction, amount = amount };
        string jsonPayload = JsonUtility.ToJson(data);
        yield return SendPostRequest(url, jsonPayload);
    }

    public IEnumerator ControlYaw(string direction, int amount)
    {
        string url = $"{baseUrl}/yaw";
        var data = new ControlData { direction = direction, amount = amount };
        string jsonPayload = JsonUtility.ToJson(data);
        yield return SendPostRequest(url, jsonPayload);
    }

    public IEnumerator EmergencyStop()
    {
        string url = $"{baseUrl}/emergency_stop";
        yield return SendPostRequest(url, "{}");
    }

    private IEnumerator SendPostRequest(string url, string jsonPayload)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        
        if (!string.IsNullOrEmpty(jsonPayload))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }
        
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"Sending request to {url} with payload: {jsonPayload}");
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Success Response: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"Request failed: {request.error}");
            Debug.LogError($"URL: {url}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response body: {request.downloadHandler.text}");
        }

        request.Dispose();
    }
}