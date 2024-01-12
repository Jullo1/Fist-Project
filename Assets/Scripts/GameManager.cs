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
    [SerializeField] OnScreenStick leftStick;
    AudioSource menuAudio;
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioClip selectSFX;

    [SerializeField] List<Enemy> enemyList = new List<Enemy>();

    public List<Entity> spawnGroup = new List<Entity>();
    [SerializeField] float waveIntensity;
    public int currentWave;
    [SerializeField] float closestSpawnPosX;
    [SerializeField] float closestSpawnPosY;
    float spawnTimer;
    public bool pauseWaves;

    ScoreKeeper scoreKeeper;
    [SerializeField] Text scoreOutput;
    int currentScore;
    bool freezeUI;

    int level;
    float experience;
    public float toNextLevel;
    [SerializeField] Image experienceUI;
    [SerializeField] Selectable invisibleButton;

    [SerializeField] GameObject levelUpWindow;
    [SerializeField] Text option1Text; int option1Value;
    [SerializeField] Text option2Text; int option2Value;
    List<string> optionsList = new List<string>() { "Power" , "Attack Speed" , "Movement" , "Special"};

    [SerializeField] Tutorial tutorial;

    void Awake()
    {
        player = FindAnyObjectByType<Player>();
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        menuAudio = GetComponent<AudioSource>();
        tutorial = FindAnyObjectByType<Tutorial>();
    }

    void Start()
    {
        if (ScoreKeeper.currentTutorialNumber == 0) tutorial.SendTutorial();
        else tutorial.gameObject.SetActive(false);

        level = 1;
        toNextLevel = 100;
        currentWave = 0;
        waveIntensity = 0.9f;
        if (!pauseWaves) NextWave(); //immediately spawn next wave at start
    }

    void Update()
    {
        if (spawnTimer > 6 && !pauseWaves) {
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
            while (Math.Abs(spawnPos.x - player.transform.position.x) < closestSpawnPosX && Math.Abs(spawnPos.y - player.transform.position.y) < closestSpawnPosY)
                spawnPos += new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-1f, 1f));

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
        AddToNextWave(enemyList[0], (int) (intensity * 3.5f));
        AddToNextWave(enemyList[1], (int) (intensity * 1.0f));
        AddToNextWave(enemyList[2], (int) (intensity * 1.0f));
        AddToNextWave(enemyList[3], (int) (intensity * 1.25f));
        AddToNextWave(enemyList[4], (int) (intensity * 0.75f));
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
            case 1: //punch harder
                player.UpgradeStat(playerStats.strength, 3);
                player.UpgradeStat(playerStats.pushForce, 80);
                break;
            case 2: //punch faster
                player.UpgradeStat(playerStats.attackSpeed, 0.25f);
                break;
            case 3: //move faster
                player.UpgradeStat(playerStats.moveSpeed, 0.25f);
                break;
            case 4: //faster special
                player.UpgradeStat(playerStats.specialCooldown, 2.5f);
                break;
        }
        ContinueGame();
    }

    public void ContinueGame()
    {
        paused = false;
        levelUpWindow.SetActive(false);
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        ScoreKeeper.score = currentScore;
        SceneManager.LoadScene("Menu");
    }
}
