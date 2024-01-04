using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score;
    public static int currentTutorialNumber;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void ResetScore()
    {
        score = 0;
    }
}
