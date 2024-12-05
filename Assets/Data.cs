using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Data", order = 1)]
public class Data : ScriptableObject
{
    public Texture2D Tex;
    public string Name;
    public int Age;
    public float Money;
}