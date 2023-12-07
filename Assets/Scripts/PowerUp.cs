using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType { None, Health, FullRecovery, Frenzy, OneHitKO, TimeStop, Perfection, Infinity, Phoenix };
public class PowerUp : Item
{
    Player player;
    AudioSource audioSource;
    [SerializeField] AudioClip audioClip;

    public PowerUpType powerUpType;
    public float duration;
    public float value; //determines the effectiveness of the power up, if applicable

    float enemyGrabTimer; //after a few seconds, enemies will be able to grab the powerUp
    bool enemyCanGrab;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        audioSource = FindObjectOfType<GameManager>().GetComponent<AudioSource>();
    }

    void Update()
    {
        if (enemyGrabTimer >= 10f)
            enemyCanGrab = true;
        else enemyGrabTimer += Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
            player.ActivatePowerUp(GetComponent<PowerUp>());
            audioSource.clip = audioClip;
            audioSource.Play();
        }

        else if (collider.tag == "Enemy")
        {
            if (enemyCanGrab)
                collider.GetComponent<Enemy>().ActivatePowerUp(GetComponent<PowerUp>());
            else return;
        }

        Destroy(gameObject);
    }
}
