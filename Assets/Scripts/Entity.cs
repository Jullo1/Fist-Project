using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;
    protected CircleCollider2D col;
    protected AudioSource aud;
    protected Animator anim;
    protected GameManager game;

    public float weight;
    public float size;
    protected bool cantAttack;
    public bool freeze;
    protected bool freezeRotation;
    public bool freezeAttack;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        aud = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        game = FindObjectOfType<GameManager>();
    }

    protected void KnockBack(GameObject pusher, float pushForce = 0f)
    {
        StartCoroutine(CantAttack(0.75f));
        Vector3 direction = (transform.position - pusher.transform.position).normalized;
        rb.AddForce((direction * pushForce)/weight, ForceMode2D.Impulse);
    }

    protected IEnumerator CantAttack(float seconds)
    {
        cantAttack = true;
        yield return new WaitForSeconds(seconds);
        cantAttack = false;
    }

    protected IEnumerator Freeze(float time)
    {
        freeze = true;
        yield return new WaitForSeconds(time);
        freeze = false;
    }

    protected IEnumerator FreezeRotation(float time)
    {
        freezeRotation = true;
        yield return new WaitForSeconds(time);
        freezeRotation = false;
    }

    public IEnumerator FreezeAttack(float time)
    {
        freezeAttack = true;
        yield return new WaitForSeconds(time);
        freezeAttack = false;
    }

    public void sendAnimTrigger(string triggerName)
    {
        anim.SetTrigger(triggerName);
    }
}
