using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndGameScoreOutput : MonoBehaviour
{
    Text scoreOutput;
    Coroutine scoreAnimationRoutine;

    void Awake()
    {
        scoreOutput = GetComponent<Text>();
    }

    void Start()
    {
        if (ScoreKeeper.score > 0)
            scoreAnimationRoutine = StartCoroutine(UpdateScoreText(ScoreKeeper.score));
    }

    public IEnumerator UpdateScoreText(int scoreAmount)
    {
        int speed = scoreAmount / 25;
        if (speed == 0) speed = 1;

        for (int i = 0; i < scoreAmount; i += speed)
        {
            scoreOutput.text = i.ToString();
            yield return new WaitForSeconds(0.025f);
        }
        scoreOutput.text = scoreAmount.ToString();
        if (scoreAmount > 2000) scoreOutput.text += "!";
        scoreAnimationRoutine = null;
    }

    public void UpdateScoreOutput()
    {
        if (scoreAnimationRoutine == null) return;
        StopCoroutine(scoreAnimationRoutine);
        scoreOutput.text = ScoreKeeper.score.ToString();
        if (ScoreKeeper.score > 2000) scoreOutput.text += "!";
    }
}
