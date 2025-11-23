using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static bool isStartGame = false;
    public GameObject GameWinMenu;
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;

    private bool m_gameWon;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        if (m_potentialMatch == null)
        {
            m_potentialMatch = new List<Cell>();
        }

        m_gameWon = false;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }



    private void Fill()
    {
        // Clear dữ liệu cũ trước khi tạo mới
        AutoGameManager autoGameManager = GameObject.FindObjectOfType<AutoGameManager>();
        if (autoGameManager != null)
        {
            autoGameManager.ClearItemTransforms();
        }
        
        m_board.Fill();
        
        // Hoàn tất việc đăng ký Transform của các item vào AutoGameManager
        if (autoGameManager != null)
        {
            autoGameManager.FinalizeItemRegistration();
        }
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                StopHints();
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;
    }

    internal void Clear()
    {
        m_board.Clear();
    }

    public int GetRemainingItemCount(List<Cell> excludedCells = null)
    {
        if (m_board == null) return 0;
        return m_board.GetRemainingItemCount(excludedCells);
    }

    

    private void ShowHint()
    {
        m_hintIsShown = true;
        
        if (m_potentialMatch != null)
        {
            foreach (var cell in m_potentialMatch)
            {
                if (cell != null)
                {
                    cell.AnimateItemForHint();
                }
            }
        }
    }

    private void StopHints()
    {
        m_hintIsShown = false;
        
        if (m_potentialMatch != null)
        {
            foreach (var cell in m_potentialMatch)
            {
                if (cell != null)
                {
                    cell.StopHintAnimation();
                }
            }

            m_potentialMatch.Clear();
        }
    }
}
