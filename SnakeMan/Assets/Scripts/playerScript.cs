using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class playerScript : MonoBehaviour
{
    // Player's lives
    private int lives = 3;
    private bool isDead = false;
    private float deadCooldown = 1.0f;

    // Pellet cooldown
    private bool pelletCollected = false;
    private float pelletCooldown = 0.5f;

    public GameObject currentFloorTile;
    public CircleCollider2D floorCollider;

    // Velocity
    public Vector2 velocity = new Vector2(1, 0) - new Vector2(0, 0);
    float hSpeed;
    float vSpeed;
    public float speed = 1.5f;

    // Powerup timer
    private float powerupCooldown = 20.0f; // In seconds
    private bool extensionActivated = false;

    private void Awake()
    {
        hSpeed = Input.GetAxisRaw("Horizontal");
        vSpeed = Input.GetAxisRaw("Vertical");
        velocity = new Vector2(1, 0) - new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            deadCooldown -= Time.deltaTime;
            if (deadCooldown <= 0.0f)
            {
                isDead = false;
                deadCooldown = 1.0f;
            }
        }
        if (pelletCollected)
        {
            pelletCooldown -= Time.deltaTime;
            if (pelletCooldown <= 0.0f)
            {
                pelletCollected = false;
                pelletCooldown = 0.5f;
            }
        }
        if (lives <= 0)
        {
            // Gameover
            if (globalScript.score > globalScript.e1Score)
            {
                SceneManager.LoadScene("endScreen");
            }
            else
            {
                SceneManager.LoadScene("loseEndScreen");
            }
        }

        // Movement
        //this.GetComponent<Rigidbody2D>().velocity = velocity.normalized * speed;
        GetComponent<Rigidbody2D>().AddForce(velocity.normalized * speed);

        List<Collider2D> objectsCollidedWith = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        noFilter.NoFilter();
        floorCollider.OverlapCollider(noFilter, objectsCollidedWith);

        if (objectsCollidedWith.Count > 0)
        {
            for (int i = 0; i < objectsCollidedWith.Count; i++)
            {
                if (objectsCollidedWith[i].gameObject.tag == "Floor")
                {
                    currentFloorTile = objectsCollidedWith[i].gameObject;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        CheckMovement();

        if (extensionActivated)
        {
            // Tick cooldown
            powerupCooldown -= Time.fixedDeltaTime;

            if (powerupCooldown <= 0.0f)
            {
                // Turn powerup off
                extensionActivated = false;

                // Change distance joint
                // Get cape parts needed
                GameObject tether = GameObject.Find("Tether");
                GameObject extraCape = GameObject.Find("Extra Cape Segment");

                // Change distance joint
                extraCape.GetComponent<DistanceJoint2D>().connectedBody = tether.GetComponent<Rigidbody2D>();

                // Change follow target
                extraCape.GetComponent<followScript>().target = tether;

                // Teleport
                extraCape.transform.position = tether.transform.position;
            }
        }
    }

    void CheckMovement()
    {
        hSpeed = Input.GetAxisRaw("Horizontal");
        vSpeed = Input.GetAxisRaw("Vertical");

        if (!(hSpeed == 0 && vSpeed == 0))
        {
            if (!(new Vector2(hSpeed, vSpeed) == -velocity))
            {
                velocity = new Vector2(hSpeed, vSpeed);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If collided with powerup
        if (collision.tag == "Powerup")
        {
            Destroy(collision.gameObject);

            // Get cape parts needed
            GameObject endOfCape = GameObject.Find("End of Cape");
            GameObject extraCape = GameObject.Find("Extra Cape Segment");

            // Change distance joint
            extraCape.GetComponent<DistanceJoint2D>().connectedBody = endOfCape.GetComponent<Rigidbody2D>();

            // Change follow target
            extraCape.GetComponent<followScript>().target = endOfCape;

            // Teleport
            extraCape.transform.position = endOfCape.transform.position;

            powerupCooldown = 20.0f;
            extensionActivated = true;
        }
        if (collision.tag == "Pellet")
        {
            if (!pelletCollected)
            {
                pelletCollected = true;

                Destroy(collision.gameObject);

                globalScript.score += 10;
                GameObject.Find("score").GetComponent<TextMesh>().text = globalScript.score.ToString();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player spawn is (1.45, -1.46)
        // Enemy spawn is (0.5, 0.54)

        // Collides with enemy
        if (collision.collider.tag == "Enemy")
        {
            if (!isDead)
            {
                isDead = true;

                // Player loses life
                lives -= 1;
                GameObject.Find("lives").GetComponent<TextMesh>().text = lives.ToString();

                GameObject.Find("Enemy").GetComponent<BasicEnemyScript>().Respawn();

                // Respawn player
                Respawn();
            }
        }
        if (collision.collider.tag == "Enemy 2")
        {
            if (!isDead)
            {
                isDead = true;

                lives -= 1;
                GameObject.Find("lives").GetComponent<TextMesh>().text = lives.ToString();

                GameObject.Find("Enemy 2").GetComponent<BasicEnemyScript>().Respawn();

                Respawn();
            }
        }
        else if (collision.collider.tag == "Enemy Cape Segment" || collision.collider.tag == "2 Enemy Cape Segment" || collision.collider.tag == "3 Enemy Cape Segment")
        {
            if (!isDead)
            {
                isDead = true;

                // Player loses life
                lives -= 1;
                GameObject.Find("lives").GetComponent<TextMesh>().text = lives.ToString();

                // Everyone should already be respawned
            }
        }
    }

    public void Respawn()
    {
        isDead = true;

        Vector3 spawnPos = GameObject.Find("FloorSegment (189)").transform.position;

        // Player respawns
        //this.transform.position = new Vector3(1.45f, -1.46f, 0.0f);
        this.transform.position = spawnPos;

        velocity = new Vector2(1, 0) - new Vector2(0, 0);
        //Vector3 spawnPos = new Vector3(1.0f, -1.46f, 0.0f);

        // Respawn cape segments
        string capeName = "Cape Segment";

        GameObject.Find(capeName).transform.position = spawnPos;

        // 2-19 inclusive
        for (uint i = 2; i <= 9; i++)
        {
            capeName = "Cape Segment " + i.ToString();
            GameObject.Find(capeName).transform.position = spawnPos;
        }

        GameObject.Find("End of Cape").transform.position = spawnPos;

        // Powerup status
        if (extensionActivated)
        {
            extensionActivated = false;
            powerupCooldown = 20.0f;

            GameObject tether = GameObject.Find("Tether");
            GameObject extraCape = GameObject.Find("Extra Cape Segment");

            // Change distance joint
            extraCape.GetComponent<DistanceJoint2D>().connectedBody = tether.GetComponent<Rigidbody2D>();

            // Change follow target
            extraCape.GetComponent<followScript>().target = tether;

            // Teleport
            extraCape.transform.position = tether.transform.position;
        }
    }
}
