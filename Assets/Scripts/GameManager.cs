using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public bool paused;
    Player player;
    AudioSource menuAudio;
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioClip selectSFX;

    public static float audioProgress;

    [SerializeField] List<Enemy> enemyList = new List<Enemy>();
    [SerializeField] List<float> enemyUnitSpawnIntensity = new List<float>();
    [SerializeField] float spawnRate;

    public List<Entity> spawnGroup = new List<Entity>();
    [SerializeField] float waveIntensity;
    [SerializeField] float waveIntensityMultiplier;
    public int currentWave;
    [SerializeField] float closestSpawnPosX;
    [SerializeField] float closestSpawnPosY;
    float spawnTimer;
    public bool pauseWaves;

    ScoreKeeper scoreKeeper;
    [SerializeField] Text scoreOutput;
    [SerializeField] int currentScore;
    bool freezeUI;

    int level;
    float experience;
    public float toNextLevel;

    public int killCount;
    public int punchCount;
    public int specialAttackCount;
    public int itemGrabCount;

    [SerializeField] Image experienceUI;
    [SerializeField] Selectable invisibleButton;
    OnScreenButton mobileButton;

    [SerializeField] GameObject levelUpWindow;
    [SerializeField] Text option1Text; int option1Value;
    [SerializeField] Text option2Text; int option2Value;
    List<string> optionsList = new List<string>() { "Power" , "Attack Speed" , "Movement" , "Special"};

    [SerializeField] Tutorial tutorial;


    void Awake()
    {
        backgroundMusic.time = audioProgress;

        menuAudio = GetComponent<AudioSource>();
        player = FindAnyObjectByType<Player>();
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        tutorial = FindAnyObjectByType<Tutorial>();
        mobileButton = FindObjectOfType<OnScreenButton>();

        level = 1;
        toNextLevel = 100;
        currentWave = 0;
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("tutorialStage") == 1) tutorial.SendTutorial();
        else tutorial.InstantExitTutorial();
        if (!pauseWaves) NextWave(); //immediately spawn next wave at start
    }

    void Update()
    {
        if (spawnTimer > spawnRate && !pauseWaves) {
            NextWave();
            spawnTimer = 0;
        } else if (!pauseWaves) spawnTimer += Time.deltaTime;
    }

    public IEnumerator StopTime(float seconds)
    {
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            enemy.FreezeUnit(seconds);

        backgroundMusic.pitch = 0.8f;
        pauseWaves = true;

        yield return new WaitForSeconds(seconds);
        pauseWaves = false;
        backgroundMusic.pitch = 1f;
        menuAudio.Stop();
    }

    public void UpdateScore(int score, bool kill = true)
    {
        if (kill) killCount++;
        currentScore += score;
        scoreOutput.text = currentScore.ToString();
    }

    void NextWave()
    {
        SetupWave();
        foreach (Entity entity in spawnGroup)
        {
            Vector2 spawnPos = player.transform.position;
            while (Math.Abs(spawnPos.x - player.transform.position.x) < closestSpawnPosX && Math.Abs(spawnPos.y - player.transform.position.y) < closestSpawnPosY)
                spawnPos += new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-1f, 1f));

            Spawn(entity, spawnPos);
        }
    }

    void SetupWave()
    {
        spawnGroup.Clear(); //clear previous wave info
        currentWave++;
        waveIntensity *= waveIntensityMultiplier;
        SetupNextWaveWithIntensity(waveIntensity);
    }

    void AddToNextWave(Enemy enemyType, int amount)
    {
        for (int i = 0; i < amount; i++)
            spawnGroup.Add(enemyType);
    }

    void SetupNextWaveWithIntensity(float intensity) //algorithm to randomize next spawn group with a given intensity. goes through the enemy unit list for that level, and each enemy had their own intensity multiplier as well (stronger enemies usually have lower intensity)
    {
        for(int i = 0; i < enemyList.Count; i++)
            AddToNextWave(enemyList[i], (int)(intensity * enemyUnitSpawnIntensity[i]));
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
        StartCoroutine(FreezeUI());
        mobileButton.gameObject.SetActive(false);
        paused = true;

        Time.timeScale = 0;
        level++;
        experience = 0;
        toNextLevel += toNextLevel/2;
        experienceUI.fillAmount = 0;

        SetupLevelUpOptions();
        levelUpWindow.SetActive(true);
        invisibleButton.Select();
    }

    IEnumerator FreezeUI()
    {
        freezeUI = true;
        yield return new WaitForSecondsRealtime(0.5f);
        freezeUI = false;
    }

    void SetupLevelUpOptions()
    {
        option1Value = UnityEngine.Random.Range(1, 5);
        do { option2Value = UnityEngine.Random.Range(1, 5); } while (option2Value == option1Value); //make sure we get 2 different options
        option1Text.text = optionsList[option1Value-1];
        option1Text.transform.GetChild(0).GetComponent<Text>().text = optionsList[option1Value - 1];
        option2Text.text = optionsList[option2Value-1];
        option2Text.transform.GetChild(0).GetComponent<Text>().text = optionsList[option2Value - 1];
    }

    public void LevelUpButton(int buttonIndex)
    {
        if (!freezeUI)
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
            StartCoroutine(player.FreezeAttack(0.05f)); //freeze attack so that the game doesnt send an input when you release the key for selecting an option
        }
    }

    void SelectUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 1: //power
                player.UpgradeStat(playerStats.strength, 3);
                player.UpgradeStat(playerStats.pushForce, 40);
                break;
            case 2: //attack speed
                player.UpgradeStat(playerStats.attackSpeed, 0.25f);
                break;
            case 3: //movement
                player.UpgradeStat(playerStats.moveSpeed, 0.20f);
                break;
            case 4: //special
                player.UpgradeStat(playerStats.specialCooldown, 2.5f);
                player.UpgradeStat(playerStats.specialAttackCount, 1);
                break;
        }
        ContinueGame();
    }

    public void ContinueGame()
    {
        mobileButton.gameObject.SetActive(false);
        paused = false;
        levelUpWindow.SetActive(false);
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        SaveProgress();
        ScoreKeeper.score = currentScore;
        SceneManager.LoadScene("Menu");
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt("totalKills", PlayerPrefs.GetInt("totalKills") + killCount);
        PlayerPrefs.SetInt("totalPunches", PlayerPrefs.GetInt("totalPunches") + punchCount);
        PlayerPrefs.SetInt("totalSpecialAttacks", PlayerPrefs.GetInt("totalSpecialAttacks") + specialAttackCount);
        PlayerPrefs.SetInt("totalItemsGrabbed", PlayerPrefs.GetInt("totalItemsGrabbed") + itemGrabCount);
        if (killCount > PlayerPrefs.GetInt("maxKillsInOneRun")) PlayerPrefs.SetInt("maxKillsInOneRun", killCount);
        if (currentScore > PlayerPrefs.GetInt("highestScore")) PlayerPrefs.SetInt("highestScore", currentScore);
        PlayerPrefs.Save();
    }
}
