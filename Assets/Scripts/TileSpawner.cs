using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 startingPosition;
    [SerializeField] private GameObject tile;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float timeBetweenSpawns = .6f;
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private Color[] pathColors;

    private LineRenderer _currentWallRenderer;
    private Color _currentPathColor;
    private readonly Stack<Vector2> _previousPos = new Stack<Vector2>();
    private readonly List<Vector2> _spawnedTilesPos = new List<Vector2>();
    private float _xMin, _xMax, _yMin, _yMax;
    
    // String references
    private const string WallRendererParentNameRef = "Wall Renderers";
    private const string TilesParentNameRef = "Tiles";

    private IEnumerator Start()
    {
        FindObjectOfType<Toggle>().isOn = autoSpawn;
        _currentWallRenderer = GetNewWallRenderer();

        var colorIndex = Random.Range(0, pathColors.Length);
        _currentPathColor = pathColors[colorIndex];
        SetCameraBounds();
        
        if (IsOutOfBounds(startingPosition))
        {
            Debug.LogWarning("The spawner starting position exceeds the bounds of the game camera");
            yield return null;
        }

        var roundedStartingXPos = Mathf.RoundToInt(startingPosition.x);
        var roundedStartingYPos = Mathf.RoundToInt(startingPosition.y);
        
        transform.position = new Vector2(roundedStartingXPos, roundedStartingYPos);
        SpawnTileAtCurrentPos();

        while (true)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
            if (autoSpawn)
            {
                MoveSpawner();
            }
        }
    }

    private LineRenderer GetNewWallRenderer()
    {
        Vector2 currentPos = transform.position;
        var newLineRenderer = Instantiate(lineRenderer, currentPos, Quaternion.identity);
        newLineRenderer.transform.parent = GetObjectParent(WallRendererParentNameRef).transform;
        
        newLineRenderer.SetPosition(0, new Vector3(currentPos.x, currentPos.y, -.1f));

        return newLineRenderer;
    }

    public void MoveSpawner()
    {
        var newPos = GetNewPos(GetPossibleAdjacentPositions());
        _previousPos.Push(newPos);
        var positionCount = _currentWallRenderer.positionCount;
        positionCount++;
        _currentWallRenderer.positionCount = positionCount;
        _currentWallRenderer.SetPosition(positionCount - 1, new Vector3(newPos.x, newPos.y, -.1f));

        if (newPos == new Vector2())
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
        if (potentialPositions == null)
        {
            transform.position = _previousPos.Pop();
            var colorIndex = Random.Range(0, pathColors.Length);
            _currentPathColor = pathColors[colorIndex];

            foreach (var lineRend in FindObjectsOfType<LineRenderer>())
            {
                if (lineRend.positionCount < 2)
                {
                    Destroy(lineRend.gameObject);
                }
            }

            _currentWallRenderer = GetNewWallRenderer();
            
            return GetNewPos(GetPossibleAdjacentPositions());
        }
        
        var index = Random.Range(0, potentialPositions.Count);
        Vector2 currentPos = transform.position;
        var potentialPos = potentialPositions[index];

        var newPos = currentPos + potentialPos;

        return _previousPos.Count > 0 && _previousPos.Peek() == newPos ? GetNewPos(potentialPositions) : newPos;
    }

    private void SpawnTileAtCurrentPos()
    {
        var currentPos = transform.position;
        var spawnedTile = Instantiate(tile, currentPos, Quaternion.identity);
        spawnedTile.GetComponent<SpriteRenderer>().color = _currentPathColor;
        spawnedTile.transform.parent = GetObjectParent(TilesParentNameRef).transform;
        
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

    private static GameObject GetObjectParent(string nameRef)
    {
        var tilesParent = GameObject.Find(nameRef);
        if(!tilesParent) tilesParent = new GameObject(nameRef);
        return tilesParent;
    }

    private Vector2[] GetPossibleAdjacentPositions()
    {
        var adjacentPositions = new List<Vector2> {Vector2.up, Vector2.down, Vector2.right, Vector2.left};
        Vector2 currentPos = transform.position;

        foreach (var spawnedToCurrent in _spawnedTilesPos.Select(spawnedTile => spawnedTile - currentPos))
        {
            for (var i = 0; i < adjacentPositions.Count; i++)
            {
                var adjacentPosition = adjacentPositions[i];
                if (spawnedToCurrent == adjacentPosition)
                {
                    adjacentPositions.RemoveAt(i);
                }

                if (IsOutOfBounds(currentPos + adjacentPosition))
                {
                    adjacentPositions.RemoveAt(i);
                }
            }
        }

        return (adjacentPositions.Count > 0 ? adjacentPositions : null)?.ToArray();
    }

    private bool IsOutOfBounds(Vector2 pos) =>
        pos.x < _xMin || pos.x > _xMax ||
        pos.y < _yMin || pos.y > _yMax;

    public void SetAutoSpawn(bool value) => autoSpawn = value;
    public void SetSpawnLapse(string value)
    {
        if(value == string.Empty) return;
        
        timeBetweenSpawns = Convert.ToSingle(value);
    }

    public void ReloadScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}