using UnityEngine;

[CreateAssetMenu (fileName = "PostmanUppgradeData", menuName = "DataObject/PostmanUppgrade", order = 1)]
public class PostmanUppgrade : ScriptableObject {
    public Sprite UppgradeSprite;
    public int Capacity, Intellect, Pollution;
    public float Speed;

}