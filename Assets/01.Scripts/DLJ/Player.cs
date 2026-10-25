using System;
using Agents;
using UnityEngine;

public class Player : MonoBehaviour
{
    //[SerializeField] private KTH_Enemy enemy;

    public HealthModule HealthModule { get; private set; }

    private void Awake()
    {
        HealthModule = GetComponent<HealthModule>();
    }
}
