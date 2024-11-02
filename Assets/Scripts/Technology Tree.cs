using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class TechnologyTree 
{
    [JsonProperty]
    public List<Technology> _researchable;
    [JsonProperty]
    public List<Technology> _researched;
    [JsonProperty]
    public Technology _currentlyResearching;

    // Add given science to the currently selected technology
    public void AddToProgress(int science)
    {
        if (_currentlyResearching != null)
        {
            _currentlyResearching.AddToProgress(science);
        }
    }

    // Finish researching the current technology
    public void ResearchTechnology(Technology tech)
    {
        _researched.Add(tech);
        SetCurrentTechnology(null);
        AddResearchableTechnologies(tech.GetSuccessors());
    }

    // Set the current technology
    public void SetCurrentTechnology(Technology tech)
    {
        _currentlyResearching = tech;
    }

    // Add the given technologies to the list of researchable technologies
    public void AddResearchableTechnologies(List<Technology> techList)
    {
        foreach (Technology tech in techList)
        {
            if (tech.IsResearchable())
            {
                _researchable.Add(tech);
            }
        }
    }
}
