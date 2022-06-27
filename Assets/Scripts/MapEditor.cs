using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        
        MapGenerator map = target as MapGenerator;

        map.GenerateMap();
    }

}
