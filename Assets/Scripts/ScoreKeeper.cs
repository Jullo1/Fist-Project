using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score;
    public static int coins;
    public static bool adPlayed = true;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void ResetScore()
    {
        score = 0;
        coins = 0;
    }
}
