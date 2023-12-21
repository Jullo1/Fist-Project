using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public enum playerStats { hitpoints, strength, pushForce, moveSpeed, attackSpeed, attackCharges, attackRange, specialDamage, specialCooldown, specialRange, dodgeChance, criticalChance, criticalDamage}
public class Player : Unit
{
    //mobile inputs
    bool usingMobileControls;
    List<Touch> touches = new List<Touch>();
    List<float> touchTimer = new List<float>();
    List<bool> touchMove = new List<bool>();
    List<bool> touchHold = new List<bool>();
    List<Vector2> initialTouchPos = new List<Vector2>();

    [SerializeField] PlayerInput mobileControls;
    [SerializeField] Canvas mobileControlsUI;
    [SerializeField] Image leftStickImage;
    [SerializeField] Image leftStickBackground;

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

    float inputX;
    float inputY;

    [SerializeField] int comboAmount;
    [SerializeField] Text comboUI;
    [SerializeField] float maxComboCDBoost;
    public float comboCDBoost;
    [SerializeField] Image comboProgressBar;

    float invincible;
    bool hasPowerUp;
    float currentFrenzyTimer;//for feedback
    Coroutine previousFreeze; //to stop previous freeze coroutine
    Coroutine previousFreezeRotation;
    /*remove serialize after testing*/
    [SerializeField] float[] powerUpDuration = new float[7] { 0, 0, 0, 0, 0, 0, 0 };
    [SerializeField] PowerUpType[] activePowerUps = new PowerUpType[7] { PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None, PowerUpType.None };

    void Start()
    {
        comboCDBoost = 1;
        for (int i = 0; i < attackCD.Count; i++) //prepare attack charges and timers
        {
            baseAttackCD.Add(attackCD[i]);
            attackTimer.Add(0);
        }

        if (!Application.isMobilePlatform && !Application.isEditor)
            mobileControlsUI.gameObject.SetActive(false); //completely disable onscreen controls
    }

    void Update()
    {
        if (!game.paused)
        {
            if (!freeze)
            {
                Vector2 mobileInput = mobileControls.actions["Move"].ReadValue<Vector2>(); //movement
                if (Input.GetKey(KeyCode.D) || mobileInput.x > 0.45f) inputX = 1; //right
                else if (Input.GetKey(KeyCode.A) || mobileInput.x < -0.45f) inputX = -1; //left
                if (Input.GetKey(KeyCode.W) || mobileInput.y > 0.45f) inputY = 1; //up
                else if (Input.GetKey(KeyCode.S) || mobileInput.y < -0.45f) inputY = -1; //down

                Move(inputX, inputY);
                inputX = 0; inputY = 0;

                if (Input.touchCount > 0) //touchscreen
                {
                    usingMobileControls = true;
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        if (touches.Count < Input.touchCount) //first fill the arrays
                        {
                            touches.Add(new Touch());
                            touchTimer.Add(0);
                            touchMove.Add(false);
                            touchHold.Add(false);
                            initialTouchPos.Add(new Vector2());
                        }

                        touches[i] = Input.GetTouch(i);
                        touchTimer[i] += Time.deltaTime;

                        if (touches[i].phase == UnityEngine.TouchPhase.Began)
                            initialTouchPos[i] = touches[i].position; //save initial position

                        if ((initialTouchPos[i] - touches[i].position).magnitude > 30)
                            touchMove[i] = true; //swipe
                        else if (touchTimer[i] > 0.2f)
                            touchHold[i] = true; //hold

                        if (touchHold[i] && !touchMove[i] && specialTimer >= specialCD)
                            preparingSpecial += Time.deltaTime; //channel special if hold
                        else
                        {
                            for (int j = 0; j < Input.touchCount; j++) //check for all touches, make sure none is hold before resetting special
                            {
                                if (touchHold[j] || !touchMove[j]) break;
                                else if (j == Input.touchCount-1) //last iteration, found no hold key
                                    preparingSpecial = 0.2f; //preparing special begins at 0.20 for mobile, due to the 0.20sec check for hold tap
                            }
                        }

                        if (touches[i].phase == UnityEngine.TouchPhase.Ended)
                        {
                            if (!touchMove[i]) CheckAttack(); //if  didn't move, then it was a tap or hold, so send attack
                            touchTimer[i] = 0; //reset defaults for next touch
                            touchMove[i] = false;
                            touchHold[i] = false;
                        }
                    }
                }
                else if (!usingMobileControls) //keyboard
                {
                    if (Input.GetKeyUp(KeyCode.Space)) CheckAttack(); //attack
                    if (Input.GetKey(KeyCode.Space) && specialTimer >= specialCD) preparingSpecial += Time.deltaTime; //special
                    else preparingSpecial = 0;
                }
                ChargeCDs();
                if (hasPowerUp) CheckPowerUp(); //checks for this bool so that it doesnt have to go through the array all the time
                UpdateUI();
            }
        }
    }

    void ShowOnscreenControls(bool show)
    {
        if (show)
        {
            leftStickImage.color = new Color32(255, 255, 255, 200);
            leftStickBackground.color = new Color32(0, 0, 0, 100);
        }
        else
        {
            leftStickImage.color = new Color32(255, 255, 255, 0);
            leftStickBackground.color = new Color32(0, 0, 0, 0);
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < attackCDUI.Count; i++)
            attackCDUI[i].fillAmount = attackTimer[i] / attackCD[i];

        specialUI.fillAmount = specialTimer / specialCD;
        
        if (preparingSpecial > 0.20f) //small delay before feedback, in case player intended to tap
        {
            specialTriggerUI1.fillAmount = (preparingSpecial - 0.20f) / 0.10f;
            specialTriggerUI2.fillAmount = (preparingSpecial - 0.20f) / 0.10f;
        }
        else
        {
            specialTriggerUI1.fillAmount = 0;
            specialTriggerUI2.fillAmount = 0;
        }

        if (comboAmount > 1)
        {
            comboProgressBar.fillAmount = comboAmount / maxComboCDBoost;
            comboUI.text = "x" + comboAmount.ToString();
            if (comboAmount >= maxComboCDBoost) //max combo boost achieved
            {
                comboUI.color = new Color32(250, 120, 0, 255);
                comboUI.fontSize = 40;
            }
            else if (comboAmount >= maxComboCDBoost / 2)
            {
                comboUI.color = new Color32(240, 180, 0, 255);
            }
            else if (comboAmount >= maxComboCDBoost / 4)
            {
                comboUI.color = new Color32(240, 200, 140, 255);
                comboUI.fontSize = 36;
            }
            else
                comboUI.color = new Color32(255, 255, 255, 255);
        }
        else //if it's 0, hide combo count and bar
        {
            comboUI.text = "";
            comboUI.fontSize = 34;
            comboProgressBar.transform.parent.parent.gameObject.SetActive(false);
        }
        UpdateComboStats();
        if (!freeze && !dead) CheckAttackTargets(); //enable hit indicator and face for closest enemy (if within attack range)
    }

    protected override void CheckHitpoints()
    {
        if (!dead)
        {
            foreach (Image bar in hitpointsUI)
                bar.gameObject.SetActive(true);

            for (int i = hitpointsUI.Count; i > hitpoints; i--)
                hitpointsUI[i - 1].gameObject.SetActive(false);
        }
    }

    void ChargeCDs()
    {
        if (specialTimer <= specialCD)
            specialTimer += Time.deltaTime;

        for (int i = 0; i < attackTimer.Count; i++)
        {
            /*if (attackTimer[i] < attackCD[i])
            {*/
                attackTimer[i] += Time.deltaTime * comboCDBoost;
                //return; //end here so that the next slider is not unstuck
            //}
        }

        for (int i = 0; i < attackTimer.Count; i++) //unstuck timers if both are 0
            attackTimer[i] += 0.0001f;

        if (invincible > 0)
            invincible -= Time.deltaTime;
        else invincible = 0;
    }

    void Move(float x, float y)
    {
        transform.position += new Vector3(x, y) * moveSpeed * Time.deltaTime;

        if (transform.position.x < -150) transform.position += Vector3.right * 0.01f; //check for out of bounds
        if (transform.position.x > +150) transform.position -= Vector3.right * 0.01f;
        if (transform.position.y < -150) transform.position += Vector3.up * 0.01f;
        if (transform.position.y > +150) transform.position -= Vector3.up * 0.01f;

        if (!freezeRotation)
        {
            if (x > 0) sr.flipX = false;
            else if (x < 0) sr.flipX = true;
        }

        if (x == 0 && y == 0)
            anim.SetBool("move", false);
        else
            anim.SetBool("move", true);
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
        if (target.transform.position.x < transform.position.x) sr.flipX = true; //if left, then flip sprite
        else if (target.transform.position.x > transform.position.x) sr.flipX = false;
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
            FaceTarget(GetClosestEnemy().gameObject); //if miss, player will face closest enemy that is not within range
            PlayAudio(missSFX); //target is still null means it failed to find a valid target, so it's a miss
            comboAmount = 0;
        }
    }

    void SpecialAttack()
    {
        if (game.paused) return;
        anim.SetTrigger("special");
        FaceTarget(GetClosestEnemy().gameObject);
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

        if (previousFreeze != null) StopCoroutine(previousFreeze); //check for multiple freeze rotation instances and end the previous one
        previousFreeze = StartCoroutine(Freeze(0.1f));

        if (usingMobileControls) preparingSpecial = 0.2f; //reset special channeling if hit
        else preparingSpecial = 0;

        comboAmount = 0;
        UpdateHealth(-damage);
        KnockBack(hitter, pushForce);
        if (invincible <= 0.5) invincible = 0.5f; //only update if not already under this buff in case the player had more than 0.5sec of invincible left

        anim.SetTrigger("takeHit");
        PlayAudio(punchSFX);
    }

    protected override void UpdateHealth(int amount)
    {
        base.UpdateHealth(amount);
        if (hitpoints <= 0) StartCoroutine(PlayerDeath());
    }

    IEnumerator PlayerDeath()
    {
        if (previousFreeze != null) StopCoroutine(previousFreeze);
        if (previousFreezeRotation != null) StopCoroutine(previousFreezeRotation);

        invincible = 10f;
        freeze = true;
        dead = true;
        anim.SetBool("death", true);
        yield return new WaitForSeconds(2f);
        game.GameOver();
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
            specialTimer += specialCD;
            return;
        }
        else if (powerUp.powerUpType == PowerUpType.Health)
        {
            UpdateHealth(1);
            specialTimer += specialCD / 4;
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
                        attackCDUI[0].color = new Color32((byte)(Mathf.Lerp(255, 0, powerUpDuration[i]/10)), 255, 255, 255);
                        attackCDUI[1].color = new Color32((byte)(Mathf.Lerp(255, 0, powerUpDuration[i]/10)), 255, 255, 255);
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
        {
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
