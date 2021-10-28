using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* TODO
 * 
 * - genel otomatik IK yerlestirici ve sinirlayici
 * - finger editor
 * + finger viewer
 * + baslangicta parmak hesaplayici
 * - vakit bulunca çok güzel bi debug gizle göster şeysi
 * parmak yumusatma
 * başlangıç ve bitiş
* el yuksekligi sorunu



*/

public class PianoPlayer : MonoBehaviour
{

	public AutoFingering autofingering = new AutoFingering ();

	[System.Serializable]
	public class HandSystem
	{
		
		public Transform handTransform;
		public Quaternion handDefaultRotation;
		public Vector3 handDefaultPosition;
		public Transform[] fingerTransforms;
		public Vector3[] fingerTipsDefaultPosition;
		public Vector3[] fingerTipsVel;

		public Color[] fingerTargetDefaultColor;

		public Vector3 pos;
		public Vector3 oldPos;

		//		public Vector3 centerOf;
		public Vector3 smoothVel;
		public bool left;
		public float lastPressTime;

		//		public List<Note> notes = new List<Note>();
		public List<TimeStep> timeSteps = new List<TimeStep> ();

		public HandState state = HandState.waiting;
		public int currentTimeStepIndex;

		//		public List<Vector3> handPos = new List<Vector3>();
		public void FindNextTimeSteps ()
		{

			for (int i = 0; i < timeSteps.Count; i++) {

				//		if ( i>0)
				for (int j = i + 1; j < timeSteps.Count; j++) {
//					if ( !timeSteps [j].empty )
					if (timeSteps [j].notes.Count > 0) {
						timeSteps [i].nextStep = timeSteps [j];
						break;
					}

				}
			}
		}

		public TimeStep Next ()
		{

			if (timeSteps.Count == 0)
				return null;

			TimeStep _timeStep = timeSteps [currentTimeStepIndex].nextStep;

	//		return _timeStep;
			//Time ste null olmayana as  timestepler bitene kadar devam

			while (_timeStep != null) {
			
				if (_timeStep.empty) {
					_timeStep = _timeStep.nextStep;
					Debug.Log (_timeStep);
				} else
					return _timeStep;

			}

			return _timeStep;
					

			//	return timeSteps [currentTimeStepIndex].nextStep;
								
		}

		public float NextTime ()
		{

			if (timeSteps.Count == 0)
				return 0;

			if (Next () != null)
				return Next ().time;
			else
				return 0;
						
		}

		
	}

	[Header("PIANO")]
	[Space(10)]

	public bool simpleFingering = false;
	public Piano piano;

	[Header("HAND PROPERTIES")]
	[Space(10)]

	public Transform headTarget;
	public float handFlightHeight = 0.1f;
	public float fingerFlightHeight = 0.1f;

	public Vector3 handOffset;
	public Vector3 unpressedFingerOffset ;
	public Vector3 pressedFingerOffset ;

	[Range(0, 1)]
	public float handStartMovingRatio = 0.2f;

	public AnimationCurve moveCurve = new AnimationCurve();
	public AnimationCurve heightCurve = new AnimationCurve();

	public HandSystem leftHand;
	public HandSystem rightHand;


	[Header("DEBUG")]
	[Space(10)]
	public bool showInfo = false;
	public int playTimeStepIndex;
	public bool songReady = false;
	public float lastTime;
	public float playerTimer;
	public bool playing = false;

	public Font myFont;


	[HideInInspector]
	public Vector3 headTargetDefaultPosition;


	public enum HandState
	{
		pressing,
		moving,
		waiting

	}

	void WriteNotes (List< Note> notes, float offsetX, float offsetY, string prefix)
	{
	
		float _offsetY = offsetY;
		if (notes != null)
		if (notes.Count > 0)
			foreach (Note note in notes) {
				GUI.Label (new Rect (offsetX, _offsetY, 400, 30.0f), prefix + piano.GetKeyName (note.note) + " " + (note.fingerNo + 1));
				_offsetY += 30.0f;
			}
	}

	Vector3 GetPosition (Note note)
	{
		
		return piano.GetKeyPosition (note.note);
	
	}

	void _OnDrawGizmos ()
	{

		if (!songReady)
			return;

		HandSystem hand = rightHand;
		for (int _playTimeStepIndex = 0; _playTimeStepIndex < hand.timeSteps.Count; _playTimeStepIndex++)
		{
			for (int i = 0; i < 5; i++)
		//		if ( i==0)
			{
				Vector3 pos = hand.timeSteps[_playTimeStepIndex].fingerTargetPos[i];
				Gizmos.color = hand.fingerTargetDefaultColor[i];
				Gizmos.DrawSphere(pos, 0.005f);
				if (_playTimeStepIndex>0)
					Gizmos.DrawLine(pos, hand.timeSteps[_playTimeStepIndex-1].fingerTargetPos[i]);
			}
		}

		if (!showInfo)
			return;

		return;
		
		Gizmos.color = Color.red;

		//	Gizmos.DrawSphere (leftHand.currentTargetPos (), 0.01f);
		//	Gizmos.DrawSphere (rightHand.currentTargetPos (), 0.02f);
		/*
		if (leftHand.fingerTargetDefaultColor.Length > 0)
			for (int i = 0; i < 5; i++) {
				Gizmos.color = leftHand.fingerTargetDefaultColor [i];
				Gizmos.DrawLine (leftHand.targetPos, leftHand.fingerTargetPos [i]);
		
				Gizmos.color = rightHand.fingerTargetDefaultColor [i];
				Gizmos.DrawLine (rightHand.targetPos, rightHand.fingerTargetPos [i]);
			}
*/
		Gizmos.color = Color.yellow;
		//	Gizmos.DrawSphere (leftHand.NextTargetPos (), 0.01f);
		//	Gizmos.DrawSphere (rightHand.NextTargetPos (), 0.02f);

	}

	string NotesToString (List<Note> notes)
	{

		string result = "";

		foreach (Note note in notes) {
			if (note.noteOn)
				result += piano.GetKeyName (note.note) + " ";
			else
				result += piano.GetKeyName (note.note) + "! ";

		}

		return result;

	
	}

	string FingerToString (int[] fingers)
	{

		string result = "";

		foreach (int finger in fingers) {

			result += (finger + 1) + " ";

		}

		return result;


	}

	void CreateTable ( HandSystem hand ) {

		GUIStyle myStyle = new GUIStyle ();
		myStyle.fontSize = 14;
		myStyle.font = myFont;
		myStyle.normal.textColor = Color.yellow;

		int offsetX = 300;
		int offSetY = 10;
		for (int idx = 0; idx < hand.timeSteps.Count; idx++) {

			//				if (hand.timeSteps [playTimeStepIndex].handPos.fingerNote [i] == -1) 
			String s;

			if (hand.timeSteps [idx].nextStep != null)
				s = hand.timeSteps [idx].nextStep.timeStepNo.ToString ();
			else
				s = "";
			GUI.Label (new Rect (offsetX, offSetY + idx * 20 + Screen.height * 0, 300, 200), "R" + idx + " " +
				String.Format (" {0,-8} {1,-15} {2,5} {3,5} {4,6}", NotesToString (hand.timeSteps [idx].notes),  

					"[ " + FingerToString (hand.timeSteps [idx].fingerNotes) + " ]", s, hand.timeSteps [idx].empty,

					hand.timeSteps [idx].handTargetposition.ToString(".000")
				)

				, myStyle);
			//				if ( hand.left )

		}

	
	}

	void OnGUI ()
	{

		if (!showInfo)
			return;
		

		if (!songReady || playTimeStepIndex < 0)
			return;



		WriteNotes (leftHand.timeSteps [playTimeStepIndex].notes, 10, 20, "");
		if (playTimeStepIndex < leftHand.timeSteps.Count - 1)
			WriteNotes (leftHand.timeSteps [playTimeStepIndex + 1].notes, 60, 20, "n");

		WriteNotes (rightHand.timeSteps [playTimeStepIndex].notes, 120, 20, "");
		if (playTimeStepIndex < rightHand.timeSteps.Count - 1)
			WriteNotes (rightHand.timeSteps [playTimeStepIndex + 1].notes, 170, 20, "n");

		int i = 0;
		foreach (Transform fing in leftHand.fingerTransforms) {

			Vector2 objPos = Camera.main.WorldToScreenPoint (fing.position);

			float XOffset = 0;
			float YOffset = 0;

			if (leftHand.timeSteps [playTimeStepIndex].fingerNotes [i] > -1)
				GUI.color = Color.white;
			else
				GUI.color = Color.gray;

			GUI.Label (new Rect (objPos.x - XOffset, -objPos.y + Screen.height + YOffset, 30, 30), "L" + (i + 1).ToString (""));
			if (leftHand.timeSteps [playTimeStepIndex].fingerNotes [i] > -1)
				GUI.Label (new Rect (objPos.x - XOffset, -objPos.y + Screen.height + YOffset + 15, 30, 30), leftHand.timeSteps [playTimeStepIndex].fingerNotes [i].ToString (""));
			else
				GUI.Label (new Rect (objPos.x - XOffset, -objPos.y + Screen.height + YOffset + 15, 30, 30), "");
						
			i++;

		
		}
			
		i = 0;
		foreach (Transform fing in rightHand.fingerTransforms) {

			Vector2 objPos = Camera.main.WorldToScreenPoint (fing.position);
			float XOffset = 0;
			float YOffset = 0;

			if (rightHand.timeSteps [playTimeStepIndex].fingerNotes [i] > -1)
				GUI.color = Color.white;
			else
				GUI.color = Color.gray;

			GUI.Label (new Rect (objPos.x - XOffset, -objPos.y + Screen.height + YOffset, 30, 30), "R" + (i + 1).ToString (""));
			if (rightHand.timeSteps [playTimeStepIndex].fingerNotes [i] > -1)
				GUI.Label (new Rect (objPos.x - XOffset, -objPos.y + Screen.height + YOffset + 15, 30, 30), rightHand.timeSteps [playTimeStepIndex].fingerNotes [i].ToString (""));
			i++;


		}

		GUI.color = Color.yellow;



		//		for (int idx = 0; idx < leftHand.timeSteps.Count; idx++) {
		//			if (rightHand.timeSteps [i].notes.Count > 0)

		CreateTable( leftHand );


	
	
	}




	void InitHand (HandSystem hand)
	{


		hand.handDefaultRotation = hand.handTransform.rotation;
		hand.handDefaultPosition = hand.handTransform.position;

		headTargetDefaultPosition = headTarget.position;

		hand.fingerTipsDefaultPosition = new Vector3[5];
		hand.fingerTargetDefaultColor = new Color[5];

		for (int i = 0; i < 5; i++) {
			hand.fingerTipsDefaultPosition [i] =  hand.fingerTransforms [i].position- hand.handTransform.position;
			hand.fingerTargetDefaultColor [i] = hand.fingerTransforms [i].GetComponent<Renderer>().material.GetColor("_EmissionColor");
		}


	}

	void Start ()
	{

		InitHand (leftHand);
		InitHand (rightHand);

		autofingering.Init ();

	}



	


	void SetFingerTargetColors (HandSystem hand)
	{
		for (int i = 0; i < 5; i++) {

			hand.fingerTransforms[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", hand.fingerTargetDefaultColor[i] / 3.0f);
			hand.fingerTransforms[i].GetComponent<Renderer>().material.SetColor("_Color", new Color(0,0,0) );

			if (hand.timeSteps [playTimeStepIndex].fingerNotes [i] != -1) {
				hand.fingerTransforms [i].GetComponent<Renderer> ().material.SetColor ("_EmissionColor", hand.fingerTargetDefaultColor[i] * 2.0f );
				hand.fingerTransforms [i].GetComponent<Renderer> ().material.SetColor ("_Color", new Color(0, 0, 0));
			}

		}
	}

	void DecideFingers(HandSystem hand, int indx, bool isLeftHand, List<int> fingers)
	{

		hand.left = isLeftHand;

		List<Note> notes = hand.timeSteps[indx].notes;

		if (simpleFingering)
		{
			// ref List<Note> notes,ref List<Note> notesNext,ref List<Note> notesPrev
			if (notes.Count == 5)
			{

				notes[0].fingerNo = 0;
				notes[1].fingerNo = 1;
				notes[2].fingerNo = 2;
				notes[3].fingerNo = 3;
				notes[4].fingerNo = 4;


			}
			if (notes.Count == 4)
			{

				notes[0].fingerNo = 0;
				notes[1].fingerNo = 1;
				notes[2].fingerNo = 2;
				notes[2].fingerNo = 4;

			}
			if (notes.Count == 3)
			{

				notes[0].fingerNo = 0;
				notes[1].fingerNo = 2;
				notes[2].fingerNo = 4;

			}

			if (notes.Count == 2)
			{

				notes[0].fingerNo = 0;
				notes[1].fingerNo = 4;

			}

			if (notes.Count == 1)
			{

				notes[0].fingerNo = notes[0].note % 5;

			}


			if (isLeftHand)
			{

				foreach (Note note in notes)
				{
					note.fingerNo = 4 - note.fingerNo;
				}

			}


		}
		else if (fingers != null)
			for (int n = 0; n < notes.Count; n++)
				notes[n].fingerNo = fingers[n] - 1;


		// INIT
		hand.timeSteps[indx].fingerTargetPos = new Vector3[5];
		hand.timeSteps[indx].fingerNotes = new int[5];
		hand.timeSteps[indx].handTargetposition = Vector3.zero;


		for (int i = 0; i < 5; i++)
			hand.timeSteps[indx].fingerNotes[i] = -1;

		Vector3 pressedFingersMean = Vector3.zero;

		int minFingerNo = -1;

		foreach (Note note in hand.timeSteps[indx].notes)
		{
			hand.timeSteps[indx].fingerTargetPos[note.fingerNo] = GetPosition(note);//;
			hand.timeSteps[indx].fingerNotes[note.fingerNo] = note.note;
			if (note.fingerNo>minFingerNo) { 
				pressedFingersMean = hand.timeSteps[indx].fingerTargetPos[note.fingerNo] ;
				minFingerNo = note.fingerNo;
			}
		
		}
		


		if (notes.Count > 0)
		{
			hand.timeSteps[indx].handTargetposition = hand.timeSteps[indx].fingerTargetPos[minFingerNo] - hand.fingerTipsDefaultPosition[minFingerNo];
			for (int i = 0; i < 5; i++)
				if (hand.timeSteps[indx].fingerNotes[i] == -1)
					hand.timeSteps[indx].fingerTargetPos[i] = hand.timeSteps[indx].handTargetposition + hand.fingerTipsDefaultPosition[i];
		}
		else //n no finger pressed
		{


			hand.timeSteps[indx].empty = true;
			/*
			if (indx > 0)
				hand.timeSteps[indx].handTargetposition = hand.timeSteps[indx - 1].handTargetposition;// 

			for (int i = 0; i < 5; i++)
				hand.timeSteps[indx].fingerTargetPos[i] = hand.timeSteps[indx].handTargetposition +hand.fingerTipsDefaultPosition[i];
			*/
		}

		
	}

	void SetHandAndFingersPosition (HandSystem hand)
	{
		
		if (playTimeStepIndex >= hand.timeSteps.Count)
			return;

		piano.playing = playing;

		if (!hand.timeSteps [playTimeStepIndex].empty) {

			hand.state = HandState.pressing;
			hand.lastPressTime = playerTimer;
			hand.timeSteps [playTimeStepIndex].played = true;
	
			if (!playing) {
				piano.Clear();
				hand.handTransform.position = hand.timeSteps [playTimeStepIndex].handTargetposition + handOffset;// + (useHandOffset ? handOffset : Vector3.zero);

				for (int i = 0; i < 5; i++)
					hand.fingerTransforms[i].position = hand.timeSteps[playTimeStepIndex].fingerTargetPos[i];//Vector3.Lerp( hand.fingerTargets[i].position, hand.fingerTargetPos[i], 
			}

	



			for (int i = 0; i < 5; i++)
			{
				if (hand.timeSteps[playTimeStepIndex].fingerNotes[i] > -1)
				{
					piano.PressColored(hand.timeSteps[playTimeStepIndex].fingerNotes[i], hand.fingerTargetDefaultColor[i]);
				}
			}

			
	
		} else
        {
			hand.state = HandState.waiting;
			hand.timeSteps[playTimeStepIndex].played = true;

		}


	}



	/*
	 * 
	 * 
	 * 
	 *  U P D A T E     H A N D 
	 * 
	 * 
	 */

	public Vector3 SetPressOffset(int i)
    {
		return i == -1 ? unpressedFingerOffset: pressedFingerOffset;
	}

	void UpdateHand (HandSystem hand)
	{
		if ( songReady)
			SetFingerTargetColors(hand);

		if (!playing)
			return;


			if (hand.state == HandState.pressing) {
			hand.handTransform.position = hand.timeSteps [playTimeStepIndex].handTargetposition+ handOffset;//+ (useHandOffset ? handOffset : Vector3.zero);
			/*
			for (int i = 0; i < 5; i++)
			{
				hand.fingerTransforms[i].position += SetPressOffset(hand.timeSteps[playTimeStepIndex].fingerNotes[i]);
			}

			*/
			//	if ( !hand.timeSteps [playTimeStepIndex].empty )
			//for (int i = 0; i < 5; i++)
		//		hand.fingerTransforms [i].position = hand.timeSteps [playTimeStepIndex].fingerTargetPos [i];//Vector3.Lerp( hand.fingerTargets[i].position, hand.fingerTargetPos[i], Time.deltaTime*5 );

			// nota suresinin handMovingRatio kadar zamaninda diger basilacak notaya gecmek uzere harekete gec
			if (hand.NextTime () - playerTimer <= (hand.NextTime () - hand.lastPressTime) * handStartMovingRatio) {
			
				hand.lastPressTime = playerTimer;
				hand.state = HandState.moving;

				


			}

		} 

		if (hand.state == HandState.moving || hand.state == HandState.waiting) {

			float t = 1;

			if (hand.NextTime () - hand.lastPressTime > 0) {
				
				t = (playerTimer - hand.lastPressTime) / (hand.NextTime () - hand.lastPressTime); 

			}
		



			if (hand.Next () != null) {
		//		hand.handTransform.position = Vector3.Lerp (hand.timeSteps [playTimeStepIndex].handTargetposition + handOffset, hand.Next ().handTargetposition + handOffset, moveCurve.Evaluate (t));
			hand.handTransform.position = Vector3.Lerp (hand.handTransform.position, hand.Next ().handTargetposition + handOffset, moveCurve.Evaluate (t));
				hand.handTransform.position += new Vector3(0, handFlightHeight, 0) * heightCurve.Evaluate(t) ;

				for (int i = 0; i < 5; i++)
				{
						hand.fingerTransforms[i].position = Vector3.Lerp(hand.fingerTransforms[i].position, hand.Next().fingerTargetPos[i], moveCurve.Evaluate(t));//

					//hand.fingerTransforms[i].position = Vector3.Lerp(hand.timeSteps[playTimeStepIndex].fingerTargetPos[i], hand.Next().fingerTargetPos[i], moveCurve.Evaluate(t));// ;//

					
				//	hand.fingerTransforms[i].position += SetPressOffset(hand.timeSteps[playTimeStepIndex].fingerNotes[i]);// * heightCurve.Evaluate(t);
					hand.fingerTransforms[i].position += SetPressOffset(hand.Next().fingerNotes[i]) * moveCurve.Evaluate(t);
					hand.fingerTransforms[i].position += new Vector3(0, fingerFlightHeight, 0) * heightCurve.Evaluate(t);
					/*
						if (hand.timeSteps[playTimeStepIndex].fingerNotes[i] == -1) { 
							hand.fingerTransforms[i].position += unpressedFingerOffset;
										
						hand.fingerTransforms[i].position += new Vector3(0, fingerFlightHeight, 0) * heightCurve.Evaluate(t);

					}
					else
						hand.fingerTransforms[i].position += new Vector3(0f, pressedFingerOffset, 0f); ;


					*/
				}


					/*
					for (int i = 0; i < 5; i++)
					{
						hand.fingerTransforms [i].position = Vector3.Lerp (hand.fingerTransforms [i].position , hand.Next ().fingerTargetPos [i], moveCurve.Evaluate (t));//
						//hand.fingerTransforms[i].position = Vector3.Lerp(hand.timeSteps[playTimeStepIndex].fingerTargetPos[i], hand.Next().fingerTargetPos[i], moveCurve.Evaluate(t));
					hand.fingerTransforms[i].position += new Vector3(0, handHeight / 5.0f, 0) * heightCurve.Evaluate(t);
					} */
				}
			else {
			//	Debug.Log("***************");
				/*hand.handTransform.position =hand.handDefaultPosition;
				for (int i = 0; i < 5; i++) {
					hand.fingerTransforms [i].position = hand.fingerTipsDefaultPosition[i];//Vector3.Lerp( hand.fingerTargets[i].position, hand.fingerTargetPos[i], Time.deltaTime*5 );
				}*/
			}

		}


		//	SetFingerTargets (hand);

	}

	private Vector3 headVel;
	public float headSmooth = 1f;
	void UpdateHead ()
	{


		Vector3 center = Vector3.zero;
		int divide = 0;
		if (playTimeStepIndex + 1 < leftHand.timeSteps.Count)
		if (leftHand.timeSteps [playTimeStepIndex+1].notes.Count > 0) {
			center += leftHand.handTransform.position;
			divide++;
		}
		if (playTimeStepIndex + 1 < rightHand.timeSteps.Count)
			if (rightHand.timeSteps [playTimeStepIndex+1].notes.Count > 0) {
			center += rightHand.handTransform.position;
			divide++;
		}

		if (divide > 0)
			headTarget.position = Vector3.SmoothDamp(headTarget.position, center / divide+new Vector3(0, 0, 0.1f * Mathf.Sin(Time.time * 3f)), ref headVel, headSmooth);
		else
		headTarget.position = Vector3.SmoothDamp(headTarget.position, headTargetDefaultPosition + new Vector3(0, 0.5f, 0.1f * Mathf.Sin(Time.time * 3f)), ref headVel, headSmooth);

	}

	public void Act (float time, TimeStep timeStep)
	{

		rightHand.currentTimeStepIndex = playTimeStepIndex;
		leftHand.currentTimeStepIndex = playTimeStepIndex;

		SetHandAndFingersPosition (leftHand);
		SetHandAndFingersPosition (rightHand);

	}


	void Update ()
	{
	
		if (!songReady)
			return;

		if (playTimeStepIndex < 0)
			return;

		if (!playing)
			return;

		UpdateHand (leftHand);
		UpdateHand (rightHand);

		UpdateHead ();
	}

	void AddNote (List<Note> notes, Note note)
	{

		for (int i = 0; i < notes.Count; i++)
			if (notes [i].note == note.note)
				return;
			
		notes.Add (note);

	}


	void RemoveNote (List<Note> notes, Note note)
	{

		for (int i = 0; i < notes.Count; i++) {
		
			if (notes [i].note == note.note) {
				notes.RemoveAt (i);
				return;
			}

		}
	}

	public string TimeStepToText (TimeStep timeStep)
	{
	
		string s = "";
		s += timeStep.time.ToString ("");
		foreach (Note note in timeStep.notes)
			s += " " + piano.GetKeyName (note.note); 

		return s;
		
	}


	float GetTime (List< Note> notes)
	{
	
		if (notes.Count > 0)
			return notes [0].time;
		else
			return 0;

	}

	void SetNotesForHand (int channel, TimeStep prev_Step, TimeStep timeStep, List< Note> notes)
	{

		foreach (Note note in timeStep.notes) {

			if (note.channel == channel) {
				if (note.noteOn)
					AddNote (notes, note);
			}

		}

		notes.Sort ((x, y) => x.note.CompareTo (y.note));
		return;


		if (prev_Step != null)
			foreach (Note note in prev_Step.notes) {

				AddNote (notes, note);

			}
				
		foreach (Note note in timeStep.notes) {

			if (note.channel == channel) {
				if (note.noteOn)
					AddNote (notes, note);
				else
					RemoveNote (notes, note);
			}

		}

		notes.Sort ((x, y) => x.note.CompareTo (y.note));
		       
	}


	public void PreProcessScore (List<TimeStep> timeSteps)
	{
		
		leftHand.timeSteps = new List<TimeStep> ();
		rightHand.timeSteps = new List<TimeStep> ();
		TimeStep l_lastStep = null;
		TimeStep r_lastStep = null;
				
		autofingering.InitLayers ();
		int timeStepCount = 0;

		foreach (TimeStep _timeStep in timeSteps) {
			
			TimeStep l_timeStep = new TimeStep ();
			TimeStep r_timeStep = new TimeStep ();

			SetNotesForHand (piano.leftHandChannel, l_lastStep, _timeStep, l_timeStep.notes);
			SetNotesForHand (piano.rightHandChannel, r_lastStep, _timeStep, r_timeStep.notes);

			if (!simpleFingering) {

				if (l_timeStep.notes.Count > 0)
				if (!l_timeStep.empty) {
					l_timeStep.notesInt.Clear ();
					foreach (Note note in l_timeStep.notes)
						if (note.noteOn)
							l_timeStep.notesInt.Add (note.note);
					if (l_timeStep.notesInt.Count > 0)
						autofingering.AddLayers (timeStepCount, l_timeStep.notesInt, "left");
				}

				if (r_timeStep.notes.Count > 0)
				if (!r_timeStep.empty) {
					r_timeStep.notesInt.Clear ();
					foreach (Note note in r_timeStep.notes)
						if (note.noteOn)
							r_timeStep.notesInt.Add (note.note);
					if (r_timeStep.notesInt.Count > 0)
						autofingering.AddLayers (timeStepCount, r_timeStep.notesInt, "right");
				}
			}


			l_timeStep.time = _timeStep.time;
			r_timeStep.time = _timeStep.time;
			l_timeStep.timeStepNo = timeStepCount;
			r_timeStep.timeStepNo = timeStepCount;


			leftHand.timeSteps.Add (l_timeStep);
			rightHand.timeSteps.Add (r_timeStep);
			l_lastStep = l_timeStep;
			r_lastStep = r_timeStep;

			timeStepCount++;

		} 


		Dictionary< int, List<int> > fingersLeft = new Dictionary< int, List<int> > ();
		Dictionary< int, List<int> > fingersRight = new Dictionary< int, List<int> > ();

		if (!simpleFingering) {
			fingersLeft = autofingering.computeFingering (autofingering.layersLeft, "left");
			autofingering.DebugResultFingers (fingersLeft);

			fingersRight = autofingering.computeFingering (autofingering.layersRight, "right");
			autofingering.DebugResultFingers (fingersRight);
		}

		for (int i = 0; i < timeSteps.Count; i++) {

			List<int> fing = new List<int> ();
			fingersLeft.TryGetValue (i, out fing); 
			DecideFingers (leftHand, i, true, fing);
			fingersRight.TryGetValue (i, out fing); 
			DecideFingers (rightHand, i, false, fing);

		}


		leftHand.FindNextTimeSteps ();
		rightHand.FindNextTimeSteps ();

		songReady = true; 
		rightHand.currentTimeStepIndex = 0;
		leftHand.currentTimeStepIndex = 0;



	}

}
