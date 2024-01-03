using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool paused;
    Player player;
    [SerializeField] OnScreenStick leftStick;
    AudioSource menuAudio;
    [SerializeField] AudioClip selectSFX;

    [SerializeField] List<Enemy> enemyList = new List<Enemy>();

    public List<Entity> spawnGroup = new List<Entity>();
    [SerializeField] float waveIntensity;
    public int currentWave;
    [SerializeField] float closestSpawnPos;
    float spawnTimer;
    public bool pauseWaves;

    ScoreKeeper scoreKeeper;
    [SerializeField] Text scoreOutput;
    int currentScore;

    int level;
    float experience;
    public float toNextLevel;
    [SerializeField] Image experienceUI;
    [SerializeField] Selectable invisibleButton;

    [SerializeField] GameObject levelUpWindow;
    [SerializeField] Text option1Text; int option1Value;
    [SerializeField] Text option2Text; int option2Value;
    List<string> optionsList = new List<string>() { "Power" , "Attack Speed" , "Movement" , "Special"};

    void Awake()
    {
        player = FindAnyObjectByType<Player>();
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        menuAudio = GetComponent<AudioSource>();
    }

    void Start()
    {
        level = 1;
        toNextLevel = 100;
        currentWave = 0;
        waveIntensity = 0.9f;
        if (!pauseWaves) NextWave(); //immediately spawn next wave at start
    }

    void Update()
    {
        if (spawnTimer > 6) {
            NextWave();
            spawnTimer = 0;
        } else if (!pauseWaves) spawnTimer += Time.deltaTime;
    }

    public void UpdateScore(int score)
    {
        currentScore += score;
        scoreOutput.text = currentScore.ToString();
    }

    void NextWave()
    {
        SetupWave();
        foreach (Entity entity in spawnGroup)
        {
            Vector2 spawnPos = player.transform.position;
            while (Vector2.Distance(spawnPos, player.transform.position) < closestSpawnPos)
                spawnPos += new Vector2(Random.Range(-7, 7), Random.Range(-5, 5));

            Spawn(entity, spawnPos);
        }
    }

    void SetupWave()
    {
        spawnGroup.Clear(); //clear previous wave info
        currentWave++;
        waveIntensity *= 1.075f;
        SetupNextWaveWithIntensity(waveIntensity);
    }

    void AddToNextWave(Enemy enemyType, int amount)
    {
        for (int i = 0; i < amount; i++)
            spawnGroup.Add(enemyType);
    }

    void SetupNextWaveWithIntensity(float intensity) //algorithm to randomize next spawn group with a given intensity
    {
        AddToNextWave(enemyList[0], (int) (intensity * 5));
        AddToNextWave(enemyList[1], (int) (intensity * 1.5f));
        AddToNextWave(enemyList[2], (int) (intensity * 0.5f));
    }

    void Spawn(Entity entity, Vector2 position)
    {
        GameObject newEnemy = Instantiate(entity.gameObject);
        newEnemy.transform.position = position;
    }

    public void GainExperience(int amount)
    {
        UpdateScore(amount);
        experience += amount;
        if (experience >= toNextLevel) LevelUp();
        else experienceUI.fillAmount = experience / toNextLevel;
    }

    void LevelUp()
    {
        paused = true;
        leftStick.enabled = false;

        Time.timeScale = 0;
        level++;
        experience = 0;
        toNextLevel += toNextLevel/2;
        experienceUI.fillAmount = 0;

        SetupLevelUpOptions();
        levelUpWindow.SetActive(true);
        invisibleButton.Select();
    }

    void SetupLevelUpOptions()
    {
        option1Value = Random.Range(1, 5);
        do { option2Value = Random.Range(1, 5); } while (option2Value == option1Value); //make sure we get 2 different options
        option1Text.text = optionsList[option1Value-1];
        option1Text.transform.GetChild(0).GetComponent<Text>().text = optionsList[option1Value - 1];
        option2Text.text = optionsList[option2Value-1];
        option2Text.transform.GetChild(0).GetComponent<Text>().text = optionsList[option2Value - 1];
    }

    public void LevelUpButton(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 1:
                SelectUpgrade(option1Value);
                break;
            case 2:
                SelectUpgrade(option2Value);
                break;
        }
        menuAudio.clip = selectSFX;
        menuAudio.Play();
        player.sendAnimTrigger("cancel");
        StartCoroutine(player.FreezeAttack(0.05f));
    }

    void SelectUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 1: //punch harder
                player.UpgradeStat(playerStats.strength, 3);
                player.UpgradeStat(playerStats.pushForce, 100);
                break;
            case 2: //punch faster
                player.UpgradeStat(playerStats.attackSpeed, 1.2f);
                break;
            case 3: //move faster
                player.UpgradeStat(playerStats.moveSpeed, 0.25f);
                break;
            case 4: //faster special
                player.UpgradeStat(playerStats.specialCooldown, 1.3f);
                break;
        }
        ContinueGame();
    }

    public void ContinueGame()
    {
        paused = false;
        levelUpWindow.SetActive(false);
        leftStick.enabled = true;
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        ScoreKeeper.score = currentScore;
        SceneManager.LoadScene("Menu");
    }
}
