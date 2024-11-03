using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManageApiUrl : MonoBehaviour
{
    [SerializeField] private TMP_InputField urlInputField;
    [SerializeField] public string apiUrl;
    [SerializeField] private GameObject sidebar;
    [SerializeField] private GameObject Joystick;
    [SerializeField] private GameObject urlfield;

    [SerializeField] private GameObject headlinePrefab;
    [SerializeField] private GameObject LLM_RESPONCE; // Ensure this prefab is set correctly in the Inspector
    [SerializeField] private Transform scrollViewContent;

    private NewsScraper newsScraper;
    private llm_text LLM_TEXT;

    void Start()
    {
        urlInputField.text = string.Empty;
        newsScraper = FindObjectOfType<NewsScraper>();
        LLM_TEXT = FindObjectOfType<llm_text>();

        if (newsScraper == null)
        {
            Debug.LogError("NewsScraper not found in the scene.");
        }

        if (LLM_TEXT == null)
        {
            Debug.LogError("LLM_TEXT not found in the scene.");
        }
    }

    public void OnButtonClick()
    {
        apiUrl = urlInputField.text;
        Debug.Log($"Collected API URL: {apiUrl}");
        StartCoroutine(ScrapeInternetPeriodically());
    }

    public void ToggleSidebar()
    {
        if (sidebar != null && !string.IsNullOrEmpty(urlInputField.text))
        {
            if (!sidebar.activeSelf && !Joystick.activeSelf)
            {
                sidebar.SetActive(true);
                Joystick.SetActive(true);
                urlfield.SetActive(false);
                Debug.Log("Sidebar & Joystick are now active.");
            }
        }
        else
        {
            Debug.LogWarning("Sidebar GameObject is not assigned or URL input is empty.");
        }
    }

    private IEnumerator ScrapeInternetPeriodically()
    {
        while (true)
        {
            Debug.Log("Scraping internet for crucial data...");
            ScrapeInternet();
            yield return new WaitForSeconds(1800);
        }
    }

    private void ScrapeInternet()
    {
        newsScraper.GetNewsHeadlines(headlines =>
        {
            if (headlines != null && headlines.Length > 0)
            {
                // Clear previous headlines
                foreach (Transform child in scrollViewContent)
                {
                    Destroy(child.gameObject);
                }

                // Instantiate the headline prefab
                GameObject headlineObject = Instantiate(headlinePrefab, scrollViewContent);
                Transform textTransform = headlineObject.transform.Find("Text");

                // Ensure the textTransform is not null before accessing
                if (textTransform != null)
                {
                    TextMeshProUGUI textMeshPro = textTransform.GetComponent<TextMeshProUGUI>();
                    if (textMeshPro != null)
                    {
                        // Set the text for headlines
                        textMeshPro.text = string.Join("\n", headlines);
                    }
                    else
                    {
                        Debug.LogError("TextMeshProUGUI component not found on the child 'Text'.");
                    }
                }
                else
                {
                    Debug.LogError("Child named 'Text' not found in the instantiated headline prefab.");
                }

                string headlinesString = string.Join("\n", headlines);
                LLM_TEXT.SendRequest($"From this set of news headlines for a region:\n{headlinesString}\ndetermine where to send a drone for aid like flood, civil war etc. If there is no situation in the headline like this then say no need for drone aid. but if it contains any situation then just write the headline. You can only choose one headline. and don't assume something from the headline only. If the headline explicitly says something only then you can be sure about the aid.", response =>
                {
                    if (!string.IsNullOrEmpty(response))
                    {
                        Debug.Log("Response from LLM: " + response);

                        // Instantiate the LLM response prefab
                        GameObject llmResponsePrefab = Instantiate(LLM_RESPONCE, scrollViewContent);
                        Transform textTransformLLM = llmResponsePrefab.transform.Find("Text");

                        // Ensure the textTransformLLM is not null before accessing
                        if (textTransformLLM != null)
                        {
                            TextMeshProUGUI textLLM = textTransformLLM.GetComponent<TextMeshProUGUI>();
                            if (textLLM != null)
                            {
                                textLLM.text = response;
                            }
                            else
                            {
                                Debug.LogError("TextMeshProUGUI component not found on the child 'Text' for LLM response.");
                            }
                        }
                        else
                        {
                            Debug.LogError("Child named 'Text' not found in the instantiated LLM response prefab.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No response received from LLM.");
                    }
                });
            }
            else
            {
                Debug.LogWarning("No headlines were retrieved.");
            }
        });
    }
}
