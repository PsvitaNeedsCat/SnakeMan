using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class tileData
{
    public GameObject tile;
    public int references;
}

public class BasicEnemyScript : MonoBehaviour
{
    // Variable BS
    public int lives = 3;
    private bool isDead = false;
    private float deadCooldown = 1.0f;
    private bool extensionActivated = false;
    private float powerupCooldown = 20.0f;
    private bool pelletCollected = false;
    private float pelletCooldown = 0.1f;

    public int enemyNo;

    // USED FOR BASE FUNCTIONALITY
    // --------------------------------------------------------
    public CircleCollider2D northEastCollider;
    public CircleCollider2D northWestCollider;
    public CircleCollider2D southEastCollider;
    public CircleCollider2D southWestCollider;
    public float Speed;
    // --------------------------------------------------------

    // USED FOR A* PATHFINDING
    // --------------------------------------------------------
    private GameObject RandomTile;
    public GameObject CurrentFloorTile;
    public GameObject PreviousFloorTile;
    public GameObject PlayerFloorTile;
    private GameObject CurrentTile;
    public GameObject SourceTile;
    public GameObject DestinationTile;
    private List<GameObject> OpenList;
    private List<GameObject> ClosedList;
    // --------------------------------------------------------

    // USED FOR STATE MACHINE
    // --------------------------------------------------------
    private const int stateUndefined = -1;
    private const int stateChase = 0;
    private const int stateWander = 1;
    private const int statePowerplay = 2;
    private int state = stateUndefined;
    private float timeInState = 0.0F;
    private float timeToChaseFor;
    public float minChaseTime = 7.5F;
    public float maxChaseTime = 15.0F;
    // --------------------------------------------------------

    private void ResetPathfindingData()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Floor");
        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].GetComponent<EnvironmentBlockScript>().ResetPathfindingData();
        }
        OpenList.Clear();
        ClosedList.Clear();
        CurrentTile = null;
        SourceTile = null;
        DestinationTile = null;
    }

    private void UpdateFloorTile()
    {

        // Makes sure that all colliders are on the same tile, and if they are, sets that tile as the current floor tile.
        List<Collider2D> tilesCollidedNorthEast = new List<Collider2D>();
        List<Collider2D> tilesCollidedNorthWest = new List<Collider2D>();
        List<Collider2D> tilesCollidedSouthEast = new List<Collider2D>();
        List<Collider2D> tilesCollidedSouthWest = new List<Collider2D>();
        List<Collider2D> objectsCollidedWith = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        noFilter.NoFilter();

        northEastCollider.OverlapCollider(noFilter, tilesCollidedNorthEast);
        northWestCollider.OverlapCollider(noFilter, tilesCollidedNorthWest);
        southEastCollider.OverlapCollider(noFilter, tilesCollidedSouthEast);
        southWestCollider.OverlapCollider(noFilter, tilesCollidedSouthWest);


        // Gets all the tiles in one list
        for (int i = 0; i < tilesCollidedNorthEast.Count; i++)
        {
            objectsCollidedWith.Add(tilesCollidedNorthEast[i]);
        }
        for (int i = 0; i < tilesCollidedNorthWest.Count; i++)
        {
            objectsCollidedWith.Add(tilesCollidedNorthWest[i]);
        }
        for (int i = 0; i < tilesCollidedSouthEast.Count; i++)
        {
            objectsCollidedWith.Add(tilesCollidedSouthEast[i]);
        }
        for (int i = 0; i < tilesCollidedSouthWest.Count; i++)
        {
            objectsCollidedWith.Add(tilesCollidedSouthWest[i]);
        }

        List<tileData> referenceData = new List<tileData>();

        if (objectsCollidedWith.Count > 0)
        {
            for (int i = 0; i < objectsCollidedWith.Count; i++)
            {
                if (objectsCollidedWith[i].gameObject.tag == "Floor" && objectsCollidedWith[i] != CurrentFloorTile)
                {
                    bool referenceRecorded = false;
                    for (int j = 0; j < referenceData.Count; j++)
                    {
                        if (objectsCollidedWith[i].gameObject == referenceData[j].tile)
                        {
                            // we've looked at this tile before
                            referenceData[j].references++;
                            referenceRecorded = true;

                            if (referenceData[j].references == 4)
                            {
                                if (CurrentFloorTile != null)
                                {
                                    if (CurrentFloorTile != objectsCollidedWith[i].gameObject)
                                    {
                                        if (PreviousFloorTile != null)
                                        {
                                            if (objectsCollidedWith[i].gameObject != PreviousFloorTile)
                                            {
                                                PreviousFloorTile = CurrentFloorTile;
                                                CurrentFloorTile = objectsCollidedWith[i].gameObject;
                                            }
                                        }
                                        else
                                        {
                                            PreviousFloorTile = CurrentFloorTile;
                                            CurrentFloorTile = objectsCollidedWith[i].gameObject;
                                        }

                                    }
                                }
                                else
                                {
                                    CurrentFloorTile = objectsCollidedWith[i].gameObject;
                                }
                            }
                            break;
                        }
                    }
                    if (!referenceRecorded)
                    {
                        tileData data = new tileData();
                        data.tile = objectsCollidedWith[i].gameObject;
                        data.references = 1;
                        referenceData.Add(data);
                    }
                }
            }
        }
    }

    private void UpdatePlayerFloorTile()
    {
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        PlayerFloorTile = Player.GetComponent<playerScript>().currentFloorTile;
    }

    private bool IsTileValid(GameObject _tile)
    {
        return _tile.tag == "Floor" && !_tile.GetComponent<EnvironmentBlockScript>().Closed && _tile != PreviousFloorTile;
    }

    private GameObject AStar(GameObject _destinationTile)
    {
        // We need to define the path to the player, and then return the first tile in that path.
        // --- SETUP ---

        UpdateFloorTile();

        SourceTile = CurrentFloorTile;
        DestinationTile = _destinationTile;

        CurrentTile = SourceTile;
        MoveToOpenList(CurrentTile, 0);
        int currentGCost;
        bool Done = false;
        while (!Done)
        {
            EnvironmentBlockScript CurrentTileScript = CurrentTile.GetComponent<EnvironmentBlockScript>();
            currentGCost = CurrentTileScript.Gcost;

            // Going through tiles north, east, south, west.

            // The north tile
            GameObject northTile = CurrentTileScript.northTile;
            bool northValid = false;
            if (northTile != null)
            {
                northValid = IsTileValid(northTile);
                if (northValid)
                {
                    MoveToOpenList(northTile, currentGCost + 10);
                }
            }

            // The east tile
            GameObject eastTile = CurrentTileScript.eastTile;
            bool eastValid = false;
            if (eastTile != null)
            {
                eastValid = IsTileValid(eastTile);
                if (eastValid)
                {
                    MoveToOpenList(eastTile, currentGCost + 10);
                }
            }

            // The south tile
            GameObject southTile = CurrentTileScript.southTile;
            bool southValid = false;
            if (southTile != null)
            {
                southValid = IsTileValid(southTile);
                if (southValid)
                {
                    MoveToOpenList(southTile, currentGCost + 10);
                }
            }

            // The west tile
            GameObject westTile = CurrentTileScript.westTile;
            bool westValid = false;
            if (westTile != null)
            {
                westValid = IsTileValid(westTile);
                if (westValid)
                {
                    MoveToOpenList(westTile, currentGCost + 10);
                }
            }

            // Move that tile from the open list to the closed list
            MoveToClosedList(CurrentTile);

            if (OpenList.Count == 0)
            {
                Done = true;
                break;
            }

            int lowestFCost = 0;
            GameObject lowestFCostTile = null;
            for (int i = 0; i < OpenList.Count; i++)
            {
                int IFCost = OpenList[i].GetComponent<EnvironmentBlockScript>().Fcost;
                if (i == 0)
                {
                    lowestFCost = IFCost;
                    lowestFCostTile = OpenList[i];
                }
                else if (IFCost < lowestFCost)
                {
                    lowestFCost = IFCost;
                    lowestFCostTile = OpenList[i];
                }
            }
            CurrentTile = lowestFCostTile;
            
            if (CurrentTile == DestinationTile)
            {
                Done = true;
            }

        }

        if (CurrentTile == DestinationTile)
        {
            while (CurrentTile.GetComponent<EnvironmentBlockScript>().ParentTile != SourceTile)
            {
                CurrentTile = CurrentTile.GetComponent<EnvironmentBlockScript>().ParentTile;
            }
        }
        else
        {
            CurrentTile = CurrentFloorTile;
        }
        return CurrentTile;
    }

    private void PathfindPlayer()
    {
        ResetPathfindingData();
        UpdatePlayerFloorTile();
        SeekPosition(AStar(PlayerFloorTile).GetComponent<Transform>().position);
    }

    private void PathfindRandomTile()
    {
        ResetPathfindingData();
        SeekPosition(AStar(RandomTile).GetComponent<Transform>().position);
    }

    private GameObject RandomFloorTile()
    {
        GameObject[] floorTiles = GameObject.FindGameObjectsWithTag("Floor");
        return floorTiles[Random.Range(0, floorTiles.Length)];
    }

    private GameObject RandomPowerupTile()
    {
        // Gets all powerups.
        GameObject[] powerups = GameObject.FindGameObjectsWithTag("Powerup");
        // Selects a random powerup.
        GameObject powerup = powerups[Random.Range(0, powerups.Length)];
        // Gets that powerup's collider.
        CircleCollider2D powerupCollider = powerup.GetComponent<CircleCollider2D>();
        // Populates objectsCollidedWith with the colliders colliding with powerupCollider.
        List<Collider2D> objectsCollidedWith = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        powerupCollider.OverlapCollider(noFilter, objectsCollidedWith);
        // Defines tile as the tile that the powerup is colliding with.
        GameObject tile = null;
        for (int i = 0; i < objectsCollidedWith.Count; i++)
        {
            if (objectsCollidedWith[i].gameObject.tag == "Floor")
            {
                tile = objectsCollidedWith[i].gameObject;
            }
        }
        // Returns that tile.
        return tile;
    }

    private void TransitionTo(int _state)
    {
        switch (_state)
        {
            case stateChase:
                state = stateChase;
                timeToChaseFor = Random.Range(minChaseTime, maxChaseTime);
                break;
            case stateWander:
                state = stateWander;
                RandomTile = RandomFloorTile();
                break;
            case statePowerplay:
                state = statePowerplay;
                RandomTile = RandomPowerupTile();
                break;
        }
        timeInState = 0.0F;
    }

    private void MoveToOpenList(GameObject _tile, int _GCost)
    {
        EnvironmentBlockScript _tileScript = _tile.GetComponent<EnvironmentBlockScript>();
        if (_tileScript.Fcost != -1)
        {
            if (_tileScript.Gcost > _GCost)
            {
                _tileScript.Gcost = _GCost;
                _tileScript.Fcost = _GCost + _tileScript.Hcost;
                _tileScript.ParentTile = CurrentTile;
            }
            _tileScript.Open = true;
        }
        else
        {
            Vector2 tilePosition = _tile.GetComponent<Transform>().position;
            Vector2 destinationPosition = DestinationTile.GetComponent<Transform>().position;
            float xDifference = Mathf.Abs(destinationPosition.x - tilePosition.x) / 8;
            float yDifference = Mathf.Abs(destinationPosition.y - tilePosition.y) / 8;
            int HCost = Mathf.RoundToInt(xDifference + yDifference);
            _tileScript.Hcost = HCost;
            _tileScript.Gcost = _GCost;
            _tileScript.Fcost = HCost + _GCost;
            if (_tile != CurrentTile)
            {
                _tileScript.ParentTile = CurrentTile;
            }
            OpenList.Add(_tile);
            _tileScript.Open = true;
        }
    }

    private void MoveToClosedList(GameObject _tile)
    {
        OpenList.Remove(_tile);
        ClosedList.Add(_tile);
        _tile.GetComponent<EnvironmentBlockScript>().Open = false;
        _tile.GetComponent<EnvironmentBlockScript>().Closed = true;
    }

    private void SeekPosition(Vector2 position)
    {
        Rigidbody2D rgbd2D = GetComponent<Rigidbody2D>();
        Vector2 desiredVelocity = position - rgbd2D.position;
        desiredVelocity.Normalize();
        desiredVelocity *= Speed;
        GetComponent<Rigidbody2D>().AddForce(desiredVelocity);
    }

    // Start is called before the first frame update
    void Start()
    {
        OpenList = new List<GameObject>();
        ClosedList = new List<GameObject>();
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

        if (extensionActivated)
        {
            powerupCooldown -= Time.deltaTime;

            if (powerupCooldown <= 0.0f)
            {
                extensionActivated = false;

                // Change distance joint
                // Get cape parts needed
                GameObject tether = GameObject.Find("Tether2");
                GameObject extraCape = GameObject.Find("Enemy Extra Segment");

                // Change distance joint
                extraCape.GetComponent<DistanceJoint2D>().connectedBody = tether.GetComponent<Rigidbody2D>();

                // Change follow target
                extraCape.GetComponent<followScript>().target = tether;

                // Teleport
                extraCape.transform.position = tether.transform.position;
            }
        }

        if (pelletCollected)
        {
            pelletCooldown -= Time.deltaTime;

            if (pelletCooldown <= 0.0f)
            {
                pelletCollected = false;
                pelletCooldown = 0.1f;
            }
        }

        timeInState += Time.deltaTime;
        switch (state)
        {
            case stateUndefined:

                // We need to find a state for the enemy
                // Randomly choose another state
                TransitionTo(Random.Range(stateChase, statePowerplay));
                //TransitionTo(statePowerplay);
                break;

            case stateChase:

                // Seeks the first tile in the path towards the player defined by A* algorithm
                PathfindPlayer();
                if (timeInState >= timeToChaseFor)
                {
                    // Go to a different random state
                    do { TransitionTo(Random.Range(stateChase, statePowerplay)); }
                    while (state == stateChase);
                }
                break;

            case stateWander:
                
                // Seeks a tile selected when TransitionTo is called
                PathfindRandomTile();
                if (CurrentFloorTile == RandomTile)
                {
                    // Go to a different random state
                    do { TransitionTo(Random.Range(stateChase, statePowerplay)); }
                    while (state == stateWander);
                }
                break;

            case statePowerplay:

                // Chase a random power pellet
                PathfindRandomTile();
                // If we got to the power pellet, change state
                if (CurrentFloorTile == RandomTile)
                {
                    // Go to a different random state
                    do { TransitionTo(Random.Range(stateChase, statePowerplay)); }
                    while (state == statePowerplay);
                }
                break;
        }

        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Pellet")
        {
            if (!pelletCollected)
            {
                pelletCollected = true;

                Destroy(collision.gameObject);

                switch (enemyNo)
                {
                    case 1:
                        {
                            globalScript.e1Score += 10;
                            GameObject.Find("eScore").GetComponent<TextMesh>().text = globalScript.e1Score.ToString();

                            break;
                        }

                    case 2:
                        {
                            globalScript.e2Score += 10;
                            GameObject.Find("e2Score").GetComponent<TextMesh>().text = globalScript.e1Score.ToString();

                            break;
                        }

                    case 3:
                        {
                            globalScript.e3Score += 10;
                            GameObject.Find("e3Score").GetComponent<TextMesh>().text = globalScript.e1Score.ToString();

                            break;
                        }

                    default:
                        break;
                }
            }
        }

        if (collision.tag == "Powerup")
        {
            Destroy(collision.gameObject);

            GameObject endOfCape = GameObject.Find("Enemy Cape Segment (10)");
            GameObject extraCape = GameObject.Find("Enemy Extra Segment");

            switch (enemyNo)
            {
                case 1:
                    break;

                case 2:
                    {
                        endOfCape = GameObject.Find("2 Enemy Cape Segment (10)");
                        extraCape = GameObject.Find("2 Enemy Extra Segment");

                        break;
                    }

                case 3:
                    {
                        endOfCape = GameObject.Find("3 Enemy Cape Segment (10)");
                        extraCape = GameObject.Find("3 Enemy Extra Segment");

                        break;
                    }

                default:
                    break;
            }

            // Change distance joint
            extraCape.GetComponent<DistanceJoint2D>().connectedBody = endOfCape.GetComponent<Rigidbody2D>();

            // Change follow target
            extraCape.GetComponent<followScript>().target = endOfCape;

            // Teleport
            extraCape.transform.position = endOfCape.transform.position;

            powerupCooldown = 20.0f;
            extensionActivated = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Collides with other enemies
        // Please don't look at how gross this is
        switch (enemyNo)
        {
            case 1:
                {
                    if (collision.collider.tag == "Enemy 2")
                    {
                        // Respawn this and enemy 2
                        Respawn();

                        GameObject.Find("Enemy 2").GetComponent<BasicEnemyScript>().Respawn();
                    }

                    if (collision.collider.tag == "Enemy 3")
                    {
                        Respawn();

                        GameObject.Find("Enemy 3").GetComponent<BasicEnemyScript>().Respawn();
                    }

                    if (collision.collider.tag == "2 Enemy Cape Segment" || collision.collider.tag == "3 Enemy Cape Segment")
                    {
                        Respawn();
                    }

                    break;
                }

            case 2:
                {
                    if (collision.collider.tag == "Enemy")
                    {
                        Respawn();

                        GameObject.Find("Enemy").GetComponent<BasicEnemyScript>().Respawn();
                    }

                    if (collision.collider.tag == "Enemy 3")
                    {
                        Respawn();

                        GameObject.Find("Enemy 3").GetComponent<BasicEnemyScript>().Respawn();
                    }

                    if (collision.collider.tag == "Enemy Cape Segment" || collision.collider.tag == "3 Enemy Cape Segment")
                    {
                        Respawn();
                    }

                    break;
                }

            case 3:
                {
                    if (collision.collider.tag == "Enemy")
                    {
                        Respawn();

                        GameObject.Find("Enemy").GetComponent<BasicEnemyScript>().Respawn();
                    }

                    if (collision.collider.tag == "Enemy 2")
                    {
                        Respawn();

                        GameObject.Find("Enemy 2").GetComponent<BasicEnemyScript>().Respawn();
                    }

                    if (collision.collider.tag == "2 Enemy Cape Segment" || collision.collider.tag == "Enemy Cape Segment")
                    {
                        Respawn();
                    }

                    break;
                }

            default:
                break;
        }
    }

    public void Respawn()
    {
        if (!isDead)
        {
            isDead = true;

            lives -= 1;
            switch (enemyNo)
            {
                case 1:
                    {
                        GameObject.Find("e1Lives").GetComponent<TextMesh>().text = lives.ToString();

                        break;
                    }

                case 2:
                    {
                        GameObject.Find("e2Lives").GetComponent<TextMesh>().text = lives.ToString();

                        break;
                    }

                case 3:
                    {
                        GameObject.Find("e3Lives").GetComponent<TextMesh>().text = lives.ToString();

                        break;
                    }

                default:
                    break;
            }
        }

        Vector3 spawnPos = GameObject.Find("FloorSegment (191)").transform.position;

        string capeName = "";

        switch (enemyNo)
        {
            case 1:
                {
                    capeName = "Enemy Cape Segment";

                    break;
                }

            case 2:
                {
                    capeName = "2 Enemy Cape Segment";

                    spawnPos = GameObject.Find("FloorSegment (190)").transform.position;

                    break;
                }

            case 3:
                {
                    capeName = "3 Enemy Cape Segment";

                    spawnPos = GameObject.Find("FloorSegment (63)").transform.position;

                    break;
                }

            default:
                break;
        }

        if (lives <= 0)
        {
            // Destroy enemy
            lives = 0;
            Destroy(this.gameObject);

            Destroy(GameObject.Find(capeName));

            switch (enemyNo)
            {
                case 1:
                    {
                        for (uint i = 1; i <= 10; i++)
                        {
                            capeName = "Enemy Cape Segment (" + i.ToString() + ")";

                            Destroy(GameObject.Find(capeName));
                        }

                        break;
                    }

                case 2:
                    {
                        for (uint i = 1; i <= 10; i++)
                        {
                            capeName = "2 Enemy Cape Segment (" + i.ToString() + ")";

                            Destroy(GameObject.Find(capeName));
                        }

                        break;
                    }

                case 3:
                    {
                        for (uint i = 1; i <= 10; i++)
                        {
                            capeName = "3 Enemy Cape Segment (" + i.ToString() + ")";

                            Destroy(GameObject.Find(capeName));
                        }

                        break;
                    }

                default:
                    break;
            }
        }

        //Vector3 spawnPos = new Vector3(0.5f, 0.54f, 0.0f);

        this.transform.position = spawnPos;

        GameObject.Find(capeName).transform.position = spawnPos;

        switch (enemyNo)
        {
            case 1:
                {
                    // 1-10 inclusive
                    for (uint i = 1; i <= 10; i++)
                    {
                        capeName = "Enemy Cape Segment (" + i.ToString() + ")";

                        GameObject.Find(capeName).transform.position = spawnPos;
                    }

                    break;
                }

            case 2:
                {
                    // 1-10 inclusive
                    for (uint i = 1; i <= 10; i++)
                    {
                        capeName = "2 Enemy Cape Segment (" + i.ToString() + ")";

                        GameObject.Find(capeName).transform.position = spawnPos;
                    }

                    break;
                }

            case 3:
                {
                    // 1-10 inclusive
                    for (uint i = 1; i <= 10; i++)
                    {
                        capeName = "3 Enemy Cape Segment (" + i.ToString() + ")";

                        GameObject.Find(capeName).transform.position = spawnPos;
                    }

                    break;
                }

            default:
                break;
        }

        if (extensionActivated)
        {
            extensionActivated = false;
            powerupCooldown = 20.0f;

            GameObject tether = GameObject.Find("Tether2");
            GameObject extraCape = GameObject.Find("Enemy Extra Segment");

            switch (enemyNo)
            {
                case 1:
                    break;

                case 2:
                    {
                        tether = GameObject.Find("Tether3");
                        extraCape = GameObject.Find("2 Enemy Extra Segment");

                        break;
                    }

                case 3:
                    {
                        tether = GameObject.Find("Tether4");
                        extraCape = GameObject.Find("3 Enemy Extra Segment");

                        break;
                    }

                default:
                    break;
            }

            // Change distance joint
            extraCape.GetComponent<DistanceJoint2D>().connectedBody = tether.GetComponent<Rigidbody2D>();

            // Change follow target
            extraCape.GetComponent<followScript>().target = tether;

            // Teleport
            extraCape.transform.position = tether.transform.position;
        }
    }
}
