using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public int currentLevel;

    private bool alreadySavedFlag;

    public TMP_Text levelText;

    private void Start()
    {
        LoadProgress();

        if(levelText != null)
        {
            if(currentLevel<=10)
            {
                levelText.text = "Level " + currentLevel.ToString();
            }
            else
            {
                levelText.text = "Finished";
            }

        }
    }

    public void LoadProgress()
    {
        // avoid null value
        currentLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
    }

    public void CompleteLevel()
    {
        // increase the level and save it
        if (!alreadySavedFlag)
        {
            currentLevel++;
            SaveProgress();
        }

        alreadySavedFlag = true;
    }

    public void SaveProgress()
    {
        // store the progress
        PlayerPrefs.SetInt("PlayerLevel", currentLevel);

        // to save to the disk
        PlayerPrefs.Save();
    }
    public void LoadLevel()
    {
        // load the level based
        SceneManager.LoadScene("Level " + currentLevel);
        resetAlreadySavedFlagFlag();
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    private void resetAlreadySavedFlagFlag()
    {
        alreadySavedFlag = false;

    }

    // for testing
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("PlayerLevel");
        currentLevel = 1;  
        PlayerPrefs.Save();  
    }

    // reload the current level
    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  
    }

}
