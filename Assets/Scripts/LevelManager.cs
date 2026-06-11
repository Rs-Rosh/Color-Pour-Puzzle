using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Tube[] allTubes;

    public int currentLevel = 1;

    private Tube.TubeColor[] possibleColors =
    {
        Tube.TubeColor.Red,
        Tube.TubeColor.Blue,
        Tube.TubeColor.Green,
        Tube.TubeColor.Yellow,
        Tube.TubeColor.Purple
    };

    void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        // Clear all tubes
        foreach (Tube tube in allTubes)
        {
            tube.colors.Clear();
        }

        // Create color pool
        List<Tube.TubeColor> colorPool =
            new List<Tube.TubeColor>();

        // Add each color 4 times
        foreach (Tube.TubeColor color in possibleColors)
        {
            for (int i = 0; i < 4; i++)
            {
                colorPool.Add(color);
            }
        }

        // Shuffle colors
        ShuffleList(colorPool);

        // Fill first 5 tubes
        int colorIndex = 0;

        for (int tubeIndex = 0; tubeIndex < 5; tubeIndex++)
        {
            for (int slot = 0; slot < 4; slot++)
            {
                allTubes[tubeIndex]
                    .colors
                    .Add(colorPool[colorIndex]);

                colorIndex++;
            }
        }

        // Refresh visuals
        foreach (Tube tube in allTubes)
        {
            tube.RefreshTubeVisual();
        }

        Debug.Log("Level " + currentLevel + " Generated");
    }

    // Shuffle helper
    void ShuffleList(List<Tube.TubeColor> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Tube.TubeColor temp = list[i];

            int randomIndex =
                Random.Range(i, list.Count);

            list[i] = list[randomIndex];

            list[randomIndex] = temp;
        }
    }

    // Next Level
    public void NextLevel()
    {
        currentLevel++;

        GenerateLevel();
    }
}