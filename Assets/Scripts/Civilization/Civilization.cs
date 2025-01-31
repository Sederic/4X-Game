using System;
using System.Collections.Generic;

public class Civilization {
    private string name;
    public List<Unit> Units { get; private set; } = new List<Unit>();

    public static List<Civilization> AllCivs { get; private set; } = new List<Civilization>();

    public Civilization(string name) {
        name = name;
    }

    public void AddUnit(Unit unit) { Units.Add(unit); }

    public static void SetAllCivs(List<Civilization> civs) { AllCivs = civs; }
}