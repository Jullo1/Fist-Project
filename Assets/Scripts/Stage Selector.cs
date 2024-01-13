using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelector : MonoBehaviour
{
    public static int currentStage;
    
    List<string> stageNames = new List<string>();
    [SerializeField] Text title;
    [SerializeField] List<Sprite> stageSprites = new List<Sprite>();
    [SerializeField] SpriteRenderer floor;
    Text[] UITexts;
    Outline[] UIOutlines;
    [SerializeField] Outline StartButtonOutline;

    void Awake()
    {
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
    }
    void Start()
    {
        ApplyStageSkin();
    }

    public void ChangeLevel(bool next)
    {
        if (next) currentStage++;
        else currentStage--;

        if (currentStage >= stageNames.Count) currentStage = 0;
        else if (currentStage < 0) currentStage = stageNames.Count - 1;

        ApplyStageSkin();
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
}
