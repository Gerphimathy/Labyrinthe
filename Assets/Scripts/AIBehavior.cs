using System;
using UnityEngine;
using UnityEngine.AI;


public class AIBehavior : MonoBehaviour
{
    private GameHandler _gameHandler;

    private Vector3 _target;
    
    private NavMeshAgent _agent;
    
    public void Generate(GameHandler ga)
    {
        _gameHandler = ga;
        _target = this.gameObject.transform.position;
        _agent = this.gameObject.AddComponent<NavMeshAgent>();
    }

    void Update()
    {
        //Check if the distance to target is less than the threshold
        if (Vector3.Distance(this.gameObject.transform.position, _target) < _gameHandler.creaturesRoamingThreshold)
        {
            //Get random point on navmesh
            _target = _gameHandler.RandomNavmeshLocation(_gameHandler.creaturesRoamingRange);
        }
        
        //Move towards target using Navmesh
        _agent.SetDestination(_target);
    }
}