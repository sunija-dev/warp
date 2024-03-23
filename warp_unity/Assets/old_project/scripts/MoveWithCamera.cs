using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithCamera : MonoBehaviour
{
    public GameObject goFollow;
    private Vector3 v3Offset = Vector3.zero;

    void Start()
    {
        v3Offset = (transform.position - goFollow.transform.position);
    }

    void Update()
    {
        transform.position = goFollow.transform.position;
    }
}
