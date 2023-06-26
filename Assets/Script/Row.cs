using UnityEngine;

public class Row : MonoBehaviour
{
    public Tile[] Tiles { get; private set; }

    private void Awake()
    {
        Tiles = GetComponentsInChildren<Tile>();
    }

    public override string ToString()
    {
        string word = "";
        for(int i = 0; i < Tiles.Length; i++)
        {
            word += Tiles[i].Letter;
        }
        return word;
    }
}
