using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : Unit
{
    Player player;
    [SerializeField] int experienceDrop;
    [SerializeField] GameObject furyIcon;
    [SerializeField] SpriteRenderer hitIndicator;
    SpriteRenderer tint;

    [SerializeField] GameObject healthDrop;
    [SerializeField] GameObject fullRecoveryDrop;
    [SerializeField] GameObject frenzyDrop;
    [SerializeField] GameObject experiencePotionDrop;
    [SerializeField] GameObject timeStopDrop;
    TextMeshPro scoreBubble;

    public List<PowerUpType> dropList = new List<PowerUpType>();
    bool spawnCheck = true;
    float aliveTime;
    bool fury;

    public void HitIndicator(bool isCurrentTarget)
    {
        hitIndicator.gameObject.SetActive(isCurrentTarget);
    }

    protected override void Awake()
    {
        base.Awake();

        player = FindAnyObjectByType<Player>();
        scoreBubble = GetComponentInChildren<TextMeshPro>();
        scoreBubble.text = (experienceDrop * StageSelector.scoreMultiplier).ToString();
        attackTimer.Add(0);

        tint = transform.GetChild(2).GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Move();
        CheckStats();
        if (attackTimer[0] <= attackCD[0]) attackTimer[0] += Time.deltaTime;

        if (hitpoints <= 0 && frozenTime <= 0 && !dead && hitQueue <= 0)
            StartCoroutine(Death());
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !freezeAttack) Attack();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (spawnCheck) //to prevent enemies overlapping on spawn
            if (collision.gameObject.tag == "Enemy")
                transform.position += new Vector3(Random.Range(0f,0.5f), Random.Range(0f, 0.5f));

        spawnCheck = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !freezeAttack) Attack();
    }

    void Attack()
    {
        if (frozenTime > 0 || dead) return;

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
            rb.mass = 5;
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            if (player.transform.position.x < transform.position.x) { sr.flipX = true; hitIndicator.flipX = true; tint.flipX = true; }
            else { sr.flipX = false; hitIndicator.flipX = false; tint.flipX = false ; }
        }
    }

    void CheckStats()
    {
        if (aliveTime > 15) {
            if (!fury) Fury();
        } else if (frozenTime <= 0) aliveTime += Time.deltaTime;
        if (aliveTime > 0.1f)
            spawnCheck = false;

        if (frozenTime <= 0) anim.enabled = true;
        else frozenTime -= Time.deltaTime;
    }

    void Fury()
    {
        fury = true;
        furyIcon.gameObject.SetActive(true);
        moveSpeed *= 1.25f;
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

    protected override void UpdateHealth(int amount)
    {
        base.UpdateHealth(amount);
    }

    IEnumerator Death()
    {
        dead = true;
        CalculateDrop();
        col.isTrigger = true;
        anim.SetBool("dead", true);
        game.GainExperience((int)(experienceDrop*StageSelector.scoreMultiplier));
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
