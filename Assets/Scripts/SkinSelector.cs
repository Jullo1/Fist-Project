using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelector : MonoBehaviour
{
    static int currentSkin;
    List<string> skinNames = new List<string>();
    List<string> skinDescriptions = new List<string>();
    [SerializeField] List<Sprite> skinSprites = new List<Sprite>();
    [SerializeField] Image selectedSkin;
    [SerializeField] Text title;
    [SerializeField] Text description;


    void Start()
    {
        ApplySkin();
    }
    void Awake()
    {
        skinNames.Add("Fist Hero");
        skinNames.Add("Fist Warrior");
        skinNames.Add("Fist Machine");
        skinNames.Add("Golden Fist Hero");
        skinNames.Add("Fist Wizard");
        skinNames.Add("Super Fist Hero");

        skinDescriptions.Add("");
        skinDescriptions.Add("The other fist guy");
        skinDescriptions.Add("The most efficient punching machine");
        skinDescriptions.Add("When you throw so many punches that you unlock the golden skin");
        skinDescriptions.Add("Sleep-inducing spells");
        skinDescriptions.Add("Fist Hero that has transcended the limits of a regular Fist Hero");
    }

    public void ChangeSkin(bool next)
    {
        if (next) currentSkin++;
        else currentSkin--;

        if (currentSkin >= skinNames.Count) currentSkin = 0;
        else if (currentSkin < 0) currentSkin = skinNames.Count - 1;

        ApplySkin();
    }

    void ApplySkin()
    {
        title.text = skinNames[currentSkin];
        description.text = skinDescriptions[currentSkin];
        selectedSkin.sprite = skinSprites[currentSkin];
        SpriteChanger.spriteSheetName = skinNames[currentSkin];
    }
}
