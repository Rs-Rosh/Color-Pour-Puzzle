using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCardUI : MonoBehaviour
{
    public Image backgroundImage;

    public TextMeshProUGUI levelText;

    [Header("Easy")]
    public Sprite easyNormal;
    public Sprite easyCurrent;

    [Header("Medium")]
    public Sprite mediumNormal;
    public Sprite mediumCurrent;

    [Header("Hard")]
    public Sprite hardNormal;
    public Sprite hardCurrent;

    private int levelNumber;

    public void Setup(int level)
    {
        levelNumber = level;

        levelText.text = level.ToString();

        SetCurrent(false);
    }

    public void SetCurrent(bool isCurrent)
    {
        int type = levelNumber % 5;

        // Easy
        if (type == 1 || type == 2)
        {
            backgroundImage.sprite =
                isCurrent
                ? easyCurrent
                : easyNormal;
        }

        // Medium
        else if (type == 3 || type == 4)
        {
            backgroundImage.sprite =
                isCurrent
                ? mediumCurrent
                : mediumNormal;
        }

        // Hard
        else
        {
            backgroundImage.sprite =
                isCurrent
                ? hardCurrent
                : hardNormal;
        }
    }
}