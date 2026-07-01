using UnityEngine;

[System.Serializable]
public class MoveData
{
    public Tube fromTube;
    public Tube toTube;

    public Tube.TubeColor color;

    public int amount;

    public MoveData(
        Tube from,
        Tube to,
        Tube.TubeColor moveColor,
        int moveAmount)
    {
        fromTube = from;
        toTube = to;
        color = moveColor;
        amount = moveAmount;
    }
}