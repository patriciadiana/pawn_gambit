using System.Collections.Generic;
using UnityEngine;

public class TargetGeneration : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField, Range(1, 16)] private int numberOfTargets = 10;
    [SerializeField] private float zOffset = -1f;

    public int spawnedTargets { get; private set; }

    private void Start()
    {
        GenerateTargets();
    }

    private void GenerateTargets()
    {
        if (targetPrefab == null)
            return;

        spawnedTargets = 0;

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        int attempts = 0;
        int maxAttempts = 500;

        while (spawnedTargets < numberOfTargets && attempts < maxAttempts)
        {
            attempts++;

            int x = Random.Range(0, BoardConfig.BoardWidth);
            int y = Random.Range(0, BoardConfig.BoardHeight);

            bool preferWhite = Random.value < 0.6f;
            if (preferWhite && (x + y) % 2 != 0)
                continue;
            if (!preferWhite && (x + y) % 2 == 0)
                continue;

            Vector2Int gridPos = new Vector2Int(x, y);

            if (occupied.Contains(gridPos))
                continue;

            Vector3 worldPos = BoardConfig.GridToWorld(gridPos);
            worldPos.z = zOffset;

            Instantiate(targetPrefab, worldPos, Quaternion.identity);

            occupied.Add(gridPos);
            spawnedTargets++;
        }
    }


}
