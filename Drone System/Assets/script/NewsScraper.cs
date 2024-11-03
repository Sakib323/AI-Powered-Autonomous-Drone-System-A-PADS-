using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NewsScraper : MonoBehaviour
{
    private const string apiKey = "b89c9ad002d64e9ab11d73c094aa7610"; 
    private const string apiUrl = "https://newsapi.org/v2/everything?q=Sylhet&apiKey=";

    // Public method to fetch news headlines
    public void GetNewsHeadlines(System.Action<string[]> callback)
    {
        StartCoroutine(ScrapeNews(callback));
    }

    private IEnumerator ScrapeNews(System.Action<string[]> callback)
    {
        string url = apiUrl + apiKey;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + webRequest.error);
                callback(null); // Return null in case of an error
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                string[] headlines = ParseJson(jsonResponse);
                callback(headlines); // Return headlines via callback
            }
        }
    }

    private string[] ParseJson(string json)
    {
        NewsApiResponse response = JsonUtility.FromJson<NewsApiResponse>(json);
        if (response.articles != null && response.articles.Length > 0)
        {
            string[] headlines = new string[response.articles.Length];

            for (int i = 0; i < response.articles.Length; i++)
            {
                headlines[i] = response.articles[i].title;
                Debug.Log(headlines[i]); // Output the headline to the console
            }

            return headlines; // Return the headlines array
        }
        else
        {
            Debug.LogWarning("No headlines found!");
            return new string[0]; // Return an empty array instead of null
        }
    }
}

[System.Serializable]
public class NewsApiResponse
{
    public Article[] articles; // Use an array instead of List
}

[System.Serializable]
public class Article
{
    public string title;
}
