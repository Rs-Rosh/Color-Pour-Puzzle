using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject homePanel;
    public GameObject gameplayPanel;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Gameplay")]
    public Tube selectedTube;
    public Tube[] allTubes;

    [Header("Locked Tube")]
    public Tube lockedTube;
    public bool isLockedTubeUnlocked = false;

    [Header("Player Data")]
    public int coins = 100;
    public int lives = 1;
    public int undoCount = 1;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI levelCompleteText;
    
    [Header("Undo Popup")]
    public GameObject undoPopup;

    public TextMeshProUGUI undoText;

    [Header("Level Track")]
    public Transform levelTrackContainer;

    [Header("Restart Popups")]
    public GameObject restartPopup;

    public Transform[] cardPositions;

    public GameObject levelCardPrefab;

    private List<LevelCardUI> activeCards =
    new List<LevelCardUI>();

    private List<List<Tube.TubeColor>> levelSnapshot =
    new List<List<Tube.TubeColor>>();

    void Start()
    {
        LoadPlayerData();
        UpdateUI();
        BuildLevelTrack();

       // homePanel.SetActive(true);
        gameplayPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    // START GAMEPLAY
    public void StartGameplay()
    {
        homePanel.SetActive(false);
        gameplayPanel.SetActive(true);

        GenerateLevel();
    }

    // HOME BUTTON
    public void GoHome()
    {
        gameplayPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        homePanel.SetActive(true);
    }

    // TUBE CLICK
    public void OnTubeClicked(Tube clickedTube)
    {
        // Locked tube check
        if (clickedTube == lockedTube &&
            !isLockedTubeUnlocked)
        {
            Debug.Log("Tube Locked");
            return;
        }

        // First selection
        if (selectedTube == null)
        {
            if (clickedTube.IsEmpty())
                return;

            selectedTube = clickedTube;

            selectedTube.SelectTube();
        }
        else
        {
            if (selectedTube == clickedTube)
            {
                selectedTube.DeselectTube();

                selectedTube = null;

                return;
            }

            TryPour(selectedTube, clickedTube);

            selectedTube.DeselectTube();

            selectedTube = null;
        }
    }

    void TryPour(
        Tube fromTube,
        Tube toTube)
    {
        if (fromTube.IsEmpty()) return;

        if (toTube.IsFull()) return;

        Tube.TubeColor movingColor =
            fromTube.GetTopColor();

        if (!toTube.IsEmpty())
        {
            if (toTube.GetTopColor() != movingColor)
            {
                return;
            }
        }

        int sameColorCount = 0;

        for (int i = fromTube.colors.Count - 1; i >= 0; i--)
        {
            if (fromTube.colors[i] == movingColor)
            {
                sameColorCount++;
            }
            else
            {
                break;
            }
        }

        int availableSpace =
            toTube.maxCapacity - toTube.colors.Count;

        int pourAmount =
            Mathf.Min(sameColorCount, availableSpace);

        AnimatePour(
            fromTube,
            toTube,
            movingColor,
            pourAmount);
    }

    // POUR ANIMATION
    void AnimatePour(
        Tube fromTube,
        Tube toTube,
        Tube.TubeColor movingColor,
        int pourAmount)
    {
        RectTransform tubeRect =
            fromTube.GetComponent<RectTransform>();

        RectTransform targetRect =
            toTube.GetComponent<RectTransform>();

        tubeRect.SetAsLastSibling();

        Vector2 originalPos =
            tubeRect.anchoredPosition;

        bool pouringRight =
            targetRect.anchoredPosition.x >
            originalPos.x;

        Vector2 liftedPos =
            new Vector2(
                originalPos.x,
                originalPos.y + 220f);

        float farOffset =
            pouringRight ? -260f : 260f;

        Vector2 sidePos =
            new Vector2(
                targetRect.anchoredPosition.x +
                farOffset,

                targetRect.anchoredPosition.y + 220f);

        float tiltAngle =
            pouringRight ? -75f : 75f;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            tubeRect.DOAnchorPos(
                liftedPos,
                0.2f));

        seq.Append(
            tubeRect.DOAnchorPos(
                sidePos,
                0.25f));

        seq.Append(
            tubeRect.DORotate(
                new Vector3(0, 0, tiltAngle),
                0.15f));

        seq.AppendInterval(0.15f);

        seq.AppendCallback(() =>
        {
            for (int i = 0; i < pourAmount; i++)
            {
                fromTube.RemoveTopColor();

                toTube.AddColor(movingColor);
            }

            CheckWinCondition();
        });

        seq.AppendInterval(0.08f);

        seq.Append(
            tubeRect.DORotate(
                Vector3.zero,
                0.18f));

        seq.Append(
            tubeRect.DOAnchorPos(
                originalPos,
                0.25f));
    }

    // WIN CHECK
    void CheckWinCondition()
    {
        foreach (Tube tube in allTubes)
        {
            if (!tube.IsComplete())
            {
                return;
            }
        }

        WinLevel();
    }

    // LOSE CHECK
    void CheckLoseCondition()
    {
        // Check all possible tube combinations
        for (int i = 0; i < allTubes.Length; i++)
        {
            for (int j = 0; j < allTubes.Length; j++)
            {
                // Skip same tube
                if (i == j)
                    continue;

                Tube fromTube = allTubes[i];
                Tube toTube = allTubes[j];

                // Ignore locked tube if locked
                if (toTube == lockedTube &&
                    !isLockedTubeUnlocked)
                {
                    continue;
                }

                // Empty source cannot pour
                if (fromTube.IsEmpty())
                    continue;

                // Full destination cannot receive
                if (toTube.IsFull())
                    continue;

                // Empty destination is valid move
                if (toTube.IsEmpty())
                {
                    return;
                }

                // Matching top colors is valid move
                if (fromTube.GetTopColor() ==
                    toTube.GetTopColor())
                {
                    return;
                }
            }
        }
        // No valid moves found
        LoseLevel();
    }

    // WIN
    void WinLevel()
    {
        gameplayPanel.SetActive(false);

        winPanel.SetActive(true);

        int rewardCoins = 0;

        // Determine level type
        int levelType =
            currentLevel % 5;

        // EASY
        if (levelType == 1 || levelType == 2)
        {
            rewardCoins = 10;
        }

        // MEDIUM
        else if (levelType == 3 || levelType == 4)
        {
            rewardCoins = 30;
        }

        // HARD
        else
        {
            rewardCoins = 50;
            // Hard reward
            lives += 1;
            undoCount += 1;
        }
        coins += rewardCoins;
        levelCompleteText.text = "Level " + currentLevel + " Completed";

        UpdateUI();
        SavePlayerData();
    }

    // LOSE
    public void LoseLevel()
    {
        // Deduct life
        lives--;

        UpdateUI();
        SavePlayerData();

        gameplayPanel.SetActive(false);

        losePanel.SetActive(true);
    }

    // PLAY AGAIN
    public void PlayAgain()
    {
        if (lives <= 0)
        {
            Debug.Log("No Lives Left");
            return;
        }

        // Hide lose panel
        losePanel.SetActive(false);

        // Show gameplay
        gameplayPanel.SetActive(true);

        // Clear any selected tube
        if (selectedTube != null)
        {
            selectedTube.DeselectTube();
            selectedTube = null;
        }

        // Reset locked tube
        isLockedTubeUnlocked = false;

        // Restore the original puzzle
        RestoreLevelSnapshot();
    }

    public void CollectReward()
    {
        // Next level
        currentLevel++;

        // Save progress
        SavePlayerData();

        // Close win panel
        winPanel.SetActive(false);

        // Go back home
        homePanel.SetActive(true);

        // Level Meter Progress
        DOVirtual.DelayedCall(0.4f, () =>
        {
            AnimateLevelProgression();
        });
    }

    public void ShowRestartPopup()
    {
        restartPopup.SetActive(true);
    }

    public void CancelRestart()
    {
        restartPopup.SetActive(false);
    }

    public void RestartLevel()
    {
        restartPopup.SetActive(false);

        // Need at least 2 lives
        if (lives < 2)
        {
            LoseLevel();
            return;
        }

        // Deduct 2 lives
        lives -= 2;

        if (selectedTube != null)
        {
            selectedTube.DeselectTube();
            selectedTube = null;
        }

        isLockedTubeUnlocked = false;

        RestoreLevelSnapshot();

        UpdateUI();
        SavePlayerData();
    }

    public void ShowUndoPopup()
    {
        undoPopup.SetActive(true);
    }

    public void CloseUndoPopup()
    {
        undoPopup.SetActive(false);
    }

    public void OnUndoButtonPressed()
    {
        if (undoCount > 0)
        {
            // Actual undo will come in Phase 2
            Debug.Log("Undo Move");

            return;
        }

        ShowUndoPopup();
    }

    public void BuyUndo()
    {
        if (coins < 50)
        {
            Debug.Log("Not enough coins");
            return;
        }

        coins -= 50;

        undoCount++;

        UpdateUI();

        SavePlayerData();

        undoPopup.SetActive(false);
    }

    public void WatchAdUndo()
    {
        Debug.Log("Rewarded Ads Coming Soon");
    }

    // UPDATE UI
    void UpdateUI()
    {
        coinText.text = coins.ToString();

        lifeText.text = lives.ToString();

        undoText.text = undoCount.ToString();
    }

    void BuildLevelTrack()
    {
        foreach (LevelCardUI card in activeCards)
        {
            Destroy(card.gameObject);
        }

        activeCards.Clear();

        for (int i = 0; i < 6; i++)
        {
            int levelNumber =
                currentLevel + (5 - i);

            GameObject cardObj =
                Instantiate(
                    levelCardPrefab,
                    levelTrackContainer);

            RectTransform cardRect =
                cardObj.GetComponent<RectTransform>();

            cardRect.position =
                cardPositions[i].position;

            LevelCardUI card =
                cardObj.GetComponent<LevelCardUI>();

            card.Setup(levelNumber);

            activeCards.Add(card);
        }

        HighlightCurrentCard();
    }

    void HighlightCurrentCard()
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].SetCurrent(i == 5);
        }
    }

    void AnimateLevelProgression()
    {
        float duration = 0.65f;

        // Current level card leaving screen
        LevelCardUI completedCard =
            activeCards[5];

        Sequence seq = DOTween.Sequence();

        // Move all remaining cards DOWN one slot
        completedCard.GetComponent<RectTransform>()
        .DOMoveY(cardPositions[5].position.y - 300f,duration * 0.8f)
        .SetEase(Ease.InOutSine);
        DOVirtual.DelayedCall(0.12f, () =>
        {
            for (int i = activeCards.Count - 2; i >= 0; i--)
            {
                activeCards[i]
                    .GetComponent<RectTransform>()
                    .DOMove(
                        cardPositions[i + 1].position,
                        duration)
                    .SetEase(Ease.InOutSine);
            }
        });

        // Current level exits bottom
        completedCard
            .GetComponent<RectTransform>()
            .DOMoveY(
                cardPositions[5].position.y - 300f,
                duration)
            .SetEase(Ease.InOutSine);

        seq.AppendInterval(duration);

        seq.AppendCallback(() =>
        {
            // Remove completed card
            activeCards.Remove(completedCard);

            Destroy(completedCard.gameObject);

            // Create new hidden top card
            GameObject newCard =
                Instantiate(
                    levelCardPrefab,
                    levelTrackContainer);

            RectTransform newRect =
                newCard.GetComponent<RectTransform>();

            // Spawn above Pos0
            Vector3 spawnPos =
                cardPositions[0].position +
                Vector3.up * 300f;

            newRect.position = spawnPos;

            // Highest level visible + 1
            int newLevel =
                currentLevel + 5;

            LevelCardUI cardUI =
                newCard.GetComponent<LevelCardUI>();

            cardUI.Setup(newLevel);

            activeCards.Insert(0, cardUI);

            // Slide into hidden position
            newRect.DOMove(
                cardPositions[0].position,
                duration)
                .SetEase(Ease.InOutSine);

            HighlightCurrentCard();
        });
    }

    void SavePlayerData()
    {
        PlayerPrefs.SetInt("Coins", coins);

        PlayerPrefs.SetInt("Lives", lives);

        PlayerPrefs.SetInt("Undo", undoCount);

        PlayerPrefs.SetInt("Level", currentLevel);

        PlayerPrefs.Save();
    }

    void LoadPlayerData()
    {
        coins =
            PlayerPrefs.GetInt("Coins", 100);

        lives =
            PlayerPrefs.GetInt("Lives", 5);

        undoCount =
            PlayerPrefs.GetInt("Undo", 0);

        currentLevel =
            PlayerPrefs.GetInt("Level", 1);
    }

    // GENERATE LEVEL
    void GenerateLevel()
    {
        // Clear all tubes
        foreach (Tube tube in allTubes)
        {
            tube.colors.Clear();

            tube.RefreshTubeVisual();
        }

        // Reset locked tube
        isLockedTubeUnlocked = false;

        // 5 gameplay colors
        Tube.TubeColor[] possibleColors =
        {
        Tube.TubeColor.Red,
        Tube.TubeColor.Blue,
        Tube.TubeColor.Green,
        Tube.TubeColor.Yellow,
        Tube.TubeColor.Purple
            };

        // Create color pool
        System.Collections.Generic.List<Tube.TubeColor>
            colorPool =
            new System.Collections.Generic.List<Tube.TubeColor>();

        foreach (Tube.TubeColor color in possibleColors)
        {
            for (int i = 0; i < 4; i++)
            {
                colorPool.Add(color);
            }
        }

        // Shuffle colors
        for (int i = 0; i < colorPool.Count; i++)
        {
            Tube.TubeColor temp =
                colorPool[i];

            int randomIndex =
                Random.Range(i, colorPool.Count);

            colorPool[i] =
                colorPool[randomIndex];

            colorPool[randomIndex] =
                temp;
        }

        // Fill first 5 tubes
        int colorIndex = 0;

        for (int tubeIndex = 0;
             tubeIndex < 5;
             tubeIndex++)
        {
            for (int slot = 0;
                 slot < 4;
                 slot++)
            {
                allTubes[tubeIndex]
                    .colors
                    .Add(colorPool[colorIndex]);

                colorIndex++;
            }

            allTubes[tubeIndex]
                .RefreshTubeVisual();
        }

        // Empty tubes refresh
        allTubes[5].RefreshTubeVisual();
        allTubes[6].RefreshTubeVisual();
        allTubes[7].RefreshTubeVisual();

        SaveLevelSnapshot();
    }

    // Reset Game 
    void SaveLevelSnapshot()
    {
        levelSnapshot.Clear();

        foreach (Tube tube in allTubes)
        {
            List<Tube.TubeColor> colors =
                new List<Tube.TubeColor>();

            colors.AddRange(tube.colors);

            levelSnapshot.Add(colors);
        }
    }

    void RestoreLevelSnapshot()
    {
        for (int i = 0; i < allTubes.Length; i++)
        {
            allTubes[i].colors.Clear();

            allTubes[i].colors.AddRange(levelSnapshot[i]);

            allTubes[i].RefreshTubeVisual();
        }

        isLockedTubeUnlocked = false;
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }
}