using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
#if UNITY_ANDROID
using Google.Play.Review;
#endif
using Unity.Services.Core;
using Unity.Services.Analytics;
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
    [SerializeField] GameObject marketTab;
    [SerializeField] Text autoModeText;

    public Text scoreOutput;
    [SerializeField] GameObject scoreKeeper;
    [SerializeField] GameObject economySystemPrefab;
    [SerializeField] GameObject rewardedAdButton;
    Text coinsOutput;
    EconomySystem economySystem;
    public GameObject lootGoldCoin;
#if UNITY_ANDROID
    ReviewManager reviewManager;
    PlayReviewInfo playReviewInfo;
#endif
    bool loadSceneSent;
    bool loadingRewardedAd;

    [Obsolete]
    async void Start()
    {
        //ads
#if UNITY_ANDROID
        string appKey = "1d5d6bddd";
#elif UNITY_IPHONE
        string appKey = "1d5d6bddd";
#else
        string appKey = "unexpected_platform";
#endif

        menuAudio = GetComponent<AudioSource>();
        coinsOutput = GameObject.FindGameObjectWithTag("CoinOutput").GetComponent<Text>();
        if (Application.internetReachability == NetworkReachability.NotReachable) coinsOutput.text = "Offline"; //offline mode, will not gather coins

        //InitiateSaveData(); //for testing, resets all save data
        //InitiateCheatSaveData(); //unlock everything

        LockedStage(false);
        Time.timeScale = 1;

        //first run, initialize all functionality elements
        if (!FindObjectOfType<ScoreKeeper>())
        {
            RebuildSaveData();
            Instantiate(scoreKeeper);
            Instantiate(economySystemPrefab);
#if UNITY_ANDROID
            IronSource.Agent.setConsent(true);
            IronSource.Agent.setMetaData("do_not_sell", "false");
            IronSource.Agent.setMetaData("is_child_directed", "false");
            IronSource.Agent.setManualLoadRewardedVideo(true);
            IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
            IronSource.Agent.shouldTrackNetworkState(true);
            IronSource.Agent.validateIntegration();
            IronSourceAdQuality.Initialize(appKey);
            if (Application.isMobilePlatform || Application.isEditor) StartCoroutine(LoadAds());
#endif
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
        }
        //returned to main menu
        else
        {
#if UNITY_ANDROID
            IronSource.Agent.displayBanner();
            if (!ScoreKeeper.adPlayed)
            {
                IronSource.Agent.showInterstitial(); //send ad after death
                ScoreKeeper.adPlayed = true;
            } else ScoreKeeper.adPlayed = false;
#endif
            coinsOutput.text = EconomySystem.balance.ToString(); //starts UI with previous balance unless it's 0
            FindObjectOfType<EconomySystem>().LoadInventory();
        }

        //webgl sendscore
        if (ScoreKeeper.score > 0)
        {
#if UNITY_WEBGL
            StartCoroutine(SendScore(ScoreKeeper.score)); //sending score for webgl build
#endif
            startButtonText.text = "AGAIN";
        }

#if UNITY_ANDROID
        //android review
        if (PlayerPrefs.GetInt("totalKills") > 300 && ScoreKeeper.score > 0) 
        {
            if (PlayerPrefs.GetInt("reviewCount") >= 5 && PlayerPrefs.GetInt("optedOutReview") < 5)
            {
                StartCoroutine(ReviewTab());
                if (PlayerPrefs.GetInt("optedOutReview") < 3) PlayerPrefs.SetInt("reviewCount", 0);
                else PlayerPrefs.SetInt("reviewCount", -30);  //after showing review tab 3 times, it will appear again after a long time

                PlayerPrefs.SetInt("optedOutReview", PlayerPrefs.GetInt("optedOutReview") + 1);
            }
            else PlayerPrefs.SetInt("reviewCount", PlayerPrefs.GetInt("reviewCount") + 1);

            PlayerPrefs.Save();
        }
#endif

        economySystem = FindObjectOfType<EconomySystem>();
        StartCoroutine(AddCoins(ScoreKeeper.coins));
        StartCoroutine(CheckRewarded()); //if reward ad is ready, enable button

        RefreshUITexts();
    }

    public IEnumerator CheckRewarded()
    {
        yield return new WaitForSeconds(0.5f);
#if UNITY_ANDROID
        IronSource.Agent.loadRewardedVideo();
        while (!IronSource.Agent.isRewardedVideoAvailable()) yield return new WaitForSeconds(5f);

        if (!IronSource.Agent.isRewardedVideoPlacementCapped("Main_Menu")) ShowRewardedAdIcon(true);
#endif
    }

    void RefreshUITexts()
    {
        if (PlayerPrefs.GetInt("AutoMode") == 1)
            autoModeText.text = "Auto Mode";
        else if (PlayerPrefs.GetInt("AutoMode") == 0)
            autoModeText.text = "Manual Mode";
    }
    public void ToggleAutoMode()
    {
        ResetTutorial();
        int isAutoMode = PlayerPrefs.GetInt("AutoMode");
        if (isAutoMode == 1)
        {
            PlayerPrefs.SetInt("AutoMode", 0);
            autoModeText.text = "Manual Mode";
            PlayerPrefs.Save();
        }
        else
        {
            PlayerPrefs.SetInt("AutoMode", 1);
            autoModeText.text = "Auto Mode";
            PlayerPrefs.Save();
        }
    }

    IEnumerator LoadAds()
    {
        yield return new WaitForSeconds(0.5f);

#if UNITY_ANDROID
        //load all ads
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
        IronSource.Agent.loadInterstitial();
        IronSource.Agent.loadRewardedVideo();
#endif
    }

#if UNITY_ANDROID
    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    public void LaunchReviewTab()
    {
        StartCoroutine(ReviewTab());
    }
#endif

#if UNITY_ANDROID
    public void ShowRewardedAd()
    {
        if (IronSource.Agent.isRewardedVideoAvailable())
            IronSource.Agent.showRewardedVideo();
        else if (!loadingRewardedAd) StartCoroutine(ManualShowRewardedVideo()); //in case rewarded failed to load, reload it once more manually, wait a few seconds, then show the ad
}
#endif

#if UNITY_ANDROID
    public IEnumerator ManualShowRewardedVideo()
    {
        loadingRewardedAd = true;
        IronSource.Agent.loadRewardedVideo();
        yield return new WaitForSeconds(3f);
        loadingRewardedAd = false;
        IronSource.Agent.showRewardedVideo();
    }
#endif

#if UNITY_ANDROID
    IEnumerator ReviewTab()
    {
        yield return new WaitForSeconds(0.5f);
        reviewManager = new ReviewManager();
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }
        playReviewInfo = requestFlowOperation.GetResult();

        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        yield return launchFlowOperation;
        playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }
        // The flow has finished. The API does not indicate whether the user
        // reviewed or not, or even whether the review dialog was shown. Thus, no
        // matter the result, we continue our app flow.
}
#endif

    void RebuildSaveData()
    {
        if (!PlayerPrefs.HasKey("AutoMode")) PlayerPrefs.SetInt("AutoMode", 1);
        if (!PlayerPrefs.HasKey("totalKills")) PlayerPrefs.SetInt("totalKills", 0);
        if (!PlayerPrefs.HasKey("maxKillsInOneRun")) PlayerPrefs.SetInt("maxKillsInOneRun", 0);
        if (!PlayerPrefs.HasKey("totalItemsGrabbed")) PlayerPrefs.SetInt("totalItemsGrabbed", 0);
        if (!PlayerPrefs.HasKey("totalPunches")) PlayerPrefs.SetInt("totalPunches", 0);
        if (!PlayerPrefs.HasKey("totalSpecialAttacks")) PlayerPrefs.SetInt("totalSpecialAttacks", 0);
        if (!PlayerPrefs.HasKey("highestScore")) PlayerPrefs.SetInt("highestScore", 0);
        if (!PlayerPrefs.HasKey("reviewCount")) PlayerPrefs.SetInt("reviewCount", 4);
        if (!PlayerPrefs.HasKey("optedOutReview")) PlayerPrefs.SetInt("optedOutReview", 0);

        //set all upgrades at 0 on startup before checking player inventory
        PlayerPrefs.SetInt("STRENGTH", 0);
        PlayerPrefs.SetInt("ATTACKSPEED", 0);
        PlayerPrefs.SetInt("SPECIAL", 0);
        PlayerPrefs.SetInt("MOVEMENT", 0);

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
        PlayerPrefs.SetInt("AutoMode", 1);
        PlayerPrefs.SetInt("totalKills", 0);
        PlayerPrefs.SetInt("maxKillsInOneRun", 0);
        PlayerPrefs.SetInt("totalItemsGrabbed", 0);
        PlayerPrefs.SetInt("totalPunches", 0);
        PlayerPrefs.SetInt("totalSpecialAttacks", 0);
        PlayerPrefs.SetInt("highestScore", 0);
        PlayerPrefs.SetInt("reviewCount", 0);
        PlayerPrefs.SetInt("optedOutReview", 0);

        for (int i = 0; i < 9; i++)
            PlayerPrefs.SetInt("hiscore" + i.ToString(), 0);

        PlayerPrefs.Save();
    }

    void InitiateCheatSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("AutoMode", 1);
        PlayerPrefs.SetInt("totalKills", 4368);
        PlayerPrefs.SetInt("maxKillsInOneRun", 152);
        PlayerPrefs.SetInt("totalItemsGrabbed", 321);
        PlayerPrefs.SetInt("totalPunches", 7843);
        PlayerPrefs.SetInt("totalSpecialAttacks", 875);
        PlayerPrefs.SetInt("highestScore", 2815);
        PlayerPrefs.SetInt("reviewCount", 5);
        PlayerPrefs.SetInt("optedOutReview", 0);

        for (int i = 0; i < 9; i++)
            PlayerPrefs.SetInt("hiscore" + i.ToString(), 123123);
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
        #if UNITY_ANDROID
        IronSource.Agent.loadInterstitial(); //load ad for next run
        IronSource.Agent.hideBanner(); //hide banner before moving to game
        #endif
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

    public void ShowRewardedAdIcon(bool status)
    {
        rewardedAdButton.SetActive(status);
    }

    public IEnumerator AddCoins(int coins)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) coinsOutput.text = "Offline"; //offline mode, will not gather coins

        yield return new WaitForSeconds(0.5f);
        Text currecyOutput = GameObject.FindGameObjectWithTag("CoinOutput").GetComponent<Text>();
        while (!economySystem.ready) yield return new WaitForSeconds(0.05f);
        economySystem.AddCoins(coins);
        for (int i = 0; i < coins; i++)
        {
            GameObject coin = Instantiate(lootGoldCoin);
            coin.transform.SetParent(GameObject.FindGameObjectWithTag("CurrencyUI").transform, false);
            coin.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.28f);
            coin.GetComponent<AudioSource>().Play();
            currecyOutput.text = (Convert.ToInt32(currecyOutput.text) + 1).ToString();
        }
    }

    public IEnumerator UpdateCoinsUI(long amount)
    {
        Text coinOutput = null;
        while (coinOutput == null)
        {
            yield return new WaitForSeconds(0.05f);
            coinOutput = GameObject.FindGameObjectWithTag("CoinOutput").GetComponent<Text>();
        }
        coinOutput.text = amount.ToString();
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

    public void ToggleMarket()
    {
        marketTab.SetActive(!marketTab.activeSelf);
    }

    public void OpenWebsite(string tab)
    {
        Application.OpenURL("https://julianlerej.com/" + tab);
    }
}
