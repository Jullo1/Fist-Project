using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool paused;
    Player player;
    [SerializeField] List<Enemy> enemyList = new List<Enemy>();

    public List<Entity> spawnGroup = new List<Entity>();
    float waveIntensity;
    public int currentWave;
    [SerializeField] float closestSpawnPos;
    float spawnTimer;

    int level;
    float experience;
    float toNextLevel;
    [SerializeField] Image experienceUI;
    [SerializeField] GameObject levelUpWindow;

    void Awake()
    {
        player = FindAnyObjectByType<Player>();
    }

    void Start()
    {
        level = 1;
        toNextLevel = 50;
        currentWave = 0;
        waveIntensity = 0.5f;
        NextWave(); //immediately spawn next wave at start
    }

    void Update()
    {
        if (spawnTimer > 5) {
            NextWave();
            spawnTimer = 0;
        } else spawnTimer += Time.deltaTime;
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
        waveIntensity *= 1.05f;
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
        AddToNextWave(enemyList[1], (int) (intensity * 3));
        AddToNextWave(enemyList[2], (int) (intensity * 1));
    }

    void Spawn(Entity entity, Vector2 position)
    {
        GameObject newEnemy = Instantiate(entity.gameObject);
        newEnemy.transform.position = position;
    }

    public void GainExperience(int amount)
    {
        experience += amount;
        if (experience >= toNextLevel) LevelUp();
        else experienceUI.fillAmount = experience / toNextLevel;
    }

    void LevelUp()
    {
        paused = true;
        Time.timeScale = 0;
        level++;
        experience = 0;
        toNextLevel += toNextLevel;
        experienceUI.fillAmount = 0;
        levelUpWindow.SetActive(true);
    }

    public void SelectUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 1: //punch faster
                player.UpgradeStat(playerStats.attackSpeed);
                break;
            case 2: //punch harder
                player.strength++;
                player.pushForce += 0.02f;
                break;
            case 3: //move faster
                player.moveSpeed += 0.2f;
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
        SceneManager.LoadScene("Main");
    }
}
