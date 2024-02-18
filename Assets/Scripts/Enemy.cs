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
    [SerializeField] SpriteRenderer tint;

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
    bool chasingPlayer = true;
    string sortingLayerName;

    public void HitIndicator(bool isCurrentTarget)
    {
        hitIndicator.gameObject.SetActive(isCurrentTarget);
        if (isCurrentTarget) ChangeSortingLayer("TargetEnemy");
        else ChangeSortingLayer(sortingLayerName);
    }

    protected void ChangeSortingLayer(string layerName)
    {
        sr.sortingLayerName = layerName;
        tint.sortingLayerName = layerName;
        hitIndicator.sortingLayerName = layerName;
    }

    protected override void Awake()
    {
        base.Awake();

        player = FindAnyObjectByType<Player>();
        scoreBubble = GetComponentInChildren<TextMeshPro>();
        scoreBubble.text = (experienceDrop * StageSelector.scoreMultiplier).ToString();
        attackTimer.Add(0);
        sortingLayerName = sr.sortingLayerName;
    }

    void Update()
    {
        Move();
        CheckStats();
        if (attackTimer[0] <= attackCD[0]) attackTimer[0] += Time.deltaTime;

        if (hitpoints <= 0 && frozenTime <= 0 && !dead && chasingPlayer && hitQueue <= 0)
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
        if (dead)
        {
            if (!chasingPlayer) //enemy expired
            {
                transform.position -= (player.transform.position - transform.position).normalized * moveSpeed * Time.deltaTime;
                if (player.transform.position.x < transform.position.x) { sr.flipX = false; tint.flipX = false; }
                else { sr.flipX = true; tint.flipX = true; }
            }
            return;
        }
        else if (player && frozenTime <= 0) //enemy alive and active
        {
            transform.position += (player.transform.position - transform.position).normalized * moveSpeed * Time.deltaTime;

            if (player.transform.position.x < transform.position.x) { sr.flipX = true; hitIndicator.flipX = true; tint.flipX = true; }
            else { sr.flipX = false; hitIndicator.flipX = false; tint.flipX = false ; }
        }
    }

    void CheckStats()
    {
        if (frozenTime <= 0)
        {
            aliveTime += Time.deltaTime;

            rb.bodyType = RigidbodyType2D.Dynamic;
            anim.enabled = true;

            if (aliveTime > 15) if (!fury) Fury();
            if (aliveTime > 30) StartCoroutine(Death(false));
        } else frozenTime -= Time.deltaTime;

        if (aliveTime > 0.1f) spawnCheck = false;
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

    IEnumerator Death(bool triggerRewards = true)
    {
        col.isTrigger = true;
        dead = true;
        sr.sortingLayerName = "DeadEnemy";
        tint.GetComponent<SpriteRenderer>().sortingLayerName = "DeadEnemy";
        if (triggerRewards)
        {
            CalculateDrop();
            game.GainExperience((int)(experienceDrop * StageSelector.scoreMultiplier));
            anim.SetBool("dead", true);
            furyIcon.SetActive(false);
        }
        else
        {
            chasingPlayer = false;
            anim.SetTrigger("expired");
        }
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
