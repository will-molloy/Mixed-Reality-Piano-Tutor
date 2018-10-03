//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Spawns and destroys the bunnies
/// </summary>
public class EnemyBehavior : MonoBehaviour
{
    private const float lifeMax = 100;
    private SphereCollider capsuleCollider;
    private bool isDying;
    public float life = lifeMax;

    private void Start()
    {
        life = lifeMax;
        capsuleCollider = GetComponent<SphereCollider>();
        capsuleCollider.enabled = true;
        isDying = false;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    /// <summary>
    ///     Set the dammage to an object
    /// </summary>
    /// <param name="value"></param>
    public void Dammage(float value)
    {
        if (!isDying)
        {
            life -= value;
            if (life < 0) Dead();
        }
    }

    /// <summary>
    ///     Disables the gameobject
    /// </summary>
    public void StartSinking()
    {
        capsuleCollider.enabled = false;
        Destroy(gameObject, 2.0f);
    }

    /// <summary>
    ///     Play the animation of dead
    /// </summary>
    private void Dead()
    {
        isDying = true;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Animator>().SetTrigger("Dead");
    }

    private void OnDestroy()
    {
        isDying = false;
    }
}