using System;
using UnityEngine;

[Serializable]
public class ParticleExamples
{
    [TextArea] public string description;

    public bool isWeaponEffect;
    public Vector3 particlePosition, particleRotation;
    public GameObject particleSystemGO;

    public string title;
}