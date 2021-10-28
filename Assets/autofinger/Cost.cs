using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://github.com/Kanma/piano_fingering

public  class Cost 
{

	public float MOVE_CUTOFF = 7.5f;



	public Dictionary<string, float> FINGER_DISTANCE = new  Dictionary<string, float> () {
		{ "1,1", 0f },
		{ "1,2", 2f },
		{ "1,3", 3.5f }, //# Making an allowance since this seriously is either 3 or 4 about half the time
		{ "1,4", 5f },
		{ "1,5", 7f },
		{ "2,1", 2f },
		{ "2,2", 0f },
		{ "2,3", 2f },
		{ "2,4", 3.5f },  //# Same
		{ "2,5", 5f },
		{ "3,1", 3.5f }, // # Same
		{ "3,2", 2f },
		{ "3,3", 0f },
		{ "3,4", 2f },
		{ "3,5", 3.5f },// # Same
		{ "4,1", 5f },
		{ "4,2", 3.5f }, //# Same
		{ "4,3", 2f },
		{ "4,4", 0f },
		{ "4,5", 2f },
		{ "5,1", 7f },
		{ "5,2", 5f },
		{ "5,3", 3.5f }, //# Same
		{ "5,4", 2f },
		{ "5,5", 0f }
	};
		

	public Dictionary<int, float> MOVE_HASH_BASE = new  Dictionary<int, float> () {
		{ 1 , 0f },
		{ 2 , 0.5f },
		{ 3 , 1.8f },
		{ 4 , 3f },
		{ 5 , 5f },
		{ 6 , 7f },
		{ 7 , 8f },
		{ 8 , 8.9f },
		{ 9 , 9.7f },
		{ 10 , 10.5f },
		{ 11 , 11f },
		{ 12 , 11.4f },
		{ 13 , 11.8f },
		{ 14 , 12.2f },
		{ 15 , 12.5f },
		{ 16 , 12.8f },
		{ 17 , 13.1f },
		{ 18 , 13.4f },
		{ 19 , 13.7f },
		{ 20 , 14f },
		{ 21 , 14.3f },
		{ 22 , 14.6f },
		{ 23 , 14.9f },
		{ 24 , 15.2f }
	};



	public Dictionary<int, string> COLOR = new  Dictionary<int, string> {
		{ 0, "White" },
		{ 1, "Black" },
		{ 2, "White" },
		{ 3, "Black" },
		{ 4, "White" },
		{ 5, "White" },
		{ 6, "Black" },
		{ 7, "White" },
		{ 8, "Black" },
		{ 9, "White" },
		{ 10, "Black" },
		{ 11, "White" }
	};
		





	public Dictionary<string, float> DESC_THUMB_STRETCH_VALS = new  Dictionary<string, float> {
		{ "1,2" , 1f },
		{ "1,3" , 1f },
		{ "1,4" , 0.9f },
		{ "1,5" , 0.95f }
	};




	public Dictionary<string, float> ASC_THUMB_STRETCH_VALS = new  Dictionary<string, float> {
		{ "2,1", 0.95f },
		{ "3,1", 1f },
		{ "4,1", 0.95f },
		{ "5,1", 0.95f }
	};



	public Dictionary<string, float> FINGER_STRETCH = new  Dictionary<string, float> {
		{ "1,1", 0.8f },
		{ "1,2", 1.15f },
		{ "1,3", 1.4f },
		{ "1,4", 1.45f },
		{ "1,5", 1.6f },
		{ "2,1", 1.15f },
		{ "2,2", 0.6f },
		{ "2,3", 0.9f },
		{ "2,4", 1.15f },
		{ "2,5", 1.3f },
		{ "3,1", 1.4f },
		{ "3,2", 0.9f },
		{ "3,3", 0.6f },
		{ "3,4", 0.9f },
		{ "3,5", 1.15f },
		{ "4,1", 1.45f },
		{ "4,2", 1.15f },
		{ "4,3", 0.9f },
		{ "4,4", 0.7f },
		{ "4,5", 0.7f },
		{ "5,1", 1.6f },
		{ "5,2", 1.3f },
		{ "5,3", 1.15f },
		{ "5,4", 0.8f },
		{ "5,5", 0.6f }
	};
		

	public Dictionary<string, float> right_hand_cost_database = new  Dictionary<string, float> ();
	public Dictionary<string, float> left_hand_cost_database = new  Dictionary<string, float> ();
	public Dictionary<int, float> MOVE_HASH = new Dictionary<int, float> ();



	public Dictionary<int, float> makeMoveHash (int fixed_cost)
	{
		Dictionary<int, float> result = new Dictionary<int, float> ();

		foreach (int key in MOVE_HASH_BASE.Keys) {
			float val = MOVE_HASH_BASE [key];
			result.Add (key, val + fixed_cost);
		}

		return result;


		/*for k,v in MOVE_HASH_BASE.items():
		result[k] = v + fixed_cost
		return result */
	}





	float colorRules (int n1, int n2, int  f1, int  f2, float finger_distance)
	{  
		//# If you're moving up from white to black with pinky or thumb, that's much harder
		//# than white-to-white would be. So we're adding some amount.
		if ((COLOR [n1 % 12] == "White") && (COLOR [n2 % 12] == "Black")) {
			if ((f2 == 5) || (f2 == 1))
				return 4f;    //# Using thumb or pinky on black is extra expensive

			if (finger_distance == 0f)
				return 4f;  // # Using same finger is extra expensive
		}

		if ((COLOR [n1 % 12] == "Black") && (COLOR [n2 % 12] == "White")) {
			if ((f1 == 5) || (f1 == 1))
				return 4f;    //# Moving from thumb or pinky that's already on black is extra expensive

			if (finger_distance == 0f)
				return -1f;   //# Moving black to white with same finger is a slide. That's easy and
			//  # common. reduce slightly.
		}

		return 0f;   //# If none of the rules apply, then don't add or subtract anything

	}


	float  ascMoveFormula (int note_distance, float finger_distance, int n1, int n2, int f1, int f2)
	{
		//"""This is for situations where direction of notes and fingers are opposite,
		//because either way, you want to add the distance between the fingers.
		//"""

		//# The math.ceil part is so it really hits a value in our moveHash.
		//# This could be fixed if I put more resolution into the moveHash
    
		int total_distance = Mathf.CeilToInt (note_distance + finger_distance);

		float cost = 0f;
		//# This adds a small amount for every additional halfstep over 24. Fairly
		//# representative of what it should be. 
		if (total_distance > 24)
			return MOVE_HASH [24] + (total_distance - 24f) / 5f;
		else {
			cost = MOVE_HASH [total_distance];
			cost += colorRules (n1, n2, f1, f2, finger_distance);
			return cost;
		}

	}

	float fingerDistance (int f1, int f2)
	{
		// """Currently assumes your on Middle C. Could potentially take into account n1 as
		// a way to know how to handle the irregularities. Such as E-F being 1 half step,
		// but G-A being 2.
		//"""
		string key = f1 + "," + f2;
		return FINGER_DISTANCE [key];
	}




	float  ascThumbStretch (int f1, int f2)
	{
    
		string key = f1 + "," + f2;
		return ASC_THUMB_STRETCH_VALS [key];

	}


	float descThumbStretch (int f1, int f2)
	{
    
		string key = f1 + "," + f2;
		return DESC_THUMB_STRETCH_VALS [key];
	}



	float fingerStretch (int f1, int f2)
	{
		string key = f1 + "," + f2;
		return FINGER_STRETCH [key];
	}


	float thumbCrossCostFunc (float x)
	{
		//  """Got this crazy function from regressing values I wanted at about 15 points
		// along the graph."""
		return 0.0002185873295f * Mathf.Pow (x, 7) - 0.008611946279f * Mathf.Pow (x, 6) +
		0.1323250066f * Mathf.Pow (x, 5) - 1.002729677f * Mathf.Pow (x, 4) +
		3.884106308f * Mathf.Pow (x, 3) - 6.723075747f * Mathf.Pow (x, 2) +
		1.581196785f * x + 7.711241722f;

	}


	float ascMoveFormula (float note_distance, float finger_distance, int n1, int n2, int f1, int f2)
	{
//    """This is for situations where direction of notes and fingers are opposite,
		//  because either way, you want to add the distance between the fingers.

//    # The math.ceil part is so it really hits a value in our moveHash.
		//  # This could be fixed if I put more resolution into the moveHash
		int total_distance = Mathf.CeilToInt (note_distance + finger_distance);
		float cost = 0f;

//    # This adds a small amount for every additional halfstep over 24. Fairly
		//  # representative of what it should be. 
		if (total_distance > 24) {
        
			//   Debug.Log( "ascMoveFormula cost" +  MOVE_HASH[24] + (total_distance - 24f) / 5f );

			return (MOVE_HASH [24] + (total_distance - 24f) / 5f);
    
		} else {
			cost = MOVE_HASH [total_distance];
			cost += colorRules (n1, n2, f1, f2, finger_distance);
			//    Debug.Log( "ascMoveFormula cost" + cost);
			return cost;
		}
	}


	float descMoveFormula (float note_distance, float finger_distance, int n1, int n2, int f1, int f2)
	{
		//"""This is for situations where direction of notes and fingers is the
		//same. You want to subtract finger distance in that case.
		// """

		//# The math.ceil part is so it really hits a value in our moveHash.
		//# This could be fixed if I put more resolution into the moveHash
		int total_distance = Mathf.CeilToInt (note_distance - finger_distance);
		float cost = 0f;

		//# This adds a small amount for every additional halfstep over 24. Fairly
		//# representative of what it should be. 
		if (total_distance > 24) {

			//	 Debug.Log( "descMoveFormula cost" +  MOVE_HASH[24] + (total_distance - 24f) / 5f );

			return MOVE_HASH [24] + (total_distance - 24f) / 5f;
		} else {
			cost = MOVE_HASH [total_distance];
			cost += colorRules (n1, n2, f1, f2, finger_distance);
			//	  Debug.Log( "descMoveFormula cost" + cost);
			return cost;
		}
	}



	float ascThumbCost (float note_distance, float finger_distance, int n1, int n2, int f1, int f2)
	{
		float stretch = ascThumbStretch (f1, f2);
		float x = (note_distance + finger_distance) / stretch;
		float cost = 0f;

//    # If it's over 10, again use the move formula
		if (x > 10f) {

			return ascMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
		} else {
			cost = thumbCrossCostFunc (x);
			if ((COLOR [n1 % 12] == "White") && (COLOR [n2 % 12] == "Black"))
				cost += 8f;
			//   	  Debug.Log( "ascThumbCost cost" + cost);
			return cost;
		}
	}



	float descThumbCost (float note_distance, float finger_distance, int n1, int n2, int f1, int f2)
	{

		float stretch = descThumbStretch (f1, f2);
		float x = (note_distance + finger_distance) / stretch;
   
		float cost = 0f;

		//# If it's over 10, again use the move formula
		if (x > 10f)
			return ascMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
		else {
			cost = thumbCrossCostFunc (x);
			if ((COLOR [n1 % 12] == "Black") && (COLOR [n2 % 12] == "White"))
				cost += 8f;

			//       Debug.Log( "descThumbCost cost" + cost);

			return cost;
		}
	}


	float costFunc (float x)
	{
		return -0.0000006589793725f * Mathf.Pow (x, 10) -
		0.000002336381414f * Mathf.Pow (x, 9) +
		0.00009925769823f * Mathf.Pow (x, 8) +
		0.0001763353131f * Mathf.Pow (x, 7) -
		0.004660305277f * Mathf.Pow (x, 6) -
		0.004290746384f * Mathf.Pow (x, 5) +
		0.06855725903f * Mathf.Pow (x, 4) +
		0.03719817227f * Mathf.Pow (x, 3) +
		0.4554696705f * Mathf.Pow (x, 2) -
		0.08305450359f * x +
		0.3020594956f;

	}

	float ascDescNoCrossCost (float note_distance, float finger_distance,float x, int n1, int n2, int f1, int f2)
	{
		// checked
   
		float cost = 0f;


		if ((x > 6.8f) && (x <= MOVE_CUTOFF)) {
			//	Debug.Log( "ascDescNoCrossCost" + costFunc(6.8f) + (x - 6.8f) * 3f);
			return costFunc (6.8f) + (x - 6.8f) * 3f;
    
		} else {
			cost = costFunc (x);
			cost += colorRules (n1, n2, f1, f2, finger_distance);
			//  	Debug.Log( "ascDescNoCrossCost" + cost);
			return cost;
		}

  
	}



	void  computeLeftHandCost (int n1, int n2, int  f1, int  f2)
	{
		string key = n1 + "," + n2 + "," + f1 + "," + f2;
		int note_distance = Mathf.Abs (n2 - n1);
		float finger_distance = fingerDistance (f1, f2);

		if ((note_distance > 0) && (f2 - f1 == 0))
			left_hand_cost_database [key] = ascMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
		else if ((n2 - n1 <= 0) && (f2 - f1 < 0) && (f2 != 1))
			left_hand_cost_database [key] = ascMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
		else if ((n2 - n1 > 0) && (f2 - f1 > 0) && (f1 != 1))
			left_hand_cost_database [key] = ascMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
		else if ((n2 - n1 <= 0) && (f2 - f1 < 0) && (f2 == 1))
			left_hand_cost_database [key] = ascThumbCost (note_distance, finger_distance, n1, n2, f1, f2);
		else if ((n2 - n1 >= 0) && (f1 == 1) && (f2 != 1))
			left_hand_cost_database [key] = descThumbCost (note_distance, finger_distance, n1, n2, f1, f2);
		else {
			float stretch = fingerStretch (f1, f2);
			float x = Mathf.Abs (note_distance - finger_distance) / stretch;
			if (x > MOVE_CUTOFF)
				left_hand_cost_database [key] = descMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
			else
				left_hand_cost_database [key] = ascDescNoCrossCost (note_distance, finger_distance, x, n1, n2, f1, f2);
		}
	}


	void computeRightHandCost (int n1, int n2, int  f1, int  f2)
	{
		string key = n1 + "," + n2 + "," + f1 + "," + f2;
		int note_distance = Mathf.Abs (n2 - n1);
		float finger_distance = fingerDistance (f1, f2);

		if ((note_distance > 0) && (f2 - f1 == 0))
			right_hand_cost_database [key] = ascMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
		else if ((n2 - n1 >= 0) && (f2 - f1 < 0) && (f2 != 1))
			right_hand_cost_database [key] = ascMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
		else if ((n2 - n1 < 0) && (f2 - f1 > 0) && (f1 != 1))
			right_hand_cost_database [key] = ascMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
		else if ((n2 - n1 >= 0) && (f2 - f1 < 0) && (f2 == 1))
			right_hand_cost_database [key] = ascThumbCost (note_distance, finger_distance, n1, n2, f1, f2);
		else if ((n2 - n1 < 0) && (f1 == 1) && (f2 != 1))
			right_hand_cost_database [key] = descThumbCost (note_distance, finger_distance, n1, n2, f1, f2);
		else {

			float stretch = fingerStretch (f1, f2);
			float x = Mathf.Abs (note_distance - finger_distance) / stretch;

			if (x > MOVE_CUTOFF)
				right_hand_cost_database [key] = descMoveFormula (note_distance, finger_distance, n1, n2, f1, f2);
			else
				right_hand_cost_database [key] = ascDescNoCrossCost (note_distance, finger_distance, x, n1, n2, f1, f2);
		}

	}



	
	// Use this for initialization
	public  void Init ()
	{
		
		MOVE_HASH = makeMoveHash (4);
			
		//Debug.Log( MOVE_HASH[3]);

		for (int finger1 = 1; finger1 < 6; finger1++)
			for (int note1 = 21; note1 < 109; note1++)
				for (int finger2 = 1; finger2 < 6; finger2++)
					for (int note2 = 21; note2 < 109; note2++) {

						computeRightHandCost (note1, note2, finger1, finger2);
						computeLeftHandCost (note1, note2, finger1, finger2);

					} 
//			for note1 in range(21, 109):    # in MIDI land, note 21 is actually the lowest
				

	//	Debug.Log (right_hand_cost_database ["48,50,1,2"]);
	//	Debug.Log (right_hand_cost_database ["48,51,1,5"]); //2.17
	
		Debug.Log ( left_hand_cost_database["21,22,1,2"] );
	//	Debug.Log ( left_hand_cost_database["21,22,5,4"] );

		//	Debug.Log ( right_hand_cost_database["21,21,1,1"] ); // ascDescNoCrossCost

		//			computeRightHandCost(21, 22, 1, 1);

		//	Debug.Log ( right_hand_cost_database["21,22,1,1"] ); // 


	}



}
