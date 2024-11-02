using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class CityProject
{
    [JsonProperty]
    private int _projectName;
    [JsonProperty]
    private int _cost;
    [JsonProperty]
    private int _currentProductionProgress;

    public CityProject(int name, int cost)
    {
        _projectName = name;
        _cost = cost;
        _currentProductionProgress = 0;
    }
    
    public void AddToProgress(int production)
    {
        _currentProductionProgress += production;
        
        if (_currentProductionProgress >= _cost)
        {
            Complete();
        }
    }

    public void Complete()
    {
        
    }

    public int GetName()
    {
        return _projectName;
    }

}
