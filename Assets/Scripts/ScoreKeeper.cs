using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void ResetScore()
    {
        score = 0;
    }
}
