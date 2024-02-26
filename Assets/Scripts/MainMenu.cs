using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
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
    [SerializeField] GameObject reviewWindow;

    InterstitalAds interstitalAd;
    BannerAds bannerAds;

    bool loadSceneSent;

    void Awake()
    {
        bannerAds = FindObjectOfType<BannerAds>();
        interstitalAd = FindObjectOfType<InterstitalAds>();
        menuAudio = GetComponent<AudioSource>();

        //InitiateSaveData(); //for testing, resets all save data
        //InitiateCheatSaveData(); //unlock everything
        RebuildSaveData();
        LockedStage(false);

        if (!FindObjectOfType<ScoreKeeper>()) //instantiate scoreKeeper if there isn't one yet
        {
            Instantiate(scoreKeeper);
            if (Application.isMobilePlatform || Application.isEditor) { interstitalAd.LoadAd(); }//only load ad on first run
        }
        else if (Application.isMobilePlatform || Application.isEditor)
        {
            interstitalAd.ShowAd(); //send ad after death
            interstitalAd.LoadAd(); //then get ad ready for next run
            bannerAds.ShowBannerAd();
        }

        if (PlayerPrefs.GetInt("totalKills") > 300 && !PlayerPrefs.HasKey("optOutReview"))
        {
            reviewWindow.SetActive(true);
            PlayerPrefs.SetInt("optOutReview", 1);
            PlayerPrefs.Save();
        }
    }

    [Obsolete]
    void Start()
    {
        Time.timeScale = 1;
        if (ScoreKeeper.score > 0)
        {
            if (!Application.isMobilePlatform) StartCoroutine(SendScore(ScoreKeeper.score));
            startButtonText.text = "AGAIN";
        }
        StartCoroutine(ShowBannerAdsWithDelay());
    }

    IEnumerator ShowBannerAdsWithDelay()
    {
        startButton.interactable = false;
        yield return new WaitForSeconds(1);

        bannerAds.LoadBanner();
        bannerAds.ShowBannerAd();
        startButton.interactable = true;
    }

    void RebuildSaveData()
    {
        if (!PlayerPrefs.HasKey("totalKills")) PlayerPrefs.SetInt("totalKills", 0);
        if (!PlayerPrefs.HasKey("maxKillsInOneRun")) PlayerPrefs.SetInt("maxKillsInOneRun", 0);
        if (!PlayerPrefs.HasKey("totalItemsGrabbed")) PlayerPrefs.SetInt("totalItemsGrabbed", 0);
        if (!PlayerPrefs.HasKey("totalPunches")) PlayerPrefs.SetInt("totalPunches", 0);
        if (!PlayerPrefs.HasKey("totalSpecialAttacks")) PlayerPrefs.SetInt("totalSpecialAttacks", 0);
        if (!PlayerPrefs.HasKey("highestScore")) PlayerPrefs.SetInt("highestScore", 0);

        string temp;
        for (int i = 0; i < 9; i++)
        {
            temp = "hiscore" + i.ToString();
            if (!PlayerPrefs.HasKey(temp)) PlayerPrefs.SetInt(temp, 0);
        }
        PlayerPrefs.Save();
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

        for (int i = 0; i < 9; i++)
            PlayerPrefs.SetInt("hiscore" + i.ToString(), 0);

        PlayerPrefs.Save();
    }

    void InitiateCheatSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("totalKills", 4368);
        PlayerPrefs.SetInt("maxKillsInOneRun", 152);
        PlayerPrefs.SetInt("totalItemsGrabbed", 321);
        PlayerPrefs.SetInt("totalPunches", 7843);
        PlayerPrefs.SetInt("totalSpecialAttacks", 875);
        PlayerPrefs.SetInt("highestScore", 2815);
        PlayerPrefs.Save();
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
        bannerAds.HideBannerAd();
        PlayerPrefs.SetInt("selectedStage", StageSelector.currentStage);
        PlayerPrefs.SetInt("selectedSkin", SkinSelector.currentSkin);
        PlayerPrefs.Save();

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

        string temp = "hiscore" + StageSelector.currentStage.ToString();
        if (locked) { lockedStageBackground.gameObject.SetActive(true); lockedStageText.text = "Defeat " + StageRequirements((StageSelector.currentStage)).ToString() + " more enemies"; }
        else { lockedStageBackground.gameObject.SetActive(false); if (!PlayerPrefs.HasKey(temp)) { lockedStageText.text = ""; return; } else if (PlayerPrefs.GetInt(temp) == 0) lockedStageText.text = ""; else lockedStageText.text = "Best Score: " + (PlayerPrefs.GetInt(temp)).ToString(); }
    }

    public void LockedCharacter(bool locked, string unlockText)
    {
        if (locked)
        {
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
            case 2: return 150 - currentKills;
            case 3: return 300 - currentKills;
            case 4: return 500 - currentKills;
            case 5: return 750 - currentKills;
            case 6: return 1250 - currentKills;
            case 7: return 2000 - currentKills;
            case 8: return 3000 - currentKills;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void RateGame()
    {
        Application.OpenURL("market://details?id=com.Jullo.FistProject");
        if (reviewWindow) reviewWindow.SetActive(false);
    }

    public void CloseReviewWindow()
    {
        reviewWindow.SetActive(false);
    }

    public void OpenWebsite(string tab)
    {

        Application.OpenURL("https://julianlerej.com/" + tab);
    }
}
