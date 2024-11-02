using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class Technology 
{
    // private Civilization _civilization; // Owner
    [JsonProperty]
    private TechnologyTree _tree;
    [JsonProperty]
    private int _technologyCost;
    [JsonProperty]
    private int _technologyProgress;
    [JsonProperty]
    private List<Technology> _dependencies; // All technologies that must be researched before this tech can be researched
    [JsonProperty]
    private List<Technology> _successors; // All technologies that can be researched after this tech is researched
    [JsonProperty]
    private bool _researched = false;
    
    public void AddToProgress(int science)
    {
        _technologyProgress += science;

        if (_technologyProgress >= _technologyCost)
        {
            // Complete research
            Complete();
        }
        
        void Complete()
        {
            // Complete research
            _researched = true;
            _tree.ResearchTechnology(this);
        }
    }
    
    public List<Technology> GetSuccessors()
    {
        return _successors;
    }

    // Return true if this has been researched
    public bool IsResearched()
    {
        return _researched;
    }

    // Return true is this can be researched
    public bool IsResearchable()
    {
        // If any dependencies are not researched, return false
        foreach (Technology tech in _dependencies)
        {
            if (!tech.IsResearched())
            {
                return false;
            }
        }

        return true;
    }

}
