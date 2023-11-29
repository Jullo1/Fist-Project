using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    /*remove serialize after testing*/[SerializeField] float[] powerUpDuration = new float[5] { 0, 0, 0, 0, 0 };
    [SerializeField] PowerUp[] powerUpList = new PowerUp[5];

    void Start()
    {
        comboCDBoost = 1;
        foreach (float attacks in attackCD)
            attackTimer.Add(0);
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.D)) Move(1, 0);//right
        if (Input.GetKey(KeyCode.A)) Move(-1, 0); //left
        if (Input.GetKey(KeyCode.W)) Move(0, 1);//up
        if (Input.GetKey(KeyCode.S)) Move(0, -1); //down

        if (Input.GetKey(KeyCode.Space) && specialTimer >= specialCD) preparingSpecial += Time.deltaTime; //special
        if (Input.GetKeyUp(KeyCode.Space)) //attack
        {
            if (preparingSpecial >= 0.5f) SpecialAttack();
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
        ChargeCDs();
        UpdateUI();
        if (hasPowerUp) CheckPowerUp(); //checks for this bool so that it doesnt have to go through the array all the time
    }

    void UpdateUI()
    {
        for (int i = 0; i < attackCDUI.Count; i++)
            attackCDUI[i].fillAmount = attackTimer[i] / attackCD[i];

        specialUI.fillAmount = specialTimer / specialCD;
        
        if (preparingSpecial > 0.2f) //small delay before feedback, in case player intended to tap
        {
            specialTriggerUI1.fillAmount = (preparingSpecial - 0.2f) / 0.3f;
            specialTriggerUI2.fillAmount = (preparingSpecial - 0.2f) / 0.3f;
        }
        else
        {
            specialTriggerUI1.fillAmount = 0;
            specialTriggerUI2.fillAmount = 0;
        }

        if (comboAmount > 0)
        {
            comboUI.color = Color.white;
            comboUI.text = "x" + comboAmount.ToString();
            if (comboAmount >= maxComboCDBoost)
                comboUI.color = Color.red;
            else if (comboAmount >= maxComboCDBoost / 2)
                comboUI.color = Color.yellow;
            else if (comboAmount >= maxComboCDBoost / 4)
                comboUI.color = Color.grey;
        }
        else comboUI.text = "";
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
				//attackCDUI[i].transform.SetAsLastSibling();
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

    void UpdateComboStats()
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
        for (int i = 0; i < powerUpList.Length; i++)
            if (!powerUpList[i])
            {
                powerUpList[i] = powerUp;
                powerUpDuration[i] = powerUp.value;
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
                powerUpDuration[i] = 0;
                powerUpList[i] = null;
            }
        }
        hasPowerUp = false; //all powerUpDuration floats are 0
    }
}
