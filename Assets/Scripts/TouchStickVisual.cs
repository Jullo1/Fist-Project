using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchStickVisual : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] GameObject background;
    int id;

    public void OnPointerDown(PointerEventData eventData)
    {
        id = eventData.pointerId;
        background.transform.position = transform.position;
        gameObject.GetComponent<RawImage>().color = new Color32(255, 255, 255, 150);
        background.GetComponent<RawImage>().color = new Color32(0, 0, 0, 100);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == id)
            background.transform.position = transform.position;

        gameObject.GetComponent<RawImage>().color = new Color32(255, 255, 255, 25);
        background.GetComponent<RawImage>().color = new Color32(0, 0, 0, 50);
    }
}
