using UnityEngine;

[CreateAssetMenu (fileName = "TerminalData", menuName = "DataObject/Terminal", order = 1)]
public class TerminalData : ScriptableObject {
    public Sprite TerminalSprite;
    public int Capacity;
    public bool GatherMailFromHouseholds;
    public float GatherMailRadius;

}