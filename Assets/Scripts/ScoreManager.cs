using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    [SerializeField, Tooltip("The text to show the score on.")] private TextMeshProUGUI scoreText;
    [SerializeField, Tooltip("The current amount of ships.")] private TextMeshProUGUI shipsText;
    [SerializeField, Tooltip("The minimum duration for the score change.")] private float minScoreAnimationDuration = 0.5f;
    [SerializeField, Tooltip("The maximum duration for the score change.")] private float maxScoreAnimationDuration = 2f;
    [SerializeField, Tooltip("The score animation range (the larger the number, the bigger the amount has to be in order to reach the max score duration).")] private float scoreAnimationDurationChange = 100f;
    [SerializeField, Tooltip("The shake intensity for the score text when updated.")] private float shakeDisplayIntensity;
    [SerializeField, Tooltip("The shake frequency for the score text when updated.")] private float shakeDisplayFrequency;

    private float currentScore;
    private float displayedScore;
    private float transitionStartTime;
    private bool scoreUpdated;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentScore = 0;
        displayedScore = 0;
        scoreText.text = ScoreToString();
        AdjustShipNumber(0);
    }

    public void AddToScore(int points)
    {
        currentScore += points;
        scoreText.text = ScoreToString();
        transitionStartTime = Time.time;
    }

    public void AdjustShipNumber(int shipsNumber)
    {
        shipsText.text = "Ships: " + (shipsNumber + 1).ToString();
    }

    private void Update()
    {
        RefreshScoreDisplay();
    }

    private void RefreshScoreDisplay()
    {
        if (displayedScore != currentScore)
        {
            //Calculate the progress based on the time elapsed and the time the transition started
            scoreUpdated = false;
            float transitionDuration = CalculateTransitionDuration();
            float progress = Mathf.Clamp01((Time.time - transitionStartTime) / transitionDuration);

            //Lerp between the displayed resources and the current resources
            displayedScore = Mathf.Round(Mathf.Lerp(displayedScore, currentScore, progress));

            //Make sure the display score ends up as the current score
            if (progress >= 1.0f)
            {
                displayedScore = currentScore;
            }
        }
        else if (!scoreUpdated)
        {
            scoreUpdated = true;
        }

        scoreText.text = ScoreToString();
    }

    private float CalculateTransitionDuration() => Mathf.Lerp(minScoreAnimationDuration, maxScoreAnimationDuration, Mathf.Abs(currentScore - displayedScore) / scoreAnimationDurationChange);
    public string ScoreToString() => displayedScore.ToString("n0");
    public float GetScore() => currentScore;
}
