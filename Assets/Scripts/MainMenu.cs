using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading;
using System;

public class MainMenu : MonoBehaviour
{
    AudioSource menuAudio;
    [SerializeField] AudioSource menuMusic;
    [SerializeField] Text startButton;

    public Text scoreOutput;
    [SerializeField] GameObject scoreKeeper;

    [SerializeField] AdInitializer ads;
    [SerializeField] InterstitalAds interstitalAd;

    bool loadSceneSent;

    [Obsolete]
    void Awake()
    {
        //if (Application.isMobilePlatform) interstitalAd.LoadAd(); //enable these 2 lines, and the one in adinitializer for ads
        Time.timeScale = 1;
        menuAudio = GetComponent<AudioSource>();
        if (ScoreKeeper.score > 0)
        {
            if (!Application.isMobilePlatform) StartCoroutine(SendScore(ScoreKeeper.score));
            if (ScoreKeeper.score > 2000)
                scoreOutput.text = ScoreKeeper.score.ToString() + "!";
            else
                scoreOutput.text = ScoreKeeper.score.ToString();

            startButton.text = "AGAIN";
        }
    }

    void Start()
    {
        if (!FindObjectOfType<ScoreKeeper>()) //instantiate scoreKeeper if there isn't one yet
            Instantiate(scoreKeeper);
        /*else
            if (Application.isMobilePlatform) interstitalAd.ShowAd();*/ //enable these 2 lines, and the one in adinitializer for ads
    }

    public void ResetTutorial()
    {
        PlayerPrefs.SetInt("tutorialStage", 1);
        PlayerPrefs.Save();
    }

    [Obsolete]
    IEnumerator SendScore(int value)
    {
        WWWForm form = new WWWForm();
        form.AddField("game", "Fist");
        form.AddField("score", value);

        WWW www = new WWW("https://julianlerej.com/app/views/sendScore.php", form);
        yield return www;
    }

    void LoadScene(string sceneName)
    {
        menuAudio.Play();
        if (!loadSceneSent) StartCoroutine(DelayBeforeLoad(sceneName));
        loadSceneSent = true;
    }

    public void StartGame()
    {
        LoadScene(StageSelector.currentStage.ToString());
    }

    IEnumerator DelayBeforeLoad(string sceneName)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = false;
        yield return new WaitForSeconds(1f);
        GameManager.audioProgress = menuMusic.time;
        asyncOp.allowSceneActivation = true;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
