using UnityEngine;
using DawgSharp;
using System.IO;
using TMPro;

public class Board : MonoBehaviour
{

    private string SECRET_WORD;
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[]
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H
        , KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P
        , KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X
        , KeyCode.Y, KeyCode.Z,
    };

    private Row[] Rows;
    private int RowIndex;
    private int ColumnIndex;
    private Dawg<bool> dawg;
    private string path = "Assets/Script/WordList.txt";

    [Header("States")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;

    [Header("UI")]
    public GameObject invalidNote;
    public GameObject winNote;
    public GameObject button;
    public GameObject loseNote;

    private void Awake()
    {
        StreamReader reader = new StreamReader(path);
        var dawgBuilder = new DawgBuilder<bool>();

        string line;
        while((line = reader.ReadLine()) != null){
            dawgBuilder.Insert(line, true);
        }
        dawg = dawgBuilder.BuildDawg();


        this.Rows = GetComponentsInChildren<Row>();
        enabled = true;
    }

    public void Start()
    {
        NewWord();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        Row CurrentRow = Rows[RowIndex];

        if (Input.GetKeyDown(KeyCode.Backspace)) {
            ColumnIndex = Mathf.Max(ColumnIndex - 1, 0);
            CurrentRow.Tiles[ColumnIndex].SetLetter('\0');
            CurrentRow.Tiles[ColumnIndex].SetState(emptyState);

            invalidNote.SetActive(false);

        } else if (ColumnIndex >= CurrentRow.Tiles.Length) {
            if (Input.GetKeyDown(KeyCode.Return)){
                print(CurrentRow.ToString());
                SubmitRow(CurrentRow);

            }
            
        } else {
            foreach (KeyCode key in SUPPORTED_KEYS) {
                if (Input.GetKeyDown(key)) {
                    CurrentRow.Tiles[ColumnIndex].SetLetter((char)key);
                    CurrentRow.Tiles[ColumnIndex].SetState(occupiedState);
                    ColumnIndex = Mathf.Min(ColumnIndex + 1, CurrentRow.Tiles.Length);
                    break;
                }
            }
        }

    }

    private void OnEnable()
    {
        button.SetActive(false);
        loseNote.SetActive(false);
    }

    private void OnDisable()
    {
        button.SetActive(true);
    }

    private void SubmitRow (Row row)
    {
        if (!dawg[row.ToString()])
        {
            invalidNote.SetActive(true);
            return;
        }
        string remaining = SECRET_WORD;

        for(int i = 0; i < row.Tiles.Length; i++)
        {
            Tile tile = row.Tiles[i];
            if (tile.Letter == SECRET_WORD[i])
            {
                tile.SetState(correctState);
                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, "^");
            }
            else if(!SECRET_WORD.Contains(tile.Letter.ToString()))
            {
                tile.SetState(incorrectState);
            }
        }

        for(int i = 0; i < row.Tiles.Length; i++)
        {
            Tile tile = row.Tiles[i];

            if(tile.state != correctState && tile.state != incorrectState)
            {
                if (remaining.Contains(tile.Letter.ToString())){
                    tile.SetState(wrongSpotState);

                    int index = remaining.IndexOf(tile.Letter);
                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, "^");
                }
                else
                {
                    tile.SetState(incorrectState);
                }

            }
        }
        if (HasWon(row))
        {
            winNote.SetActive(true);
            enabled = false;
        }
        RowIndex++;
        ColumnIndex = 0;

        if(RowIndex >= Rows.Length)
        {
            loseNote.SetActive(true);
            TextMeshProUGUI textMeshPro = loseNote.GetComponentInChildren<TextMeshProUGUI>();
            textMeshPro.SetText("Helaas het woord was: " + SECRET_WORD);
            enabled = false;
        }
    }

    private bool HasWon(Row row)
    {
        for(int i = 0; i < row.Tiles.Length; i++)
        {
            if (row.Tiles[i].state != correctState)
            {
                return false;
            }
        }
        return true;
    }

    public void StartOver()
    {
        enabled = true;
        for (int row = 0; row < Rows.Length; row++)
        {
            for(int col = 0; col < Rows[row].Tiles.Length; col++)
            {
                Rows[row].Tiles[col].SetLetter('\0');
                Rows[row].Tiles[col].SetState(emptyState);
            }
        }
        RowIndex = 0;
        ColumnIndex = 0;
        NewWord();
    }

    public void NewWord()
    {
        System.Random randomForPrefix = new System.Random();
        int randomNum = randomForPrefix.Next(0, SUPPORTED_KEYS.Length);
        char randomLetter = (char)SUPPORTED_KEYS[randomNum];
        var test = dawg.MatchPrefix(randomLetter.ToString());
        int max = 0;
        foreach (var word in test)
        {
            max++;
        }

        System.Random randomForWord = new System.Random();
        int randomIndex = randomForWord.Next(0, max);
        int index = 0;
        foreach (var randomWord in test)
        {
            if (index == randomIndex)
            {
                SECRET_WORD = randomWord.Key;
            }
            index++;
        }
        print(SECRET_WORD);
    }
}
