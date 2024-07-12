using UnityEngine;
using UnityEngine.UI;

public class GameVersion : MonoBehaviour
{
    [SerializeField] string step;

    void Start()
    {
        GetComponent<Text>().text = "v" + Application.version + " " + step + " - Found a bug? Click here to let me know!";
    }
}
