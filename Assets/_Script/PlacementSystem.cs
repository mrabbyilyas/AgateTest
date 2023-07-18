using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject mouseIndicator, cellIndicator;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private ObjectsDatabaseSO database;
    private int selectedObjectIndex = -1;

    [SerializeField]
    private GameObject gridVisualization;

    private GridData floorData, furnitureData;

    private Renderer previewRenderer;

    public GameObject BishopPanel, DragonPanel, RockPanel, KnightPanel, StartButton, retryButton;

    public Text scoreText;

    int score = 0;

    private List<GameObject> placedGameObjects = new();

    private void Awake() {
        
    }

    public void retry(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Start()
    {
        StopPlacement();
        floorData = new();
        furnitureData = new();
        previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();

        scoreText.text = "SCORE : "+ score.ToString();
    }

    public void AddPoint() {
        scoreText.text = "SCORE : "+ score.ToString();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedObjectIndex = Random.Range(0, 4);

        StartButton.SetActive(false);
        retryButton.SetActive(true);

        switch(selectedObjectIndex)
        {
            case 0:
                BishopPanel.SetActive(true);
                DragonPanel.SetActive(false);
                RockPanel.SetActive(false);
                KnightPanel.SetActive(false);
                score += 2;
                break;
            case 1:
                BishopPanel.SetActive(false);
                DragonPanel.SetActive(true);
                RockPanel.SetActive(false);
                KnightPanel.SetActive(false);
                score += 1;
                break;
            case 2:
                BishopPanel.SetActive(false);
                DragonPanel.SetActive(false);
                RockPanel.SetActive(false);
                KnightPanel.SetActive(true);
                score += 1;
                break;
            case 3:
                BishopPanel.SetActive(false);
                DragonPanel.SetActive(false);
                RockPanel.SetActive(true);
                KnightPanel.SetActive(false);
                score += 2;
                break;
        }

        if(selectedObjectIndex < 0)
        {
            Debug.LogError($"No ID found {ID}");
            return;
        }
        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if(inputManager.IsPointerOverUI())
        {
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
        {
            return;
        }

        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);
        placedGameObjects.Add(newObject);
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 5 ?
            floorData :
            furnitureData;
        selectedData.AddObjectAt(gridPosition,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID,
            placedGameObjects.Count - 1);
        AddPoint();
        StartPlacement(Random.Range(0, 4));
        
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 5 ? 
            floorData : 
            furnitureData;

        return selectedData.CanPlaceObejctAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }

    private void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
            return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        previewRenderer.material.color = placementValidity ? Color.green : Color.red;

        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }
}