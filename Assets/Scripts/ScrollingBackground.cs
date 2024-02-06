using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    RawImage image;
    public bool scrollingHorizontal = true;
    public bool scrollingVertical = true;
    [SerializeField] bool mainMenuScrolling = false;

    void Awake()
    {
        image = GetComponent<RawImage>();
    }

    public void ScrollBackground(float x, float y, float speed)
    {
        image.uvRect = new Rect(image.uvRect.position + new Vector2(x * 1.078f, y) * Time.deltaTime * speed, image.uvRect.size);
    }

    public IEnumerator ScrollBackgroundOverTime(float x, float y, float speed, float duration)
    {
        float timer = duration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            ScrollBackground(x, y, speed);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    void Update()
    {
        if (mainMenuScrolling) image.uvRect = new Rect(image.uvRect.position + new Vector2(1.078f, 1) * Time.deltaTime * 0.5f, image.uvRect.size);
    }
}
