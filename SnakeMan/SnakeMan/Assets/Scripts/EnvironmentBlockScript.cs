using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentBlockScript : MonoBehaviour
{
    public enum ObjectType
    {
        WALL,
        FLOOR
    }

    //public ObjectType thisType = objectType.WALL;
    public ObjectType thisType;

    public CircleCollider2D eastCollider;
    public CircleCollider2D westCollider;
    public CircleCollider2D northCollider;
    public CircleCollider2D southCollider;

    public GameObject eastTile;
    public GameObject westTile;
    public GameObject northTile;
    public GameObject southTile;


    // Pathfinding data
    public int Fcost = -1;
    public int Gcost = -1;
    public int Hcost = -1;
    public bool Open = false;
    public bool Closed = false;
    public GameObject ParentTile;


    public void ResetPathfindingData()
    {
        Fcost = -1;
        Gcost = -1;
        Hcost = -1;
        Open = false;
        Closed = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        List<Collider2D> objectsCollidedWith = new List<Collider2D>();
        ContactFilter2D noFilter = new ContactFilter2D();
        noFilter.NoFilter();
        for (int colliderI = 0; colliderI < 4; colliderI++)
        {
            objectsCollidedWith.Clear();
            switch (colliderI)
            {
                case 0:
                    eastCollider.OverlapCollider(noFilter, objectsCollidedWith);
                    break;
                case 1:
                    westCollider.OverlapCollider(noFilter, objectsCollidedWith);
                    break;
                case 2:
                    northCollider.OverlapCollider(noFilter, objectsCollidedWith);
                    break;
                case 3:
                    southCollider.OverlapCollider(noFilter, objectsCollidedWith);
                    break;
            }
            if (objectsCollidedWith.Count > 0)
            {
                for (int i = 0; i < objectsCollidedWith.Count; i++)
                {
                    if (objectsCollidedWith[i].gameObject.tag == "Floor" || objectsCollidedWith[i].gameObject.tag == "Wall")
                    {
                        switch (colliderI)
                        {
                            case 0:
                                eastTile = objectsCollidedWith[i].gameObject;
                                break;
                            case 1:
                                westTile = objectsCollidedWith[i].gameObject;
                                break;
                            case 2:
                                northTile = objectsCollidedWith[i].gameObject;
                                break;
                            case 3:
                                southTile = objectsCollidedWith[i].gameObject;
                                break;
                        }
                        break;
                    }
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
