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
    [SerializeField] List<PowerUp> powerUpList = new List<PowerUp>();

    public List<Entity> nextSpawnGroup = new List<Entity>();
    public int currentWave;
    [SerializeField] float closestSpawnPos;

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
        toNextLevel = 100;
    }

    void Update()
    {
        if (FindObjectsOfType<Enemy>().Length == 0)
            NextWave();
    }

    void NextWave()
    {
        currentWave++;
        foreach (Entity entity in nextSpawnGroup)
        {
            Vector2 spawnPos = player.transform.position;
            while (Vector2.Distance(spawnPos, player.transform.position) < closestSpawnPos)
                spawnPos += new Vector2(Random.Range(-7, 7), Random.Range(-5, 5));

            Spawn(entity, spawnPos);
        } SetupNextSpawn();
    }

    void SetupNextSpawn()
    {
        nextSpawnGroup.Clear();
        switch (currentWave)
        {
            case 1:
                for (int i = 0; i < 3; i++)
                    nextSpawnGroup.Add(enemyList[0]);
                break;
            case 2:
                for (int i = 0; i < 3; i++)
                    nextSpawnGroup.Add(enemyList[0]);
                for (int i = 0; i < 1; i++)
                    nextSpawnGroup.Add(enemyList[1]);
                break;
            case 3:
                for (int i = 0; i < 2; i++)
                    nextSpawnGroup.Add(enemyList[1]);
                break;
            case 4:
                for (int i = 0; i < 5; i++)
                    nextSpawnGroup.Add(enemyList[0]);
                break;
            case 5:
                for (int i = 0; i < 4; i++)
                    nextSpawnGroup.Add(enemyList[0]);
                for (int i = 0; i < 1; i++)
                    nextSpawnGroup.Add(enemyList[2]);
                break;
            case 6:
                for (int i = 0; i < 4; i++)
                    nextSpawnGroup.Add(enemyList[1]);
                break;
            case 7:
                for (int i = 0; i < 2; i++)
                    nextSpawnGroup.Add(enemyList[2]);
                break;
            case 8:
                for (int i = 0; i < 5; i++)
                    nextSpawnGroup.Add(enemyList[0]);
                for (int i = 0; i < 3; i++)
                    nextSpawnGroup.Add(enemyList[1]);
                break;
            case 9:
                for (int i = 0; i < 10; i++)
                    nextSpawnGroup.Add(enemyList[0]);
                break;
        }
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
        experienceUI.fillAmount = 1;
        levelUpWindow.SetActive(true);
    }

    public void SelectUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 1: //punch faster
                for (int i = 0; i < player.attackCD.Count; i++)
                    player.attackCD[i] /= 1.1f;
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
