using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType { None, FullRecovery, Frenzy, OneHitKO, DoubleXP };
public class PowerUp : Item
{
    Player player;
    AudioSource audioSource;
    [SerializeField] AudioClip audioClip;

    public PowerUpType powerUpType;
    public float duration;
    public float value; //determines the effectiveness of the power up, if applicable

    void Awake()
    {
        player = FindObjectOfType<Player>();
        audioSource = FindObjectOfType<GameManager>().GetComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
            player.ActivatePowerUp(GetComponent<PowerUp>());

        else if (collision.collider.tag == "Enemy")
            collision.collider.GetComponent<Enemy>().ActivatePowerUp(GetComponent<PowerUp>());

        audioSource.clip = audioClip;
        audioSource.Play();

        Destroy(gameObject);
    }
}
