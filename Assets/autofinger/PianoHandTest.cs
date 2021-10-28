using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoHandTest : MonoBehaviour {

	[SerializeField]

	public class PlayData {

		public int[] notes;
	}

	[SerializeField]
	public List<PlayData> datas  = new List<PlayData>();


	// Use this for initialization
	public Piano piano;

	public int index=0;
	int[] esNotes;

	void DoTest () {

		if (esNotes!=null)
		if ( esNotes.Length>0 )
		foreach ( int note  in esNotes ) {
			piano.MidiNoteOffHandler (1, note);
		}

		esNotes = new int[ datas[index].notes.Length];
		int i =0;
		foreach ( int note  in datas[index].notes ) {
			//	if (note>500)
			//	piano.MidiNoteOffHandler (0, note-500);
			//	else
				piano.MidiNoteOnHandler (1, note,20);

			esNotes[i] = note;
			i++;

		}


		
			
	}
	void AddData ( int[] notes ) {
	
		PlayData data = new PlayData();
		data.notes = notes;
		datas.Add( data );

		return;

		int[] esNotes = new int[notes.Length];
		for ( int i = 0; i< notes.Length; i++ ) {
			esNotes[i] = notes[i] + 500; 
		}

		PlayData esdata = new PlayData();
		esdata.notes = esNotes;
		datas.Add( esdata );


	}


	void Start () {





	//	AddData ( new int[]  {} );
//		AddData ( new int[]  {30, 34, 37} );
//		AddData ( new int[]  {40, 44, 49} );
//		AddData ( new int[]  {50, 54, 58,59} );
		AddData ( new int[]  {29} );
		AddData ( new int[]  {49} );
		AddData ( new int[]  {50} );
		AddData ( new int[]  {68} );
		AddData ( new int[]  {30, 34, 37} );
		AddData ( new int[]  {40, 44, 49} );
		//		
	/*	AddData ( new int[]  {60, 64, 67} );
		AddData ( new int[]  {80, 84, 89} );
		AddData ( new int[]  {70, 74, 78,79} );
		AddData ( new int[]  {60} );
		AddData ( new int[]  {59} );
		AddData ( new int[]  {63} );
		AddData ( new int[]  {68} ); */


		DoTest ();
	//	InvokeRepeating("DoTest",0.0f, 2.0f);
		
	}
	
	// Update is called once per frame
	void Update () {

		if ( Input.GetKeyDown("right") ) {
		
			index++;

			if (index>=datas.Count)
				index=0;

			DoTest();
		
		}

		if ( Input.GetKeyDown("left") ) {

			index--;

			if (index<0)
				index=datas.Count-1;

			DoTest();

		}
	

	}
}
