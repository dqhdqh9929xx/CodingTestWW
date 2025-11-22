using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GamePlayManager : MonoBehaviour
{
    public static int playerCount = 0;
    public GameObject GameWinMenu;

    public RectTransform slot1;
    public RectTransform slot2;
    public RectTransform slot3;
    public RectTransform slot4;
    public RectTransform slot5;

    public List<Cell> PlayerSelectedItem { get; private set; }

    private Camera m_cam;
    private Camera m_uiCamera;
    private BoardController m_boardController;
    private RectTransform[] m_slots;
    private Canvas m_canvas;
    private GameManager m_gameManager;

    private float m_gameOverTimer = 0f;
    private const float GAME_OVER_DELAY = 3f;
    private int m_lastItemCount = 0;

    private void Awake()
    {
        PlayerSelectedItem = new List<Cell>(5);
        
        m_slots = new RectTransform[] { slot1, slot2, slot3, slot4, slot5 };
        
        playerCount = 0;
    }

    private void Start()
    {
        m_cam = Camera.main;
        m_boardController = FindObjectOfType<BoardController>();
        
        m_gameManager = FindObjectOfType<GameManager>();
        
        m_canvas = FindObjectOfType<Canvas>();
        if (m_canvas != null && m_canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            m_uiCamera = m_canvas.worldCamera;
        }
    }

    private void Update()
    {
        CheckAndMatchThreeItems();

        CheckGameOverCondition();

        CheckWinCondition();

        if (PlayerSelectedItem.Count >= 5) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleItemClick();
        }
    }

    private void HandleItemClick()
    {
        var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        
        if (hit.collider != null)
        {
            Cell clickedCell = hit.collider.GetComponent<Cell>();
            
            if (clickedCell != null && !clickedCell.IsEmpty && !PlayerSelectedItem.Contains(clickedCell))
            {
                PlayerSelectedItem.Add(clickedCell);
                
                playerCount++;
                
                int slotIndex = PlayerSelectedItem.Count - 1;
                
                MoveItemToSlot(clickedCell, m_slots[slotIndex], slotIndex);
            }
        }
    }

    private void MoveItemToSlot(Cell cell, RectTransform targetSlot, int slotIndex)
    {
        if (cell == null || cell.Item == null || targetSlot == null) return;

        Vector3 worldPosition = RectTransformToWorldPosition(targetSlot);

        if (cell.Item.View != null)
        {
            cell.Item.View.DOMove(worldPosition, 0.3f)
                .SetEase(Ease.OutQuad);
        }

        cell.transform.DOMove(worldPosition, 0.3f)
            .SetEase(Ease.OutQuad);

        Debug.Log($"Item moved to slot {slotIndex + 1}. Total selected: {PlayerSelectedItem.Count}");
    }

    private Vector3 RectTransformToWorldPosition(RectTransform rectTransform)
    {
        if (rectTransform == null) return Vector3.zero;

        if (m_canvas != null && m_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
            Vector3 worldPos = m_cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, m_cam.nearClipPlane + 1f));
            worldPos.z = 0;
            return worldPos;
        }
        else if (m_canvas != null && m_canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Camera canvasCamera = m_canvas.worldCamera != null ? m_canvas.worldCamera : m_cam;
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            Vector3 center = (worldCorners[0] + worldCorners[2]) / 2f;
            
            Vector3 screenPoint = canvasCamera.WorldToScreenPoint(center);
            Vector3 worldPos = m_cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, m_cam.nearClipPlane + 1f));
            worldPos.z = 0;
            return worldPos;
        }
        else if (m_canvas != null && m_canvas.renderMode == RenderMode.WorldSpace)
        {
            return rectTransform.position;
        }
        else
        {
            return rectTransform.position;
        }
    }

    private void CheckAndMatchThreeItems()
    {
        if (PlayerSelectedItem.Count < 3) return;

        Dictionary<NormalItem.eNormalType, List<Cell>> itemsByType = new Dictionary<NormalItem.eNormalType, List<Cell>>();

        foreach (Cell cell in PlayerSelectedItem)
        {
            if (cell == null || cell.Item == null) continue;

            NormalItem normalItem = cell.Item as NormalItem;
            if (normalItem != null)
            {
                NormalItem.eNormalType itemType = normalItem.ItemType;

                if (!itemsByType.ContainsKey(itemType))
                {
                    itemsByType[itemType] = new List<Cell>();
                }
                itemsByType[itemType].Add(cell);
            }
        }

        foreach (var kvp in itemsByType)
        {
            if (kvp.Value.Count >= 3)
            {
                List<Cell> matchedCells = kvp.Value;
                
                List<Cell> cellsToRemove = new List<Cell>();
                for (int i = 0; i < 3 && i < matchedCells.Count; i++)
                {
                    cellsToRemove.Add(matchedCells[i]);
                }

                ProcessMatch(cellsToRemove);
                break;
            }
        }
    }

    private void ProcessMatch(List<Cell> matchedCells)
    {
        if (matchedCells == null || matchedCells.Count != 3) return;

        Debug.Log($"Match found! Removing 3 items of type from PlayerSelectedItem");

        foreach (Cell cell in matchedCells)
        {
            if (cell != null && cell.Item != null)
            {
                cell.Item.ExplodeView();
                
                cell.Free();
            }
        }

        foreach (Cell cell in matchedCells)
        {
            PlayerSelectedItem.Remove(cell);
        }

        RearrangeRemainingItems();
    }

    private void RearrangeRemainingItems()
    {
        for (int i = 0; i < PlayerSelectedItem.Count; i++)
        {
            Cell cell = PlayerSelectedItem[i];
            if (cell != null && cell.Item != null && i < m_slots.Length)
            {
                Vector3 worldPosition = RectTransformToWorldPosition(m_slots[i]);

                if (cell.Item.View != null)
                {
                    cell.Item.View.DOMove(worldPosition, 0.3f)
                        .SetEase(Ease.OutQuad);
                }

                cell.transform.DOMove(worldPosition, 0.3f)
                    .SetEase(Ease.OutQuad);
            }
        }
    }

    private void CheckGameOverCondition()
    {
        int currentItemCount = PlayerSelectedItem.Count;

        if (currentItemCount != m_lastItemCount)
        {
            m_gameOverTimer = 0f;
            m_lastItemCount = currentItemCount;
        }

        if (currentItemCount == 5)
        {
            m_gameOverTimer += Time.deltaTime;

            if (m_gameOverTimer >= GAME_OVER_DELAY)
            {
                GameOver();
            }
        }
        else
        {
            m_gameOverTimer = 0f;
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Player has 5 items for 3 seconds without matching.");

        if (m_gameManager != null)
        {
            m_gameManager.GameOver();
        }
        else
        {
            Debug.LogWarning("GameManager not found! Cannot call GameOver.");
        }

        m_gameOverTimer = 0f;
    }

    private void CheckWinCondition()
    {
        if (playerCount == 21)
        {
            GameWinMenu.SetActive(true);
        }
    }

    public void ClearSelectedItems()
    {
        PlayerSelectedItem.Clear();
        m_gameOverTimer = 0f;
        m_lastItemCount = 0;
        playerCount = 0;
    }

    public int GetSelectedCount()
    {
        return PlayerSelectedItem.Count;
    }
}
