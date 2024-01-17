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
    [SerializeField] Image lockedStageOrCharacterBackground;
    [SerializeField] Image characterPreview;
    [SerializeField] public Text lockedStageText;
    [SerializeField] public Text lockedCharacterText;
    int currentKills;
    bool lockedStage;
    bool lockedCharacter;

    public Text scoreOutput;
    [SerializeField] GameObject scoreKeeper;

    [SerializeField] AdInitializer ads;
    [SerializeField] InterstitalAds interstitalAd;

    bool loadSceneSent;

    [Obsolete]
    void Awake()
    {
        PlayerPrefs.DeleteAll(); //for testing, resets all save data

        if (!PlayerPrefs.HasKey("totalKills")) InitiateSaveData();
        currentKills = PlayerPrefs.GetInt("totalKills");

        //if (Application.isMobilePlatform) interstitalAd.LoadAd(); //enable these 2 lines, and the one in adinitializer for ads
        Time.timeScale = 1;
        menuAudio = GetComponent<AudioSource>();
        if (ScoreKeeper.score > 0)
        {
            if (!Application.isMobilePlatform) StartCoroutine(SendScore(ScoreKeeper.score));
            if (ScoreKeeper.score > 2000) scoreOutput.text = ScoreKeeper.score.ToString() + "!";
            else scoreOutput.text = ScoreKeeper.score.ToString();

            startButton.text = "AGAIN";
        }
    }

    void InitiateSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("totalKills", 0);
        PlayerPrefs.SetInt("maxKillsInOneRun", 0);
        PlayerPrefs.SetInt("totalItemsGrabbed", 0);
        PlayerPrefs.SetInt("totalPunches", 0);
        PlayerPrefs.SetInt("totalSpecialAttacks", 0);
        PlayerPrefs.SetInt("highestScore", 0);
        PlayerPrefs.Save();
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

    public void LockedStage(bool locked)
    {
        lockedStage = locked;
        if (StageSelector.currentStage == 0) lockedStageOrCharacterBackground.color = new Color32(255, 255, 255, 20);
        else lockedStageOrCharacterBackground.color = new Color32(0, 0, 0, 200);

        if (locked) { lockedStageOrCharacterBackground.gameObject.SetActive(true); lockedStageText.text = "Defeat " + StageRequirements((StageSelector.currentStage)).ToString() + " more enemies"; }
        else if (!lockedCharacter) { lockedStageOrCharacterBackground.gameObject.SetActive(false); lockedStageText.text = ""; }
        else lockedStageText.text = "";
    }

    public void LockedCharacter(bool locked, string unlockText)
    {
        lockedCharacter = locked;
        if (StageSelector.currentStage == 0) lockedStageOrCharacterBackground.color = new Color32(255, 255, 255, 20);
        else lockedStageOrCharacterBackground.color = new Color32(0, 0, 0, 200);

        if (locked) {
            lockedStageOrCharacterBackground.gameObject.SetActive(true);
            lockedCharacterText.text = unlockText;
            characterPreview.color = new Color32(50, 50, 50, 255);
        }
        else if (!lockedStage) {
            lockedStageOrCharacterBackground.gameObject.SetActive(false);
            lockedCharacterText.text = unlockText;
            characterPreview.color = new Color32(255, 255, 255, 255);
        }
        else
        {
            lockedCharacterText.text = "";
            characterPreview.color = new Color32(255, 255, 255, 255);
        }
    }

    public int StageRequirements(int stageNum)
    {
        int currentKills = PlayerPrefs.GetInt("totalKills");
        switch (stageNum)
        {
            default: return 0; //includes first stage
            case 1: return 50 - currentKills;
            case 2: return 150 - currentKills;
            case 3: return 300 - currentKills;
            case 4: return 600 - currentKills;
            case 5: return 1000 - currentKills;
            case 6: return 1500 - currentKills;
            case 7: return 2200 - currentKills;
            case 8: return 3000 - currentKills;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
