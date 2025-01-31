using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitPrefab : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Button unitButton;
    private Unit unit;
    private GameUI UI;

    [Header("Civilian Units")]
    public Sprite settler;
    public Sprite scout;
    [Header("Military Units")]
    public Sprite warrior;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        unitButton = GetComponentInChildren<Button>(); 
        if (spriteRenderer == null)
        {
            Debug.LogError("Sprite Renderer Component on UnitPrefab not found");
        }
    }
    
    public void SetUnitPrefab(Unit unit, GameUI UI)
    {
        this.unit = unit;
        unitButton.gameObject.SetActive(unit.civ.Name == Game.Instance.civ.Name);
        switch (unit.unitType)
        {
            // Civilian Units
            case UnitType.Settler:
                spriteRenderer.sprite = settler;
                break;
            case UnitType.Scout:
                spriteRenderer.sprite = scout;
                break;

            // Military Units
            case UnitType.Warrior:
                spriteRenderer.sprite = warrior;
                break;

            default:
                Debug.LogWarning($"Unit type {unit.unitType} does not have a corresponding sprite.");
                spriteRenderer.sprite = null; // Ensures no leftover sprites
                break;
        }
    }

    public void DestroyUnit()
    {
        gameObject.SetActive(false);
        // Destroy(gameObject);
    }

    public void OnClick()
    {
        if (!UI._zoomedIn)
        {
            UI.SelectUnit(unit);
            // UI.selectedUnitPrefab = this.gameObject;
        }
    }
}