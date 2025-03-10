using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Game_Controller : MonoBehaviour
{
    [SerializeField] private Puzzle_Controller puzzle;
    [SerializeField] private GameObject finishMenu;
    
    [HideInInspector] public List<Texture2D> imgInfo;
    [HideInInspector] public int imgI;
    [HideInInspector] public bool isFullHD;
    
    public float widthScreen => Screen.width;
    public float heightScreen => Screen.height;
    
    private int tileID, gridRows, gridCols;
    private bool isMoving, isSolved;

    private Texture2D originalPicture;

    private List<Tile> tiles, places;
    private float oneCellSize, minusY;

    private void Start()
    {
        float targetAspect = 16f / 9f;
        float currentAspect = (float)Screen.width / Screen.height;
        isFullHD = currentAspect >= targetAspect;
        SetDefaultGrid();
        Reset();
    }

    private void Reset()
    {
        imgI = 0;
        puzzle.gameObject.SetActive(false);
    }

    private void SetGeneratorParams()
    {
        puzzle.gameObject.SetActive(true);
        puzzle.CreateGame(gridRows, gridCols);
    }
    
    private void FinishGame()
    {
        ResetGame();
        finishMenu.SetActive(true);
    }

    private void SetDefaultGrid()
    {
        gridRows = 1;
        gridCols = 2;
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    public void Exit()
    {
        Application.Quit();
    }
    
    public void SetGridSize(int i)
    {
        switch (i)
        {
            case 1:
                SetDefaultGrid();
                break;
            case 2:
                gridRows = 2;
                gridCols = 2;
                break;
            case 3:
                gridRows = 2;
                gridCols = 3;
                break;
            default:
                SetDefaultGrid();
                break;
        }
    }
    
    public void NextClick()
    {
        imgI++;
        
        if (imgInfo.Count == imgI)
            FinishGame();
        else
        {
            puzzle.RemoveTiles();
            puzzle.CreateTiles();
        }
    }

    public void SetPictureInfo(List<Texture2D> info)
    {
        imgInfo = new();
        imgInfo.AddRange(info);
        SetGeneratorParams();
    }

    private void ResetGame()
    {
        Reset();
        puzzle.ClearImgPlace();
    }

    private void OnDestroy()
    {
        ResetGame();
    }
}