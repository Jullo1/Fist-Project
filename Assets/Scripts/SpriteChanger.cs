using UnityEngine;
using System;

public class SpriteChanger : MonoBehaviour
{
    public static string spriteSheetName;

    void LateUpdate()
    {
        var subSprites = Resources.LoadAll<Sprite>("Characters/Player/" + spriteSheetName);

        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            string spriteName = renderer.sprite.name;
            var newSprite = Array.Find(subSprites, item => item.name == spriteName);

            if (newSprite)
                renderer.sprite = newSprite;

        }
    }
}
