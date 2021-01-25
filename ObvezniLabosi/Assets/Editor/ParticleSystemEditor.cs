using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParticleSystem))]
[CanEditMultipleObjects]
public class ParticleSystemEditor : Editor
{
	private SerializedProperty _particleTexture;
	private SerializedProperty _particleMaterial;
	private SerializedProperty _particleCreationFrequency;
	private SerializedProperty _particleLifetime;
	private SerializedProperty _particleSizeOverLifetime;
	private SerializedProperty _particleColorOverLifetime;

	private SerializedProperty _initialVelocityMagnitude;
	private SerializedProperty _initialVelocityDirection;
	private SerializedProperty _randomVelocityDirection;
	
	private SerializedProperty _initialForceMagnitude;
	private SerializedProperty _initialForceDirection;
	private SerializedProperty _randomForceDirection;


	public void OnEnable()
	{
		_particleTexture = serializedObject.FindProperty("_particleTexture");
		_particleMaterial = serializedObject.FindProperty("_particleMaterial");
		_particleCreationFrequency = serializedObject.FindProperty("_particleCreationFrequency");
		_particleLifetime = serializedObject.FindProperty("_particleLifetime");
		_particleSizeOverLifetime = serializedObject.FindProperty("_particleSizeOverLifetime");
		_particleColorOverLifetime = serializedObject.FindProperty("_particleColorOverLifetime");
		_initialVelocityMagnitude = serializedObject.FindProperty("_initialVelocityMagnitude");
		_initialVelocityDirection = serializedObject.FindProperty("_initialVelocityDirection");
		_randomVelocityDirection = serializedObject.FindProperty("_randomVelocityDirection");
		_initialForceMagnitude = serializedObject.FindProperty("_initialForceMagnitude");
		_initialForceDirection = serializedObject.FindProperty("_initialForceDirection");
		_randomForceDirection = serializedObject.FindProperty("_randomForceDirection");
	}

    public override void OnInspectorGUI()
    {
		serializedObject.Update();
		EditorGUILayout.PropertyField(_particleTexture);
		EditorGUILayout.PropertyField(_particleMaterial);
		EditorGUILayout.PropertyField(_particleCreationFrequency);
		EditorGUILayout.PropertyField(_particleLifetime);
		EditorGUILayout.PropertyField(_particleSizeOverLifetime);
		EditorGUILayout.PropertyField(_particleColorOverLifetime);
		EditorGUILayout.Separator();

		EditorGUILayout.PropertyField(_initialVelocityMagnitude);
		EditorGUILayout.PropertyField(_randomVelocityDirection);
		if (!_randomVelocityDirection.boolValue) EditorGUILayout.PropertyField(_initialVelocityDirection);
		EditorGUILayout.Separator();
		
		EditorGUILayout.PropertyField(_initialForceMagnitude);
		EditorGUILayout.PropertyField(_randomForceDirection);
		if (!_randomForceDirection.boolValue) EditorGUILayout.PropertyField(_initialForceDirection);
		EditorGUILayout.Separator();

		serializedObject.ApplyModifiedProperties();
    }

}
