using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCardUI : MonoBehaviour
{
    public Image backgroundImage;

    public TextMeshProUGUI levelText;

    [Header("Sprites")]
    public Sprite easySprite;

    public Sprite mediumSprite;

    public Sprite hardSprite;

    public void Setup(int levelNumber)
    {
        levelText.text = levelNumber.ToString();

        int type = levelNumber % 5;

        // Easy
        if (type == 1 || type == 2)
        {
            backgroundImage.sprite = easySprite;
        }

        // Medium
        else if (type == 3 || type == 4)
        {
            backgroundImage.sprite = mediumSprite;
        }

        // Hard
        else
        {
            backgroundImage.sprite = hardSprite;
        }
    }
}