using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class AutoGameManager : MonoBehaviour
{
    public Transform[] itemType1;
    public Transform[] itemType2;
    public Transform[] itemType3;
    public Transform[] itemType4;
    public Transform[] itemType5;
    public Transform[] itemType6;
    public Transform[] itemType7;

    // Mảng RectTransform tương ứng với các item type
    public RectTransform[] itemRectType1;
    public RectTransform[] itemRectType2;
    public RectTransform[] itemRectType3;
    public RectTransform[] itemRectType4;
    public RectTransform[] itemRectType5;
    public RectTransform[] itemRectType6;
    public RectTransform[] itemRectType7;

    private Dictionary<NormalItem.eNormalType, List<Transform>> m_itemTransforms;
    private Dictionary<NormalItem.eNormalType, List<RectTransform>> m_itemRectTransforms;
    
    private Canvas m_canvas;
    private Camera m_mainCamera;
    private bool m_isAutoWinRunning = false;
    private bool m_isAutoLoseRunning = false;

    private void Awake()
    {
        InitializeItemTransforms();
        InitializeCanvas();
    }

    private void InitializeCanvas()
    {
        m_canvas = FindObjectOfType<Canvas>();
        m_mainCamera = Camera.main;
    }

    private void InitializeItemTransforms()
    {
        m_itemTransforms = new Dictionary<NormalItem.eNormalType, List<Transform>>();
        m_itemRectTransforms = new Dictionary<NormalItem.eNormalType, List<RectTransform>>();
        
        // Khởi tạo các list cho từng loại item
        m_itemTransforms[NormalItem.eNormalType.TYPE_ONE] = new List<Transform>();
        m_itemTransforms[NormalItem.eNormalType.TYPE_TWO] = new List<Transform>();
        m_itemTransforms[NormalItem.eNormalType.TYPE_THREE] = new List<Transform>();
        m_itemTransforms[NormalItem.eNormalType.TYPE_FOUR] = new List<Transform>();
        m_itemTransforms[NormalItem.eNormalType.TYPE_FIVE] = new List<Transform>();
        m_itemTransforms[NormalItem.eNormalType.TYPE_SIX] = new List<Transform>();
        m_itemTransforms[NormalItem.eNormalType.TYPE_SEVEN] = new List<Transform>();

        m_itemRectTransforms[NormalItem.eNormalType.TYPE_ONE] = new List<RectTransform>();
        m_itemRectTransforms[NormalItem.eNormalType.TYPE_TWO] = new List<RectTransform>();
        m_itemRectTransforms[NormalItem.eNormalType.TYPE_THREE] = new List<RectTransform>();
        m_itemRectTransforms[NormalItem.eNormalType.TYPE_FOUR] = new List<RectTransform>();
        m_itemRectTransforms[NormalItem.eNormalType.TYPE_FIVE] = new List<RectTransform>();
        m_itemRectTransforms[NormalItem.eNormalType.TYPE_SIX] = new List<RectTransform>();
        m_itemRectTransforms[NormalItem.eNormalType.TYPE_SEVEN] = new List<RectTransform>();
    }

    public void ClearItemTransforms()
    {
        if (m_itemTransforms != null)
        {
            foreach (var list in m_itemTransforms.Values)
            {
                list.Clear();
            }
        }
        
        if (m_itemRectTransforms != null)
        {
            // Xóa các GameObject RectTransform đã tạo
            foreach (var list in m_itemRectTransforms.Values)
            {
                foreach (var rectTransform in list)
                {
                    if (rectTransform != null && rectTransform.gameObject != null)
                    {
                        Destroy(rectTransform.gameObject);
                    }
                }
                list.Clear();
            }
        }
    }

    public void RegisterItemTransform(NormalItem.eNormalType itemType, Transform itemTransform)
    {
        if (m_itemTransforms.ContainsKey(itemType) && itemTransform != null)
        {
            m_itemTransforms[itemType].Add(itemTransform);
        }
    }

    private void UpdateArrays()
    {
        itemType1 = m_itemTransforms[NormalItem.eNormalType.TYPE_ONE].ToArray();
        itemType2 = m_itemTransforms[NormalItem.eNormalType.TYPE_TWO].ToArray();
        itemType3 = m_itemTransforms[NormalItem.eNormalType.TYPE_THREE].ToArray();
        itemType4 = m_itemTransforms[NormalItem.eNormalType.TYPE_FOUR].ToArray();
        itemType5 = m_itemTransforms[NormalItem.eNormalType.TYPE_FIVE].ToArray();
        itemType6 = m_itemTransforms[NormalItem.eNormalType.TYPE_SIX].ToArray();
        itemType7 = m_itemTransforms[NormalItem.eNormalType.TYPE_SEVEN].ToArray();

        // Chuyển đổi sang RectTransform
        ConvertToRectTransforms();
        
        itemRectType1 = m_itemRectTransforms[NormalItem.eNormalType.TYPE_ONE].ToArray();
        itemRectType2 = m_itemRectTransforms[NormalItem.eNormalType.TYPE_TWO].ToArray();
        itemRectType3 = m_itemRectTransforms[NormalItem.eNormalType.TYPE_THREE].ToArray();
        itemRectType4 = m_itemRectTransforms[NormalItem.eNormalType.TYPE_FOUR].ToArray();
        itemRectType5 = m_itemRectTransforms[NormalItem.eNormalType.TYPE_FIVE].ToArray();
        itemRectType6 = m_itemRectTransforms[NormalItem.eNormalType.TYPE_SIX].ToArray();
        itemRectType7 = m_itemRectTransforms[NormalItem.eNormalType.TYPE_SEVEN].ToArray();
    }

    private void ConvertToRectTransforms()
    {
        if (m_canvas == null)
        {
            Debug.LogWarning("Canvas not found! Cannot convert to RectTransform.");
            return;
        }

        foreach (var kvp in m_itemTransforms)
        {
            NormalItem.eNormalType itemType = kvp.Key;
            List<Transform> transforms = kvp.Value;
            List<RectTransform> rectTransforms = m_itemRectTransforms[itemType];

            foreach (Transform worldTransform in transforms)
            {
                if (worldTransform == null) continue;

                // Tạo GameObject mới với RectTransform
                GameObject rectObject = new GameObject($"Rect_{itemType}_{rectTransforms.Count}");
                RectTransform rectTransform = rectObject.AddComponent<RectTransform>();
                
                // Đặt parent là Canvas
                rectTransform.SetParent(m_canvas.transform, false);
                
                // Chuyển đổi vị trí từ world position sang UI anchored position
                Vector2 anchoredPosition = WorldToRectTransformPosition(worldTransform.position);
                rectTransform.anchoredPosition = anchoredPosition;
                
                // Thiết lập kích thước và anchor
                rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                
                // Thêm Image (EventTrigger không cần thiết vì đang click trực tiếp vào Cell)
                Image image = rectObject.AddComponent<Image>();
                image.color = new Color(1, 1, 1, 0.01f); // Gần như trong suốt
                
                rectTransforms.Add(rectTransform);
            }
        }
    }

    private Vector2 WorldToRectTransformPosition(Vector3 worldPosition)
    {
        if (m_canvas == null) return Vector2.zero;

        RectTransform canvasRect = m_canvas.GetComponent<RectTransform>();
        Vector2 screenPoint = Vector2.zero;

        // Chuyển đổi world position sang screen position
        if (m_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
        }
        else if (m_canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Camera cameraToUse = m_canvas.worldCamera != null ? m_canvas.worldCamera : m_mainCamera;
            if (cameraToUse != null)
            {
                screenPoint = RectTransformUtility.WorldToScreenPoint(cameraToUse, worldPosition);
            }
        }
        else if (m_canvas.renderMode == RenderMode.WorldSpace)
        {
            // World space canvas - chuyển đổi trực tiếp
            Vector3 localPos = canvasRect.InverseTransformPoint(worldPosition);
            return new Vector2(localPos.x, localPos.y);
        }

        // Chuyển screen position sang anchored position trong Canvas
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            screenPoint, 
            m_canvas.renderMode == RenderMode.ScreenSpaceCamera ? (m_canvas.worldCamera != null ? m_canvas.worldCamera : m_mainCamera) : null, 
            out localPoint);

        return localPoint;
    }

    public void FinalizeItemRegistration()
    {
        UpdateArrays();
    }

    public void AutoWin()
    {
        if (m_isAutoWinRunning)
        {
            Debug.LogWarning("AutoWin is already running!");
            return;
        }

        StartCoroutine(AutoWinCoroutine());
    }

    private IEnumerator AutoWinCoroutine()
    {
        m_isAutoWinRunning = true;
        
        // Danh sách tất cả các mảng Transform (world position của items)
        Transform[][] allTransformArrays = new Transform[][]
        {
            itemType1,
            itemType2,
            itemType3,
            itemType4,
            itemType5,
            itemType6,
            itemType7
        };

        // Duyệt qua từng mảng
        foreach (Transform[] array in allTransformArrays)
        {
            if (array == null) continue;

            // Duyệt qua từng phần tử trong mảng
            foreach (Transform itemTransform in array)
            {
                if (itemTransform == null) continue;

                // Click vào Cell tương ứng với item này
                ClickOnCell(itemTransform);

                // Đợi 1 giây trước khi click phần tử tiếp theo
                yield return new WaitForSeconds(1f);
            }
        }

        m_isAutoWinRunning = false;
        Debug.Log("AutoWin completed!");
    }

    public void AutoLose()
    {
        if (m_isAutoLoseRunning)
        {
            Debug.LogWarning("AutoLose is already running!");
            return;
        }

        StartCoroutine(AutoLoseCoroutine());
    }

    private IEnumerator AutoLoseCoroutine()
    {
        m_isAutoLoseRunning = true;
        
        // Danh sách 5 mảng Transform đầu tiên (world position của items)
        Transform[][] firstFiveArrays = new Transform[][]
        {
            itemType1,
            itemType2,
            itemType3,
            itemType4,
            itemType5
        };

        // Duyệt qua từng mảng và chỉ bấm vào phần tử đầu tiên
        foreach (Transform[] array in firstFiveArrays)
        {
            if (array == null || array.Length == 0) continue;

            // Chỉ lấy phần tử đầu tiên
            Transform firstItemTransform = array[0];
            
            if (firstItemTransform != null)
            {
                // Click vào Cell tương ứng với item này
                ClickOnCell(firstItemTransform);

                // Đợi 1 giây trước khi click phần tử tiếp theo
                yield return new WaitForSeconds(1f);
            }
        }

        m_isAutoLoseRunning = false;
        Debug.Log("AutoLose completed!");
    }

    private void ClickOnCell(Transform itemTransform)
    {
        if (itemTransform == null || m_mainCamera == null) return;

        // Tìm Cell từ itemTransform
        // Item.View là Transform của item, cần tìm Cell tương ứng
        Cell targetCell = FindCellFromItemTransform(itemTransform);
        
        if (targetCell == null)
        {
            Debug.LogWarning($"Cell not found for item {itemTransform.name}");
            return;
        }

        // Simulate click vào Cell bằng cách gọi GamePlayManager
        GamePlayManager gamePlayManager = FindObjectOfType<GamePlayManager>();
        if (gamePlayManager != null)
        {
            // Chuyển đổi world position sang screen position để simulate click
            Vector3 worldPos = targetCell.transform.position;
            Vector3 screenPos = m_mainCamera.WorldToScreenPoint(worldPos);
            
            // Tạo raycast để tìm Cell
            RaycastHit2D hit = Physics2D.Raycast(m_mainCamera.ScreenToWorldPoint(screenPos), Vector2.zero);
            
            if (hit.collider != null)
            {
                Cell clickedCell = hit.collider.GetComponent<Cell>();
                if (clickedCell != null && clickedCell == targetCell && !clickedCell.IsEmpty)
                {
                    // Gọi logic click của game thông qua GamePlayManager
                    SimulateCellClick(clickedCell, gamePlayManager);
                    Debug.Log($"Clicked on Cell at position {clickedCell.transform.position}");
                }
            }
        }
        else
        {
            Debug.LogWarning("GamePlayManager not found!");
        }
    }

    private Cell FindCellFromItemTransform(Transform itemTransform)
    {
        if (itemTransform == null) return null;

        // Tìm Cell bằng cách:
        // 1. Kiểm tra xem itemTransform có phải là child của Cell không
        Transform parent = itemTransform.parent;
        while (parent != null)
        {
            Cell cell = parent.GetComponent<Cell>();
            if (cell != null && !cell.IsEmpty)
            {
                return cell;
            }
            parent = parent.parent;
        }

        // 2. Nếu không tìm thấy, tìm Cell gần nhất với vị trí của item
        Vector3 itemPosition = itemTransform.position;
        Cell[] allCells = FindObjectsOfType<Cell>();
        
        Cell closestCell = null;
        float closestDistance = float.MaxValue;
        
        foreach (Cell cell in allCells)
        {
            if (cell == null || cell.IsEmpty) continue;
            
            float distance = Vector3.Distance(cell.transform.position, itemPosition);
            if (distance < closestDistance && distance < 0.5f) // Cho phép sai số nhỏ
            {
                closestDistance = distance;
                closestCell = cell;
            }
        }
        
        return closestCell;
    }

    private void SimulateCellClick(Cell cell, GamePlayManager gamePlayManager)
    {
        // Gọi public method ProcessCellClick trong GamePlayManager
        gamePlayManager.ProcessCellClick(cell);
    }


    
}
