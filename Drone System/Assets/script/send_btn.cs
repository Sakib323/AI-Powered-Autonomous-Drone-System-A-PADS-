using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class send_btn : MonoBehaviour
{
    public TMP_InputField tmpInputField;
    private DroneController droneController;
    private string[] words;
    private string inputText;

    void Start()
    {

        string exm = "/drone /pitch forward 30";

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void send_msg()
    {
        inputText = tmpInputField.text;
        Debug.Log(inputText);


        words = inputText.Split(' ');

        if (inputText.Contains("yaw"))
        {
            Debug.Log("yaw");
            yaw();
        }
        if (inputText.Contains("pitch"))
        {
            Debug.Log("pitch");
            // Add pitch control logic here
        }        
        if (inputText.Contains("roll"))
        {
            Debug.Log("roll");
            // Add roll control logic here
        }
        if (inputText.Contains("throttle"))
        {
            Debug.Log("throttle");
            // Add throttle control logic here
        }
        if (inputText.Contains("stop"))
        {
            Debug.Log("stop");
            // Add stop control logic here
        }
        tmpInputField.text = "";
        inputText="";
    }
    
    private void yaw(){
        droneController.ControlYaw(words.Length > 2 ? words[2] : "", words.Length > 3 ? int.Parse(words[3]) : 0);
    }
}
