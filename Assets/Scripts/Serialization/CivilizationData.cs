using System;
using System.Collections.Generic;

[Serializable]
public class CivilizationData {
    public int civID;
    public string civName;
    public List<int> unitIDs; // Stores Unit IDs instead of Unit references

    public CivilizationData() {}
    public CivilizationData(Civilization civ) {
        civID = civ.ID;
        civName = civ.Name;

        // Store Unit IDs instead of actual Unit objects
        unitIDs = new List<int>();
        // foreach (var unit in civ.units) {
        //     unitIDs.Add(unit.ID);
        // }
    }
}
