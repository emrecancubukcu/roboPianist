using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  CSharpSynth.Midi;
using CSharpSynth.Synthesis;
using UnityEngine.UI;

public class Note {

	public byte channel;
	public byte note;
	public byte vel;
	public bool noteOn;
	public float duration;
	public float time;
	public bool played;
	public int fingerNo;
	public bool newNote;

}



public class TimeStep {
	public int timeStepNo;
	public float time;
	public bool played;
	public List<Note> notes = new List<Note>();
	public List<int> notesInt = new List<int>();

	public TimeStep nextStep;
	public TimeStep previousStep;

	public bool empty;
	public float meanVel;


	public Vector3 handTargetposition;
	public Vector3[] fingerTargetPos;
	public int[] fingerNotes = new int[5];


}

public class MyMidiPlayer : MonoBehaviour {



	public List<TimeStep> timeSteps = new List<TimeStep>();

	public CSharpSynth.Midi.MidiFile mf;

	[Header("SONG")]
	[Space(10)]

	public int selectedFileIndex=0;
	public string[] files  ;
	public float playSpeed;



	[Header("MIDI")]
	[Space(10)]

	public string bankFilePath = "GM Bank/gm";
	public int bufferSize = 1024;
	private float[] sampleBuffer;
	private float gain = 1f;
	private StreamSynthesizer midiStreamSynthesizer;


	[Header("PIANO")]
	[Space(10)]
	public Piano piano;
	public PianoPlayer pianoPlayer;

	[Header("DEBUG")]
	[Space(10)]
	public bool showMidiEvents;
	public float timeScaleZoomCoeff = 1f;
	public int timeScaleOffset = 0;
	public Texture2D noteTexture;
	public bool playing = false;
	public float playerTimer;
	public float totalTime;

	[HideInInspector]
	public int playTimeStepIndex;
	public List<Note> notes = new List<Note>();
	public Text songNameText;
	void Awake ()
	{
		midiStreamSynthesizer = new StreamSynthesizer (44100, 2, bufferSize, 40);
		sampleBuffer = new float[midiStreamSynthesizer.BufferSize];		

		midiStreamSynthesizer.LoadBank (bankFilePath);
	}

	void OnGUI () {

		if ( showMidiEvents )
			DrawMidi();
		
	}

	void DrawMidi() {

		Rect grid = new Rect(  50, 50,  Screen.width-10, Screen.height-10  );

		timeScaleOffset = ( int ) Mathf.Round(  GUI.HorizontalSlider ( new Rect(20, 20, 500, 30), timeScaleOffset, 0f, timeSteps.Count) );
		timeScaleZoomCoeff =   GUI.HorizontalSlider ( new Rect(20, 50, 500, 30), timeScaleZoomCoeff, 0f, 100f);


		float timeScale = grid.width / totalTime * timeScaleZoomCoeff  ;

		for ( int i = timeScaleOffset; i< timeSteps.Count; i++ ) {
			TimeStep _timeStep = timeSteps[i];

			foreach ( Note note in _timeStep.notes) {
				
				if ( note.channel==0)  {
					GUI.color = Color.green;
				}
				else {
					GUI.color = Color.red;
				}
					
				if ( note.noteOn ) {
					GUI.DrawTexture(new Rect( grid.x + ( note.time - timeScaleOffset )*timeScale, grid.y + grid.height- note.note*grid.height/128f,  note.duration*timeScale,1f), noteTexture);
					GUI.color = Color.white;
					GUI.Label(new Rect( grid.x +  ( note.time - timeScaleOffset )*timeScale, grid.y + grid.height- note.note*grid.height/128f, 50,20),note.note.ToString("") + " "+ (note.fingerNo+1).ToString(""));
				}

				else {

					GUI.color = Color.blue;

				}
			}

		}

		GUI.color = Color.magenta;
		GUI.DrawTexture(new Rect(grid.x + playerTimer*timeScale, grid.y,  1f,grid.height), noteTexture);

			

	}

	float DeltaTimeToTime ( float deltaTime ) {
	
		return deltaTime * 60.0f / ( mf.BeatsPerMinute * mf.MidiHeader.DeltaTiming) ;

	}

	void SetDuration (Note _note ) {
	
		Note foundNote = null;
		foreach ( Note note in notes ) {

			if ( _note.note == note.note & note.channel==note.channel & note.noteOn ) {
				foundNote = note;
			}

		}

		if ( foundNote!=null ) {
			foundNote.duration = _note.time - foundNote.time;


		}
	
	}

	void AddTimeStep( Note note ) {


		foreach ( TimeStep _timeStep in timeSteps) {
			if ( note.time == _timeStep.time ) {
				_timeStep.notesInt.Add(note.note);
				_timeStep.notes.Add(note );
				return;
			
			}

		}

		TimeStep timeStep = new TimeStep();
		timeStep.time = note.time;
		timeStep.notesInt.Add(note.note);
		timeStep.notes.Add(note );
		timeStep.played = false;
		timeSteps.Add(timeStep);

	}



	void Start () {

		mf = new MidiFile(files[selectedFileIndex]);
		songNameText.text = files[selectedFileIndex];
		totalTime = 0;
		foreach( MidiTrack mt in mf.tracks) {

			uint _time = 0;

			foreach ( MidiEvent me in mt.MidiEvents ) {

				_time +=me.deltaTime;

				if (  MidiHelper.MidiChannelEvent.Note_Off == me.midiChannelEvent ) {
					
					Note note = new Note();
					note.channel = me.channel;
					note.note  = me.parameter1;
					note.duration = 0;
					note.time = DeltaTimeToTime ( (float) _time);
					note.noteOn = false;
					note.played = false;
					notes.Add( note );


					if ( _time> totalTime )
						totalTime = _time;
					
					AddTimeStep( note );
					SetDuration (note);

				}

				if (  MidiHelper.MidiChannelEvent.Note_On == me.midiChannelEvent ) {
					
					Note note = new Note();
					note.channel = me.channel;
					note.note  = me.parameter1;
					note.vel = me.parameter2;
					note.duration = 0;
					note.time = DeltaTimeToTime ( (float) _time); 
					note.noteOn = true;
					note.played = false;
					notes.Add( note );

					if ( _time> totalTime )
						totalTime = _time;
					
					AddTimeStep( note );


				}
			}



		}
	
		totalTime = DeltaTimeToTime ( (float) totalTime);
		timeSteps.Sort((x, y) => x.time.CompareTo(y.time));

		for ( int timeStepNo = 0; timeStepNo<timeSteps.Count; timeStepNo++) {
		
			if ( timeStepNo>0 )
				timeSteps[timeStepNo].previousStep = timeSteps[timeStepNo-1];
			if ( timeStepNo<timeSteps.Count-1 )
				timeSteps[timeStepNo].nextStep = timeSteps[timeStepNo+1];
		
		}

		int i = 0;

		foreach ( TimeStep _timeStep in timeSteps) {
			_timeStep.notes.Sort((x, y) => x.note.CompareTo(y.note));
			_timeStep.notesInt.Sort();


		}

		foreach ( TimeStep _timeStep in timeSteps) {
			string s;
			s = i + " " + _timeStep.time.ToString("");
			s += " ";
			foreach ( Note note in _timeStep.notes) 
				s +=" "+note.note; 

			i++;
		}

		pianoPlayer.PreProcessScore ( timeSteps );

			
	} 
	
	// Update is called once per frame

	void ResetPlayedNotes ( float _time) {

		foreach ( var note in notes ) 
			if ( note.time>=_time )
			note.played = false; 

		foreach ( TimeStep _timeStep in timeSteps) {
			
			if ( _timeStep.time>=_time )
				_timeStep.played = false;
		}			

			
	}

	int FindNextTimeStep ( float time ) {

		int index=0;
		int  _playTimeStepIndex = -1;


		foreach ( TimeStep _timeStep in timeSteps) {

			if ( _timeStep.nextStep != null ) {
			
				if ( time >= _timeStep.time &  time < _timeStep.nextStep.time ) {
					return index;
				}

			
			} else {
			
				if ( time >= _timeStep.time) {
					return index;
					}
				
			
			}


			index ++;

		}

		return _playTimeStepIndex;
	}

	public void Play() {

		if (playing ) {
			
			playing = false;
			Pause();

		} else {

			playing = true;
		}
	}

	public void NextStep() {
		
		playTimeStepIndex ++;
		playTimeStepIndex = Mathf.Clamp( playTimeStepIndex, 0,timeSteps.Count-1 );
		playerTimer = timeSteps[playTimeStepIndex].time;
			
	}
	public void PreviousStep() {
		
		playTimeStepIndex --;
		playTimeStepIndex = Mathf.Clamp( playTimeStepIndex, 0,timeSteps.Count-1 );
		playerTimer = timeSteps[playTimeStepIndex].time;
		ResetPlayedNotes (playerTimer);
		midiStreamSynthesizer.NoteOffAll( true );
		piano.NoteOffAll();
	}

	public void Pause() {

		ResetPlayedNotes (playerTimer);
		midiStreamSynthesizer.NoteOffAll( true );
		piano.NoteOffAll();
	
	}

	public void Stop () {
		
		playing = false;
		playerTimer = -0.01f;
		ResetPlayedNotes (playerTimer);
		midiStreamSynthesizer.NoteOffAll( true );
		piano.NoteOffAll();
	
	}

	void Update () {

		if ( Input.GetKeyDown("right" ) ) {
			NextStep();
		}

		if ( Input.GetKeyDown("left" ) ) {
			PreviousStep();
			return;
		}
		if ( Input.GetKeyDown("space" ) ) {
			Play();
		}

		if ( Input.GetKeyDown("return" ) ) {
			Stop();
		}

		playTimeStepIndex = FindNextTimeStep (playerTimer);
		pianoPlayer.playerTimer = playerTimer;
		pianoPlayer.playTimeStepIndex = playTimeStepIndex;
		pianoPlayer.playing = playing;

		foreach ( TimeStep _timeStep in timeSteps) 
			if ( playerTimer >= _timeStep.time ) 
			if (  !_timeStep.played ) {
				
			
			_timeStep.played = true;
				pianoPlayer.Act(  playerTimer, _timeStep );
				foreach ( var note in _timeStep.notes ) {
				
				if ( note.noteOn ) {
					midiStreamSynthesizer.NoteOn (note.channel, note.note, note.vel/2, 1);

							

				}
				else {
					midiStreamSynthesizer.NoteOff (note.channel, note.note);
				}

			}
		
		}



		if ( playing )
			playerTimer += Time.deltaTime * playSpeed ;




	}


	private void OnAudioFilterRead (float[] data, int channels)
	{

		//This uses the Unity specific float method we added to get the buffer
		midiStreamSynthesizer.GetNext (sampleBuffer);

		for (int i = 0; i < data.Length; i++) {
			data [i] = sampleBuffer [i] * gain;
		}

	}

}
