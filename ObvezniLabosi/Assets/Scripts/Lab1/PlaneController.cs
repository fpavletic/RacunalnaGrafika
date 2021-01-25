using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
	[SerializeField] private Vector3[] _vertices = null;

	private BSpline _bSpline;
	private float _time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _bSpline = new BSpline(_vertices, true);
    }

    // Update is called once per frame
    void Update()
    {
	    if (_time > _vertices.Length - 3) return;

	    var (position, rotation) = _bSpline[_time];
	    transform.localPosition = position;
		transform.forward = rotation.forward;
		transform.up = rotation.up;
		transform.right = rotation.right;
		_time += Time.deltaTime;

    }
}
