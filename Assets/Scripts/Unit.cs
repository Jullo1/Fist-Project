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
    protected List<float> attackTimer = new List<float>();

    public int hitpoints;
    public int maxHitpoints;
    public bool dead;

    public virtual void TakeHit(int damage, GameObject hitter, float pushForce = 0f)
    {
        anim.SetTrigger("takeHit");
        UpdateHealth(-damage);
    }

    protected IEnumerator Death()
    {
        dead = true;
        col.isTrigger = true;
        anim.SetBool("dead", true);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
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
        CheckHitpoints();
    }

    protected virtual void CheckHitpoints()
    {

    }

    public virtual void ActivatePowerUp(PowerUp powerUp)
    {
        if (powerUp.powerUpType == PowerUpType.FullRecovery)
        {
            UpdateHealth(maxHitpoints);
        }
    }
}
