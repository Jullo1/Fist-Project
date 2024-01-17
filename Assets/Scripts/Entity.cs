using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;
    protected BoxCollider2D col;
    protected AudioSource aud;
    protected Animator anim;
    protected GameManager game;

    public float weight;
    public float size;
    public bool freeze;
    protected bool freezeRotation;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        aud = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        game = FindObjectOfType<GameManager>();
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

    public void sendAnimTrigger(string triggerName)
    {
        anim.SetTrigger(triggerName);
    }
}
