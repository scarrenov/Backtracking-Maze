﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 startingPosition;
    [SerializeField] private GameObject tile;

    private Stack<Vector2> _previousPos = new Stack<Vector2>();
    private readonly List<Vector2> _spawnedTilesPos = new List<Vector2>();
    private const string TilesParentNameRef = "Tiles";
    private float _xMin, _xMax, _yMin, _yMax;
    private bool _mustReturn;

    private void Start()
    {
        SetCameraBounds();
        
        if (IsOutOfBounds(startingPosition))
        {
            Debug.LogWarning("The spawner starting position exceeds the bounds of the game camera");
            return;
        }

        var roundedStartingXPos = Mathf.RoundToInt(startingPosition.x);
        var roundedStartingYPos = Mathf.RoundToInt(startingPosition.y);
        
        transform.position = new Vector2(roundedStartingXPos, roundedStartingYPos);
        SpawnTileAtCurrentPos();
    }

    public void MoveSpawner()
    {
        var newPos = GetNewPos(GetPossibleAdjacentPositions());
        _previousPos.Push(transform.position);

        if (newPos == Vector2.positiveInfinity)
            return;

        if (IsOutOfBounds(newPos))
        {
            _previousPos.Pop();
            MoveSpawner();
            return;
        }

        transform.position = newPos;
        SpawnTileAtCurrentPos();
    }

    private Vector2 GetNewPos(IReadOnlyList<Vector2> potentialPositions)
    {
        var index = Random.Range(0, potentialPositions.Count);
        Vector2 currentPos = transform.position;

        Vector2 potentialPos;
        if (potentialPositions.Count > 0)
        {
            potentialPos = potentialPositions[index];
        }
        else
        {
            Debug.Log("No available adjacent positions were found");
            return Vector2.positiveInfinity;
        }
        
        
        var newPos = currentPos + potentialPos;

        return _previousPos.Count > 0 && _previousPos.Peek() == newPos ? GetNewPos(potentialPositions) : newPos;
    }

    private void SpawnTileAtCurrentPos()
    {
        var currentPos = transform.position;
        var spawnedTile = Instantiate(tile, currentPos, Quaternion.identity);
        spawnedTile.transform.parent = GetTilesParent().transform;
        _spawnedTilesPos.Add(spawnedTile.transform.position);
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

    private static GameObject GetTilesParent()
    {
        var tilesParent = GameObject.Find(TilesParentNameRef);
        if(!tilesParent) tilesParent = new GameObject(TilesParentNameRef);
        return tilesParent;
    }

    private List<Vector2> GetPossibleAdjacentPositions()
    {
        var adjacentPositions = new List<Vector2> {Vector2.up, Vector2.down, Vector2.right, Vector2.left};
        Vector2 currentPos = transform.position;

        foreach (var spawnedToCurrent in _spawnedTilesPos.Select(spawnedTile => spawnedTile - currentPos))
        {
            for (var i = 0; i < adjacentPositions.Count; i++)
            {
                if (spawnedToCurrent == adjacentPositions[i])
                {
                    adjacentPositions.RemoveAt(i);
                }
            }
        }

        return adjacentPositions.Count > 0 ? adjacentPositions : null;
    }

    private bool IsOutOfBounds(Vector2 pos)
    {
        return pos.x < _xMin || pos.x > _xMax ||
               pos.y < _yMin || pos.y > _yMax;
    }
}