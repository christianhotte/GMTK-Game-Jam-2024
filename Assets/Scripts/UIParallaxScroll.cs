using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParallaxScroll : MonoBehaviour
{
    [SerializeField, Tooltip("The current background pieces.")] private RectTransform[] backgroundPieces;
    [SerializeField, Tooltip("The scrolling speed for the background.")] private float scrollSpeed = 50f;

    private float backgroundWidth;
    private int primaryBackgroundPiece = 1;

    void Start()
    {
        backgroundWidth = backgroundPieces[primaryBackgroundPiece].rect.width;
    }

    void Update()
    {
        // Calculate the scrolling distance
        float deltaX = scrollSpeed * Time.deltaTime;

        // Move the backgrounds horizontally
        for(int i = 0; i < backgroundPieces.Length; i++)
            backgroundPieces[i].anchoredPosition += new Vector2(-deltaX, 0);

        // Check if the leftmost background has moved completely off-screen to the left
        if (backgroundPieces[primaryBackgroundPiece].anchoredPosition.x <= -backgroundWidth)
        {
            backgroundPieces[GetBackgroundPieceIndex(primaryBackgroundPiece - 1)].anchoredPosition = new Vector2(backgroundWidth, 0);
            primaryBackgroundPiece = GetBackgroundPieceIndex(primaryBackgroundPiece + 1);
        }

        // Check if the rightmost background has moved completely off-screen to the right
        else if (backgroundPieces[primaryBackgroundPiece].anchoredPosition.x >= backgroundWidth)
        {
            backgroundPieces[GetBackgroundPieceIndex(primaryBackgroundPiece + 1)].anchoredPosition = new Vector2(-backgroundWidth, 0);
            primaryBackgroundPiece = GetBackgroundPieceIndex(primaryBackgroundPiece - 1);
        }
    }

    /// <summary>
    /// Returns the appropriate index for the background piece.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns>Returns 0 if the index is over the length of the array, the largest array index if negative, or just the index if the index is appropriate.</returns>
    private int GetBackgroundPieceIndex(int index)
    {
        if (index >= backgroundPieces.Length)
            return 0;

        if (index < 0)
            return backgroundPieces.Length - 1;

        return index;
    }
}

