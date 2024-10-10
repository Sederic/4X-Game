using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityProject : MonoBehaviour
{
    private int _projectName;
    private int _cost;
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
