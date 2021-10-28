using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSharpSynth.Effects;
using CSharpSynth.Sequencer;
using CSharpSynth.Synthesis;
using CSharpSynth.Midi;

[RequireComponent (typeof(AudioSource))]
public class UnitySynthTest2 : MonoBehaviour
{
	//Public
	//Check the Midi's file folder for different songs
	public string midiFilePath = "Midis/Groove.mid";
	//Try also: "FM Bank/fm" or "Analog Bank/analog" for some different sounds
	public string bankFilePath = "GM Bank/gm";
	public int bufferSize = 1024;
	public int midiNote = 60;
	public int midiNoteVolume = 100;
	public int midiInstrument = 1;
	//Private 
	private float[] sampleBuffer;
	private float gain = 1f;
	private MidiSequencer midiSequencer;
	private StreamSynthesizer midiStreamSynthesizer;
	
	private float sliderValue = 1.0f;
	private float maxSliderValue = 127.0f;

	public Piano piano;
	
	// Awake is called when the script instance
	// is being loaded.
	void Awake ()
	{
		midiStreamSynthesizer = new StreamSynthesizer (44100, 2, bufferSize, 40);
		sampleBuffer = new float[midiStreamSynthesizer.BufferSize];		
		
		midiStreamSynthesizer.LoadBank (bankFilePath);
		
		midiSequencer = new MidiSequencer (midiStreamSynthesizer);
		midiSequencer.LoadMidi (midiFilePath, false);
		//These will be fired by the midiSequencer when a song plays. Check the console for messages
		midiSequencer.NoteOnEvent += new MidiSequencer.NoteOnEventHandler (MidiNoteOnHandler);
		midiSequencer.NoteOffEvent += new MidiSequencer.NoteOffEventHandler (MidiNoteOffHandler);	

		midiSequencer.NoteOnEvent += new MidiSequencer.NoteOnEventHandler (piano.MidiNoteOnHandler);
		midiSequencer.NoteOffEvent += new MidiSequencer.NoteOffEventHandler (piano.MidiNoteOffHandler);

	}
	
	// Start is called just before any of the
	// Update methods is called the first time.
	void Start ()
	{
		midiSequencer.Play ();

	}
	
	// Update is called every frame, if the
	// MonoBehaviour is enabled.
	void Update ()
	{

//        if (Input.GetKeyUp(KeyCode.K))
  //          midiStreamSynthesizer.NoteOff (1, midiNote + 12);		
		
		
	}
	
	// OnGUI is called for rendering and handling
	// GUI events.
	void OnGUI ()
	{
		return;
		// Make a background box
		GUILayout.BeginArea (new Rect (Screen.width / 2 - 75, Screen.height / 2 - 50, 150, 300));
		
		
		if (GUILayout.Button ("Play Song")) {
			midiSequencer.Play ();
		}
		if (GUILayout.Button ("Stop Song")) {
			midiSequencer.Stop (true);
		}		
//		GUILayout.Box("Instrument: " + Mathf.Round(midiInstrument));
//		midiInstrument = (int)GUILayout.HorizontalSlider (midiInstrument, 0.0f, maxSliderValue);
//		GUILayout.Box("Volume: " + Mathf.Round(midiNoteVolume));
//		midiNoteVolume = (int)GUILayout.HorizontalSlider (midiNoteVolume, 0.0f, maxSliderValue);
		// End the Groups and Area	
		GUILayout.EndArea ();
		
        Event e = Event.current;
//        if (e.isKey)
//            Debug.Log("Detected key code: " + e.keyCode);		
	}
	

	
	// See http://unity3d.com/support/documentation/ScriptReference/MonoBehaviour.OnAudioFilterRead.html for reference code
	//	If OnAudioFilterRead is implemented, Unity will insert a custom filter into the audio DSP chain.
	//
	//	The filter is inserted in the same order as the MonoBehaviour script is shown in the inspector. 	
	//	OnAudioFilterRead is called everytime a chunk of audio is routed thru the filter (this happens frequently, every ~20ms depending on the samplerate and platform). 
	//	The audio data is an array of floats ranging from [-1.0f;1.0f] and contains audio from the previous filter in the chain or the AudioClip on the AudioSource. 
	//	If this is the first filter in the chain and a clip isn't attached to the audio source this filter will be 'played'. 
	//	That way you can use the filter as the audio clip, procedurally generating audio.
	//
	//	If OnAudioFilterRead is implemented a VU meter will show up in the inspector showing the outgoing samples level. 
	//	The process time of the filter is also measured and the spent milliseconds will show up next to the VU Meter 
	//	(it turns red if the filter is taking up too much time, so the mixer will starv audio data). 
	//	Also note, that OnAudioFilterRead is called on a different thread from the main thread (namely the audio thread) 
	//	so calling into many Unity functions from this function is not allowed ( a warning will show up ). 	
	private void OnAudioFilterRead (float[] data, int channels)
	{

		//This uses the Unity specific float method we added to get the buffer
		midiStreamSynthesizer.GetNext (sampleBuffer);
			
		for (int i = 0; i < data.Length; i++) {
			data [i] = sampleBuffer [i] * gain;
		}
	}

	public void MidiNoteOnHandler (int channel, int note, int velocity)
	{
//		Debug.Log ("NoteOn: " + channel );// note.ToString () + " Velocity: " + velocity.ToString ());
	}
	
	public void MidiNoteOffHandler (int channel, int note)
	{
//		Debug.Log ("NoteOff: " + note.ToString ());
	}

	
}
