using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    private EnemyController[] enemies;
    public EnemyStruct[] _enemiesStruct;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

#region Parameters
[System.Serializable]
public struct EnemyStruct
{
    public EnemyController enemyController;
    public bool enemyAvailability;
}