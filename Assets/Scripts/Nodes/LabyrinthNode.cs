using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class LabyrinthNode : MonoBehaviour
{
    public char symbol = (char)0;
    
    public Vector3Int[] connections;
    
    public void Init()
    {

    }
}
