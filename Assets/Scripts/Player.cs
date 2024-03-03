using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum playerStats { hitpoints, strength, pushForce, moveSpeed, attackSpeed, attackCharges, attackRange, specialDamage, specialAttackCount, specialCooldown, specialRange, dodgeChance, criticalChance, criticalDamage}
public class Player : Unit
{
    PlayerUIHandler ui;

    //audio
    AudioSource levelMusic;
    [SerializeField] protected AudioClip punchSFX;
    [SerializeField] protected AudioClip missSFX;
    [SerializeField] protected AudioClip specialSFX;
    [SerializeField] protected AudioClip deathSFX;

    public List<float> baseAttackCD = new List<float>(); //inclues upgrades, excludes powerups

    public float specialCD;
    public float specialTimer;
    public float specialChannel;
    bool appliedSpecialOffset;
    public bool channelingSpecial;
    public float specialRange;
    public int specialCharges;

    public int comboAmount;
    public float maxComboCDBoost;
    public float comboCDBoost;

    public float invincible;
    public bool hasPowerUp;
    [SerializeField] float currentFrenzyTimer;//for feedback

    Coroutine previousFreeze; //to stop previous freeze coroutine
    Coroutine previousFreezeRotation;
    bool facingRight;

    ScrollingBackground scrollingBackground;
    bool colliding;
    Coroutine timeStopRoutine;

    float[] powerUpDuration = new float[7] { 0, 0, 0, 0, 0, 0, 0 };
    PowerUpType[] activePowerUps = new PowerUpType[7] { PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None };

    protected override void Awake()
    {
        base.Awake();

        levelMusic = GameObject.FindGameObjectWithTag("Floor").GetComponent<AudioSource>();
        ui = GetComponent<PlayerUIHandler>();
        scrollingBackground = FindObjectOfType<ScrollingBackground>();

        comboCDBoost = 1;
        for (int i = 0; i < attackCD.Count; i++) //prepare attack charges and timers
        {
            baseAttackCD.Add(attackCD[i]);
            attackTimer.Add(0);
        }
    }

    void Start()
    {
        switch(SkinSelector.currentSkin)
        {
            default: break;
            case 1: //warrior
                UpgradeStat(playerStats.attackSpeed, 0.20f);
                break;
            case 2: //machine
                UpgradeStat(playerStats.attackSpeed, 0.40f);
                UpgradeStat(playerStats.moveSpeed, -0.20f);
                break;
            case 3: //golden
                UpgradeStat(playerStats.strength, 3);
                UpgradeStat(playerStats.pushForce, 50);
                UpgradeStat(playerStats.attackSpeed, 0.20f);
                UpgradeStat(playerStats.specialCooldown, -2.5f);
                UpgradeStat(playerStats.specialAttackCount, -1);
                break;
            case 4: //wizard
                UpgradeStat(playerStats.specialCooldown, 5.0f);
                UpgradeStat(playerStats.specialAttackCount, 2);
                UpgradeStat(playerStats.attackSpeed, -0.20f);
                break;
            case 5: //super
                UpgradeStat(playerStats.strength, 6);
                UpgradeStat(playerStats.pushForce, 150);
                break;

        }
    }

    void Update()
    {
        if (!game.paused)
        {
            ChargeCDs();
            if (hasPowerUp) CheckPowerUp(); //checks for this bool so that it doesnt have to go through the array all the time
            if (!freeze && !dead) CheckAttackTargets(); //enable hit indicator and face for closest enemy (if within attack range)
        }

        if (!channelingSpecial) specialChannel = 0;
    }

    public void ChannelSpecial(float inputHoldOffset = 0f)
    {
        if (specialTimer >= specialCD)
        {
            if (!appliedSpecialOffset)
            {
                appliedSpecialOffset = true;
                specialChannel = inputHoldOffset;
            }
            channelingSpecial = true;
            specialChannel += Time.deltaTime;
        }
    }

    void ChargeCDs()
    {
        if (specialTimer <= specialCD)
            specialTimer += Time.deltaTime;
        else if (PlayerPrefs.GetInt("tutorialStage") == 2)
            ui.SendTutorial();
        else
            ui.SpecialReady(true);

        for (int i = 0; i < attackTimer.Count; i++)
            attackTimer[i] += Time.deltaTime * comboCDBoost;

        if (invincible > 0)
            invincible -= Time.deltaTime;
        else invincible = 0;
    }

    public void Move(float x, float y)
    {
        transform.position += new Vector3(x, y) * moveSpeed * Time.deltaTime;
        if (!colliding) scrollingBackground.ScrollBackground(x, y, moveSpeed * StageSelector.scrollMultiplier); //scroll background

        if (transform.position.x < -150) transform.position += Vector3.right * 0.01f; //check for out of bounds
        if (transform.position.x > +150) transform.position -= Vector3.right * 0.01f;
        if (transform.position.y < -150) transform.position += Vector3.up * 0.01f;
        if (transform.position.y > +150) transform.position -= Vector3.up * 0.01f;

        if (!freezeRotation)
        {
            if (x > 0) ChangeFaceDirection(true);
            else if (x < 0) ChangeFaceDirection(false);
        }

        if (x == 0 && y == 0)
            anim.SetBool("move", false);
        else
            anim.SetBool("move", true);
    }

    public void CheckAttack()
    {
        if (specialChannel > 0.4f)
            StartCoroutine(SpecialAttack());
        else if (!channelingSpecial)
        {
            specialChannel = 0;
            appliedSpecialOffset = false;

            for (int i = attackTimer.Count - 1; i >= 0; i--)
                if (attackTimer[i] >= attackCD[i])
                {
                    Attack(i);
                    break;
                }
        }
    }

    Enemy CheckAttackTargets()
    {
        Enemy target = null;
        float targetProximity = 200f;
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.HitIndicator(false);
            if (!enemy.dead)
            {
                float proximity = Vector2.Distance(enemy.transform.position, transform.position) / enemy.size; //the bigger the enemy, the lower range is required
                if (proximity < targetProximity && attackRange > proximity) //if is closer than the previous iteration and within player range
                {
                    targetProximity = proximity;
                    target = enemy;
                }
            }
        }
        if (target)
        {
            target.HitIndicator(true); //enable hit indicator for this enemy
            if (anim.GetBool("move") == false) //if player is idle, face the closest enemy
                FaceTarget(target.gameObject);
        }
        return target;
    }

    Enemy GetClosestEnemy()
    {
        Enemy target = null;
        float targetProximity = 200f;
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            if (!enemy.dead)
            {
                float proximity = Vector2.Distance(enemy.transform.position, transform.position) / enemy.size;
                if (proximity < targetProximity) //if is closer than the previous iteration
                {
                    targetProximity = proximity;
                    target = enemy; //get the closest enemy
                }
            }
        } return target;
    }

    void FaceTarget(GameObject target)
    {
        if (target.transform.position.x < transform.position.x) ChangeFaceDirection(false); //if left, then flip sprite
        else if (target.transform.position.x > transform.position.x) ChangeFaceDirection(true);
    }

    void ChangeFaceDirection(bool right) //true for left; false for right
    {
        if (right) { sr.flipX = false; facingRight = true; }
        else { sr.flipX = true; facingRight = false; }
    }
    void Attack(int attackIndex)
    {
        if (previousFreeze != null) StopCoroutine(previousFreeze); //in case its overlapping with another instance of freeze
        previousFreeze = StartCoroutine(Freeze(0.1f)); //freeze player when attacking

        if (previousFreezeRotation != null) StopCoroutine(previousFreezeRotation); //same but with sprite flipX freeze
        previousFreezeRotation = StartCoroutine(FreezeRotation(0.28f));

        anim.SetTrigger("attack");
        attackTimer[attackIndex] = 0; //consume 1 attack, regardless of hit or miss
        Enemy target = CheckAttackTargets();

        if (target != null)
        {
            FaceTarget(target.gameObject); //face the enemy on attack
            target.TakeHit(strength, gameObject, pushForce);
            PlayAudio(punchSFX);
            comboAmount++;
            specialTimer += 1f;
        }
        else
        {
            if (GetClosestEnemy()) FaceTarget(GetClosestEnemy().gameObject); //if miss, player will face closest enemy that is not within range
            PlayAudio(missSFX); //target is still null means it failed to find a valid target, so it's a miss
            comboAmount = 0;
        }
        game.punchCount++;
        StartCoroutine(FreezeAttack(0.1f));
    }

    IEnumerator SpecialAttack()
    {
        if (game.paused) yield return null;
        anim.SetTrigger("special");
        FaceTarget(GetClosestEnemy().gameObject);
        specialTimer = 0f;
        specialChannel = 0;
        appliedSpecialOffset = false;
        freezeRotation = true;
        ui.SpecialReady(false);
        bool hit;
        for (int i = 0; i < specialCharges; i++)
        {
            hit = false;
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                if (Vector2.Distance(transform.position, enemy.transform.position) <= attackRange * specialRange)
                {
                    if ((facingRight && enemy.transform.position.x > transform.position.x) || (!facingRight && enemy.transform.position.x < transform.position.x)) //only attack enemies to the right or to the left
                    {
                        hit = true;
                        comboAmount++;
                        enemy.TakeHit(strength, gameObject, pushForce);
                    }
                }
                if (hit || i == 0) PlayAudio(punchSFX);
            }
            yield return new WaitForSeconds(0.2f);
        }
        freezeRotation = false;

        game.specialAttackCount++;
    }

    public override void TakeHit(int damage, GameObject hitter, float pushForce = 0f)
    {
        if (game.paused || invincible > 0) return;

        if (previousFreeze != null) StopCoroutine(previousFreeze); //check for multiple freeze rotation instances and end the previous one
        previousFreeze = StartCoroutine(Freeze(0.2f));

        specialChannel = 0; //reset special channeling if hit
        comboAmount = 0;
        UpdateHealth(-damage);
        KnockBack(hitter, pushForce);

        Vector2 direction = (transform.position - hitter.transform.position).normalized; //move background
        StartCoroutine(scrollingBackground.ScrollBackgroundOverTime(direction.x, direction.y, (pushForce/200), 0.1f));

        if (invincible <= 0.5) invincible = 0.5f; //only update if not already under this buff in case the player had more than 0.5sec of invincible left

        if (damage > 1) anim.SetTrigger("glowHit");
        else anim.SetTrigger("takeHit");

        if (hitpoints <= 0) { hitpoints = 0; PlayAudio(deathSFX); }
        else PlayAudio(punchSFX);
    }

    protected override void UpdateHealth(int amount)
    {
        base.UpdateHealth(amount);
        if (hitpoints <= 0) { hitpoints = 0; StartCoroutine(PlayerDeath()); }
        ui.CheckHitpoints();
    }

    IEnumerator PlayerDeath()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        levelMusic.volume /= 2;
        if (previousFreeze != null) StopCoroutine(previousFreeze);
        if (previousFreezeRotation != null) StopCoroutine(previousFreezeRotation);

        invincible = 10f;
        freeze = true;
        dead = true;
        anim.SetBool("death", true);
        PlayAudio(deathSFX);
        yield return new WaitForSeconds(2f);
        game.GameOver();
    }

    public override void ActivatePowerUp(PowerUp powerUp)
    {
        if (dead) return; //in case player grabs a power up after dying

        //consumable power ups here, check UpdatePowerUpEffect() for temporary boost power ups
        switch (powerUp.powerUpType)
        {
            case PowerUpType.Health:
                UpdateHealth(1);
                specialTimer += specialCD / 4;
                return;
            case PowerUpType.FullRecovery:
                UpdateHealth(maxHitpoints);
                specialTimer += specialCD;
                return;
            case PowerUpType.Experience:
                game.GainExperience(50);
                return;
            case PowerUpType.TimeStop:
                if (timeStopRoutine != null) StopCoroutine (timeStopRoutine);
                timeStopRoutine = StartCoroutine(game.StopTime(powerUp.duration));
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
                switch (activePowerUps[i]) //calculate feedback elements
                {   
                    case PowerUpType.Frenzy:
                        ui.attackCDOutlines[0].effectColor = new Color32(200, 50, 0, (byte)(Mathf.Lerp(0, 255, powerUpDuration[i] / 5)));
                        ui.attackCDOutlines[1].effectColor = new Color32(200, 50, 0, (byte)(Mathf.Lerp(0, 255, powerUpDuration[i] / 5)));
                        break;
                }
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
        { //temporary power ups here
            case PowerUpType.Frenzy:
                for (int i = 0; i < attackCD.Count; i++)
                {
                    if (status) attackCD[i] = attackCD[i] / 2;
                    else attackCD[i] = baseAttackCD[i];
                    attackTimer[i] = attackCD[i]; //fully charge next attack when frenzy activates or expires
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
                    baseAttackCD[i] -= value;
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
                specialCD -= value;
                break;
            case playerStats.specialAttackCount:
                specialCharges += (int) value;
                break;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        colliding = true;
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        colliding = false;
    }
}
