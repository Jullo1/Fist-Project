using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : Unit
{
    Player player;
    [SerializeField] int experienceDrop;
    [SerializeField] GameObject damageVisual;
    public List<Item> dropList = new List<Item>();
    bool spawnCheck = true;
    float aliveTime;
    //bool fury;

    public override void TakeHit(int damage, GameObject hitter, float pushForce = 0f)
    {
        base.TakeHit(damage, hitter, pushForce);
        if (hitpoints <= 0)
        {
            game.GainExperience(experienceDrop);
            CalculateDrop();
            StartCoroutine(Death());
            damageVisual.SetActive(false);
        }

        KnockBack(hitter, player.pushForce);//knockback
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
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player") Attack();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (spawnCheck)
            if (collision.gameObject.tag == "Enemy")
                transform.position += new Vector3(Random.Range(0f,0.5f), Random.Range(0f, 0.5f));

        spawnCheck = false;
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
        if (player && !dead) transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
    }

    void CheckStats()
    {
        aliveTime += Time.deltaTime;
        if (aliveTime > 0.1f)
            spawnCheck = false;
        /*if (aliveTime > 30)
            fury = true;*/
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

    protected override void CheckHitpoints()
    {
        if (hitpoints <= (maxHitpoints / 2)) //feedback for enemies below half hp
            damageVisual.SetActive(true);
        else
            damageVisual.SetActive(false);
    }

    protected override void UpdateHealth(int amount)
    {
        base.UpdateHealth(amount);
        CheckHitpoints();
    }

}