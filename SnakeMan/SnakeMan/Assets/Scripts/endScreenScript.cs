using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endScreenScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Set score
        GameObject.Find("score").GetComponent<TextMesh>().text = globalScript.score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
