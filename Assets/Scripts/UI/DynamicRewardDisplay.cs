using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicRewardDisplay : MonoBehaviour
{
    [Header("Reward Slots (Image + Text)")]
    [SerializeField] private Image[] slotImages;
    [SerializeField] private TextMeshProUGUI[] slotTexts;

    [Header("Piece Sprites")]
    [SerializeField] private Sprite queenSprite;
    [SerializeField] private Sprite bishopSprite;
    [SerializeField] private Sprite rookSprite;
    [SerializeField] private Sprite kingSprite;
    [SerializeField] private Sprite knightSprite;

    [Header("Piece Texts")]
    [SerializeField] private string queenText = "The Queen will fight alongside you in the final game!";
    [SerializeField] private string bishopText = "The Bishop joins your team in the final game!";
    [SerializeField] private string rookText = "The Rook will support your strategy in the final game!";
    [SerializeField] private string kingText = "With the King, there is no time limit to checkmate your opponent!";
    [SerializeField] private string knightText = "The Knight will assist you in the final game!";


    public void ShowRewards()
    {
        if (PuzzleManager.Instance == null || PuzzleManager.Instance.rewardPieces == null)
        {
            Debug.LogWarning("No reward pieces found!");
            return;
        }

        ClearSlots();

        List<string> rewards = PuzzleManager.Instance.rewardPieces;

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i >= rewards.Count) break;

            string reward = rewards[i];
            Sprite sprite = GetSpriteForReward(reward);
            string text = GetTextForReward(reward);

            if (slotImages[i] != null && sprite != null)
            {
                slotImages[i].sprite = sprite;
                slotImages[i].gameObject.SetActive(true);
            }

            if (slotTexts[i] != null)
            {
                slotTexts[i].text = text;
                slotTexts[i].gameObject.SetActive(true);
            }
        }
    }

    private void ClearSlots()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
                slotImages[i].gameObject.SetActive(false);
            if (slotTexts[i] != null)
                slotTexts[i].gameObject.SetActive(false);
        }
    }

    private Sprite GetSpriteForReward(string reward)
    {
        return reward.ToLower() switch
        {
            "queen" => queenSprite,
            "bishop" => bishopSprite,
            "rook" => rookSprite,
            "king" => kingSprite,
            "knight" => knightSprite,
            _ => null
        };
    }

    private string GetTextForReward(string reward)
    {
        return reward.ToLower() switch
        {
            "queen" => queenText,
            "bishop" => bishopText,
            "rook" => rookText,
            "king" => kingText,
            "knight" => knightText,
            _ => reward
        };
    }
}
