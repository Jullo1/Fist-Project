using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelector : MonoBehaviour
{
    public static int currentStage;
    MainMenu menuManager;
    
    List<string> stageNames = new List<string>();
    [SerializeField] Text title;
    [SerializeField] List<Sprite> stageSprites = new List<Sprite>();
    [SerializeField] SpriteRenderer floor;
    Text[] UITexts;
    Outline[] UIOutlines;
    [SerializeField] Outline StartButtonOutline;

    [SerializeField] List<AudioClip> musicList = new List<AudioClip>();
    [SerializeField] AudioSource backgroundMusic;

    void Awake()
    {
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

    public void ChangeLevel(bool next)
    {
        if (next) currentStage++;
        else currentStage--;

        if (currentStage >= stageNames.Count) currentStage = 0;
        else if (currentStage < 0) currentStage = stageNames.Count - 1;

        ApplyStageSkin();
        ApplyStageMusic();
    }

    void ApplyStageSkin()
    {
        title.text = stageNames[currentStage];
        floor.sprite = stageSprites[currentStage];

        //recolor texts so that they remain visible
        switch (currentStage)
        {
            default:
                foreach (Text text in UITexts)
                    text.color = Color.black;
                foreach (Outline outline in UIOutlines)
                    outline.enabled = false;
                StartButtonOutline.effectColor = new Color32(0, 0, 0, 128);
                break;
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
                foreach (Text text in UITexts)
                    text.color = Color.white;
                foreach (Outline outline in UIOutlines)
                    outline.enabled = true;
                StartButtonOutline.effectColor = new Color32(255, 255, 255, 64);
                break;
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
}
