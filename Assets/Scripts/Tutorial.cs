using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    Player player;
    public bool tutorialActive;
    [SerializeField] Text header;
    List<string> headerMessages = new List<string>();
    int currentStep;
    Coroutine exitTutorial;

    bool freeze;

    void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey("tutorialStage"))
        {
            PlayerPrefs.SetInt("tutorialStage", 1);
            PlayerPrefs.Save();
        }
    }

    public void SendTutorial()
    {
        if (player.dead) return;
        tutorialActive = true;
        Time.timeScale = 0;

        switch (PlayerPrefs.GetInt("tutorialStage"))
        {
            default:
                InstantExitTutorial();
                return;
            case 1: //movement and attack
                if (PlayerPrefs.GetInt("FirstRun") == 0) headerMessages.Add("Welcome to Fist Project!");
                else
                {
                    if (PlayerPrefs.GetInt("AutoMode") == 1)
                        headerMessages.Add("Auto Mode enabled!");
                    else 
                        headerMessages.Add("Manual Mode enabled!");
                }
                if (Application.isMobilePlatform)
                {
                    if (PlayerPrefs.GetInt("FirstRun") == 0) { headerMessages.Add("Use the left stick to move"); PlayerPrefs.SetInt("FirstRun", 1); }
                    if (PlayerPrefs.GetInt("AutoMode") == 0)
                    {
                        headerMessages.Add("Tap anywhere to attack");
                    }
                }
                else
                {
                    if (PlayerPrefs.GetInt("FirstRun") == 0) { headerMessages.Add("Move with WASD keys"); PlayerPrefs.SetInt("FirstRun", 1); }
                    if (PlayerPrefs.GetInt("AutoMode") == 0)
                    {
                        headerMessages.Add("Press space to attack");
                    }
                }
                break;
            case 2: //special
                headerMessages.Add("Your special attack is ready!");
                if (Application.isMobilePlatform)
                {
                    headerMessages.Add("Hold tap to send enemies flying");
                }
                else
                {
                    headerMessages.Add("Hold space to send enemies flying");
                }
                break;
        }
        header.text = headerMessages[0];

        StartCoroutine(Freeze(0.5f));

        PlayerPrefs.SetInt("tutorialStage", PlayerPrefs.GetInt("tutorialStage") + 1);
        PlayerPrefs.Save();
    }

    public void InstantExitTutorial()
    {
        tutorialActive = false;
        Time.timeScale = 1;
        headerMessages.Clear();
        currentStep = 0;
        gameObject.SetActive(false);
    }

    public void NextStep()
    {
        if (!freeze)
        {
            currentStep++;
            if (currentStep < headerMessages.Count) header.text = headerMessages[currentStep];
            else { if (exitTutorial != null) StopCoroutine(exitTutorial); exitTutorial = StartCoroutine(ExitTutorial()); }
        }
        StartCoroutine(Freeze(0.2f));
    }

    IEnumerator ExitTutorial()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        tutorialActive = false;
        Time.timeScale = 1;
        headerMessages.Clear();
        currentStep = 0;
        gameObject.SetActive(false);
    }

    IEnumerator Freeze(float time)
    {
        freeze = true;
        yield return new WaitForSecondsRealtime(time);
        freeze = false;
    }
}
