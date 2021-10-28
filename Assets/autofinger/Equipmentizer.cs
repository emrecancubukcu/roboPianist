using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Equipmentizer : MonoBehaviour { public GameObject target;


	// Use this for initialization 

	void Start () { 
		SkinnedMeshRenderer targetRenderer = target.GetComponent<SkinnedMeshRenderer>(); 
		Dictionary<string,Transform> boneMap = new Dictionary<string,Transform>(); 
		foreach( Transform bone in targetRenderer.bones ) 
			boneMap[bone.gameObject.name] = bone;
		
	SkinnedMeshRenderer myRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
	Transform[] newBones = new Transform[myRenderer.bones.Length];
	for( int i = 0; i < myRenderer.bones.Length; ++i )
	{
		GameObject bone = myRenderer.bones[i].gameObject;
		if( !boneMap.TryGetValue(bone.name, out newBones[i]) )
		{
			Debug.Log("Unable to map bone \"" + bone.name + "\" to target skeleton.");
			break;
		}
	}
	myRenderer.bones = newBones;

}

} 

