using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "SO/PuzzleGame/PuzzleInfo")]
public class PuzzleInfo : ScriptableObject
{
    public PuzzlesList[] data;
}

[System.Serializable]
public class PuzzlesList
{
    public string namePuzzle;
    public List<Texture2D> imagesPuzzle;
}