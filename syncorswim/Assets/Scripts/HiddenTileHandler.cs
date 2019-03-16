using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HiddenTileHandler : MonoBehaviour
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
        TurnBlack();
    }

    // Update is called once per frame
    void Update()
    {
        ColorHandler();
    }

    //handling changing the color of the tile based on win state
    void ColorHandler()
    {

        WinState = gameManager.GetComponent<GameManager>().BigWinState;

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
            TurnBlack();
            WinTrigger = false;
        }

    }

    //change the color to blue
    void TurnBlack()
    {
        myColor.r = 0;
        myColor.g = 0;
        myColor.b = 0;
        myColor.a = 0;

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
