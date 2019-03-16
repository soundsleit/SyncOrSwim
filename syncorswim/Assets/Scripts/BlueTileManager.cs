using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlueTileManager : MonoBehaviour
{
    //place to hold the color variable
    private Color myColor;

    //vars for tempo from game manager
    public GameManager gameManager;
    private int TempoBase;

    //state flags
    public bool WinState = false;
    private bool WinTrigger = false;

    // Start is called before the first frame update
    void Start()
    {
        //turn the tile blue
        TurnBlue();
    }

    // Update is called once per frame
    void Update()
    {
        ColorHandler();
    }

    //handling changing the color of the tile based on win state
    void ColorHandler()
    {

        WinState = gameManager.GetComponent<GameManager>().WinState;

        if (WinState && !WinTrigger)
        {

            //get the tempo from the game
            TempoBase = gameManager.GetComponent<GameManager>().TempoBase;

            //start shifting
            float _tempo = (TempoBase / Random.Range(1, 4)) / 1000F;
            StartCoroutine(randomColorShift(_tempo));

            WinTrigger = true;
        }

        if (!WinState)
        {
            TurnBlue();
            WinTrigger = false;
        }

    }

    //change the color to blue
    void TurnBlue()
    {
        myColor.r = 0f;
        myColor.g = .09F;
        myColor.b = .42f;
        myColor.a = 1f;

        this.GetComponent<Image>().color = myColor;
    }


    //step through random colors in time with music during win sequence
    IEnumerator randomColorShift(float time)
    {
        while (WinState)
        { 
            //set random color
            myColor.r = Random.Range(0F, 1F);
            myColor.g = Random.Range(0F, 1F);
            myColor.b = Random.Range(0F, 1F);
            myColor.a = Random.Range(0F, 1F);

            this.GetComponent<Image>().color = myColor;

            //pause
            yield return new WaitForSeconds(time);
        }

    }

}

