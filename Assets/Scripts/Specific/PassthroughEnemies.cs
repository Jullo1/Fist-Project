using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassthroughEnemies : MonoBehaviour
{
    BoxCollider2D col;
    Unit unit;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        unit = GetComponent<Enemy>();
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && unit.frozenTime <= 0) col.isTrigger = false;
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        col.isTrigger = true;
    }
}
