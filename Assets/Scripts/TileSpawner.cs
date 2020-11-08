using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 startingPosition;
    [SerializeField] private GameObject tile;

    private Stack<Vector2> _previousPos = new Stack<Vector2>();
    private List<Vector2> _spawnedTilePos = new List<Vector2>();
    private const string TilesParentName = "Tiles";
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

    public void MoveSpawner()
    {
        var newPos = GetNewPos(GetAdjacentPositions());
        _previousPos.Push(transform.position);
        
        var isOutOfBounds = newPos.x < _xMin || newPos.x > _xMax ||
                            newPos.y < _yMin || newPos.y > _xMax;
        
        if (isOutOfBounds)
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
        var currentPos = transform.position;
        var potentialPos = potentialPositions[index];

        var newXPos = currentPos.x + potentialPos.x;
        var newYPos = currentPos.y + potentialPos.y;
        var newPos = new Vector2(newXPos, newYPos);

        return _previousPos.Count > 0 && _previousPos.Peek() == newPos ? GetNewPos(potentialPositions) : newPos;
    }

    private void SpawnTileAtCurrentPos()
    {
        var currentPos = transform.position;
        var spawnedTile = Instantiate(tile, currentPos, Quaternion.identity);
        spawnedTile.transform.parent = GetTilesParent().transform;
        _spawnedTilePos.Add(spawnedTile.transform.position);
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
        var tilesParent = GameObject.Find(TilesParentName);
        if(!tilesParent) tilesParent = new GameObject(TilesParentName);
        return tilesParent;
    }

    private List<Vector2> GetAdjacentPositions()
    {
        var adjacentPositions = new List<Vector2> {Vector2.up, Vector2.down, Vector2.right, Vector2.left};

        for (var i = 0; i < adjacentPositions.Count; i++)
        {
            var position = adjacentPositions[i];
            Vector2 currentPos = transform.position;
            var tileToAdjacent = currentPos + position;

            foreach (var unused in _spawnedTilePos.Where(tilePos => tilePos == tileToAdjacent))
            {
                adjacentPositions.RemoveAt(i);
            }
        }

        return adjacentPositions;
    }
}