using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AIBehavior : MonoBehaviour
{
    private GameHandler _gameHandler;

    private Vector3 _target;
    
    private NavMeshAgent _agent;

    public enum Behavior
    {
        Stalker,
        Roamer,
        Circler,
    }
    public Behavior behavior;
    private List<Vector3> _path = new List<Vector3>();
    private int circleIndex = 0;
    
    public void Generate(GameHandler ga)
    {
        _gameHandler = ga;
        _target = this.gameObject.transform.position;
        _agent = this.gameObject.AddComponent<NavMeshAgent>();
        
        behavior = (Behavior)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Behavior)).Length);

        if (behavior == Behavior.Circler)
        {
            //Choose a random number of points to circle between:
            int nodes = UnityEngine.Random.Range(2, _gameHandler.creatureCirclerMaxPathLength);
            for (int i = 0; i < nodes; i++)
            {
                //Get random point on navmesh
                _path.Add(_gameHandler.RandomNavmeshLocation(_gameHandler.creaturesRoamingRange));
                _target = _path[0];
            }
        }
    } 
    
    void Update()
    {
        //Check if the distance to target is less than the threshold
        if (Vector3.Distance(this.gameObject.transform.position, _target) < _gameHandler.creaturesRoamingThreshold)
        {
            //Get random point on navmesh
            if(behavior == Behavior.Roamer) _target = _gameHandler.RandomNavmeshLocation(_gameHandler.creaturesRoamingRange);
            if (behavior == Behavior.Stalker)
            {
                _target = _gameHandler.GetPlayerPosition();
                _agent.isStopped = false;
                
                if(Vector3.Distance(gameObject.transform.position, _target) < _gameHandler.creatureStalkerRange)
                {
                    _agent.isStopped = true;
                }
            }
            if(behavior == Behavior.Circler)
            {
                _target = _path[circleIndex];
                circleIndex++;
                if (circleIndex >= _path.Count) circleIndex = 0;
            }
        }
        
        //Move towards target using Navmesh
        _agent.SetDestination(_target);
    }
}