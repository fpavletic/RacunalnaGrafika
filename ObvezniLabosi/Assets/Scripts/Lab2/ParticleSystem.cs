using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

public class ParticleSystem : MonoBehaviour
{
    private static readonly Random _rng = new Random(Environment.TickCount);

    [SerializeField] private Texture2D _particleTexture = null;
	[SerializeField] private Material _particleMaterial = null;
	[SerializeField] private int _particleCreationFrequency = 60;
    [SerializeField] private int _particleLifetime = 10000; //miliseconds
    [SerializeField] private AnimationCurve _particleSizeOverLifetime = AnimationCurve.Constant(0, 1, 1);
    [SerializeField] private Gradient _particleColorOverLifetime = new Gradient();

    [SerializeField] private float _initialVelocityMagnitude = 0f;
    [SerializeField] private Vector3 _initialVelocityDirection = Vector3.zero;
    [SerializeField] private bool _randomVelocityDirection = false;

    [SerializeField] private float _initialForceMagnitude = 0f;
    [SerializeField] private Vector3 _initialForceDirection = Vector3.zero;
    [SerializeField] private bool _randomForceDirection = false;

    private IList<(Transform transform, float ttl, Vector3 velocity)> _particles = 
	    new List<(Transform transform, float ttl, Vector3 velocity)>();

    // Start is called before the first frame update
    void Start()
    {
        _particleMaterial.SetTexture("_MainTex", _particleTexture);
     
        var particleCount = _particleCreationFrequency * _particleLifetime / 1000;
	    _particles = Enumerable.Range(1, particleCount)
		    .Select(i => (ParticlePlane(), i / (float)_particleCreationFrequency,
			    Vector3.zero))
		    .ToArray();
	    foreach (var particle in _particles)
	    {
		    particle.transform.gameObject.SetActive(false);
	    }
    }

    // Update is called once per frame
    void Update()
    {
	    for (int i = 0; i < _particles.Count; i++)
	    {
		    var particle = _particles[i];
		    particle.ttl -= Time.deltaTime;
		    
		    if (particle.ttl <= 0)
		    {
                particle.transform.gameObject.SetActive(true);
                particle.transform.localPosition = Vector3.zero;
                particle.transform.localScale = Vector3.one * _particleSizeOverLifetime.Evaluate(0f);
                particle.transform.gameObject.GetComponent<Renderer>().sharedMaterial.color = _particleColorOverLifetime.Evaluate(0f);
                particle.velocity = _initialVelocityMagnitude * (_randomVelocityDirection ? RandomVectorNorm() : _initialVelocityDirection);
                particle.ttl = _particleLifetime / 1000f;
		    }
		    else
		    {
			    particle.velocity -= Vector3.up * 2f * Time.deltaTime;
			    particle.transform.localPosition += particle.velocity * Time.deltaTime;
			    particle.transform.localScale = Vector3.one * _particleSizeOverLifetime.Evaluate(1f - particle.ttl / (_particleLifetime / 1000));
			    particle.transform.gameObject.GetComponent<Renderer>().sharedMaterial.color = _particleColorOverLifetime.Evaluate(1f - particle.ttl / (_particleLifetime / 1000));
                particle.transform.LookAt(Camera.main.transform.position);
                particle.transform.Rotate(Vector3.right, 90f);
            }
            
		    _particles[i] = particle;
	    }
    }

    private Transform ParticlePlane()
    {
	    var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(transform, false);
	    var planeRenderer = plane.GetComponent<Renderer>();
	    planeRenderer.material = new Material(_particleMaterial);
	    return plane.transform;
    }

    private static Vector3 RandomVectorNorm()
    {
        return new Vector3(_rng.Next(200) - 100, _rng.Next(200) - 100, _rng.Next(200) - 100).normalized;
    }
}