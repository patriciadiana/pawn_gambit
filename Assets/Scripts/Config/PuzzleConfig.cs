using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleConfig", menuName = "Scriptable Objects/PuzzleConfig")]
public class PuzzleConfig : ScriptableObject
{
    [Header("Puzzle Info")]
    public string puzzleName;
    public string initialSceneName; /*Scene to spawn after win*/

    [Header("Next Piece Settings")]
    public string nextPieceType;       
    public Vector2Int nextPiecePosition;

    [Header("Reward Settings")]
    public string rewardType;

    [Header("Fail Conditions")]
    public bool hasTimeLimit = false;
    public float timeLimitinSeconds = 60f;   
}
