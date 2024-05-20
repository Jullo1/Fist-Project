using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public bool paused;
    Player player;
    AudioSource menuAudio;
    AudioSource backgroundMusic;
    [SerializeField] AudioClip selectSFX;

    public static float audioProgress;

    [SerializeField] List<Enemy> enemyList = new List<Enemy>();
    [SerializeField] List<float> enemyUnitSpawnIntensity = new List<float>();
    [SerializeField] float spawnRate;
    public Color32 enemyTint;

    public List<Entity> spawnGroup = new List<Entity>();
    [SerializeField] float waveIntensity;
    [SerializeField] float waveIntensityMultiplier;
    public int currentWave;
    [SerializeField] float closestSpawnPosX;
    [SerializeField] float closestSpawnPosY;
    float spawnTimer;
    public bool pauseWaves;

    [SerializeField] Text scoreOutput;
    [SerializeField] int currentScore;
    bool freezeUI;
    public bool timeStop;

    float experience;
    public float toNextLevel;

    public int killCount;
    public int punchCount;
    public int specialAttackCount;
    public int itemGrabCount;

    [SerializeField] Image experienceUI;
    [SerializeField] Selectable invisibleButton;
    [SerializeField] OnScreenButton mobileButton;
    [SerializeField] Animator levelUpCoin;
    Text lootedCoins;

    [SerializeField] GameObject levelUpWindow;
    [SerializeField] Text option1Text; int option1Value;
    [SerializeField] Text option2Text; int option2Value;
    List<string> optionsList = new List<string>() { "Power" , "Attack Speed" , "Movement" , "Special"};

    Tutorial tutorial;


    void Awake()
    {
        ScoreKeeper.coins = 0;
        ScoreKeeper.score = 0;
        player = FindAnyObjectByType<Player>();
        menuAudio = GetComponent<AudioSource>();
        backgroundMusic = GameObject.FindGameObjectWithTag("Floor").GetComponent<AudioSource>();
        tutorial = FindAnyObjectByType<Tutorial>();
        mobileButton = FindObjectOfType<OnScreenButton>();
        lootedCoins = GameObject.FindGameObjectWithTag("CoinOutput").GetComponent<Text>();

        backgroundMusic.time = audioProgress;

        toNextLevel = 100;
        currentWave = 0;

        ApplyEnemyTint();
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

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    public IEnumerator StopTime(float seconds)
    {
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            enemy.FreezeUnit(seconds, true);

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
        StartCoroutine(UpdateScoreText(score));
    }

    IEnumerator UpdateScoreText(int scoreAmount)
    {
        for (int i = 0; i < scoreAmount; i++)
        {
            currentScore++;
            scoreOutput.text = currentScore.ToString();
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator UpdateExperienceBar(int scoreAmount)
    {
        for (int i = 0; i < scoreAmount; i++)
        {
            if (experienceUI.fillAmount == 1) break;
            experienceUI.fillAmount += 1 / toNextLevel;
            yield return new WaitForSeconds(0.05f);
        }
        if (experience >= toNextLevel) LevelUp();
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
        if (waveIntensity < 3) waveIntensity *= waveIntensityMultiplier;
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
        newEnemy.transform.GetChild(0).gameObject.SetActive(true);
        newEnemy.transform.GetChild(0).GetComponent<SpriteRenderer>().color = enemyTint;
        newEnemy.transform.position = position;
    }

    public void GainExperience(int amount)
    {
        UpdateScore(amount);
        experience += amount;
        StartCoroutine(UpdateExperienceBar(amount)); 
    }

    void LevelUp()
    {
        if (player.dead) return; //cancel if already game over

        StartCoroutine(FreezeUI());
        if (Application.isMobilePlatform) mobileButton.gameObject.SetActive(false);
        paused = true;

        Time.timeScale = 0;
        experience = 0;
        toNextLevel += toNextLevel/2;
        experienceUI.fillAmount = 0;

        SetupLevelUpOptions();
        levelUpWindow.SetActive(true);
        invisibleButton.Select();
        levelUpCoin.SetTrigger("levelUp");
        GainCoins(1);
    }

    public void GainCoins(int amount)
    {
        ScoreKeeper.coins += amount;
        lootedCoins.text = ScoreKeeper.coins.ToString();
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
                player.UpgradeStat(playerStats.pushForce, 75);
                break;
            case 2: //attack speed
                player.UpgradeStat(playerStats.attackSpeed, 0.20f);
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

    void ApplyEnemyTint()
    {
        switch(StageSelector.currentStage)
        {
            case 0: enemyTint = new Color32(0, 0, 0, 0); break;
            case 1: enemyTint = new Color32(100, 125, 40, 175); break;
            case 2: enemyTint = new Color32(180, 140, 85, 220); break;
            case 3: enemyTint = new Color32(150, 130, 100, 150); break;
            case 4: enemyTint = new Color32(170, 170, 170, 50); break;
            case 5: enemyTint = new Color32(150, 200, 225, 150); break;
            case 6: enemyTint = new Color32(125, 125, 125, 150); break;
            case 7: enemyTint = new Color32(150, 50, 50, 135); break;
            case 8: enemyTint = new Color32(35, 35, 35, 125); break;
        }
    }

    public void ContinueGame()
    {
        if (Application.isMobilePlatform) mobileButton.gameObject.SetActive(false);
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

        string stageHiscoreKey = "hiscore" + StageSelector.currentStage.ToString();
        if (currentScore > PlayerPrefs.GetInt(stageHiscoreKey))
            PlayerPrefs.SetInt("hiscore" + StageSelector.currentStage.ToString(), currentScore);

        if (killCount > PlayerPrefs.GetInt("maxKillsInOneRun")) PlayerPrefs.SetInt("maxKillsInOneRun", killCount);
        if (currentScore > PlayerPrefs.GetInt("highestScore")) PlayerPrefs.SetInt("highestScore", currentScore);
        PlayerPrefs.Save();
    }
}
