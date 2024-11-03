using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json; // Add this line

public class ImageCaptionGenerator : MonoBehaviour
{
    private static readonly string apiKey = "AIzaSyAN9qNx4GGERzFuF6NEAifaqdB9Ix7zoH4"; // Replace with your actual API key

    public async Task<string> GenerateCaption(byte[] imageBytes)
    {
        string caption = await GetImageCaption(apiKey, imageBytes);
        Debug.Log("Image Caption: " + caption);
        return caption;
    }

    private async Task<string> GetImageCaption(string apiKey, byte[] imageBytes)
    {
        string url = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key={apiKey}";

        string mimeType = "image/png"; // Assuming the image is PNG
        string encodedImage = Convert.ToBase64String(imageBytes);

        if (string.IsNullOrEmpty(encodedImage))
        {
            return "Error reading image";
        }

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = "Generate a detailed caption for this image. Describe what you see in detail." },
                        new { inline_data = new { mime_type = mimeType, data = encodedImage } }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                topK = 32,
                topP = 1,
                maxOutputTokens = 4096
            },
            safetySettings = new[] 
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
            }
        };

        try
        {
            using (HttpClient client = new HttpClient())
            {
                // Remove the Content-Type header here
                string jsonPayload = JsonConvert.SerializeObject(payload);
                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);

                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log("Response Status Code: " + response.StatusCode);
                Debug.Log("Response Text: " + responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    return $"API request error: {response.ReasonPhrase}";
                }

                var jsonResponse = JsonConvert.DeserializeObject<JsonResponse>(responseBody);
                var candidates = jsonResponse.Candidates;

                if (candidates.Length > 0 && candidates[0].Content != null)
                {
                    var parts = candidates[0].Content.Parts;
                    if (parts.Length > 0)
                    {
                        return parts[0].Text;
                    }
                }
                return "Could not extract caption from API response";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
            return $"Unexpected error: {e.Message}";
        }
    }

    private class JsonResponse
    {
        public Candidate[] Candidates { get; set; }
    }

    private class Candidate
    {
        public Content Content { get; set; }
    }

    private class Content
    {
        public Part[] Parts { get; set; }
    }

    private class Part
    {
        public string Text { get; set; }
    }
}
