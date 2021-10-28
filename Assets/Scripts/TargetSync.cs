using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSync : MonoBehaviour
{
    [System.Serializable]

    public class Couple
    {
        public Transform source;
        public Transform target;

    }
    public Couple[] couples;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        for (int i = 1; i < couples.Length; i++)
        {
            couples[i].source.position = couples[i].target.position;
            couples[i].source.rotation = couples[i].target.rotation;

        }
        Vector3 diff = (couples[0].target.position - couples[0].source.position);
        if ( diff.magnitude>0)
            couples[0].source.rotation = Quaternion.LookRotation(diff, Vector3.up);


    }




}



