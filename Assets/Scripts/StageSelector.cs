using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelector : MonoBehaviour
{
    MainMenu menuManager;
    EndGameScoreOutput endGameScoreUpdator;

    public static int currentStage;
    public static float scoreMultiplier = 1;
    public static float scrollMultiplier = 0.484f;

    int previousStage;
    int maxAvailableStage = 3;
    int playerLevel;

    List<string> stageNames = new List<string>();
    [SerializeField] Text title;
    [SerializeField] List<Sprite> stageSprites = new List<Sprite>();
    RawImage floor;
    Text[] UITexts;
    [SerializeField] Text warningText;
    Outline[] UIOutlines;
    [SerializeField] Outline startButtonOutline;
    [SerializeField] Image rewardAdButton;
    [SerializeField] Image experienceBar;
    [SerializeField] Image experienceBarBackground;
    [SerializeField] Text experienceText;
    [SerializeField] Outline rewardAdButtonOutline;
    [SerializeField] Outline marketButtonOutline;
    [SerializeField] Button[] navigateStagesButtons;

    [SerializeField] List<AudioClip> musicList = new List<AudioClip>();
    [SerializeField] AudioSource backgroundMusic;

    void Awake()
    {
        menuManager = FindObjectOfType<MainMenu>();
        endGameScoreUpdator = FindObjectOfType<EndGameScoreOutput>();
        floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<RawImage>();
        UITexts = FindObjectsOfType<Text>();
        UIOutlines = FindObjectsOfType<Outline>();

        for (int i = 0; i < UIOutlines.Length; i++)
            if (UIOutlines[i].gameObject.tag == "MarketButton") UIOutlines[i] = UIOutlines[0];

        stageNames.Add("Plains");
        stageNames.Add("Forest");
        stageNames.Add("Desert");
        stageNames.Add("Urban");
        stageNames.Add("Prison");
        stageNames.Add("Tundra");
        stageNames.Add("Mountain");
        stageNames.Add("Volcano");
        stageNames.Add("Underground");

        if (PlayerPrefs.HasKey("selectedStage")) currentStage = PlayerPrefs.GetInt("selectedStage");
        ApplyStageSkin(); ApplyStageMusic();
    }

    void Start()
    {
        CheckMaxAvailableStage();
        previousStage = currentStage;
        HideNavigateButtons();
    }

    void CheckMaxAvailableStage()
    {
        playerLevel = PlayerPrefs.GetInt("PlayerLevel");
        if (playerLevel >= 6) maxAvailableStage = 9;
        else if (playerLevel >= 4) maxAvailableStage = 7;
        else if (playerLevel >= 3) maxAvailableStage = 5;
        else if (playerLevel >= 2) maxAvailableStage = 4;
        else  maxAvailableStage = 3;
    }

    void HideNavigateButtons()
    {
        if (currentStage == 0)
            navigateStagesButtons[0].gameObject.SetActive(false);
        else
            navigateStagesButtons[0].gameObject.SetActive(true);

        if (currentStage == maxAvailableStage - 1)
            navigateStagesButtons[1].gameObject.SetActive(false);
        else
            navigateStagesButtons[1].gameObject.SetActive(true);
    }

    public void ChangeLevel(bool next)
    {
        CheckMaxAvailableStage();
        endGameScoreUpdator.UpdateScoreOutput();
        if (next) currentStage++;
        else currentStage--;
        HideNavigateButtons();

        //loops through stages after reaching the end, just in case in previous/next buttons fail to hide
        if (currentStage >= stageNames.Count || currentStage > maxAvailableStage - 1) currentStage = 0;
        else if (currentStage < 0) currentStage = maxAvailableStage - 1;

        if (currentStage == 0) //adjust locked background colors to the current stage text color
        {
            menuManager.lockedStageBackground.color = new Color32(255, 255, 255, 20);
            menuManager.lockedCharacterMask.color = new Color32(255, 255, 255, 20);
            navigateStagesButtons[0].gameObject.SetActive(false);
        }
        else
        {
            menuManager.lockedStageBackground.color = new Color32(0, 0, 0, 200);
            menuManager.lockedCharacterMask.color = new Color32(0, 0, 0, 200);
            navigateStagesButtons[0].gameObject.SetActive(true);
        }

        if (ScoreKeeper.score > 0) //hide score when changing stage, useful for sharing screenshots of highest score
        {
            if (previousStage != currentStage) menuManager.scoreOutput.gameObject.SetActive(false);
            else menuManager.scoreOutput.gameObject.SetActive(true);
        }

        ApplyStageBonus();
        ApplyStageSkin();
        ApplyStageMusic();
    }

    void ApplyStageBonus()
    {
        switch (currentStage)
        {
            default:
                scoreMultiplier = 1;
                break;
            case 1: case 2: case 3:
                scoreMultiplier = 1;
                break;
            case 4: case 5: case 6:
                scoreMultiplier = 1.25f;
                break;
            case 7: case 8: case 9:
                scoreMultiplier = 1.5f;
                break;
        }
    }
    void ApplyStageSkin()
    {
        title.text = stageNames[currentStage];
        floor.texture = stageSprites[currentStage].texture;

        //recolor texts and outlines
        switch (currentStage)
        {
            default: ApplyStageColors(Color.black, new Color32(255, 255, 255, 32), false); break;
            case 1: ApplyStageColors(new Color32(160, 180, 60, 255), new Color32(120, 80, 40, 255), true); break;
            case 2: ApplyStageColors(new Color32(240, 170, 90, 255), new Color32(50, 50, 50, 128), true); break;
            case 3: ApplyStageColors(new Color32(250, 220, 175, 255), new Color32(0, 0, 0, 64), true); break;
            case 4: ApplyStageColors(new Color32(210, 210, 210, 255), new Color32(0, 0, 0, 184), true); break;
            case 5: ApplyStageColors(new Color32(110, 165, 200, 255), new Color32(200, 255, 255, 212), true); break;
            case 6: ApplyStageColors(new Color32(40, 40, 40, 255), new Color32(200, 200, 200, 255), true); break;
            case 7: ApplyStageColors(new Color32(200, 90, 40, 255), new Color32(40, 40, 40, 184), true); break;
            case 8: ApplyStageColors(new Color32(100, 100, 200, 255), new Color32(0, 0, 0, 128), true); break; 
        }
    }

    void ApplyStageColors(Color32 textColor, Color32 outlineColor, bool outlineEnabled)
    {
        foreach (Outline outline in UIOutlines)
        {
            outline.enabled = outlineEnabled;
            outline.effectColor = outlineColor;
            warningText.color = textColor;
        }
        foreach (Text text in UITexts)
        {
            text.color = textColor;
            startButtonOutline.effectColor = new Color32(textColor.r, textColor.g, textColor.b, 128);
        }

        rewardAdButton.color = textColor;
        experienceBar.color = textColor;
        experienceText.color = textColor;
        experienceText.GetComponent<Outline>().effectColor = outlineColor;
        experienceBarBackground.color = outlineColor;
        rewardAdButtonOutline.effectColor = outlineColor;
        marketButtonOutline.effectColor = textColor;
    }

    void ApplyStageMusic()
    {
        switch (currentStage)
        {
            default:
                if (backgroundMusic.clip != musicList[0])
                {
                    backgroundMusic.clip = musicList[0];
                    backgroundMusic.Play();
                }
                break;
            case 3:
            case 4:
            case 5:
                if (backgroundMusic.clip != musicList[1])
                {
                    backgroundMusic.clip = musicList[1];
                    backgroundMusic.Play();
                }
                break;
            case 6:
            case 7:
            case 8:
                if (backgroundMusic.clip != musicList[2])
                {
                    backgroundMusic.clip = musicList[2];
                    backgroundMusic.Play();
                }
                break;
        }
    }

    public void CheckIfUnlocked()
    {
        if (menuManager.StageRequirements(currentStage) <= PlayerPrefs.GetInt("PlayerLevel")) menuManager.LockedStage(false);
        else menuManager.LockedStage(true);
    }
}
