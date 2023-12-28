using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    AudioSource menuAudio;

    public Text scoreOutput;
    [SerializeField] GameObject scoreKeeper;

    [SerializeField] AdInitializer ads;
    [SerializeField] InterstitalAds interstitalAd;

    void Awake()
    {
        menuAudio = GetComponent<AudioSource>();

        if (!FindObjectOfType<ScoreKeeper>()) //instantiate scoreKeeper if there isn't one yet
            Instantiate(scoreKeeper);
        else
        {
            interstitalAd.LoadAd();
            interstitalAd.ShowAd();
        }
    }
    void Start()
    {
        Time.timeScale = 1;
        if (ScoreKeeper.score > 0)
            scoreOutput.text = "Score: " + ScoreKeeper.score.ToString();
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(DelayBeforeLoad(sceneName));
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
