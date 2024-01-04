using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    Player player;
    public bool tutorialActive;
    [SerializeField] Text header;
    List<string> headerMessages = new List<string>();
    int currentStep;
    Coroutine exitTutorial;

    void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    public void SendTutorial(int tutorialNumber = 0)
    {
        if (player.dead) return;
        tutorialActive = true;

        int num;
        if (tutorialNumber == 0)
        {
            ScoreKeeper.currentTutorialNumber++;
            num = ScoreKeeper.currentTutorialNumber;
        } else num = tutorialNumber; //send tutorial number in parameters if specified when calling this function, for example SendTutorial(3) will send tutorial 3
            
        Time.timeScale = 0;
        switch (num)
        {
            default:
                tutorialActive = false;
                Time.timeScale = 1;
                headerMessages.Clear();
                currentStep = 0;
                gameObject.SetActive(false);
                break;
            case 1: //attack
                headerMessages.Add("Welcome to Fist Project");
                if (Application.isMobilePlatform) { headerMessages.Add("Move with the left stick"); headerMessages.Add("Tap anywhere else to attack"); }
                else { headerMessages.Add("Move with WASD"); headerMessages.Add("Press space to attack"); }
                break;
            case 2: //combo
                headerMessages.Add("Your special attack is ready!");
                headerMessages.Add("Hold attack to send enemies flying");
                break;
        }
        header.text = headerMessages[0];
    }

    public void NextStep()
    {
        currentStep++;
        if (currentStep < headerMessages.Count) header.text = headerMessages[currentStep];
        else { if (exitTutorial != null) StopCoroutine(exitTutorial); exitTutorial = StartCoroutine(ExitTutorial()); }
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
}
