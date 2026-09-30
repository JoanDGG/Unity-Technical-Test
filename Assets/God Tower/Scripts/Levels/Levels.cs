using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "God Tower/Levels")]
public class Levels : ScriptableObject
{
    public List<LevelConfig> collection = new();
}
