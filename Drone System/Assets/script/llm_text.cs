using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class llm_text : MonoBehaviour
{
    private const string apiUrlGemini = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";
    private const string apiKeyGemini = "AIzaSyAN9qNx4GGERzFuF6NEAifaqdB9Ix7zoH4";

    // Public method to send request
    public void SendRequest(string customPrompt, System.Action<string> callback)
    {
        StartCoroutine(AskGemini(customPrompt, callback));
    }

    private IEnumerator AskGemini(string prompt, System.Action<string> callback)
    {
        string url = $"{apiUrlGemini}?key={apiKeyGemini}";

        // Create the JSON body using built-in classes
        var jsonBody = new GenerateContentRequest
        {
            contents = new[]
            {
                new Content
                {
                    parts = new[]
                    {
                        new Part { text = prompt }
                    }
                }
            }
        };

        string jsonString = JsonUtility.ToJson(jsonBody);
        Debug.Log("Sending JSON: " + jsonString); // Log the JSON to debug

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {webRequest.error}, Response: {webRequest.downloadHandler.text}");
                callback(null); // Return null in case of an error
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response JSON: " + jsonResponse); // Log the response for debugging
                string ideaText = ParseJsonResponse(jsonResponse);
                callback(ideaText); // Return the idea text via callback
            }
        }
    }

    private string ParseJsonResponse(string json)
    {
        GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(json);
        if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
        {
            Debug.Log(response.candidates[0].content.parts[0].text);
            return response.candidates[0].content.parts[0].text; // Return the extracted text
        }
        else
        {
            Debug.LogWarning("No valid text found in the response.");
            return null;
        }
    }

    // Classes for the request payload
    [System.Serializable]
    public class GenerateContentRequest
    {
        public Content[] contents;
    }

    [System.Serializable]
    public class Content
    {
        public Part[] parts;
    }

    [System.Serializable]
    public class Part
    {
        public string text;
    }

    // Classes for the response payload
    [System.Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;
    }

    [System.Serializable]
    public class Candidate
    {
        public Content content;
    }
}
