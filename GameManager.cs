using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;
    public EventSystem eventSystem;
    private PlayerController playerController;
    public GameObject firstSelected;
    

    //the language is "en" for englisch and "de" for german
    public string language = "en";
    public GameObject iconEnglish;
    public GameObject iconGerman;


    private void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }
    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            PauseGame();
        }
    }

    public void LeaveGame()
    {
        //INSERT: This ends the game and moves back to the Main menu
        Debug.Log("End game");
    }

    public void PauseGame()
    {
            if (Time.timeScale == 1)
            {
                pausePanel.SetActive(true);
                playerController.playerFrozen = true;
                Time.timeScale = 0;
                eventSystem.SetSelectedGameObject(firstSelected);
            }
            else
            {
                pausePanel.SetActive(false);
                playerController.playerFrozen = false;
                Time.timeScale = 1;
            }
            //Debug.Log("Pause-Button pressed.");
    }

    public void LoadGame()
    {
        string sceneToLoad = "SampleScene2";
        StartCoroutine(LoadSpecificScene(sceneToLoad));
    }

    public void LoadMainMenu()
    {
        string sceneToLoad = "MainMenu";
        StartCoroutine(LoadSpecificScene(sceneToLoad));
    }

    IEnumerator LoadSpecificScene(string sceneToLoad)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        
        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void SwitchLanguage()
    {
        if(language == "en")
        {
            language = "de";
            iconEnglish.SetActive(false);
            iconGerman.SetActive(true);
            SavePlayerPrefs();
        }
        else
        {
            language = "en";
            iconEnglish.SetActive(true);
            iconGerman.SetActive(false);
            SavePlayerPrefs();
        }
        //Debug.Log("language switched to " + language);
    }

    void SavePlayerPrefs()
    {
        PlayerPrefs.SetString("SavedLanguage", language);
        PlayerPrefs.Save();
        //Debug.Log("PlayerPrefs saved");
    }

    void LoadPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("SavedLanguage"))
        {
            language = PlayerPrefs.GetString("SavedLanguage");
        }
        else
        {
            Debug.Log("No PlayerPrefs saved");
        }

    }

    private void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        language = "en";
        Debug.Log("PlayerPrefs back to standard");
    }
}
