using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class capeSegment : MonoBehaviour
{
    public enum Mode
    {
        PLAYER,
        ENEMY
    }

    public Mode capeMode;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (capeMode)
        {
            case Mode.PLAYER:
                {
                    // Player cape collides with enemy
                    if (collision.collider.tag == "Enemy")
                    {
                        GameObject.Find("Enemy").GetComponent<BasicEnemyScript>().Respawn();
                    }

                    break;
                }
            case Mode.ENEMY:
                {
                    // Enemy cape collides with player
                    if (collision.collider.tag == "Player")
                    {
                        GameObject.Find("Player").GetComponent<playerScript>().Respawn();
                    }

                    break;
                }

            default:
                break;
        }
    }
}
