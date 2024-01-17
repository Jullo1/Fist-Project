using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelector : MonoBehaviour
{
    MainMenu menuManager;

    static int currentSkin;
    List<string> skinNames = new List<string>();
    List<string> skinDescriptions = new List<string>();
    [SerializeField] List<Sprite> skinSprites = new List<Sprite>();
    [SerializeField] Image selectedSkin;
    [SerializeField] Text title;
    [SerializeField] Text description;
    string unlockText;


    void Awake()
    {
        menuManager = FindObjectOfType<MainMenu>();

        skinNames.Add("Fist Hero");
        skinNames.Add("Fist Warrior");
        skinNames.Add("Fist Machine");
        skinNames.Add("Golden Fist Hero");
        skinNames.Add("Fist Wizard");
        skinNames.Add("Super Fist Hero");

        skinDescriptions.Add("");
        skinDescriptions.Add("The other fist guy");
        skinDescriptions.Add("AI-powered punching bot");
        skinDescriptions.Add("When you throw so many punches that you unlock the golden skin");
        skinDescriptions.Add("Sleep-inducing magic");
        skinDescriptions.Add("Fist Hero that has transcended the limits of a regular Fist Hero");

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
        SpriteChanger.spriteSheetName = skinNames[currentSkin];
    }

    public bool CheckIfUnlocked()
    {
        switch (currentSkin)
        {
            default: { unlockText = "";  return true; }
            case 1: if (PlayerPrefs.GetInt("maxKillsInOneRun") >= 100) { unlockText = "Defeated " + PlayerPrefs.GetInt("maxKillsInOneRun") + " enemies in one run!"; return true; } else { unlockText = "Unlock:\nDefeat 100 enemies in one run"; return false; }
            case 2: if (PlayerPrefs.GetInt("totalItemsGrabbed") >= 100) { unlockText = "Grabbed " + PlayerPrefs.GetInt("totalItemsGrabbed") + " items!"; return true; } else { unlockText = "Unlock:\nGrab " + (100 - PlayerPrefs.GetInt("totalItemsGrabbed")) + " more items"; return false; }
            case 3: if (PlayerPrefs.GetInt("highestScore") >= 2000) { unlockText = "Highest score: " + PlayerPrefs.GetInt("highestScore") + "!"; return true; } else { unlockText = "Unlock:\nReach over 2000 score"; return false; }
            case 4: if (PlayerPrefs.GetInt("totalSpecialAttacks") >= 150) { unlockText = "Used special attack " + PlayerPrefs.GetInt("totalSpecialAttacks") + " times!"; return true; } else { unlockText = "Unlock:\nUse special attack " + (150 - PlayerPrefs.GetInt("totalSpecialAttacks")) + " times"; return false; }
            case 5: if (PlayerPrefs.GetInt("totalPunches") >= 9000) { unlockText = "Threw " + PlayerPrefs.GetInt("totalPunches") + " punches!"; return true; } else { unlockText = "Unlock:\nThrow " + (9000 - PlayerPrefs.GetInt("totalPunches")) + " more punches"; return false; }
        }
    }
}
