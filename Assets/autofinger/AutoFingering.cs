using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

public class AutoFingering 
{
	Cost cost = new Cost ();


	public Dictionary<string, int> NOTE_INDICES = new Dictionary<string, int> () {
		{ "C", 0 },
		{ "D", 2 },
		{ "E", 4 },
		{ "F", 5 },
		{ "G", 7 },
		{ "A", 9 },
		{ "B", 11 }
	};

	string[][] notesString = {
		new string[] { "C4" },
		new string[] { "E4" },
		new string[] { "G4" },
		new string[] { "C4", "E4", "A4" },
		new string[] { "B5" }
	};

	List<List<int>> notes = new List<List<int>> ();
	List<List<List<int>>> ALL_FINGER_OPTIONS_LEFT = new List<List<List<int>>> ();
	List<List<List<int>>> ALL_FINGER_OPTIONS_RIGHT = new List<List<List<int>>> ();

	public Dictionary< int, List<int> > fingersLeft = new Dictionary< int, List<int> >();
	public Dictionary< int, List<int> > fingersRight = new Dictionary< int, List<int> >();


	public int nameToMidi (string name)
	{
		if ( name.Length==0 )
			return 0;
		string fl = name [0].ToString ();
		int note = NOTE_INDICES [fl];

		int octave = 5;

		int offset = 1;

		if (name.Length > 1) {
			if (name [1] == 'b') {
				note -= 1;
				offset = 2;
			} else if (name [1] == '#') {
				note += 1;
				offset = 2;
			}
		}

		if (name.Length > offset) {
			octave = int.Parse (name [offset].ToString ());
		}
		return octave * 12 + note;
	}

	public string debugString;


	public class Node
	{
		public int id;
		public List<int> notes;
		public List<int> fingers;
		public float score = 0;
		public Node best_previous_node;

	}

	public class Layer
	{

		public List<Node> nodes;

	}

	public List<Layer> layersLeft = new List<Layer> ();
	public List<Layer> layersRight = new List<Layer> ();

	public void InitLayers() {
	
		layersLeft.Clear();
		layersLeft.Add (makeLayer (-1, new List<int> (), "left"));

		layersRight.Clear();
		layersRight.Add (makeLayer (-1, new List<int> (), "right"));

	}

	public void AddLayers(int id, List<int>notelist, string left_or_right ) {
		if ( left_or_right =="left" )
			layersLeft.Add ( makeLayer (id, notelist, left_or_right));
		else
			layersRight.Add ( makeLayer (id, notelist, left_or_right));
		
//		if (notelist.Count>0)
//			Debug.Log( id + "  " +notelist[0] );

	}

	public   Layer makeLayer (int id, List<int> notes, string left_or_right)
	{

		Layer layer = new Layer ();

		List<List<int>> options = new List<List<int>> ();
		if (notes.Count > 0) {
			if (left_or_right == "right")
				options = ALL_FINGER_OPTIONS_RIGHT [notes.Count - 1];
			else
				options = ALL_FINGER_OPTIONS_LEFT [notes.Count - 1];
		}

		layer.nodes = new List<Node> ();

		foreach (List<int> option in options) {
			Node node = new Node ();
			node.id = id;
			node.notes = new List<int> ();
			node.fingers = new List<int> ();
			node.notes.AddRange (notes);
			node.fingers.AddRange (option);

			layer.nodes.Add (node);
		}

		return layer;
	}

	public  float computeRightHandCost (int n1, int  n2, int  f1, int  f2)
	{
		string key = n1 + "," + n2 + "," + f1 + "," + f2;
		return cost.right_hand_cost_database [key];
	}

	public  float computeLeftHandCost (int n1, int  n2, int  f1, int  f2)
	{
		string key = n1 + "," + n2 + "," + f1 + "," + f2;
		return cost.left_hand_cost_database [key];
	}




	public float calcCost (Node current_node, Node  previous_node, string left_or_right)
	{
		
		float total_cost = 0;

		//# Go through each note in the current node
		for (int i = 0; i < current_node.notes.Count; i++) {
			int current_note = current_node.notes [i];
			int current_finger = current_node.fingers [i];

			//# This helps add the "state" cost of actually using those fingers for
			//# that chord. This isn't captured by the transition costs
			bool has_next_note = i < current_node.notes.Count - 1;
			
			if (has_next_note) {
				int next_note = current_node.notes [i + 1];
				int next_finger = current_node.fingers [i + 1];

				if (left_or_right == "right")
					total_cost += computeRightHandCost (current_note, next_note, current_finger, next_finger);
				else
					total_cost += computeLeftHandCost (current_note, next_note, current_finger, next_finger);

			}
							
			//# Add up scores for each of the previous nodes notes trying to get to current node note
			for (int j = 0; j < previous_node.notes.Count; j++) {
				int previous_note = previous_node.notes [j];
				int previous_finger = previous_node.fingers [j];

				if (left_or_right == "right")
					total_cost += computeRightHandCost (previous_note, current_note, previous_finger, current_finger);
				else
					total_cost += computeLeftHandCost (previous_note, current_note, previous_finger, current_finger);

			}
		}

		return total_cost;
	}

	public Dictionary< int, List<int> >  computeFingering (List<Layer> layers, string left_or_right)
	{

		Debug.Log( "layer " + layers.Count );


		for (int layer_index = 1; layer_index < layers.Count; layer_index++) 

		//# Go through each node in the layer
			foreach (Node current_node in layers[layer_index].nodes) {
				float min_score = 1000;


				//# Go through each node in the previous layer
				foreach (Node previous_node in layers[layer_index - 1].nodes) {
					float total_cost = previous_node.score;

					float cost = calcCost (current_node, previous_node, left_or_right);

					total_cost += cost;

					if (total_cost < min_score) {
						min_score = total_cost;
						current_node.score = total_cost;
						current_node.best_previous_node = previous_node;
					}
				}
			}


		//# Find the best final node
		Node best_node = layers [layers.Count - 1].nodes [0];
		foreach (Node node in layers[layers.Count-1].nodes)
			if (node.score < best_node.score)
				best_node = node;


	//	List<List<int>> result = new List<List<int>> ();
		Dictionary< int, List<int> > result = new Dictionary< int, List<int> >();

			
		while (best_node != null) {
			//result.insert(0, dict(notes=best_node.notes, fingers=best_node.fingers));
			result.Add (best_node.id, best_node.fingers);
			best_node = best_node.best_previous_node;
		}

		return result;


			


	}


	List<List<int>> walkResult = new List<List<int>> ();


	void walk (int nb_fingers, List<int>  current_fingers, List<int>  finger_options, string left_or_right)
	{


		if (current_fingers.Count == nb_fingers) {
			List<int> current = new List<int> ();
			current.AddRange (current_fingers);
			current.Sort ();
			if (left_or_right == "left")
				current.Reverse ();
			walkResult.Add (current);
			//return;

		} else
			for (int i = 0; i < finger_options.Count; i++) {  
				List<int> current = new List<int> ();
				current_fingers.Add (finger_options [i]);
				current.AddRange (current_fingers);


				walk (nb_fingers, current, finger_options.GetRange (0, i), left_or_right);
				if (current_fingers.Count > 0)
					current_fingers.RemoveAt (current_fingers.Count - 1);
			}



	}

	

	public List<List<int>>  getAllFingerOptions (int nb_fingers, string left_or_right)
	{

		List <int> finger_options = new List <int> () { 1, 2, 3, 4, 5 };
		walkResult.Clear ();

		walk (nb_fingers, new List<int> (), finger_options, left_or_right);

		return walkResult;
	
			
	}

	public void ClearNotes(){

		notes.Clear();
	}


	public void  AddNotes(List<int> _notes ){

		notes.Add( _notes );
	
	}

	public void  AddSampleNotes() {
	
		foreach (string[] notelist in notesString) {
			List<int> _notes = new  List<int> ();
			for (int i = 0; i < notelist.Length; i++) {
				_notes.Add (nameToMidi (notelist [i]));

			}
			notes.Add (_notes);
		}

		string left_or_right = "right";

		layersRight.Add (makeLayer (0, new List<int> (), left_or_right));

		int count = 0;
		foreach (List<int> notelist in notes) {
			layersRight.Add ( makeLayer (count, notelist, left_or_right));
			count++;
		}
		fingersRight = computeFingering ( layersRight, left_or_right);

		 left_or_right = "left";

		layersLeft.Add (makeLayer (0, new List<int> (), left_or_right));

		count = 0;
		foreach (List<int> notelist in notes) {
			layersLeft.Add ( makeLayer (count, notelist, left_or_right));
			count++;
		}




		fingersLeft = computeFingering ( layersLeft, left_or_right);


		//DebugResultFingers( fingersRight );
		//DebugResultFingers( fingersLeft );

	}

	public void DebugResultFingers( Dictionary< int, List<int> >  fingers) {
	


		debugString = "";
		int g = 0;

		foreach (var  item in fingers) { 
			string s = "";
			//	s += nlist.Count;
			s += item.Key;
			foreach (int finger in item.Value ) {
				s += " " + finger.ToString (); 
			}

			debugString += " " + s + "\n";
		}

		Debug.Log (debugString);
	
	}

	// Use this for initialization
	public void Init ()
	{
		

		cost.Init ();

	

		for (int i = 1; i < 6; i++) {
			List<List<int>> temp = new List<List<int>> ();
			temp.AddRange (getAllFingerOptions (i, "right"));
			ALL_FINGER_OPTIONS_RIGHT.Add (temp);

			List<List<int>> temp2 = new List<List<int>> ();
			temp2.AddRange (getAllFingerOptions (i, "left"));
			ALL_FINGER_OPTIONS_LEFT.Add (temp2);
		}

		//AddSampleNotes();


	
	



	}
	

}

/* 
  debugString = "";
		int g=0;
		foreach (List<List<int>> nlist in ALL_FINGER_OPTIONS_LEFT) { 
			string s = "";
			s += nlist.Count;
			foreach (List<int> list in nlist) {
				
				s += "[";
				foreach (int i in list)
					s += " " + i.ToString (); 
				s += "]";

			}

			debugString += g + " " + s + "\n";
			g++;
		}  

		Debug.Log (debugString);




debugString = "";
		int g=0;
		foreach (Layer layer in layers) { 
			string s = "";
		//	s += nlist.Count;
			foreach (Node node in layer.nodes) {

				s += "[";
				foreach (int note in node.notes)
					s += " " + note.ToString (); 
				s += "]";

				s += "[";
				foreach (int note in node.fingers)
					s += " " + note.ToString (); 
				s += "]";


			}

			debugString += g + " " + s + "\n";
			g++;
		}  

		Debug.Log (debugString);
*/