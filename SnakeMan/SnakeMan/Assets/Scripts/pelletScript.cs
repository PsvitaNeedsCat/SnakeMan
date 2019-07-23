using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pelletScript : MonoBehaviour
{
    Collider2D playerCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        playerCollider = GameObject.Find("Player").GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            globalScript.score += 10;

            Destroy(this.gameObject);
        }
    }
}
