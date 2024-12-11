using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Unity.VisualScripting;


namespace City_Projects
{
    [System.Serializable]
    public abstract class CityProject
    {
        [JsonProperty] public string projectName;
        [JsonProperty] public int projectCost;
        [JsonProperty] public int currentProductionProgress = 0;
        [JsonProperty] public string projectType;
        [JsonProperty] public bool alreadyBuilt;
        public GameManager gameManager;
        public Settlement settlement;
        
        public virtual void AddToProgress(int production)
        {
            currentProductionProgress += production;

            if (currentProductionProgress >= projectCost)
            {
                Complete();
            }
        }

        public virtual void Complete()
        {
            // Raise completion event
           
        }
    }
}
