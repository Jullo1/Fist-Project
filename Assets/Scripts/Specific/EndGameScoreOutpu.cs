using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndGameScoreOutput : MonoBehaviour
{
    Text scoreOutput;

    void Awake()
    {
        scoreOutput = GetComponent<Text>();
    }

    void Start()
    {
        if (ScoreKeeper.score > 0)
            StartCoroutine(UpdateScoreText(ScoreKeeper.score));
    }

    IEnumerator UpdateScoreText(int scoreAmount)
    {
        int currentScore = 0;
        for (int i = 0; i < scoreAmount; i += 10)
        {
            currentScore += 10;
            scoreOutput.text = currentScore.ToString();
            yield return new WaitForSeconds(0.01f);
        }
        scoreOutput.text = ScoreKeeper.score.ToString();
        if (ScoreKeeper.score > 2000) scoreOutput.text += "!";
    }
}
