using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Syncit : MonoBehaviour {

	public int Voices = 3;
	public int Beats = 4;
	public int NoteBase = 50;
	public int TempoBase = 200;

	public int CodeOffset = 0;
	public int CodeKnobValue = 0;
	public int Sync;

    public bool QuitMe = false;
    public bool QuitAlready = false;

	public GameObject GameManager;

	// Use this for initialization
	void Start () {

		//launch syncit for chuck!
		GetComponent<ChuckMainInstance> ().RunCode (string.Format(@"

// Sync or Swim, ChucK edition
// v. 4.11 stripped
// Scott Smallwood, 2018
// 
// syncit.ck is the main patch

//init variables
-2 => int sync;
{0} => int voices;
{1} => int beats;
{2} => int noteBase;
{3} => int tempoBase;
[4,7,9,11,12,14] @=> int intervalChoices[];

//quit flat
0 => global int QuitMe;

//check for args (for command line)
if (me.args() > 0)
{{
    Std.atoi(me.arg(0)) => voices;
    Std.atoi(me.arg(1)) => beats;
    Std.atoi(me.arg(2)) => noteBase;
    Std.atoi(me.arg(3)) => tempoBase;
}}

// controller boxes
int control[99];
{4} => int codeOffset;
{5} => global int codeKnobValue;

//modules
SinOsc s[voices];
ADSR e[voices];
Gain g[voices];
Pan2 p[voices];

//patch, plus initial gain and panning
for (0=>int i; i<voices; i++) {{
    s[i] => e[i] => g[i] => p[i] => dac;
    .5/voices => g[i].gain;
    ((((1. / (voices - 1)) * i) * 2) - 1) * Math.random2f(1.0,1.1) => p[i].pan;
}}

//vars per voices
int intervals[voices][beats];
float envAp[voices];
float envRp[voices];

//seup
setupVariables();
popIntArray();

spork ~ syncme();

//go
for (0=>int i; i<voices; i++)
    spork ~ syncLayer(i);

//hangout
while (!QuitMe) 10::ms => now;

0=>beats;

500::ms => now;

///////////////////////////
////////functions
////////



fun void syncme () {{
	//check for knob from Unity!    
    while (!QuitMe) {{
        
        //check to see if we are in sync (if sync == 0)
        (codeKnobValue - codeOffset)*-1 => sync;
		
		1::ms => now;
    }}
}}

fun void syncLayer(int voice) {{
    
    //offset each synclayer by a bit
    //when sync is zero, all layers will be in sync!
    //tempoBase + ((voice * 4) * sync) => int _tempoBase;
    //randomly doubletemp
    Math.random2(1,2) * tempoBase => int _tempoBase;
    //randomly upoctave
    Math.random2(1,2) => int octaver;
    float attack;
    float release;
    
    while (!QuitMe) {{
        for (0=>int i; i < beats; i++) {{
            
            _tempoBase + ((voice * 4) * sync) => int _tempoBase;
            //calc based on randomized percentages
            _tempoBase/4. * envAp[voice] => attack;
            _tempoBase * envRp[voice] => release;
            
            //attack
            1 => e[voice].keyOn;
            Std.mtof(noteBase+intervals[voice][i])*octaver => s[voice].freq;
            attack::ms => e[voice].attackTime => now;
            
            //decay
            0::ms => e[voice].decayTime;
            1 => e[voice].sustainLevel;
            
            //release
            1 => e[voice].keyOff;
            release::ms => e[voice].releaseTime;
            (_tempoBase - attack)::ms => now;
        }}
    }}
}}

fun void setupVariables() {{
    
    //randomize tempoBase (check to see if arg initialized first)
    if (tempoBase == 0) Math.random2(200,500) => tempoBase;
    
    //randomize noteBase (check to see if arg initialized first)
    if (noteBase == 0) Math.random2(40,60) => noteBase;
    
    for (0=>int i; i<voices; i++) {{
        //envelope variables (percentages)
        Math.random2f(.1,.99) => envAp[i];
        Math.random2f(.1,.99) => envRp[i];
    }}
    
}}


fun void popIntArray () {{
    //this function populates an array of 8 notes
    //randomly based on intervalChoices[] 
    //always returning to 0 or 7 on odd beats
    for (0=>int v; v < voices; v++) {{
        //rand low note (0 or 7)
        0 => int lowNote;
        if (Math.random2(0,2) == 0) 7 => lowNote;
        //set low notes
        for (0=>int i; i<beats; i+2=>i) {{
            lowNote => intervals[v][i];
        }}    
        //set middle notes, randomly chosen from intervalChoices[];
        for (0=>int i; i<beats; i+2=>i) {{
            intervalChoices[Math.random2(0,intervalChoices.cap()-1)] => intervals[v][i+1];
        }}
    }}
}}
		

", Voices, Beats, NoteBase, TempoBase, CodeOffset, CodeKnobValue));


	}

	// Update is called once per frame
	void Update () {


        if (!QuitMe)
        {
            GetComponent<ChuckMainInstance>().SetInt("codeKnobValue", CodeKnobValue);
            Sync = (CodeKnobValue - CodeOffset) * -1;
        }


        if (QuitMe && !QuitAlready)
        {
            StartCoroutine(DestroyThisSyncit());
            QuitAlready = true;
        }
    }


    IEnumerator DestroyThisSyncit()
    {
        GetComponent<ChuckMainInstance>().SetInt("QuitMe", 1);
        yield return new WaitForSeconds(.5F);
        Destroy(gameObject);
    }

}

