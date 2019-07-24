using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followScript : MonoBehaviour
{
    public GameObject target;

    float speed = 2.0f;
    Vector2 velocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        velocity = new Vector2(0, 0) - new Vector2(1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Look at player and follow
        velocity = target.transform.position - this.transform.position;
        //this.GetComponent<Rigidbody2D>().velocity = velocity * speed;
        this.GetComponent<Rigidbody2D>().AddForce(velocity * speed);
    }
}
