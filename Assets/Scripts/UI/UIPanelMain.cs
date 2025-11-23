using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;

    [SerializeField] private BoardController boardController;

    private UIMainManager m_mngr;

    private void Awake()
    {
        // Tìm BoardController nếu chưa được gán
        if (boardController == null)
        {
            boardController = FindObjectOfType<BoardController>();
        }

        // Kiểm tra null trước khi sử dụng
        if (btnMoves != null)
        {
            btnMoves.onClick.AddListener(OnClickMoves);
        }
        
        if (btnTimer != null)
        {
            btnTimer.onClick.AddListener(OnClickTimer);
        }
    }

    private void OnDestroy()
    {
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        if (btnTimer) btnTimer.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }
    public void AA1()
    {
        StartCoroutine(AA2());
    }

    public IEnumerator AA2()
    {
        yield return new WaitForSeconds(3f);
        BoardController.isStartGame = true;
    }




    private void OnClickTimer()
    {
        m_mngr.LoadLevelTimer();
    }

    private void OnClickMoves()
    {
        m_mngr.LoadLevelMoves();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
