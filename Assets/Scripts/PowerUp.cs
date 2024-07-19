using UnityEngine;

public enum PowerUpType { None, Health, FullRecovery, Experience, Frenzy, OneHitKO, TimeStop, Perfection, Infinity, Phoenix , Coin, MiniTimeStop };
public class PowerUp : Entity
{
    Player player;
    AudioSource audioSource;
    [SerializeField] AudioClip audioClip;

    public PowerUpType powerUpType;
    public float duration;
    public float value; //determines the effectiveness of the power up, if applicable

    float timer; //after a few seconds, enemies will be able to destroy the power up. After some more the power up will disappear

    protected override void Awake()
    {
        base.Awake();
        player = FindObjectOfType<Player>();

        if (FindObjectOfType<GameManager>().transform.GetComponentInChildren<AudioSource>())
            audioSource = FindObjectOfType<GameManager>().transform.GetComponentInChildren<AudioSource>();
    }

    void Update()
    {
        if (timer > 10) {
            if (timer > 60) Destroy(gameObject);
        } timer += Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
            player.ActivatePowerUp(GetComponent<PowerUp>());
            audioSource.clip = audioClip;
            audioSource.Play();
            game.itemGrabCount++;
            Destroy(gameObject);
        }
    }
}
