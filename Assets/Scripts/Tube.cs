using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tube : MonoBehaviour
{
    // Different color types
    public enum TubeColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple
    }

    [Header("Starting Colors (Bottom to Top)")]
    public List<TubeColor> colors = new List<TubeColor>();

    [Header("Water Layers")]
    public List<Image> waterLayers = new List<Image>();

    public int maxCapacity = 4;

    [Header("Pour Point")]
    public RectTransform pourPoint;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;

        RefreshTubeVisual();
    }

    // Refresh water visuals
    public void RefreshTubeVisual()
    {
        // Hide all layers first
        foreach (Image layer in waterLayers)
        {
            layer.gameObject.SetActive(false);
        }

        // Show current colors
        for (int i = 0; i < colors.Count; i++)
        {
            waterLayers[i].gameObject.SetActive(true);

            waterLayers[i].color = GetColor(colors[i]);
        }
    }

    // Convert enum to real color
    Color GetColor(TubeColor color)
    {
        switch (color)
        {
            case TubeColor.Red:
                return Color.red;

            case TubeColor.Blue:
                return Color.blue;

            case TubeColor.Green:
                return Color.green;

            case TubeColor.Yellow:
                return Color.yellow;

            case TubeColor.Purple:
                return new Color(0.5f, 0f, 0.5f);

            default:
                return Color.white;
        }
    }

    // Selection effect
    public void SelectTube()
    {
        transform.localScale = originalScale * 1.1f;
    }

    // Remove selection
    public void DeselectTube()
    {
        transform.localScale = originalScale;
    }

    // Check empty
    public bool IsEmpty()
    {
        return colors.Count == 0;
    }

    // Check full
    public bool IsFull()
    {
        return colors.Count >= maxCapacity;
    }

    // Get top color
    public TubeColor GetTopColor()
    {
        return colors[colors.Count - 1];
    }

    // Remove top color
    public void RemoveTopColor()
    {
        if (IsEmpty()) return;

        colors.RemoveAt(colors.Count - 1);

        RefreshTubeVisual();
    }

    // Add color
    public void AddColor(TubeColor color)
    {
        if (IsFull()) return;

        colors.Add(color);

        RefreshTubeVisual();
    }
    // Check if tube is completed

    public bool IsComplete()
    {
        // Empty tube is valid
        if (colors.Count == 0)
            return true;

        // Tube must be full
        if (colors.Count != maxCapacity)
            return false;

        TubeColor firstColor = colors[0];

        // Check all colors same
        foreach (TubeColor color in colors)
        {
            if (color != firstColor)
            {
                return false;
            }
        }

        return true;
    }
}