using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 startingPosition;
    [SerializeField] private GameObject tile;

    private const string TilesParentName = "Tiles";
    private Stack<Vector2> _previousPos = new Stack<Vector2>();
    private float _xMin, _xMax, _yMin, _yMax;
    private bool _mustReturn;

    private void Start()
    {
        SetCameraBounds();

        _mustReturn = startingPosition.x > _xMax || startingPosition.x < _xMin || startingPosition.y > _yMax || startingPosition.y < _yMin;
        if (_mustReturn)
        {
            Debug.LogWarning("The spawner starting position exceeds the bounds of the game camera");
            return;
        }

        var roundedStartingXPos = Mathf.RoundToInt(startingPosition.x);
        var roundedStartingYPos = Mathf.RoundToInt(startingPosition.y);
        
        transform.position = new Vector2(roundedStartingXPos, roundedStartingYPos);
        SpawnTileAtCurrentPos();
    }

    private static GameObject GetTilesParent()
    {
        var tilesParent = GameObject.Find(TilesParentName);
        if(!tilesParent) tilesParent = new GameObject(TilesParentName);
        return tilesParent;
    }

    public void MoveSpawner()
    {
        var adjacentPositions = new Vector2[4];
        adjacentPositions[0] = Vector2.up;
        adjacentPositions[1] = Vector2.down;
        adjacentPositions[2] = Vector2.right;
        adjacentPositions[3] = Vector2.left;

        var newPos = GetNewPos(adjacentPositions);

        var isOutOfBounds = newPos.x < _xMin || newPos.x > _xMax ||
                            newPos.y < _yMin || newPos.y > _xMax;

        if (isOutOfBounds)
        {
            MoveSpawner();
            return;
        }

        transform.position = newPos;
        SpawnTileAtCurrentPos();
        _previousPos.Push(newPos);
    }

    private Vector2 GetNewPos(IReadOnlyList<Vector2> potentialPositions)
    {
        var index = Random.Range(0, potentialPositions.Count);
        var currentPos = transform.position;
        var potentialPos = potentialPositions[index];

        var newXPos = currentPos.x + potentialPos.x;
        var newYPos = currentPos.y + potentialPos.y;

        var newPos = new Vector2(newXPos, newYPos);
        
        return newPos;
    }

    private void SpawnTileAtCurrentPos()
    {
        var spawnedTile = Instantiate(tile, transform.position, Quaternion.identity);
        spawnedTile.transform.parent = GetTilesParent().transform;
    }

    private void SetCameraBounds()
    {
        var mainCamera = Camera.main;
        if (mainCamera is null) return;

        var startingPoint = mainCamera.ViewportToWorldPoint(Vector3.zero);
        _xMin = startingPoint.x;
        _xMax = mainCamera.ViewportToWorldPoint(Vector3.right).x;
        _yMin = startingPoint.y;
        _yMax = mainCamera.ViewportToWorldPoint(Vector3.up).y;
    }
}