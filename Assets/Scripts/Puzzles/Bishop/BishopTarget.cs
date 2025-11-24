using UnityEngine;

public class BishopTarget : MonoBehaviour
{
    public BishopMovement bishopMovement;
    public TargetGeneration targetGen;
    private static int captured = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        ChessPiece chessPiece = other.GetComponent<ChessPiece>();
        if (chessPiece != null && chessPiece.name.Contains("bishop"))
        {
            OnBishopReached();
        }
    }

    public void OnBishopReached()
    {
        captured++;

        int total = targetGen.spawnedTargets + 1;

        if (captured >= total)
        {
            bishopMovement.MarkWin();
        }

        Destroy(gameObject);
    }

}