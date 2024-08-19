using UnityEngine;
using System;
using UnityEngine.UI;

public class SpriteChanger : MonoBehaviour
{
    public static string playerSpriteSheetName;
    [SerializeField] string spriteSheetName;
    [SerializeField] string location;
    SpriteRenderer[] sr;

    Sprite[] subSprites;

    void Awake()
    {
        sr = GetComponentsInChildren<SpriteRenderer>();
        LoadTextures();
    }
    void LateUpdate()
    {
        foreach (SpriteRenderer renderer in sr)
        {
            renderer.sprite = Array.Find(subSprites, item => item.name == renderer.sprite.name);
        }
    }

    public void LoadTextures()
    {
        if (gameObject.tag == "Player") spriteSheetName = playerSpriteSheetName;
        subSprites = Resources.LoadAll<Sprite>("Characters/" + location + "/" + spriteSheetName);
    }
}
