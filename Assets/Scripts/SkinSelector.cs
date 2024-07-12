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
    string unlockText;


    void Awake()
    {
        menuManager = FindObjectOfType<MainMenu>();


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
        skinStats.Add("+1 attack speed");
        skinStats.Add("+2 attack speed\n-1 movement");
        skinStats.Add("+1 power\n+1 attack speed\n-1 special");
        skinStats.Add("+2 special\n-1 attack speed");
        skinStats.Add("+2 power");

        if (PlayerPrefs.HasKey("selectedSkin")) currentSkin = PlayerPrefs.GetInt("selectedSkin");
        ApplyPlayerSkin();
    }

    public void ChangeSkin(bool next)
    {
        if (next) currentSkin++;
        else currentSkin--;

        if (currentSkin >= skinNames.Count) currentSkin = 0;
        else if (currentSkin < 0) currentSkin = skinNames.Count - 1;

        if (CheckIfUnlocked()) menuManager.LockedCharacter(false, unlockText);
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
            case 1: if (PlayerPrefs.GetInt("maxKillsInOneRun") >= 100) { unlockText = "Defeated " + PlayerPrefs.GetInt("maxKillsInOneRun") + " enemies in one run!"; return true; } else { unlockText = "Unlock:\nDefeat 100 enemies in one run\nBest so far: " + PlayerPrefs.GetInt("maxKillsInOneRun").ToString(); return false; }
            case 2: if (PlayerPrefs.GetInt("totalItemsGrabbed") >= 50) { unlockText = "Got " + PlayerPrefs.GetInt("totalItemsGrabbed") + " potions!"; return true; } else { unlockText = "Unlock:\nGet " + (50 - PlayerPrefs.GetInt("totalItemsGrabbed")) + " potions"; return false; }
            case 3: if (PlayerPrefs.GetInt("highestScore") >= 2500) { unlockText = "Highest score: " + PlayerPrefs.GetInt("highestScore") + "!"; return true; } else { unlockText = "Unlock:\nHit 2500 score"; return false; }
            case 4: if (PlayerPrefs.GetInt("totalSpecialAttacks") >= 100) { unlockText = "Used special " + PlayerPrefs.GetInt("totalSpecialAttacks") + " times!"; return true; } else { unlockText = "Unlock:\nUse special " + (100 - PlayerPrefs.GetInt("totalSpecialAttacks")) + " times"; return false; }
            case 5: if (PlayerPrefs.GetInt("totalPunches") >= 3000) { unlockText = PlayerPrefs.GetInt("totalPunches") + " total punches!"; return true; } else { unlockText = "Unlock:\nThrow over " + (3000 - PlayerPrefs.GetInt("totalPunches")) + " punches"; return false; }
        }
    }
}
