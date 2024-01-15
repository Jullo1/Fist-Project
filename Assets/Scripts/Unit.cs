using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Entity
{
    public float moveSpeed;
    public float attackRange;
    public int strength;
    public float pushForce;
    public List<float> attackCD = new List<float>();
    public List<float> attackTimer = new List<float>();

    public int hitpoints;
    public int maxHitpoints;
    public bool dead;
    protected float frozenTime;
    public bool freezeAttack;

    protected int hitQueue; //useful for stop time, after the buff ends, the unit will take multiple hits one after the other
    Coroutine hitQueueRoutine;
    List<int> damageQueue = new List<int>();
    [SerializeField] AudioClip hitSound;

    IEnumerator ProcessHitQueue(GameObject pusher, float pushForce = 0f)
    {
        float tempSpeed = moveSpeed;
        moveSpeed = 0f;
        while (hitQueue > 0)
        {
            hitQueue--;
            PlayAudio(hitSound);
            UpdateHealth(-damageQueue[hitQueue]);
            if (hitQueue > 1) KnockBack(pusher, 0);
            else { KnockBack(pusher, pushForce); moveSpeed = tempSpeed; } //only the last knockback will push the enemy
            yield return new WaitForSeconds(0.2f);
        }
        damageQueue.Clear();
    }

    protected void KnockBack(GameObject pusher, float pushForce = 0f)
    {
        anim.SetTrigger("takeHit");
        StartCoroutine(FreezeAttack(0.75f));
        Vector3 direction = (transform.position - pusher.transform.position).normalized;
        rb.AddForce((direction * pushForce) / weight, ForceMode2D.Impulse);
    }

    IEnumerator HitQueue(GameObject pusher, float pushForce = 0f)
    {
        while (frozenTime >= 0) yield return null;
        StartCoroutine(ProcessHitQueue(pusher, pushForce)); //execute after time stop ends
    }

    protected void AddToHitQueue(int damage, GameObject pusher, float pushForce = 0f)
    {
        hitQueue++;
        damageQueue.Add(damage);
        if (hitQueueRoutine != null) StopCoroutine(hitQueueRoutine);
        hitQueueRoutine = StartCoroutine(HitQueue(pusher, pushForce));
    }

    public virtual void TakeHit(int damage, GameObject hitter, float pushForce = 0f)
    {
        if (frozenTime <= 0)
        {
            UpdateHealth(-damage);
            KnockBack(hitter, pushForce);
        }
        else AddToHitQueue(damage, hitter, pushForce);
    }

    protected void PlayAudio(AudioClip clip)
    {
        aud.clip = clip;
        aud.Play();
    }

    protected virtual void UpdateHealth(int amount)
    {
        hitpoints += amount;
        if (hitpoints > maxHitpoints)
            hitpoints = maxHitpoints;
    }

    public IEnumerator FreezeAttack(float time)
    {
        freezeAttack = true;
        yield return new WaitForSeconds(time);
        freezeAttack = false;
    }

    public virtual void ActivatePowerUp(PowerUp powerUp)
    {
        if (powerUp.powerUpType == PowerUpType.FullRecovery)
        {
            UpdateHealth(maxHitpoints);
        }
    }

    public void FreezeUnit(float time)
    {
        frozenTime = time;
        anim.enabled = false;
    }
}
