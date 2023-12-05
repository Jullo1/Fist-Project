using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum playerStats { hitpoints, strength, pushForce, moveSpeed, attackSpeed, attackCharges, attackRange, specialDamage, specialCooldown, specialRange, dodgeChance, criticalChance, criticalDamage}
public class Player : Unit
{
    //audio
    [SerializeField] protected AudioClip punchSFX;
    [SerializeField] protected AudioClip missSFX;
    [SerializeField] protected AudioClip specialFX;

    //UI Elements
    [SerializeField] List<Image> hitpointsUI = new List<Image>();
    [SerializeField] List<Image> attackCDUI = new List<Image>();

    [SerializeField] Image specialUI;
    [SerializeField] Image specialTriggerUI1;
    [SerializeField] Image specialTriggerUI2;

    public List<float> baseAttackCD = new List<float>(); //inclues upgrades, excludes powerups

    public float specialCD;
    float specialTimer;
    float preparingSpecial;
    [SerializeField] float specialRange;

    [SerializeField] int comboAmount;
    [SerializeField] Text comboUI;
    [SerializeField] float maxComboCDBoost;
    public float comboCDBoost;

    float invincible;
    bool hasPowerUp;
    /*remove serialize after testing*/[SerializeField] float[] powerUpDuration = new float[7] { 0, 0, 0, 0, 0, 0, 0 };
    [SerializeField] PowerUpType[] activePowerUps = new PowerUpType[7] { PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None };

    void Start()
    {
        comboCDBoost = 1;
        for (int i = 0; i < attackCD.Count; i++) //prepare attack charges and timers
        {
            baseAttackCD.Add(attackCD[i]);
            attackTimer.Add(0);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.D)) Move(1, 0);//right
        if (Input.GetKey(KeyCode.A)) Move(-1, 0); //left
        if (Input.GetKey(KeyCode.W)) Move(0, 1);//up
        if (Input.GetKey(KeyCode.S)) Move(0, -1); //down

        if (Input.GetKey(KeyCode.Space) && specialTimer >= specialCD) preparingSpecial += Time.deltaTime; //special
        if (Input.GetKeyUp(KeyCode.Space)) CheckAttack(); //attack
        ChargeCDs();
        UpdateUI();
        if (hasPowerUp) CheckPowerUp(); //checks for this bool so that it doesnt have to go through the array all the time
    }

    void UpdateUI()
    {
        for (int i = 0; i < attackCDUI.Count; i++)
            attackCDUI[i].fillAmount = attackTimer[i] / attackCD[i];

        specialUI.fillAmount = specialTimer / specialCD;
        
        if (preparingSpecial > 0.15f) //small delay before feedback, in case player intended to tap
        {
            specialTriggerUI1.fillAmount = (preparingSpecial - 0.15f) / 0.15f;
            specialTriggerUI2.fillAmount = (preparingSpecial - 0.15f) / 0.15f;
        }
        else
        {
            specialTriggerUI1.fillAmount = 0;
            specialTriggerUI2.fillAmount = 0;
        }

        if (comboAmount > 1)
        {
            comboUI.text = "x" + comboAmount.ToString();
            if (comboAmount >= maxComboCDBoost) //max combo boost achieved
            {
                comboUI.color = new Color32(250, 120, 0, 255);
                comboUI.fontSize = 14;
            }
            else if (comboAmount >= maxComboCDBoost / 2)
            {
                comboUI.color = new Color32(240, 180, 0, 255);
            }
            else if (comboAmount >= maxComboCDBoost / 4)
            {
                comboUI.color = new Color32(240, 200, 140, 255);
                comboUI.fontSize = 13;
            }
            else
                comboUI.color = new Color32(255, 255, 255, 255);
        }
        else
        {
            comboUI.text = "";
            comboUI.fontSize = 11;
        }
        UpdateComboStats();
    }

    protected override void CheckHitpoints()
    {
        foreach (Image bar in hitpointsUI)
            bar.gameObject.SetActive(true);

        for (int i = hitpointsUI.Count; i > hitpoints; i--)
            hitpointsUI[i-1].gameObject.SetActive(false);
    }

    void ChargeCDs()
    {
        if (specialTimer <= specialCD)
            specialTimer += Time.deltaTime;

        for (int i = 0; i < attackTimer.Count; i++)
        {
            if (attackTimer[i] < attackCD[i])
            {
                attackTimer[i] += Time.deltaTime * comboCDBoost;
                return; //end here so that the next slider is not unstuck
            }
        }

        for (int i = 0; i < attackTimer.Count; i++) //unstuck timers if both are 0
            attackTimer[i] += 0.0001f;

        if (invincible > 0)
            invincible -= Time.deltaTime;
        else invincible = 0;
    }

    void Move(float x, float y)
    {
        if (transform.position.x < -150) transform.position += Vector3.right * 0.01f; //check for out of bounds
        if (transform.position.x > +150) transform.position -= Vector3.right * 0.01f;
        if (transform.position.y < -150) transform.position += Vector3.up * 0.01f;
        if (transform.position.y > +150) transform.position -= Vector3.up * 0.01f;

        transform.position += new Vector3(x, y) * moveSpeed * Time.deltaTime;
    }

    void CheckAttack()
    {
        if (preparingSpecial >= 0.3f) SpecialAttack();
        else
            for (int i = attackTimer.Count - 1; i >= 0; i--)
            {
                if (attackTimer[i] >= attackCD[i])
                {
                    Attack(i);
                    break;
                }
            }

        preparingSpecial = 0f;
    }

    void Attack(int attackIndex)
    {
        if (game.paused) return;
        attackTimer[attackIndex] = 0;
        Enemy target = null;
        float targetProximity = 200f;
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            if (!enemy.dead)
            {
                float proximity = Vector2.Distance(enemy.transform.position, transform.position)/enemy.size; //the bigger the enemy, the lower range is required
                if (proximity < attackRange && proximity < targetProximity) //if within player range and is closer than the previous iteration
                {
                    target = enemy;
                    targetProximity = proximity;
                }
            }
                
        }
        if (target != null)
        {
            target.TakeHit(strength, gameObject, pushForce);
            PlayAudio(punchSFX);
            comboAmount++;
            specialTimer += 1f;
        }
        else
        {
            PlayAudio(missSFX); //target is still null means it failed to find a valid target, so it's a miss
            comboAmount = 0;
        }
    }

    void SpecialAttack()
    {
        if (game.paused) return;
        specialTimer = 0f;
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            if (!enemy.dead)
                if (Vector2.Distance(transform.position, enemy.transform.position) <= attackRange * specialRange)
                {
                    comboAmount++;
                    enemy.TakeHit(strength, gameObject, pushForce);
                }
        }
        PlayAudio(specialFX);
    }

    public override void TakeHit(int damage, GameObject hitter, float pushForce = 0f)
    {
        if (game.paused || invincible > 0) return;

        comboAmount = 0;
        UpdateHealth(-damage);
        KnockBack(hitter, pushForce);
        invincible = 0.5f;

        anim.SetTrigger("takeHit");
        PlayAudio(punchSFX);

    }

    protected override void UpdateHealth(int amount)
    {
        base.UpdateHealth(amount);
        if (hitpoints <= 0) game.GameOver();
    }

    void UpdateComboStats() //comboAmount makes attackTimer floats increase faster (does not affect the cooldown number)
    {
        if (comboAmount == 0)
        {
            comboCDBoost = 1f;
            return;
        }
        if (comboAmount < maxComboCDBoost) comboCDBoost = 1 + (comboAmount / maxComboCDBoost);
        else comboCDBoost = 2;
    }

    public override void ActivatePowerUp(PowerUp powerUp)
    {
        if (powerUp.powerUpType == PowerUpType.FullRecovery)
        {
            UpdateHealth(maxHitpoints);
            specialTimer = specialCD;
            return;
        }

        hasPowerUp = true;
        for (int i = 0; i < activePowerUps.Length; i++)
            if (activePowerUps[i] == PowerUpType.None || activePowerUps[i] == powerUp.powerUpType) //check for first empty powerup slot, or overwrite the same power up type to refresh the duration
            {
                activePowerUps[i] = powerUp.powerUpType;
                powerUpDuration[i] = powerUp.duration;
                UpdatePowerUpEffect(activePowerUps[i], true);
                break;
            }
    }

    void CheckPowerUp()
    {
        for (int i = 0; i < powerUpDuration.Length; i++)
        {
            if (powerUpDuration[i] > 0)
            {
                powerUpDuration[i] -= Time.deltaTime;
                return;
            }
            else
            {
                UpdatePowerUpEffect(activePowerUps[i], false);
                powerUpDuration[i] = 0;
                activePowerUps[i] = PowerUpType.None;
            }
        }
        hasPowerUp = false; //all powerUpDuration floats are 0
    }

    bool HasPowerUp(PowerUpType powerUpType) //checks a certain if powerUp is active
    {
        for (int i = 0; i < activePowerUps.Length; i++)
        {
            if (activePowerUps[i] == powerUpType)
                return true;
        }
        return false;
    }

    void UpdatePowerUpEffect(PowerUpType powerUpType, bool status) //adds powerUp effect when status = true, removes when false
    {
        switch (powerUpType)
        {
            case PowerUpType.Frenzy:
                for (int i = 0; i < attackCD.Count; i++)
                {
                    if (status) attackCD[i] = 0.25f;
                    else attackCD[i] = baseAttackCD[i];
                }
                break;
        }
    }

    public void UpgradeStat(playerStats stat, float value)
    {
        switch (stat)
        {
            case playerStats.attackSpeed:
                for (int i = 0; i < baseAttackCD.Count; i++)
                {
                    baseAttackCD[i] /= value;
                    if (!HasPowerUp(PowerUpType.Frenzy)) //if frenzy is active, attackCD will become baseAttackCD when frenzy expires with CheckPowerUp()
                        attackCD[i] = baseAttackCD[i];
                }
                break;
            case playerStats.strength:
                strength += (int) value;
                break;
            case playerStats.pushForce:
                pushForce += value;
                break;
            case playerStats.moveSpeed:
                moveSpeed += value;
                break;
            case playerStats.specialCooldown:
                specialCD /= value;
                break;
        }
    }
}
