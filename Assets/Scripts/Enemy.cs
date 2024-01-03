using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Enemy : Unit
{
    Player player;
    [SerializeField] int experienceDrop;
    //[SerializeField] GameObject damageVisual;
    [SerializeField] GameObject furyIcon;
    [SerializeField] GameObject hitIndicator;
    [SerializeField] SpriteMask hitIndicatorMask;

    public List<Item> dropList = new List<Item>();
    bool spawnCheck = true;
    float aliveTime;
    bool fury;

    public override void TakeHit(int damage, GameObject hitter, float pushForce = 0f)
    {
        base.TakeHit(damage, hitter, pushForce);
        hitIndicator.SetActive(false);
        if (hitpoints <= 0)
        {
            CalculateDrop();
            StartCoroutine(Death());
            //damageVisual.SetActive(false);
        }
        KnockBack(hitter, player.pushForce);//knockback
    }

    public void HitIndicator(bool isCurrentTarget)
    {
        hitIndicator.SetActive(isCurrentTarget);
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
        UpdateHitIndicator();
        if (attackTimer[0] <= attackCD[0]) attackTimer[0] += Time.deltaTime;
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

    void UpdateHitIndicator()
    {
        hitIndicatorMask.sprite = sr.sprite;
        if (sr.flipX) hitIndicatorMask.gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
        else hitIndicatorMask.gameObject.transform.localScale = Vector3.one;
    }

    void Attack()
    {
        if (attackTimer[0] >= attackCD[0])
        {
            player.TakeHit(strength, gameObject, pushForce);
            attackTimer[0] = 0;
        }
    }

    void Move()
    {
        if (player && !dead)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            if (player.transform.position.x < transform.position.x) sr.flipX = true;
            else sr.flipX = false;
        }
    }

    void CheckStats()
    {
        if (aliveTime > 12) {
            if (!fury) Fury();
        } else aliveTime += Time.deltaTime;


        if (aliveTime > 0.1f)
            spawnCheck = false;
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
        if (dropList[num]) Drop(dropList[num]);
    }

    void Drop(Item drop)
    {
        GameObject newDrop = Instantiate(drop.gameObject);
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
        col.isTrigger = true;
        anim.SetBool("dead", true);
        yield return new WaitForSeconds(0.35f);
        game.GainExperience(experienceDrop);
        Destroy(gameObject);
    }
}
