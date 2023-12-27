using UnityEngine;
using UnityEngine.EventSystems;

public class TouchStickVisual : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] GameObject background;
    int id;

    public void OnPointerDown(PointerEventData eventData)
    {
        id = eventData.pointerId;
        background.transform.position = transform.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == id)
            background.transform.position = transform.position;
    }
}
