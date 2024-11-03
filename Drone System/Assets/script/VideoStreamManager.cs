using UnityEngine;
using UnityEngine.Video;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;


public class VideoStreamManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public ManageApiUrl ApiUrl;
    private string baseUrl;
    public string demo_mp4 = "https://images.all-free-download.com/footage_preview/mp4/natural_calm_lake_at_dusk_6891961.mp4";
    private readonly HttpClient httpClient = new HttpClient();
    
    float lastYPosition = -215f;


    private const string apiKeyGemini = "AIzaSyAN9qNx4GGERzFuF6NEAifaqdB9Ix7zoH4";
    private const string geminiEndpoint = "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent";
    private ImageCaptionGenerator captionGenerator;
    private string currentCaption;

    [SerializeField] private GameObject image_and_text;

    [SerializeField] private Transform scrollViewContent;

    private void Start()
    {
        captionGenerator = gameObject.AddComponent<ImageCaptionGenerator>();

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        baseUrl = ApiUrl.apiUrl + "/stream/new.mp4";
        Debug.Log($"Starting video playback from URL: {baseUrl}");
        StartCoroutine(PlayVideo());
    }

    private IEnumerator PlayVideo()
    {
        while (true)
        {
            videoPlayer.url = baseUrl;
            videoPlayer.Play();

            yield return new WaitForSeconds(1f);
            Debug.Log("Checking if video is playing...");

            if (!videoPlayer.isPlaying)
            {
                Debug.LogError("Video failed to play. Retrying in 10 seconds...");
                yield return new WaitForSeconds(10f);
                continue;
            }

            Debug.Log("Video is playing: " + videoPlayer.isPlaying);
            float elapsedTime = 0f;
            const float captureInterval = 10f;

            while (videoPlayer.isPlaying)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= captureInterval)
                {
                    yield return new WaitForEndOfFrame();

                    if (videoPlayer.targetTexture == null)
                    {
                        Debug.LogError("Target texture is not assigned to VideoPlayer!");
                        yield break;
                    }

                    Texture2D texture2D = new Texture2D(videoPlayer.targetTexture.width, videoPlayer.targetTexture.height, TextureFormat.RGB24, false);

                    RenderTexture.active = videoPlayer.targetTexture;
                    texture2D.ReadPixels(new Rect(0, 0, videoPlayer.targetTexture.width, videoPlayer.targetTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = null;

                    if (texture2D != null)
                    {
                            Debug.Log($"Captured frame: {texture2D.width}x{texture2D.height}");
                            byte[] imageBytes = texture2D.EncodeToPNG();

                            StartCoroutine(GenerateCaptionCoroutine(imageBytes, (caption) => {

                                currentCaption = caption;

                                GameObject headlineObject = Instantiate(image_and_text, scrollViewContent);
                                RectTransform rectTransform = headlineObject.GetComponent<RectTransform>();

                                // Position the new prefab with a -75 unit offset on the Y-axis from the previous one
                                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, lastYPosition);
                                lastYPosition -= 100; // Decrease the position for the next prefab


                                if (headlineObject == null)
                                {
                                    Debug.LogError("Failed to instantiate image_and_text prefab.");
                                    return;
                                }

                                Transform textTransform = headlineObject.transform.Find("Text");

                                if (textTransform == null)
                                {
                                    Debug.LogError("Child named 'Text' not found in the instantiated headline prefab.");
                                    return;
                                }
                                TextMeshProUGUI textMeshPro = textTransform.GetComponent<TextMeshProUGUI>();
                                if (textMeshPro == null)
                                {
                                    Debug.LogError("TextMeshProUGUI component not found on the child 'Text'.");
                                    return;
                                }
                                textMeshPro.text = caption;


                            }));

                            Destroy(texture2D);


                    }
                    else
                    {
                        Debug.LogError("Failed to create texture2D.");
                    }

                    elapsedTime = 0f;
                }

                yield return null;
            }

            Debug.Log("Video has finished playing. Restarting...");
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator GenerateCaptionCoroutine(byte[] imageBytes, Action<string> onCaptionGenerated)
    {
        var task = captionGenerator.GenerateCaption(imageBytes);
        
        while (!task.IsCompleted)
        {
            yield return null;
        }

        try
        {
            string caption = task.Result;
            onCaptionGenerated?.Invoke(caption);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error generating caption: " + e.Message);
            onCaptionGenerated?.Invoke("Error: " + e.Message);
        }
    }

    // Method to get the current caption if needed from other scripts
    public string GetCurrentCaption()
    {
        return currentCaption;
    }
}