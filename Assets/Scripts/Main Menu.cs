using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Text scoreOutput;
    [SerializeField] GameObject scoreKeeper;

    void Awake()
    {
        if (!FindObjectOfType<ScoreKeeper>()) //instantiate scoreKeeper if there isn't one yet
            Instantiate(scoreKeeper);
    }
    void Start()
    {
        if (ScoreKeeper.score > 0)
            scoreOutput.text = "Score: " + ScoreKeeper.score.ToString();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
