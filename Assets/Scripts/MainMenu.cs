using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    AudioSource menuAudio;

    [SerializeField] Text startButton;

    public Text scoreOutput;
    [SerializeField] GameObject scoreKeeper;

    [SerializeField] AdInitializer ads;
    [SerializeField] InterstitalAds interstitalAd;

    Coroutine loadScene;
    bool loadSceneSent;

    void Awake()
    {
        menuAudio = GetComponent<AudioSource>();
        interstitalAd.LoadAd();

        if (!FindObjectOfType<ScoreKeeper>()) //instantiate scoreKeeper if there isn't one yet
            Instantiate(scoreKeeper);
        else
            interstitalAd.ShowAd();
    }
    void Start()
    {
        Time.timeScale = 1;
        if (ScoreKeeper.score > 0)
        {
            if (ScoreKeeper.score > 1000)
                scoreOutput.text = ScoreKeeper.score.ToString() + "!";
            else
                scoreOutput.text = ScoreKeeper.score.ToString();

            startButton.text = "AGAIN";
        }
    }

    public void LoadScene(string sceneName)
    {
        if (loadSceneSent) StopCoroutine(loadScene);
        loadScene = StartCoroutine(DelayBeforeLoad(sceneName));
        loadSceneSent = true;
    }

    IEnumerator DelayBeforeLoad(string sceneName)
    {
        menuAudio.Play();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
