using System.Collections;
using System.Collections.Generic;
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
        Vector3 direction = (transform.position - pusher.transform.position).normalized;
        rb.AddForce((direction * pushForce)/weight, ForceMode2D.Impulse);
    }
}