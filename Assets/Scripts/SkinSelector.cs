using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelector : MonoBehaviour
{
    MainMenu menuManager;

    public static int currentSkin;
    List<string> skinNames = new List<string>();
    List<string> skinDescriptions = new List<string>();
    List<string> skinStats = new List<string>();
    [SerializeField] List<Sprite> skinSprites = new List<Sprite>();
    [SerializeField] Image selectedSkin;
    [SerializeField] Text title;
    [SerializeField] Text description;
    [SerializeField] Text statsField;
    [SerializeField] Text unlockTextUI;
    Outline unlockTextOutline;
    string unlockText;

    public float timer;
    public bool unlocked;
    Color oneOpacity = new Color(0,0,0,1);
    Vector3 unlockTextInitialPos;

    void Awake()
    {
        menuManager = FindObjectOfType<MainMenu>();
        unlockTextOutline = unlockTextUI.gameObject.GetComponent<Outline>();
        unlockTextInitialPos = unlockTextUI.transform.localPosition;

        skinNames.Add("Fist Hero");
        skinNames.Add("Fist Dude");
        skinNames.Add("Fist Machine");
        skinNames.Add("Golden Fist Hero");
        skinNames.Add("Fist Wizard");
        skinNames.Add("Super Fist Hero");

        skinDescriptions.Add("");
        skinDescriptions.Add("The other fist guy");
        skinDescriptions.Add("AI-powered punching bot");
        skinDescriptions.Add("When you throw that many punches and unlock the golden skin");
        skinDescriptions.Add("Sleep-inducing magic");
        skinDescriptions.Add("Fist Hero that transcended the limits of a regular Fist Hero");

        skinStats.Add("");
        skinStats.Add("+1 power");
        skinStats.Add("+2 attack speed\n-1 movement");
        skinStats.Add("+1 power\n+1 attack speed\n-1 special");
        skinStats.Add("+2 special\n-1 attack speed");
        skinStats.Add("+2 power");

        if (PlayerPrefs.HasKey("selectedSkin")) currentSkin = PlayerPrefs.GetInt("selectedSkin");
        ApplyPlayerSkin();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (unlocked)
        {
            unlockTextUI.transform.localPosition += Vector3.up * Time.deltaTime * 5;
            if (timer > 2f)
            {
                unlockTextUI.color -= oneOpacity * Time.deltaTime;
                unlockTextOutline.effectColor -= oneOpacity * Time.deltaTime;
            }
        }
        if (unlockTextUI.color.a <= 0) unlockTextUI.gameObject.SetActive(false);
    }

    public void ChangeSkin(bool next)
    {
        unlockTextUI.gameObject.SetActive(true);
        timer = 0;
        unlockTextUI.color = new Color(unlockTextUI.color.r, unlockTextUI.color.g, unlockTextUI.color.b, 1);
        unlockTextOutline.effectColor = new Color(unlockTextOutline.effectColor.r, unlockTextOutline.effectColor.g, unlockTextOutline.effectColor.b, 0.5f);
        unlockTextUI.transform.localPosition = unlockTextInitialPos;

        if (next) currentSkin++;
        else currentSkin--;

        if (currentSkin >= skinNames.Count) currentSkin = 0;
        else if (currentSkin < 0) currentSkin = skinNames.Count - 1;

        unlocked = CheckIfUnlocked();
        if (unlocked) menuManager.LockedCharacter(false, unlockText);
        else menuManager.LockedCharacter(true, unlockText);

        ApplyPlayerSkin();
    }

    void ApplyPlayerSkin()
    {
        title.text = skinNames[currentSkin];
        description.text = skinDescriptions[currentSkin];
        selectedSkin.sprite = skinSprites[currentSkin];
        statsField.text = skinStats[currentSkin];
        SpriteChanger.playerSpriteSheetName = skinNames[currentSkin];
    }

    public bool CheckIfUnlocked()
    {
        switch (currentSkin)
        {
            default: { unlockText = "";  return true; }
            case 1: if (PlayerPrefs.GetInt("maxKillsInOneRun") >= 100) { unlockText = "Defeated " + PlayerPrefs.GetInt("maxKillsInOneRun") + " enemies in one run!"; return true; } else { unlockText = "Defeat 100 enemies in one run\nBest so far: " + PlayerPrefs.GetInt("maxKillsInOneRun").ToString(); return false; }
            case 2: if (PlayerPrefs.GetInt("totalItemsGrabbed") >= 50) { unlockText = "Grabbed " + PlayerPrefs.GetInt("totalItemsGrabbed") + " items!"; return true; } else { unlockText = "Grab " + (50 - PlayerPrefs.GetInt("totalItemsGrabbed")) + " items"; return false; }
            case 3: if (PlayerPrefs.GetInt("highestScore") >= 3000) { unlockText = "Best score: " + PlayerPrefs.GetInt("highestScore"); return true; } else { unlockText = "Hit 3000 score"; return false; }
            case 4: if (PlayerPrefs.GetInt("totalSpecialAttacks") >= 100) { unlockText = "Struck with " + PlayerPrefs.GetInt("totalSpecialAttacks") + " special attacks!"; return true; } else { unlockText = "Strike with " + (100 - PlayerPrefs.GetInt("totalSpecialAttacks")) + " special attacks"; return false; }
            case 5: if (PlayerPrefs.GetInt("totalPunches") >= 2500) { unlockText = "Over " + PlayerPrefs.GetInt("totalPunches") + " punches thrown!"; return true; } else { unlockText = "Throw " + (2500 - PlayerPrefs.GetInt("totalPunches")) + " punches"; return false; }
        }
    }
}
