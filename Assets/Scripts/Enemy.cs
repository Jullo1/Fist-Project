using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    Player player;
    [SerializeField] int experienceDrop;
    //[SerializeField] GameObject damageVisual;
    [SerializeField] GameObject furyIcon;
    [SerializeField] SpriteRenderer hitIndicator;

    [SerializeField] GameObject healthDrop;
    [SerializeField] GameObject fullRecoveryDrop;
    [SerializeField] GameObject frenzyDrop;
    [SerializeField] GameObject experiencePotionDrop;
    [SerializeField] GameObject timeStopDrop;

    public List<PowerUpType> dropList = new List<PowerUpType>();
    bool spawnCheck = true;
    float aliveTime;
    bool fury;

    public override void TakeHit(int damage, GameObject hitter, float pushForce = 0f)
    {
        base.TakeHit(damage, hitter, pushForce);
        if (frozenTime <= 0)
            KnockBack(hitter, player.pushForce);//knockback
        else StartCoroutine(DelayedKnockBack(frozenTime, hitter, player.pushForce));
    }

    public void HitIndicator(bool isCurrentTarget)
    {
        hitIndicator.gameObject.SetActive(isCurrentTarget);
    }

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        attackTimer.Add(0);
    }
    void Update()
    {
        Move();
        CheckStats();
        if (attackTimer[0] <= attackCD[0]) attackTimer[0] += Time.deltaTime;

        if (hitpoints <= 0 && frozenTime <= 0 && !dead)
            StartCoroutine(Death());
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !cantAttack) Attack();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (spawnCheck) //to prevent enemies overlapping on spawn
            if (collision.gameObject.tag == "Enemy")
                transform.position += new Vector3(Random.Range(0f,0.5f), Random.Range(0f, 0.5f));

        spawnCheck = false;
    }

    void Attack()
    {
        if (frozenTime > 0) return;

        if (attackTimer[0] >= attackCD[0])
        {
            anim.SetTrigger("attack");
            player.TakeHit(strength, gameObject, pushForce);
            attackTimer[0] = 0;
        }
    }

    void Move()
    {
        if (player && !dead && frozenTime <= 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            if (player.transform.position.x < transform.position.x) { sr.flipX = true; hitIndicator.flipX = true; }
            else { sr.flipX = false; hitIndicator.flipX = false; }
        }
    }

    void CheckStats()
    {
        if (aliveTime > 15) {
            if (!fury) Fury();
        } else if (frozenTime <= 0) aliveTime += Time.deltaTime;
        if (aliveTime > 0.1f)
            spawnCheck = false;

        if (frozenTime <= 0) anim.speed = 1;
        else frozenTime -= Time.deltaTime;
    }

    void Fury()
    {
        fury = true;
        furyIcon.gameObject.SetActive(true);
        moveSpeed *= 1.5f;
    }

    void CalculateDrop()
    {
        int num = Random.Range(0, dropList.Count);
        if (dropList[num] != PowerUpType.None) Drop(dropList[num]);
    }

    void Drop(PowerUpType drop)
    {
        GameObject newDrop = new GameObject();
        switch (drop)
        {
            case PowerUpType.Health:
                newDrop = Instantiate(healthDrop);
                break;
            case PowerUpType.FullRecovery:
                newDrop = Instantiate(fullRecoveryDrop);
                break;
            case PowerUpType.Frenzy:
                newDrop = Instantiate(frenzyDrop);
                break;
            case PowerUpType.Experience:
                newDrop = Instantiate(experiencePotionDrop);
                break;
            case PowerUpType.TimeStop:
                newDrop = Instantiate(timeStopDrop);
                break;
        }
        newDrop.transform.position = transform.position;
    }

    protected void CheckHitpoints()
    {
        /*if (hitpoints <= (maxHitpoints / 2)) //feedback for enemies below half hp
            damageVisual.SetActive(true);
        else
            damageVisual.SetActive(false);*/
    }

    protected override void UpdateHealth(int amount)
    {
        base.UpdateHealth(amount);
        CheckHitpoints();
    }

    IEnumerator Death()
    {
        dead = true;
        //damageVisual.SetActive(false);
        CalculateDrop();
        col.isTrigger = true;
        anim.SetBool("dead", true);
        yield return new WaitForSeconds(0.35f);
        game.GainExperience(experienceDrop);
        Destroy(gameObject);
    }
}
