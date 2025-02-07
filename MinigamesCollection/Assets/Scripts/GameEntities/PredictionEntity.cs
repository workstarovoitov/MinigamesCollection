using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PredictionEntity", menuName = "Scriptable Objects/PredictionEntity")]
public class PredictionEntity : ScriptableObject
{
    public List<string> predictions; 
}
