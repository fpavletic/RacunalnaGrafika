using System;
using System.IO;
using System.Linq;
using UnityEngine;
using AnalizaProjektiranjeRacunalom;

public class BSpline
{

	private readonly Func<double, Vector3>[] _positions;
	private readonly Func<double, Vector3>[] _tangents;
	private readonly Func<double, Vector3>[] _tangentDerivatives;
	private readonly LineRenderer _lineRenderer;

	public ( Vector3 position, (Vector3 forward, Vector3 right, Vector3 up) ) this[double time]
	{
		get
		{
			int segmentIndex = (int) time;
			double segmentTime = time - segmentIndex;

			var finalRotation = _tangents[segmentIndex](segmentTime);

			return (_positions[segmentIndex](segmentTime),
				CalculateRotation(_tangents[segmentIndex](segmentTime), _tangentDerivatives[segmentIndex](segmentTime)));
		}
	}

	public BSpline(string filename, bool debug) : this(
		File.ReadAllLines(filename)
			.Select(l => l.Split(' '))
			.Select(l => new Vector3(float.Parse(l[0]), float.Parse(l[1]), float.Parse(l[2]))).ToArray(), debug)
	{
	}

	public BSpline(Vector3[] vertices, bool debug = false)
	{
		_tangents = CalculateSegmentTangentFunctions(vertices);
		_positions = CalculateSegmentPositionFunctions(vertices);
		_tangentDerivatives = CalculateSegmentTangentDerivativeFunctions(vertices);

		Vector3[] positions = Enumerable.Range(0, _positions.Length)
			.SelectMany(j => Enumerable.Range(0, 100).Select(i => i / 100.0).Select(f => _positions[j](f)))
			.ToArray(); 
		var _lineRendererGo = new GameObject();
		_lineRenderer = _lineRendererGo.AddComponent<LineRenderer>();
		_lineRenderer.positionCount = positions.Length;
		_lineRenderer.startWidth = 0.2f;
		_lineRenderer.endWidth = 0.2f;
		_lineRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
		_lineRenderer.startColor = Color.black;
		_lineRenderer.endColor = Color.black;
		_lineRenderer.SetPositions(positions);
	}

	private Func<double, Vector3>[] CalculateSegmentTangentDerivativeFunctions(Vector3[] vertices, bool debug = false)
	{
		var b = new Matrix(4, 2)
		{
			[0, 0] = -1,
			[1, 0] = 3,
			[2, 0] = -3,
			[3, 0] = 1,
			[0, 1] = 1,
			[1, 1] = -2,
			[2, 1] = 1,
			[3, 1] = 0,
		};

		var segments = new Func<double, Vector3>[vertices.Length - 3];
		for (int i = 0; i < segments.Length; i++)
		{
			var r = new Matrix(3, 4);
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					r[j, k] = vertices[i + k][j];
				}
			}
			if (debug) Debug.Log($"r = {Environment.NewLine}{r}");

			var s = b * r;
			if (debug) Debug.Log($"s = {Environment.NewLine}{s}");
			
			segments[i] = (t) =>
			{
				var tMatrix = new Matrix(2, 1)
				{
					[0, 0] = t,
					[1, 0] = 1
				};
				var pMatrix = tMatrix * s;
				return new Vector3((float)pMatrix[0, 0], (float)pMatrix[1, 0], (float)pMatrix[2, 0]);
			};
		}

		return segments;
	}

	private (Vector3, Vector3 left, Vector3 up) CalculateRotation(Vector3 tangent, Vector3 tangentDerivative)
	{
		//tangent.Normalize();
		//tangentDerivative.Normalize();
		if (tangentDerivative.Equals(Vector3.zero)) Debug.Log($"Tangent derivative {tangentDerivative} is zero!");

		tangentDerivative = Vector3.right;
		Vector3 norm = Vector3.Cross(tangent, tangentDerivative);
		//norm.Normalize();
		
		Vector3 crossNorm = Vector3.Cross(tangent, norm);
		//crossNorm.Normalize();

		Matrix localSpace = new Matrix(3, 3)
		{
			[0, 0] = tangent.x,
			[1, 0] = tangent.y,
			[2, 0] = tangent.z,
			[0, 1] = norm.x,
			[1, 1] = norm.y,
			[2, 1] = norm.z,
			[0, 2] = crossNorm.x,
			[1, 2] = crossNorm.y,
			[2, 2] = crossNorm.z,
		};

		var forward = new Matrix(3, 1)
		{
			[0, 0] = 0,
			[1, 0] = 0,
			[2, 0] = 1
		};
		forward *= localSpace;

		var up = new Matrix(3, 1)
		{
			[0, 0] = 0,
			[1, 0] = 1,
			[2, 0] = 0
		};
		up *= localSpace; 
		
		var right = new Matrix(3, 1)
		{
			[0, 0] = 1,
			[1, 0] = 0,
			[2, 0] = 0
		};
		right *= localSpace;

		return (
			new Vector3((float) forward[0, 0], (float) forward[1, 0], (float) forward[2, 0]),
			new Vector3((float) right[0, 0], (float) right[1, 0], (float) right[2, 0]),
			new Vector3((float) up[0, 0], (float) up[1, 0], (float) up[2, 0])
		);

	}

	private Func<double, Vector3>[] CalculateSegmentPositionFunctions(Vector3[] vertices, bool debug=false)
	{
		var b = new Matrix(4, 4)
		{
			[0, 0] = -1,
			[1, 0] = 3,
			[2, 0] = -3,
			[3, 0] = 1,
			[0, 1] = 3,
			[1, 1] = -6,
			[2, 1] = 3,
			[3, 1] = 0,
			[0, 2] = -3,
			[1, 2] = 0,
			[2, 2] = 3,
			[3, 2] = 0,
			[0, 3] = 1,
			[1, 3] = 4,
			[2, 3] = 1,
			[3, 3] = 0,
		} * (1.0/6);
		if ( debug ) Debug.Log($"b = {Environment.NewLine}{b}");
		
		var segments = new Func<double, Vector3>[vertices.Count() - 3];
		for (int i = 0; i < segments.Length; i++)
		{
			var r = new Matrix(3, 4);
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					r[j, k] = vertices[i + k][j];
				}
			}
			if(debug) Debug.Log($"r = {Environment.NewLine}{r}");

			var s = b * r;
			if (debug) Debug.Log($"s = {Environment.NewLine}{s}");

			segments[i] = (t) =>
			{
				var tMatrix = new Matrix(4, 1)
				{
					[0, 0] = t * t * t,
					[1, 0] = t * t,
					[2, 0] = t,
					[3, 0] = 1
				};
				var pMatrix = tMatrix * s;
				return new Vector3((float) pMatrix[0, 0], (float) pMatrix[1, 0], (float) pMatrix[2, 0]);
			};
		}

		if (debug)
		{
			foreach (var segment in segments)
			{
				for (var t = 0.0; t < 1; t += 0.33)
				{
					var pVector = segment(t);
					var point = GameObject.CreatePrimitive(PrimitiveType.Cube);
					point.transform.localScale = 0.33f * Vector3.one;
					Debug.Log(pVector);
					point.transform.localPosition = pVector;
				}
			}
		}

		return segments;
	}

	private Func<double, Vector3>[] CalculateSegmentTangentFunctions(Vector3[] vertices, bool debug=false)
	{
		var b = new Matrix(4, 3)
		{
			[0, 0] = -1,
			[1, 0] = 3,
			[2, 0] = -3,
			[3, 0] = 1,
			[0, 1] = 2,
			[1, 1] = -4,
			[2, 1] = 2,
			[3, 1] = 0,
			[0, 2] = -1,
			[1, 2] = 0,
			[2, 2] = 1,
			[3, 2] = 0,
		} * 0.5;

		var segments = new Func<double, Vector3>[vertices.Count() - 3];
		for (int i = 0; i < segments.Length; i++)
		{
			var r = new Matrix(3, 4);
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					r[j, k] = vertices[i + k][j];
				}
			}

			var s = b * r;


			segments[i] = (t) =>
			{
				var tMatrix = new Matrix(3, 1)
				{
					[0, 0] = t * t,
					[1, 0] = t,
					[2, 0] = 1
				};
				var pMatrix = tMatrix * s;
				return new Vector3((float)pMatrix[0, 0], (float)pMatrix[1, 0], (float)pMatrix[2, 0]);
			};
		}

		if (debug)
		{
			foreach (var segment in segments)
			{
				var pVector = segment(0);
				var point = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Debug.Log(pVector);
				point.transform.position = pVector;
			}
		}

		return segments;
	}
	
}
