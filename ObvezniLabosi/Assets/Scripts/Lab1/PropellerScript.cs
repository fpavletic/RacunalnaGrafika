using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerScript : MonoBehaviour
{
    [SerializeField]
    private float _propellerRotationSpeed = 1800;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward, _propellerRotationSpeed * 180f / 3.14f / 2f);
    }
}
