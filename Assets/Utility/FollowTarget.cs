using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform Target;
    public float Speed = 2;
    public bool FollowRotation;

	void Update ()
    {
        transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * Speed);

        if (FollowRotation)
        {
            transform.rotation = Target.rotation;
        }
	}
}
