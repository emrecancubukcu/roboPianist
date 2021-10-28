using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBlinker : MonoBehaviour {

	public Transform[] l_eye;
	public Transform[] r_eye;
	public float minTime = 1f;
	public float maxTime = 5f;

	private Quaternion[] l_eyeDefaultRotation;
	private Quaternion[] r_eyeDefaultRotation;
	public Vector3 closingAnge = new Vector3 (0,50,0 );


	// Use this for initialization
	void Start () {

		l_eyeDefaultRotation = new Quaternion[l_eye.Length];
		r_eyeDefaultRotation = new Quaternion[r_eye.Length];

		for ( int i = 0; i < l_eye.Length;i++ ) {

			l_eyeDefaultRotation[i] = l_eye[i].localRotation;
			r_eyeDefaultRotation[i] = r_eye[i].localRotation;

		}

		StartCoroutine(RandomBlink());
		
	}

	IEnumerator RandomBlink(){
		while (true) {


			for ( int i = 0; i < l_eye.Length;i++ ) {

				l_eye[i].localRotation = l_eyeDefaultRotation[i] *Quaternion.Euler ( closingAnge ) ;
				r_eye[i].localRotation = r_eyeDefaultRotation[i] *Quaternion.Euler ( closingAnge ) ;
			}

			yield return new WaitForSeconds(0.1f);

			for ( int i = 0; i < l_eye.Length;i++ ) {

				l_eye[i].localRotation = l_eyeDefaultRotation[i] ;
				r_eye[i].localRotation = r_eyeDefaultRotation[i] ;
						}
			yield return new WaitForSeconds(Random.Range( minTime, maxTime));
		}
			
	}
}
