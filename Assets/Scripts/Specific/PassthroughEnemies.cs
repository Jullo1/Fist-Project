using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassthroughEnemies : MonoBehaviour
{
    BoxCollider2D col;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player") col.isTrigger = false;
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        col.isTrigger = true;
    }
}
