using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyScript : MonoBehaviour
{
    public CircleCollider2D northEastCollider;
    public CircleCollider2D northWestCollider;
    public CircleCollider2D southEastCollider;
    public CircleCollider2D southWestCollider;

    public GameObject CurrentFloorTile;
    public GameObject PlayerFloorTile;

    private GameObject CurrentTile;
    public GameObject SourceTile;
    public GameObject DestinationTile;

    private List<GameObject> OpenList;
    private List<GameObject> ClosedList;

    public float Speed;

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

        int references = 0;

        if (objectsCollidedWith.Count > 0)
        {
            for (int i = 0; i < objectsCollidedWith.Count; i++)
            {
                if (objectsCollidedWith[i].gameObject.tag == "Floor" && objectsCollidedWith[i] != CurrentFloorTile)
                {
                    references++;
                    if (references == 4)
                    {
                        CurrentFloorTile = objectsCollidedWith[i].gameObject;
                        break;
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

    private GameObject AStar()
    {
        // We need to define the path to the player, and then return the first tile in that path.
        // --- SETUP ---

        UpdateFloorTile();
        UpdatePlayerFloorTile();

        SourceTile = CurrentFloorTile;
        DestinationTile = PlayerFloorTile;

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
                northValid = (northTile.tag == "Floor" && !northTile.GetComponent<EnvironmentBlockScript>().Closed);
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
                eastValid = (eastTile.tag == "Floor" && !eastTile.GetComponent<EnvironmentBlockScript>().Closed);
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
                southValid = (southTile.tag == "Floor" && !southTile.GetComponent<EnvironmentBlockScript>().Closed);
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
                westValid = (westTile.tag == "Floor" && !westTile.GetComponent<EnvironmentBlockScript>().Closed);
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

    private void Pathfinding()
    {
        ResetPathfindingData();
        SeekPosition(AStar().GetComponent<Transform>().position);
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
        // Seeks the first tile in the path towards the player defined by A* algorithm
        Pathfinding();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Pellet")
        {
            Destroy(collision.gameObject);

            globalScript.e1Score += 10;
            GameObject.Find("eScore").GetComponent<TextMesh>().text = globalScript.e1Score.ToString();
        }
    }

    public void Respawn()
    {
        Vector3 spawnPos = new Vector3(0.5f, 0.54f, 0.0f);

        this.transform.position = spawnPos;

        string capeName = "Enemy Cape Segment";

        GameObject.Find(capeName).transform.position = spawnPos;

        // 1-20 inclusive
        for (uint i = 1; i <= 10; i++)
        {
            capeName = "Enemy Cape Segment (" + i.ToString() + ")";

            GameObject.Find(capeName).transform.position = spawnPos;
        }
    }
}
