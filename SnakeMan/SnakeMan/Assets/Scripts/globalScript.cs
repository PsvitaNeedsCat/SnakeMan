using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class globalScript : MonoBehaviour
{
    public static int score = 0;
    public static int e1Score = 0;

    public float timer = 100; // In seconds

    private TextMesh countdownTxt;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        countdownTxt = GameObject.Find("Countdown").GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        // Countdown
        timer -= Time.deltaTime;

        // Display timer
        string timerTxt = "";

        timerTxt += Mathf.Floor(timer / 60).ToString();
        timerTxt += ":";
        timerTxt += Mathf.Ceil(timer % 60).ToString();

        countdownTxt.text = timerTxt;

        // Check if countdown run out
        if (timer <= 0.0f)
        {
            if (score > e1Score)
            {
                SceneManager.LoadScene("endScreen");
            }
            else
            {
                SceneManager.LoadScene("loseEndScreen");
            }
        }
    }
}
