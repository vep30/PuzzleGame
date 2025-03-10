using System;
using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer border;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float scaleSpeed = 1f, moveDuration = .5f;

    private Vector3 newScale, currentScale, startPoint, endPoint;
    private bool scaleUp, isPlace, have;
    private Coroutine scaleCor;
    private float timeLost;

    [HideInInspector] public bool inPlace, correctPlace, canPress, isSelected;

    public SpriteRenderer spriteRenderer;
    public int id, placeId;

    public delegate void ClickAction(Tile tile);

    public event ClickAction OnTileSelected;
    public Action<bool> freezeUnfreeze;
    public Action afterMove;

    private void OnEnable()
    {
        border.gameObject.SetActive(false);
        isSelected = false;
        inPlace = false;
        placeId = -1;
        StopScaleCor();
    }

    public void OnOffCollider(bool on = false)
    {
        boxCollider.enabled = on;
    }

    public void SetScale(int i, Vector2 scaleNew)
    {
        newScale = scaleNew;
        if (transform.localScale.x < 0)
            i *= -1;

        transform.localScale /= i;
        currentScale = transform.localScale;
    }

    public void SetColliderSize()
    {
        var sprite = spriteRenderer.sprite;
        Vector2 spriteSize = sprite.bounds.size;
        boxCollider.size = spriteSize;
        boxCollider.offset = sprite.bounds.center;
        border.transform.localScale = new Vector2(spriteSize.x, spriteSize.y) * 1.15f;
    }

    public void SetPlace(Vector2 scaleNew)
    {
        placeId = id;
        isPlace = true;
        canPress = false;
        var spriteColor = Color.black;
        var colorA = spriteColor;
        colorA.a = .75f;
        spriteColor = colorA;
        transform.localScale = scaleNew;
        spriteRenderer.color = spriteColor;
        spriteRenderer.sortingOrder = 0;
    }

    public void SetPosition(Vector3 newPos)
    {
        gameObject.transform.localPosition = newPos;
    }

    public void OnMouseDown()
    {
        TileReaction();
    }

    public void MoveTile(Vector3 targetPos, int idPlace, bool moveToPlace)
    {
        placeId = idPlace;
        correctPlace = id == placeId;
        freezeUnfreeze?.Invoke(false);
        StartCoroutine(MoveCoroutine(targetPos, moveToPlace));
    }

    public void Unselect(bool leaveSelect = true)
    {
        border.gameObject.SetActive(false);
        isSelected = leaveSelect;
    }
    
    private void TileReaction()
    {
        if (!canPress || isSelected)
            return;

        freezeUnfreeze?.Invoke(false);
        isSelected = true;
        SelectTile();
    }

    private void SelectTile()
    {
        if (!isPlace)
        {
            spriteRenderer.sortingOrder = 6;
            border.gameObject.SetActive(true);
        }

        if (isPlace)
            OnOffCollider();

        OnTileSelected?.Invoke(this);

        if (isPlace)
            return;

        freezeUnfreeze?.Invoke(true);
    }

    private IEnumerator ScaleCoroutine()
    {
        var start = transform.localScale;
        var target = scaleUp ? newScale : currentScale;
        bool needScale = start != target;

        if (needScale)
        {
            float curTime = 0f;
            while (curTime < scaleSpeed)
            {
                curTime += Time.deltaTime;
                var speed = Mathf.Clamp01(curTime / scaleSpeed);
                transform.localScale = Vector3.Lerp(start, target, speed);

                yield return null;
            }

            transform.localScale = target;
        }

        Unselect(false);
    }

    private void StopScaleCor()
    {
        if (scaleCor == null)
            return;

        StopCoroutine(scaleCor);
        scaleCor = null;
    }

    private IEnumerator MoveCoroutine(Vector3 targetPos, bool moveToPlace)
    {
        Unselect();
        Vector3 startPos = transform.localPosition;
        float time = 0;

        while (time < moveDuration)
        {
            transform.localPosition = Vector3.Lerp(startPos, targetPos, time / moveDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
        inPlace = moveToPlace;

        if (!isPlace)
        {
            scaleUp = moveToPlace;
            StopScaleCor();
            yield return ScaleCoroutine();
        }

        spriteRenderer.sortingOrder = 3;
        afterMove?.Invoke();
        freezeUnfreeze?.Invoke(true);
    }
}