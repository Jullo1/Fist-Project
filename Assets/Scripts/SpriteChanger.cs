using UnityEngine;
using System;

public class SpriteChanger : MonoBehaviour
{
    public static string playerSpriteSheetName;
    [SerializeField] string spriteSheetName;
    [SerializeField] string location;

    void LateUpdate()
    {
        if (gameObject.tag == "Player") spriteSheetName = playerSpriteSheetName;
        var subSprites = Resources.LoadAll<Sprite>("Characters/" + location + "/" + spriteSheetName);

        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            string spriteName = renderer.sprite.name;
            var newSprite = Array.Find(subSprites, item => item.name == spriteName);

            if (newSprite)
                renderer.sprite = newSprite;

        }
    }
}
