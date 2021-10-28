using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Key {
	
	public Transform _transform;
	public bool noteOn;
	public string name;
	public Vector3 defaultPosition;
	public Vector3 targetPosition;

	public Quaternion defaultRotation;
	public Color defaultColor;
	public Color targetColor;

	public Renderer renderer;
	public bool white ;
	public int channel ;
	public Vector3 position;
	public float intensity = 0f;


	public void Release(float vel)
	{

		Vector3 pos = _transform.position;
		_transform.position = defaultPosition;
		RendererColor(defaultColor);
	}

	public void RendererColor(Color col)
	{
		renderer.material.SetColor("_EmissionColor", col);
	}

}

public class Piano : MonoBehaviour {

	public enum KeyColorFX { noColor, monoColor, fingerColor }

	
	
	[Header("KEYS")]
	[Space(10)]



	public Transform[] keys;
	public Key[] keyObjects;
	public List<Key> leftHandKeys = new List<Key>();
	public List<Key> rightHandKeys = new List<Key>();

	private string[] noteString;
	public Vector3 whiteTouchOffset;
	public Vector3 blackTouchOffset;


	[Header("COLOR FX")]
	[Space(10)]

	public KeyColorFX activeKeyColorFX;
	public Color activeKeyColor = Color.red;
	public AnimationCurve colorCurve;
	public float emissiveAmplitude = 50f;
	public float dampTime = 0.5f;

	public int leftHandChannel = 1;
	public int rightHandChannel = 0;
	[HideInInspector]
	public bool playing;

	[Header("DEBUG")]
	[Space(10)]
	public bool showGizmo = false;



	void Awake () {

		noteString = new string[12];
			
		noteString[0]="C";
		noteString[1]="C#";
		noteString[2]="D";
		noteString[3]="D#";
		noteString[4]="E";
		noteString[5]="F";
		noteString[6]="F#";
		noteString[7]="G";
		noteString[8]="G#";
		noteString[9]="A"; 
		noteString[10]="A#";
		noteString[11]="B";

		keyObjects = new Key[keys.Length];

		int i = 0;
		int octav = 0;
		int startKeyno = 9;

		foreach ( Transform key in keys) {
			keyObjects[i] = new Key();
			keyObjects[i]._transform = key;
			keyObjects[i].noteOn = false;
			keyObjects[i].renderer = key.GetComponent<Renderer>();
			keyObjects[i].defaultColor = keyObjects[i].renderer.material.GetColor("_EmissionColor");
			keyObjects[i].defaultPosition = key.position;
			keyObjects[i].defaultRotation = key.localRotation;
			keyObjects[i].intensity = 0f;
			if ( key.name.Contains( "Black" ) ) {
				keyObjects[i].white = false;
				keyObjects[i].position  = key.position + blackTouchOffset;
			}
			else {
				keyObjects[i].white = true;
				keyObjects[i].position  = key.position + whiteTouchOffset;

			}
			if ( keyObjects[i].white )
				keyObjects[i].targetPosition = new Vector3 ( key.position.x,  key.position.y -0.015f,  key.position.z );
			else
				keyObjects[i].targetPosition = new Vector3 ( key.position.x,  key.position.y -0.007f,  key.position.z );
			
			int val = (i+startKeyno ) % 12;
			octav = ( int ) Mathf.Floor( (i+startKeyno) / 12 );
			keyObjects[i].name =  noteString[val] + octav.ToString("") ;

			i++;
		}

			
	}

	void OnGUI(){

		if (!showGizmo)
			return;


		int i = 0;
		foreach ( Key key in keyObjects) {

			Vector2 objPos = Camera.main.WorldToScreenPoint(key._transform.position);
			float XOffset = 0;
			float YOffset = 0;
			GUI.Label(new Rect(objPos.x - XOffset, -objPos.y + Screen.height + YOffset, 30, 30), key.name );
			i++;
		}

		float offset = 0.0f;

		if( leftHandKeys!=null )
		if ( leftHandKeys.Count>0)
			foreach ( Key key in leftHandKeys) {
				GUI.Label( new Rect ( 10.0f, 10.0f + offset, 400,30.0f ), key.name );
				offset += 30.0f;
		}
		 offset = 0.0f;

		if( rightHandKeys!=null )
		if ( rightHandKeys.Count>0)
			foreach ( Key key in rightHandKeys) {
				GUI.Label( new Rect ( 100.0f, 10.0f + offset, 400,30.0f ), key.name );
				offset += 30.0f;
			}
		
		return;

	}

	void OnDrawGizmos() {

		if  (!showGizmo)
			return;

		
		Gizmos.color = Color.red;
		int i = 0;
		foreach ( Transform key in keys) {
			
			Gizmos.DrawSphere(key.position , 0.01f);
			int val = (i+9 )%12;
			i++;
		}
		Gizmos.color = Color.blue;
		if ( keyObjects!=null)
		foreach ( Key key in keyObjects) {
			if ( key.white )
				Gizmos.DrawSphere(key._transform.position + whiteTouchOffset, 0.01f);
			else
				Gizmos.DrawSphere(key._transform.position + blackTouchOffset, 0.01f);
		}
	}

	public Vector3 GetKeyPosition ( int midiNoteCode ) {
	
		if ( midiNoteCode  - 21 < 0 )
			return keyObjects[0].position ;

		else {
			 
				if ( keyObjects[midiNoteCode  - 21].white )
					return keyObjects[midiNoteCode  - 21]._transform.position + whiteTouchOffset ;
				else
					return keyObjects[midiNoteCode  - 21]._transform.position + blackTouchOffset ;
			
			}
		
	}

	public string  GetKeyName ( int midiNoteCode ) {

		if ( midiNoteCode  - 21 < 0 )
			return "" ;

		else
			return keyObjects[midiNoteCode  - 21].name ;

	}


	public void MidiNoteOnHandler (int channel, int midiNoteCode, int velocity) {
	
		if ( midiNoteCode  - 21 < 0 )
			return;
		if ( rightHandChannel!=channel & leftHandChannel!=channel )
			return;

		keyObjects[midiNoteCode  - 21].noteOn = true;
		keyObjects[midiNoteCode  - 21].channel = channel;

	}

	public void MidiNoteOffHandler (int channel, int midiNoteCode) {

		if ( midiNoteCode  - 21 < 0 )
		return;
		if ( rightHandChannel!=channel & leftHandChannel!=channel )
			return;
		
		keyObjects[midiNoteCode  - 21].noteOn = false;
		keyObjects[midiNoteCode  - 21].channel = channel;
					
	}

	public void NoteOffAll () {
		
		foreach ( Key key in keyObjects) {
			key.Release ( 0 );
			key.noteOn = false;
		}
			
	}

	public void Clear()
	{
		foreach (Key key in keyObjects)
		{
			{

				key.intensity = 0f;
			}
		}
	}
	public void PressColored(int i, Color color)
    {

		keyObjects[i - 21].intensity = 1f;
		keyObjects[i - 21].targetColor = color;

	}
	
	void Update () {

		for (int i=0;i< keyObjects.Length;i++)
		{
			Key key = keyObjects[i];
			if ( KeyColorFX.monoColor==activeKeyColorFX)
			key.RendererColor(Color.Lerp(key.defaultColor, activeKeyColor * emissiveAmplitude, colorCurve.Evaluate(key.intensity)));
			else if (KeyColorFX.fingerColor == activeKeyColorFX)
				key.RendererColor( Color.Lerp(key.defaultColor, key.targetColor * emissiveAmplitude, colorCurve.Evaluate( key.intensity)));
			else
				key.RendererColor(Color.Lerp(key.defaultColor, (new Color(0,0,0)) * emissiveAmplitude, colorCurve.Evaluate(key.intensity)));

			key._transform.position = Vector3.Lerp(key.defaultPosition, key.targetPosition, key.intensity);
			if ( playing)
				key.intensity -= Mathf.Clamp(Time.deltaTime * dampTime,0f,1f);
		}

	}
		
		
}

