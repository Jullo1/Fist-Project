using UnityEngine;

public class Announcement : MonoBehaviour
{
    void Update()
    {
        transform.position += Vector3.left * (Time.deltaTime / 1.5f);
        if (transform.position.x < -15f) transform.position = new Vector3(15f, transform.position.y, 0);
    }
}
