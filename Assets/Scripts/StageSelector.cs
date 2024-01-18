using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelector : MonoBehaviour
{
    MainMenu menuManager;

    public static int currentStage;
    public static float scrollMultiplier = 1;

    int previousStage;
    int maxAvailableStage = 6;
    
    List<string> stageNames = new List<string>();
    [SerializeField] Text title;
    [SerializeField] List<Sprite> stageSprites = new List<Sprite>();
    RawImage floor;
    Text[] UITexts;
    Outline[] UIOutlines;
    [SerializeField] Outline startButtonOutline;

    [SerializeField] List<AudioClip> musicList = new List<AudioClip>();
    [SerializeField] AudioSource backgroundMusic;

    void Awake()
    {
        floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<RawImage>();
        menuManager = FindObjectOfType<MainMenu>();
        UITexts = FindObjectsOfType<Text>();
        UIOutlines = FindObjectsOfType<Outline>();

        stageNames.Add("Plains");
        stageNames.Add("Garden");
        stageNames.Add("Desert");
        stageNames.Add("Urban");
        stageNames.Add("Prison");
        stageNames.Add("Mountain");
        stageNames.Add("Volcano");
        stageNames.Add("Castle");
        stageNames.Add("Dungeon");

        ApplyStageSkin();
        ApplyStageMusic();
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("totalKills") >= 1500) maxAvailableStage = 9;
        previousStage = currentStage;
    }

    public void ChangeLevel(bool next)
    {
        if (next) currentStage++;
        else currentStage--;

        if (currentStage >= stageNames.Count || currentStage > maxAvailableStage - 1) currentStage = 0;
        else if (currentStage < 0) currentStage = maxAvailableStage - 1;

        if (currentStage == 0) //adjust locked background colors to the current stage text color
        {
            menuManager.lockedStageBackground.color = new Color32(255, 255, 255, 20);
            menuManager.lockedCharacterMask.color = new Color32(255, 255, 255, 20);
        }
        else
        {
            menuManager.lockedStageBackground.color = new Color32(0, 0, 0, 200);
            menuManager.lockedCharacterMask.color = new Color32(0, 0, 0, 200);
        }

        if (ScoreKeeper.score > 0) //hide score when changing stage, useful for sharing screenshots of highest score
        {
            if (previousStage != currentStage) menuManager.scoreOutput.gameObject.SetActive(false);
            else menuManager.scoreOutput.gameObject.SetActive(true);
        }

        ApplyStageSkin();
        ApplyStageMusic();
    }

    void ApplyStageSkin()
    {
        title.text = stageNames[currentStage];
        floor.texture = stageSprites[currentStage].texture;

        //recolor texts and outlines per stage
        switch (currentStage)
        {
            default: ApplyStageColors(Color.black, new Color32(0, 0, 0, 64), false); scrollMultiplier = 0.25f; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(5.0f,2.5f)); break;
            case 1: ApplyStageColors(Color.black, new Color32(255, 255, 255, 32), true); scrollMultiplier = 1; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(20f, 10f)); break;
            case 2: ApplyStageColors(new Color32(255, 200, 140, 255), new Color32(0, 0, 0, 64), true); scrollMultiplier = 1; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(20f, 10f)); break;
            case 3: ApplyStageColors(Color.white, new Color32(0, 0, 0, 64), true); scrollMultiplier = 0.5f; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(10f, 5f)); break;
            case 4: ApplyStageColors(new Color32(255, 255, 200, 255), new Color32(0, 0, 0, 128), true); scrollMultiplier = 1; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(20f, 10f)); break;
            case 5: ApplyStageColors(new Color32(200, 100, 100, 255), new Color32(0, 0, 0, 128), true); scrollMultiplier = 1; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(20f, 10f)); break;
            case 6: ApplyStageColors(new Color32(200, 100, 50, 255), new Color32(0, 0, 0, 64), true); scrollMultiplier = 1; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(20f, 10f)); break;
            case 7: ApplyStageColors(new Color32(200, 50, 50, 255), new Color32(0, 0, 0, 128), true); scrollMultiplier = 1.2f; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(24f, 12f)); break;
            case 8: ApplyStageColors(new Color32(100, 100, 200, 255), new Color32(0, 0, 0, 128), true); scrollMultiplier = 1.2f; floor.uvRect = new Rect(floor.uvRect.position, new Vector2(24f, 12f)); break; 
        }
    }

    void ApplyStageColors(Color32 textColor, Color32 outlineColor, bool outlineEnabled)
    {
        foreach (Outline outline in UIOutlines)
        {
            outline.enabled = outlineEnabled;
            outline.effectColor = outlineColor;
        }
        foreach (Text text in UITexts)
        {
            startButtonOutline.effectColor = new Color32(textColor.r, textColor.g, textColor.b, 128);
            text.color = textColor;
        }
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
        if (menuManager.StageRequirements(currentStage) <= 0) menuManager.LockedStage(false);
        else menuManager.LockedStage(true);
    }
}
