using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    //for time
    float now=0;

	bool power=true;

	//Variables for syncit
	public int NoteBase = 50;
	public int TempoBase = 200;
	public int CodeOffset = 0;
	public int RealKnobValue=0;
	public int Sync=0;

	public int Level = 1;

	public int levelTries = 0;
	public int totalTries = 0;
	public int maxTries = 3;

	public Text LevelText;

	//the meter (round instrument)
	public GameObject Meter;

	//all the meter faces
	public Sprite[] SyncFaces;

	//winning meter faces
	public Sprite[] SyncFaceWinner;

	//blank face for meter
	public Sprite SyncFaceBlank;

	//did I win?
	public bool WinState = false;
    public bool BigWinState = false;

	//number of knob
	public Text KnobText;

	//the Syncit objects
	public GameObject Syncit;
	private GameObject SyncitSpawn;

	private IEnumerator winningState;

	//the UI buttons
	public Button ChangeButton;
	public Button SyncButton;
	public Button IncButton;
	public Button DecButton;
	public Button OffButton;

	//attempt indicators
	public GameObject[] AttemptIndicator;
	public Sprite AttemptSprite;
	public Sprite AttemptedSprite;

	public Sprite SyncSprite;
	public Sprite InSyncSprite;

    private void Awake()
    {
        //set this instance to the game manager
        instance = this;
    }

    void Start () {

        //turn off fullscreen mode
        Screen.fullScreen = false;
        Screen.SetResolution(600, 1400, false);

		now += Time.deltaTime;
		createSyncit (3, 8);
		//enable the buttons again
		ChangeButton.interactable=true;
		SyncButton.interactable=true;
		IncButton.interactable=true;
		DecButton.interactable=true;
		OffButton.interactable = true;
		KnobText.enabled = true;

	}

	// Update is called once per frame
	void Update () {
		
		now += Time.deltaTime;
		if (!WinState) GetKeys ();
		GameVariables ();
		MeterSet ();
	}

	//create a syncit instance
	public void createSyncit(int Voices, int Beats) {

        BigWinState = WinState = false;


        this.GetComponent<AudioSource> ().volume = 1F;

		//setup variables - randomize
		NoteBase = Random.Range (45, 70);
		TempoBase = Random.Range (200, 500);
		CodeOffset=Random.Range (1, 15);
		RealKnobValue=Random.Range (0, 15);
		if (RealKnobValue == CodeOffset)
			RealKnobValue=RealKnobValue/5;

		SyncitSpawn = (GameObject)Instantiate (Syncit, new Vector3(0,0,0), transform.rotation);
		Syncit mySyncit = SyncitSpawn.GetComponent<Syncit> ();
		mySyncit.Voices = Voices;
		mySyncit.Beats = Beats;
		mySyncit.NoteBase = NoteBase;
		mySyncit.TempoBase = TempoBase;
		mySyncit.CodeOffset = CodeOffset;
		mySyncit.CodeKnobValue = RealKnobValue;
	}


	public void Silence() {

		if (power) {

            if (SyncitSpawn != null) SyncitSpawn.GetComponent<Syncit>().QuitMe = true;

            this.GetComponent<AudioSource> ().volume = 0F;
			ChangeButton.interactable = false;
			SyncButton.interactable = false;
			IncButton.interactable = false;
			DecButton.interactable = false;
			power = false;
		} else {
			NewChallenge ();
			this.GetComponent<AudioSource> ().volume = 1F;
			ChangeButton.interactable = true;
			SyncButton.interactable = true;
			IncButton.interactable = true;
			DecButton.interactable = true;
			power = true;
		}
	}

	void MeterSet() {

		if (!WinState)
			Meter.GetComponent<Image> ().sprite = SyncFaces [RealKnobValue];
		else {
			//Meter.GetComponent<Image> ().sprite = SyncFaceWinner [2];
			Meter.GetComponent<grooveImageShift> ().enabled = true;
		}

	}



	void GameVariables(){

		//constrain knob value
		if (RealKnobValue > 15)
			RealKnobValue = 15;
		if (RealKnobValue < 0)
			RealKnobValue = 0;

		KnobText.text = RealKnobValue.ToString ();
		LevelText.text = "LEVEL " + Level.ToString ();

		//value of Sync (0 is in Sync)
		Sync = (RealKnobValue - CodeOffset)*-1;

	}

	public void NewChallenge(){

        if (SyncitSpawn != null) SyncitSpawn.GetComponent<Syncit>().QuitMe = true;

        this.GetComponent<AudioSource> ().volume = 1;

        if (Level==1) createSyncit(3,8);
		if (Level==2) createSyncit(3,8);
		if (Level==3) createSyncit(4,8);
		if (Level==4) createSyncit(4,8);
		if (Level==5) createSyncit(5,8);
		if (Level==6) createSyncit(6,8);
		if (Level==7) createSyncit(7,8);
		if (Level==8) createSyncit(8,8);

		ChangeButton.interactable = true;
		SyncButton.interactable=true;
		IncButton.interactable=true;
		DecButton.interactable=true;
		OffButton.interactable = true;

	}

	void GetKeys() {


		if (Input.GetKeyUp ("right")) {
			GameObject.FindWithTag ("Syncit").GetComponent<Syncit> ().CodeKnobValue = ++RealKnobValue;
			PlayClick ();
		}

		if (Input.GetKeyUp ("left")){
			GameObject.FindWithTag ("Syncit").GetComponent<Syncit> ().CodeKnobValue = --RealKnobValue;
			PlayClick ();
		}

		if (Input.GetKeyUp ("up"))
			SyncMe ();

		if (Input.GetKeyUp ("down") || Input.GetKeyUp ("space"))
			NewChallenge ();
	}

	public void IncKnob(){  
		GameObject.FindWithTag("Syncit").GetComponent<Syncit> ().CodeKnobValue = ++RealKnobValue;
		PlayClick ();
	}

	public void DecKnob(){ 
		GameObject.FindWithTag("Syncit").GetComponent<Syncit> ().CodeKnobValue = --RealKnobValue;
		PlayClick ();
	}

	public void SyncMe() {

		if (Sync == 0) {
			WinState = true;
			levelTries = 0;
			KnobText.enabled = false;
			for (int i = 0; i < AttemptIndicator.Length; i++) {
				AttemptIndicator [i].SetActive (false);
			}
			//check level number, play bloom if level 1-7, play winning bloom for 8
			//make it last for 3 groups of 16 bars at tempo - whatever that is
			if (Level < 8) {
				winningState = Bloom ((TempoBase * 16 * 3) / 1000F);
				StartCoroutine (winningState);
			}
			else if (Level == 8) {
                BigWinState = true;
				winningState = Bloom8 ((TempoBase * 152) / 1000F);
				StartCoroutine (winningState);
			}
			GetComponent<ChuckMainInstance> ().RunCode (string.Format (@"

		{0} => int NoteBase;


		ModalBar b1 => dac.left;
		ModalBar b2 => dac.right;
		.4=>b1.masterGain=>b2.masterGain;

		Std.mtof(NoteBase+24)*Math.random2f(.999,1.001)=>b1.freq;
		Std.mtof(NoteBase+24)*Math.random2f(.999,1.001)=>b2.freq;

		Math.random2f(0,1)=>b1.stickHardness;
		Math.random2f(0,1)=>b2.stickHardness;

		Math.random2f(0,1)=>b1.strikePosition;
		Math.random2f(0,1)=>b2.strikePosition;

		1=>b1.preset=>b2.preset;

		1=>b1.strike;
		1=>b1.noteOn;
		1=>b2.strike;
		1=>b2.noteOn;

		5::second=>now;


", NoteBase));

		} else {
			
			BigWinState = WinState = false;
			levelTries++;

			//set the indicator of failed attempt
			AttemptIndicator[levelTries-1].GetComponent<Image> ().sprite = AttemptedSprite;

			//check if all tries done, if so reset to level 1
			if (levelTries >= maxTries) {
				Level = 1;
				levelTries = 0;
				StartCoroutine(pauseNewChallenge());
			}
			//play buzzer with chuck
			GetComponent<ChuckMainInstance> ().RunCode (string.Format (@"

		{0} => int NoteBase;
		{1} => int TempoBase;

		TriOsc s1 => ADSR e1 => dac.left;
		TriOsc s2 => ADSR e2 => dac.right;
		.3 => s1.gain => s2.gain;

		Std.mtof(NoteBase + 23.5) => s1.freq;
		Std.mtof(NoteBase + 24.5) => s2.freq;

		2::ms => e1.attackTime => e2.attackTime;
		0::ms => e1.decayTime => e2.decayTime;
		1 => e1.sustainLevel => e2.sustainLevel;
		20::ms => e1.releaseTime => e2.releaseTime;

		for (0=>int i; i<3; i++)
		{{
	    1=>e1.keyOn=>e2.keyOn;
	    2::ms =>now;
	    1=>e1.keyOff=>e2.keyOff;
	    20::ms =>now;
    
	    (TempoBase/8.)::ms => now;
		}}


", NoteBase, TempoBase));
		}
	}

	IEnumerator pauseNewChallenge () {

        if (SyncitSpawn != null) SyncitSpawn.GetComponent<Syncit>().QuitMe = true;

        yield return new WaitForSeconds (.5F);

		//refill attempts
		for (int i = 0; i < AttemptIndicator.Length; i++) {
			AttemptIndicator[i].GetComponent<Image> ().sprite = AttemptSprite;
		}

		NewChallenge ();
	}
		
	void PlayClick() {

		//play click with chuck (to accompany knob value changes)
		GetComponent<ChuckMainInstance> ().RunCode (string.Format( @"

		{0} => int NoteBase;

		SawOsc s => ADSR e => dac;
		.2 => s.gain;

		Std.mtof(NoteBase + 60) => s.freq;

		2::ms => e.attackTime;
		0::ms => e.decayTime;
		1 => e.sustainLevel;
		10::ms => e.releaseTime;

		1=>e.keyOn;
		5::ms =>now;
		1=>e.keyOff;
		15::ms =>now;


", NoteBase));
	}


	IEnumerator Bloom(float waitTime)
	{

		//disable the buttons
		ChangeButton.interactable=false;
		SyncButton.interactable=false;
		IncButton.interactable=false;
		DecButton.interactable=false;
		OffButton.interactable = false;

		SyncButton.GetComponent<Image> ().sprite = InSyncSprite;

		//bloom in chuck!
		GetComponent<ChuckMainInstance> ().RunCode (string.Format (@"

		{0} => int NoteBase;
		{1} => int BaseTempo;

		SinOsc s[6];
		ADSR e1, e2;
		Gain g1, g2;

		s[0] => e1 => g1 => dac.left;
		s[1] => e1 => g1;
		s[2] => e1 => g1;

		s[3] => e2 => g2 => dac.right;
		s[4] => e2 => g2;
		s[5] => e2 => g2;

		//set freqs
		for (0=>int i;i<s.cap();i++) {{
		 .05=>s[i].gain;
		 Std.mtof(NoteBase-12)*(i%3+1)*Math.random2f(.99,1.01)=>s[i].freq;
		}}

		(BaseTempo * 16)::ms => e1.attackTime => e2.attackTime;
		1 => e1.sustainLevel => e2.sustainLevel;
		(BaseTempo * 16)::ms => e1.releaseTime => e2.releaseTime;
		1=>e1.keyOn=>e2.keyOn;
		(BaseTempo * 16)::ms => now;
		(BaseTempo * 16)::ms => now;
		1=>e1.keyOff=>e2.keyOff;
		(BaseTempo * 16)::ms => now;



", NoteBase, TempoBase));

		//make the lights go round
		//LightFace(TempoBase);

		yield return new WaitForSeconds(waitTime);
	
		Meter.GetComponent<grooveImageShift> ().enabled = false;


        if (SyncitSpawn != null) SyncitSpawn.GetComponent<Syncit>().QuitMe = true;

        Level++;

		if (Level==2) createSyncit(3,8);
		if (Level==3) createSyncit(4,8);
		if (Level==4) createSyncit(4,8);
		if (Level==5) createSyncit(5,8);
		if (Level==6) createSyncit(6,8);
		if (Level==7) createSyncit(7,8);
		if (Level==8) createSyncit(8,8);
		if (Level > 8) {
			Level = 1;
			createSyncit (3, 8);
		}

		PlayLevelBells (Level);

		SyncButton.GetComponent<Image> ().sprite = SyncSprite;

		//enable the buttons again
		ChangeButton.interactable=true;
		SyncButton.interactable=true;
		IncButton.interactable=true;
		DecButton.interactable=true;
		OffButton.interactable = true;
		KnobText.enabled = true;

		//refill attempts
		for (int i = 0; i < AttemptIndicator.Length; i++) {
			AttemptIndicator [i].GetComponent<Image> ().sprite = AttemptSprite;
		}

		//redraw indicators
		for (int i = 0; i < AttemptIndicator.Length; i++) {
			AttemptIndicator [i].SetActive (true);
		}

	}


	IEnumerator Bloom8(float waitTime)
	{

		//disable the buttons
		ChangeButton.interactable=false;
		SyncButton.interactable=false;
		IncButton.interactable=false;
		DecButton.interactable=false;
		OffButton.interactable = false;

		SyncButton.GetComponent<Image> ().sprite = InSyncSprite;

		//bloom in chuck!
		GetComponent<ChuckMainInstance> ().RunCode (string.Format (@"

{0} => int NoteBase;
{1} => int BaseTempo;

12=> int voices;
6 => int droneVoices;

[0,7,2,5,12] @=> int intervals[];

SinOsc s[voices];
ModalBar b[voices];
ADSR e_b[voices];
ADSR e[voices];
Gain g[voices];
Pan2 p[voices];


//patch initial setup
for (0=>int i; i<s.cap(); i++) {{
    s[i] => e[i] => g[i] => p[i] => dac;
    b[i] => e_b[i] => p[i];
    1./s.cap() => g[i].gain;
}}

//setup drone panning, envelopes and freqs
for (0=>int i; i<droneVoices; i++) {{
    Std.mtof(NoteBase-12)*(i%3+1)*Math.random2f(.99,1.01)=>s[i].freq;
    ((((1. / (droneVoices - 1)) * i) * 2) - 1) * Math.random2f(1.0,1.1) => p[i].pan;
    (BaseTempo * 64)::ms => e[i].attackTime;
    1. => e[i].sustainLevel;
    (BaseTempo * 64)::ms => e[i].releaseTime;
}}

//setup drone2 panning, envelopes and freqs
for (droneVoices=>int i; i<droneVoices*2; i++) {{
    Std.mtof(NoteBase+12)*(i%3+1)*Math.random2f(.99,1.01)=>s[i].freq;
    ((((1. / (droneVoices*2 - 1)) * i) * 2) - 1) * Math.random2f(1.0,1.1) => p[i].pan;
    (BaseTempo * 48)::ms => e[i].attackTime;
    1. => e[i].sustainLevel;
    (BaseTempo * 48)::ms => e[i].releaseTime;
}}

//setup bells
for (0=>int i; i<voices; i++) {{
    Std.mtof(NoteBase+intervals[Math.random2(0,intervals.cap()-1)])=>b[i].freq;
    Math.random2f(0,1)=>b[i].stickHardness;
    Math.random2f(0,1)=>b[i].strikePosition;
    1=>b[i].preset;
    (BaseTempo * 64)::ms => e_b[i].attackTime;
    1. => e_b[i].sustainLevel;
    (BaseTempo * 64)::ms => e_b[i].releaseTime;
}}

//move time and go

spork ~ dronePlay();
spork ~ dronePlay2();
spork ~ bellsPlay();

2::minute=>now;

fun void dronePlay() {{
    for (0=>int i; i<droneVoices; i++) {{
        1=>e[i].keyOn;
    }}
    for (0=>int i; i<voices; i++) {{
        1=>e_b[i].keyOn;
    }}
 
    (BaseTempo * 64)::ms => now;
    
    for (0=>int i; i<droneVoices; i++) {{
        1=>e[i].keyOff;
    }}
    for (0=>int i; i<voices; i++) {{
        1=>e_b[i].keyOff;
    }}
    
    (BaseTempo * 64)::ms => now;
}}

fun void dronePlay2() {{
    
    (BaseTempo * 24)::ms => now;
    
    for (droneVoices=>int i; i<droneVoices*2; i++) {{
        1=>e[i].keyOn;
    }}
    
    (BaseTempo * 64)::ms => now;
    
    for (droneVoices=>int i; i<droneVoices*2; i++) {{
        1=>e[i].keyOff;
    }}
    (BaseTempo * 64)::ms => now;
}}

fun void bellsPlay() {{
    for(0=>int i; i<64/2;i++) {{
        Math.random2(0,voices-1)=>int v;
        Math.random2f(.4,.6)=>b[v].masterGain;
        1=>b[v].strike;
        BaseTempo*4::ms=>now;
    }}
}} 




", NoteBase, TempoBase));

		//make the lights go round
		//LightFace(TempoBase);

		yield return new WaitForSeconds(waitTime);

		Meter.GetComponent<grooveImageShift> ().enabled = false;

        if (SyncitSpawn != null) SyncitSpawn.GetComponent<Syncit>().QuitMe = true;

		Level++;

		if (Level==2) createSyncit(3,8);
		if (Level==3) createSyncit(4,8);
		if (Level==4) createSyncit(4,8);
		if (Level==5) createSyncit(5,8);
		if (Level==6) createSyncit(6,8);
		if (Level==7) createSyncit(7,8);
		if (Level==8) createSyncit(8,8);
		if (Level > 8) {
			Level = 1;
			createSyncit (3, 8);
		}

		PlayLevelBells (Level);

		SyncButton.GetComponent<Image> ().sprite = SyncSprite;

		//enable the buttons again
		ChangeButton.interactable=true;
		SyncButton.interactable=true;
		IncButton.interactable=true;
		DecButton.interactable=true;
		OffButton.interactable = true;
		KnobText.enabled = true;
		

		//refill attempts
		for (int i = 0; i < AttemptIndicator.Length; i++) {
			AttemptIndicator [i].GetComponent<Image> ().sprite = AttemptSprite;
		}
		//redraw indicators
		for (int i = 0; i < AttemptIndicator.Length; i++) {
			AttemptIndicator [i].SetActive (true);
		}

	}



	void PlayLevelBells (int _level){

		GetComponent<ChuckMainInstance> ().RunCode (string.Format (@"

{0} => int NoteBase;
{1}/2 => int TempoBase;
{2} => int Level;

8=>int MaxBells;

ModalBar b1 => dac.left;
ModalBar b2 => dac.right;

ModalBar b[Level];
Gain g[Level];
Pan2 p[Level];

//patch, plus initial gain and panning
for (0=>int i; i<Level; i++) {{
    b[i] => g[i] => p[i] => dac;
    .7	 => g[i].gain;
    ((((1. / (Level - 1)) * i) * 2) - 1) * Math.random2f(1.0,1.1) => p[i].pan;
}}



1=>b1.masterGain=>b2.masterGain;

for (0=>int i; i<Level; i++) {{
    Std.mtof(NoteBase+12)*Math.random2f(.999,1.001)=>b[i].freq;
    
    Math.random2f(0,1)=>b[i].stickHardness;
    
    Math.random2f(0,1)=>b[i].strikePosition;

1=>b[i].preset;

1=>b[i].strike;
1=>b[i].noteOn;


TempoBase::ms => now;

}}

5::second => now;




", NoteBase, TempoBase, _level));


	}

}

