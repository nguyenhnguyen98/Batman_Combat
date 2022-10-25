using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private EnemyController[] _enemies;
    public EnemyStruct[] allEnemies;
    private List<int> _enemyIndexes;

    [Header("Main AI Loop - Settings")]
    private Coroutine _AI_Loop_Coroutine;

    public int aliveEnemiesCount;

    // Start is called before the first frame update
    void Start()
    {
        _enemies = GetComponentsInChildren<EnemyController>();

        allEnemies = new EnemyStruct[_enemies.Length];

        for (int i = 0; i < allEnemies.Length; i++)
        {
            allEnemies[i].enemyController = _enemies[i];
            allEnemies[i].enemyAvailability = true;
        }

        StartAI();
    }

    public void StartAI()
    {
        _AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
    }

    IEnumerator AI_Loop(EnemyController enemy)
    {
        if (AliveEnemyCount() == 0)
        {
            StopCoroutine(AI_Loop(null));
            yield break;
        }

        yield return new WaitForSeconds(Random.Range(.5f, 1.5f));

        EnemyController attackingEnemy = RandomEnemyExcludingOne(enemy);

        if (attackingEnemy == null)
        {
            attackingEnemy = RandomEnemy();
            yield break;
        }
            

        yield return new WaitUntil(() => attackingEnemy.IsRetreating() == false);
        yield return new WaitUntil(() => attackingEnemy.IsLockedTarget() == false);
        yield return new WaitUntil(() => attackingEnemy.IsStunned() == false);

        attackingEnemy.SetAttack();

        yield return new WaitUntil(() => attackingEnemy.IsPreparingAttack() == false);

        attackingEnemy.SetRetreat();

        yield return new WaitForSeconds(Random.Range(0, .5f));

        if (AliveEnemyCount() > 0)
            _AI_Loop_Coroutine = StartCoroutine(AI_Loop(attackingEnemy));
    }

    public EnemyController RandomEnemy()
    {
        _enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability)
                _enemyIndexes.Add(i);
        }

        if (_enemyIndexes.Count == 0)
            return null;

        EnemyController randomEnemy;
        int randomIndex = Random.Range(0, _enemyIndexes.Count);
        randomEnemy = allEnemies[_enemyIndexes[randomIndex]].enemyController;

        return randomEnemy;
    }

    public EnemyController RandomEnemyExcludingOne(EnemyController exclude)
    {
        _enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyController != exclude)
                _enemyIndexes.Add(i);
        }

        if (_enemyIndexes.Count == 0)
            return null;

        EnemyController randomEnemy;
        int randomIndex = Random.Range(0, _enemyIndexes.Count);
        randomEnemy = allEnemies[_enemyIndexes[randomIndex]].enemyController;

        return randomEnemy;
    }

    public int AvailableEnemyCount()
    {
        int count = 0;

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability)
                count++;
        }

        return count;
    }

    public bool AnEnemyIsPreparingAttack()
    {
        foreach (EnemyStruct enemyStruct in allEnemies)
        {
            if (enemyStruct.enemyController.IsPreparingAttack())
                return true;
        }

        return false;
    }

    public int AliveEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyController.isActiveAndEnabled)
                count++;
        }
        aliveEnemiesCount = count;
        return count;
    }

    public void SetEnemyAvailiability(EnemyController enemy, bool state)
    {
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyController == enemy)
                allEnemies[i].enemyAvailability = state;
        }

        if (FindObjectOfType<EnemyDetection>().CurrentTarget() == enemy)
            FindObjectOfType<EnemyDetection>().SetCurrentTarget(null);
    }
}

#region Parameters
[System.Serializable]
public struct EnemyStruct
{
    public EnemyController enemyController;
    public bool enemyAvailability;
}
#endregion