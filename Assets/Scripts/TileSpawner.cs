using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] private Vector2 startingPosition;
    [SerializeField] private GameObject tile;

    private bool _mustReturn;

    private void Start()
    {
        GetCameraBounds(out var xMin, out var xMax, out var yMin, out var yMax);

        _mustReturn = startingPosition.x > xMax || startingPosition.x < xMin || startingPosition.y > yMax || startingPosition.y < yMin;
        if (_mustReturn)
        {
            Debug.LogWarning("The spawner starting position exceeds the bounds of the game camera");
            return;
        }

        var roundedStartingXPos = Mathf.RoundToInt(startingPosition.x);
        var roundedStartingYPos = Mathf.RoundToInt(startingPosition.y);
        
        transform.position = new Vector2(roundedStartingXPos, roundedStartingYPos);
    }

    private static void GetCameraBounds(out float xMin, out float xMax, out float yMin, out float yMax)
    {
        var mainCamera = Camera.main;
        if (mainCamera is null)
        {
            xMin = 0;
            xMax = 0;
            yMin = 0;
            yMax = 0;
            return;
        }

        var startingPoint = mainCamera.ViewportToWorldPoint(Vector3.zero);
        xMin = startingPoint.x;
        xMax = mainCamera.ViewportToWorldPoint(Vector3.right).x;
        yMin = startingPoint.y;
        yMax = mainCamera.ViewportToWorldPoint(Vector3.up).y;
    }
}