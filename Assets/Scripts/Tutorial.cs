using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Text header;
    [SerializeField] Text footer;
    List<string> headerMessages = new List<string>();
    int currentStep;

    void Start()
    {
        if (Application.isMobilePlatform)
            footer.text = "Tap to continue";
        else if (Application.isConsolePlatform)
            footer.text = "Press any button";
        else
            footer.text = "Press any key";
    }

    public void SendTutorial(int tutorialNumber = 0)
    {
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
                ExitTutorial();
                break;
            case 1: //attack
                headerMessages.Add("Welcome to Fist Project");
                if (Application.isMobilePlatform) { headerMessages.Add("Move with the left stick"); headerMessages.Add("Tap anywhere else to attack"); }
                else { headerMessages.Add("Move with WASD"); headerMessages.Add("Press space to attack"); }
                break;
            case 2: //combo
                headerMessages.Add("Your special attack is ready!");
                if (Application.isMobilePlatform) headerMessages.Add("Hold tap for special");
                else headerMessages.Add("Hold space for special");
                break;
        }
        header.text = headerMessages[0];
    }

    public void NextStep()
    {
        currentStep++;
        if (currentStep < headerMessages.Count) header.text = headerMessages[currentStep];
        else ExitTutorial();
    }

    void ExitTutorial()
    {
        Time.timeScale = 1;
        headerMessages.Clear();
        currentStep = 0;
        gameObject.SetActive(false);
    }
}
