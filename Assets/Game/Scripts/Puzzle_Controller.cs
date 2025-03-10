using System;
using System.Linq;
using UnityEngine;
using Scripts.Base.Helpers;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Puzzle_Controller : MonoBehaviour
{
    [SerializeField] private Game_Controller game_controller;
    [SerializeField] private float samplePos, sampleHDPos, newPos;
    [SerializeField] private GameObject dopPlace, nextBtn;
    [SerializeField] private SpriteRenderer sample;
    [SerializeField] private SpriteRenderer verticalSeparator, verticalSeparator2, horizontalSeparator;
    [SerializeField] private Transform imgPlace;
    [SerializeField] private Texture2D line;
    [SerializeField] private Tile tileObj;
    [SerializeField] private Tile placeObj;

    private readonly List<Vector3> tilesPos = new();

    private static Action<string> updateInstruction, updateWinCount;
    private bool isMoving, isFullHD, isEasy, isHard;
    private int tileID, imgI, gridRows, gridCols;
    private Texture2D originalPicture;
    private List<string> descriptions;
    private List<Texture2D> pictures;
    private List<Tile> tiles, places;
    private Vector2 targetScale;
    private Tile selectedTile;
    private float oneCellSize;

    private Tile CreateNewTile()
    {
        int posIndex = Random.Range(0, tilesPos.Count);
        Tile tile = Instantiate(tileObj, imgPlace, true);
        tile.SetPosition(tilesPos[posIndex]);
        tilesPos.RemoveAt(posIndex);
        tilesPos.Shuffle();
        tile.SetScale(gridCols, targetScale);
        tile.id = tileID;
        tile.canPress = true;
        tile.correctPlace = false;
        tile.freezeUnfreeze += FreezeUnfreezeAllTiles;
        tile.OnTileSelected += OnTileSelected;
        return tile;
    }

    private void CreateNewPlace(Sprite tileSprite, int i, int j)
    {
        Tile place = Instantiate(placeObj, imgPlace, true);

        // Размеры одной ячейки с учетом масштабирования
        float cellWidth = tileSprite.bounds.size.x * oneCellSize;
        float cellHeight = tileSprite.bounds.size.y * oneCellSize;

        // Размеры сетки
        float gridWidth = gridCols * cellWidth;
        float gridHeight = gridRows * cellHeight;

        // Позиция центра imgPlace
        Vector3 imgPlacePosition = imgPlace.localPosition;

        // Начальная позиция (левый верхний угол) для центрирования сетки
        float startX = -gridWidth / 2f;
        float startY = imgPlacePosition.y - gridHeight / 2f;

        // Позиция ячейки
        float posX = startX + j * cellWidth + cellWidth / 2f;
        float posY = startY + i * cellHeight + cellHeight / 2f;

        if (i == j) // && i == 1)
        {
            var verticalX = startX + gridWidth / gridCols;
            var horizontalX = startX + gridWidth / 2;
            var verticalScale = verticalSeparator.transform.localScale;
            var y = (startY + gridHeight / 2) * 2;
            horizontalSeparator.gameObject.SetActive(!isEasy);
            verticalSeparator2.gameObject.SetActive(isHard);

            verticalSeparator.sprite = line.Texture2DResizeToSprite(originalPicture.height / gridRows, 7);
            verticalSeparator.transform.localScale = new Vector3(oneCellSize * gridRows, verticalScale.y, verticalScale.z); // Длина по ширине сетки
            verticalSeparator.transform.localPosition = new Vector3(verticalX, y, imgPlacePosition.z); // Позиция по центру

            if (!isEasy)
            {
                var horizontalScale = horizontalSeparator.transform.localScale;
                horizontalSeparator.sprite = line.Texture2DResizeToSprite(originalPicture.width / 2, 7);
                horizontalSeparator.transform.localScale = new Vector3(oneCellSize * 2, horizontalScale.y, horizontalScale.z); // Длина по ширине сетки
                horizontalSeparator.transform.localPosition = new Vector3(horizontalX, y, imgPlacePosition.z); // Позиция по центру

                if (isHard)
                {
                    verticalSeparator2.sprite = line.Texture2DResizeToSprite(originalPicture.height / 2, 7);
                    verticalSeparator2.transform.localScale = new Vector3(oneCellSize * gridRows, verticalScale.y, verticalScale.z); // Длина по ширине сетки
                    verticalSeparator2.transform.localPosition = new Vector3(-verticalX, y, imgPlacePosition.z); // Позиция по центру
                }
            }
        }

        // Устанавливаем позицию ячейки относительно родительского объекта
        place.SetPosition(new Vector3(posX, posY, imgPlacePosition.z));
        place.id = tileID;
        place.OnTileSelected += OnPlaceSelected;
        place.freezeUnfreeze += FreezeUnfreezeAllTiles;
        place.spriteRenderer.sprite = tileSprite;
        place.SetColliderSize();
        place.SetPlace(targetScale);
        places.Add(place);
    }

    private void TilesPositions()
    {
        int tilesCount = gridRows * gridCols;
        var widthAndX = GetWidthAndX(tilesCount);
        float cellWidth = widthAndX.width;
        float startX = widthAndX.x;
        float y = isEasy ? -4f : -5f;

        for (int i = 0; i < tilesCount; i++)
        {
            int row = i / gridCols;
            int col = i % gridCols;
            float posX = startX + col * cellWidth;
            float posY = y + row * 1.75f;

            tilesPos.Add(new Vector3(posX, posY, 0));
        }
    }

    private void TilesHDPositions()
    {
        int tilesCount = gridRows * gridCols;
        var widthAndX = GetWidthAndX(tilesCount);
        float cellWidth = widthAndX.width;
        float startX = widthAndX.x;

        for (int i = 0; i < tilesCount; i++)
            tilesPos.Add(new Vector3(startX + i * cellWidth, gridCols == 2 ? -3.75f : -4, 0));
    }

    private (float width, float x) GetWidthAndX(int tilesCount)
    {
        float screenSize = Camera.main.orthographicSize * game_controller.widthScreen / game_controller.heightScreen;
        screenSize *= 2;
        float delimiter = isFullHD ? tilesCount : gridCols;
        float cellWidth = screenSize / delimiter;
        float startX = -screenSize / 2 + cellWidth / 2;

        tilesPos.Clear();

        return (cellWidth, startX);
    }

    private void OnTileSelected(Tile tile)
    {
        if (isMoving)
            return;

        FreezeUnfreezeTiles(places, true);

        if (selectedTile != null)
        {
            if (selectedTile == tile)
                return;

            if (selectedTile.inPlace || tile.inPlace)
            {
                isMoving = true;
                FreezeUnfreezeTiles(places, false);
                int selectedId = selectedTile.placeId;
                bool selectedInPlace = selectedTile.inPlace;
                Vector3 selectedPos = selectedTile.transform.localPosition;
                selectedTile.MoveTile(tile.transform.localPosition, tile.placeId, tile.inPlace);
                tile.afterMove = CheckIfSolved;
                tile.MoveTile(selectedPos, selectedId, selectedInPlace);
            }
            else
                selectedTile.Unselect(false);
        }

        selectedTile = tile;
    }

    private void OnPlaceSelected(Tile place)
    {
        if (isMoving || !selectedTile)
            return;

        isMoving = true;
        selectedTile.afterMove = CheckIfSolved;
        place.Unselect();
        if (selectedTile.inPlace)
            ResetSelectedPlace(selectedTile.placeId);
        selectedTile.MoveTile(place.transform.localPosition, place.id, true);
    }

    private void ResetSelectedPlace(int placeId)
    {
        foreach (var place in places.Where(place => place.id == placeId))
        {
            place.isSelected = false;
            place.OnOffCollider(true);
        }
    }

    private void FreezeUnfreezeAllTiles(bool unfreeze)
    {
        FreezeUnfreezeTiles(tiles, unfreeze);
        if (!unfreeze)
            FreezeUnfreezeTiles(places, false);
    }

    private void FreezeUnfreezeTiles(List<Tile> freezeTiles, bool unfreeze)
    {
        foreach (var freezeTile in freezeTiles)
            freezeTile.canPress = unfreeze;
    }

    private void CheckIfSolved()
    {
        selectedTile = null;
        isMoving = false;

        if (tiles.Any(tile => !tile.correctPlace))
            return;

        FreezeUnfreezeAllTiles(false);
        nextBtn.SetActive(true);
        isMoving = false;
    }
    
    public void RemoveTiles()
    {
        dopPlace.SetActive(false);
        ClearImgPlace();
        tiles.Clear();
        places.Clear();
    }

    public void CreateGame(int rows, int cols)
    {
        isEasy = rows == 1;
        isHard = cols == 3;
        isFullHD = game_controller.isFullHD;
        var sampleCurPos = sample.transform.localPosition;
        sampleCurPos.x = isFullHD ? sampleHDPos : samplePos;
        sample.transform.localPosition = sampleCurPos;
        var curObjPos = transform.localPosition;
        curObjPos.x = isFullHD ? 0 : newPos;
        transform.localPosition = curObjPos;
        gridRows = rows;
        gridCols = cols;
        selectedTile = null;
        CreateTiles();
    }

    public void ClearImgPlace()
    {
        for (int i = imgPlace.childCount - 1; i >= 0; i--)
        {
            Transform child = imgPlace.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    public void CreateTiles()
    {
        tileID = 0;
        tiles = new List<Tile>();
        places = new List<Tile>();
        var textures = game_controller.imgInfo;
        originalPicture = textures[game_controller.imgI];

        float ratioX = game_controller.widthScreen / originalPicture.width;
        float ratioY = game_controller.heightScreen / originalPicture.height;
        float ratio = Mathf.Min(ratioX, ratioY);
        int newWidth = Mathf.RoundToInt(originalPicture.width * ratio);
        int newHeight = Mathf.RoundToInt(originalPicture.height * ratio);

        originalPicture = originalPicture.ResizeTexture(newWidth, newHeight);
        sample.sprite = originalPicture.Texture2DResizeToSprite(newWidth / 5, newHeight / 5);
        oneCellSize = (isFullHD ? game_controller.widthScreen / 1.8f : game_controller.widthScreen) /
                      (gridRows == 1 ? 2500 : 2000);

        var tileTransform = tileObj.transform;
        var startScale = new Vector2(oneCellSize, oneCellSize);
        tileTransform.localScale = startScale * (gridCols == 2 ? 1 : 1.75f);
        
        targetScale = new Vector2(oneCellSize, oneCellSize);

        if (!isFullHD)
            TilesPositions();
        else
            TilesHDPositions();

        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridCols; j++)
            {
                Tile tile = CreateNewTile();

                float widthSprite = (float)originalPicture.width / gridCols;
                float heightSprite = (float)originalPicture.height / gridRows;

                Sprite tileSprite = Sprite.Create(originalPicture,
                    new Rect(j * widthSprite, i * heightSprite, widthSprite, heightSprite),
                    new Vector2(.5f, .5f), 100f);
                tile.spriteRenderer.sprite = tileSprite;
                tile.spriteRenderer.sortingOrder = 3;
                tile.SetColliderSize();
                tiles.Add(tile);

                CreateNewPlace(tileSprite, i, j);
                tileID++;
            }
        }

        dopPlace.SetActive(true);
        tilesPos.Clear();
    }
}