using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BishopTarget : MonoBehaviour
{
    public BishopMovement bishopMovement;
    public TargetGeneration targetGen;

    public TextMeshProUGUI targetsLeftText;

    private static int captured = 0;
    private static bool uiShown = false;

    private void Start()
    {
        captured = 0;
        targetsLeftText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (HowToDisplay.hasBeenDisplayed && !uiShown)
        {
            uiShown = true;

            if (targetsLeftText != null)
            {
                targetsLeftText.gameObject.SetActive(true);
                UpdateTargetsLeftUI();
            }
        }
    }

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

        UpdateTargetsLeftUI();

        if (captured >= total)
        {
            bishopMovement.MarkWin();
        }

        Destroy(gameObject);
    }

    void UpdateTargetsLeftUI()
    {
        if (!HowToDisplay.hasBeenDisplayed || targetsLeftText == null)
            return;

        int total = targetGen.spawnedTargets + 1;
        int remaining = Mathf.Max(0, total - captured);

        targetsLeftText.text = $"Targets left: {remaining}";
    }
}
