using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class LabyrinthNode : MonoBehaviour
{
    public char symbol = (char)0;
    
    public Vector3Int[] connections;
    private LabyrinthNode[] _connectedNodes;
    
    private bool _initialized = false;
    
    public void Init()
    {
        _connectedNodes = new LabyrinthNode[connections.Length];
        
        for (int i = 0; i < connections.Length; i++)
        {
            _connectedNodes[i] = null;
        }
        
        _initialized = true;
    }
    
    public bool ConnectionIsFree(Vector3Int connection)
    {
        if(!_initialized) return true;
        
        for (int i = 0; i < connections.Length; i++)
        {
            if(connections[i] == connection)
            {
                return _connectedNodes[i] == null;
            }
        }
        
        return false;
    }

    public bool CanConnect(LabyrinthNode other)
    {
        foreach (var connection in connections)
        {
            foreach (var connOther in other.connections)
            {
                if(
                    other.ConnectionIsFree(connOther)
                    && ConnectionIsFree(connection)
                    && connection + connOther == Vector3Int.zero)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
