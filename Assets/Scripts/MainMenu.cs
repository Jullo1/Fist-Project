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
    [SerializeField] Button startButton;
    [SerializeField] Text startButtonText;
    public Image lockedStageBackground;
    public Image lockedCharacterMask;
    public Image lockedCharacterBackground;
    [SerializeField] Image characterPreview;
    [SerializeField] Text lockedStageText;
    [SerializeField] Text lockedCharacterText;

    public Text scoreOutput;
    [SerializeField] GameObject scoreKeeper;

    AdInitializer ads;
    InterstitalAds interstitalAd;

    bool loadSceneSent;

    [Obsolete]
    void Awake()
    {
        ads = FindObjectOfType<AdInitializer>();
        interstitalAd = FindObjectOfType<InterstitalAds>();
        menuAudio = GetComponent<AudioSource>();

        //InitiateSaveData(); //for testing, resets all save data
        if (!PlayerPrefs.HasKey("totalKills")) InitiateSaveData();

        if (!FindObjectOfType<ScoreKeeper>()) //instantiate scoreKeeper if there isn't one yet
        {
            Instantiate(scoreKeeper);
            if (Application.isMobilePlatform || Application.isEditor) interstitalAd.LoadAd(); //only load ad on first run
        }
        else if (Application.isMobilePlatform || Application.isEditor)
        {
            interstitalAd.ShowAd(); //send ad after death
            interstitalAd.LoadAd(); //then get ad ready for next run
        }

        Time.timeScale = 1;
        if (ScoreKeeper.score > 0)
        {
            if (!Application.isMobilePlatform) StartCoroutine(SendScore(ScoreKeeper.score));
            if (ScoreKeeper.score > 2000) scoreOutput.text = ScoreKeeper.score.ToString() + "!";
            else scoreOutput.text = ScoreKeeper.score.ToString();

            startButtonText.text = "AGAIN";
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
        startButton.GetComponentInChildren<Outline>().enabled = true;
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
        if (StageSelector.currentStage == 0) lockedStageBackground.color = new Color32(255, 255, 255, 20);
        else lockedStageBackground.color = new Color32(0, 0, 0, 200);

        if (locked) { lockedStageBackground.gameObject.SetActive(true); lockedStageText.text = "Defeat " + StageRequirements((StageSelector.currentStage)).ToString() + " more enemies"; }
        else { lockedStageBackground.gameObject.SetActive(false); lockedStageText.text = ""; }
    }

    public void LockedCharacter(bool locked, string unlockText)
    {
        if (locked) {
            lockedCharacterMask.gameObject.SetActive(true);
            lockedCharacterBackground.gameObject.SetActive(true);
            lockedCharacterText.text = unlockText;
            characterPreview.color = new Color32(50, 50, 50, 255);
        }
        else
        {
            lockedCharacterMask.gameObject.SetActive(false);
            lockedCharacterBackground.gameObject.SetActive(false);
            lockedCharacterText.text = unlockText;
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
            case 2: return 250 - currentKills;
            case 3: return 500 - currentKills;
            case 4: return 1000 - currentKills;
            case 5: return 1500 - currentKills;
            case 6: return 2000 - currentKills;
            case 7: return 3000 - currentKills;
            case 8: return 4500 - currentKills;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
