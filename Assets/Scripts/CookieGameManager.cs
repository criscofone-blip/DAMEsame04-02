using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CookieGameManager : MonoBehaviour
{
    [Header("Cookies")]
    public float cookies = 0;
    public float cookiesPerSecond = 0;

    [Header("Click Settings")]
    public float cookiesPerClick = 1;

    [Header("Raycast")]
    public LayerMask cookieLayerMask;

    [Header("References")]
    public Transform cookieTransform;
    public Animator cookieAnimator;
    public TMP_Text cookiesText;
    public TMP_Text perSecondText;

    [Header("Cursor Upgrade")]
    public int cursorsOwned = 0;
    public int maxCursors = 20;

    [Header("Cursor Rate")]
    public float cursorClicksPerSecond = 0.4f;

    public int cursorBasePrice = 20;
    public float cursorPriceMultiplier = 1.15f;

    [Header("Cursor Visual")]
    public GameObject cursorPrefab;
    public float cursorOrbitRadius = 2.2f;
    public float cursorOrbitSpeed = 90f;
    public float cursorsCenterAngle = 90f;
    public float cursorsAngleSpacing = 12f;

    [Header("UI - Shop")]
    public TMP_Text cursorPriceText;
    public Button buyCursorButton;

    [Header("Win")]
    public int winTarget = 400;
    public Button winButton;
    public GameObject youWinPanel;

    float uiRefreshTimer = 0;
    float cursorTickTimer = 0;
    bool hasWon = false;

    List<CursorAutoClick> spawnedCursors = new List<CursorAutoClick>();

    void Awake()
    {
        if (youWinPanel != null)
            youWinPanel.SetActive(false);

        RecalculateCookiesPerSecond();
        RefreshUI();
        RefreshShopUI();
        RefreshWinUI();
    }

    void Update()
    {
        if (hasWon)
            return;

        if (cursorsOwned > 0)
        {
            float interval = 1f / cursorClicksPerSecond;

            cursorTickTimer += Time.deltaTime;

            while (cursorTickTimer >= interval)
            {
                cursorTickTimer -= interval;

                cookies += cursorsOwned * cookiesPerClick;

                FlashAllCursors();
            }
        }

        uiRefreshTimer += Time.deltaTime;
        if (uiRefreshTimer >= 0.1f)
        {
            uiRefreshTimer = 0;
            RefreshUI();
            RefreshShopUI();
            RefreshWinUI();
        }

        bool clickedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
        if (!clickedThisFrame)
            return;

        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        mouseWorldPos.z = 0f;

        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos, cookieLayerMask);
        if (col == null)
            return;

        OnCookieClicked();
    }

    void OnCookieClicked()
    {
        cookies += cookiesPerClick;

        if (cookieAnimator != null)
            cookieAnimator.SetTrigger("Click");
        RefreshUI();
        RefreshShopUI();
        RefreshWinUI();
    }

    void FlashAllCursors()
    {
        for (int i = 0; i < spawnedCursors.Count; i++)
        {
            if (spawnedCursors[i] != null)
                spawnedCursors[i].FlashRed();
        }
    }

    void RecalculateCookiesPerSecond()
    {
        cookiesPerSecond = cursorsOwned * cursorClicksPerSecond * cookiesPerClick;
    }

    void RefreshUI()
    {
        int fullCookies = Mathf.FloorToInt(cookies);

        if (cookiesText != null)
            cookiesText.text = "Cookies:\n" + fullCookies;

        if (perSecondText != null)
            perSecondText.text = "Per second:\n" + cookiesPerSecond;
    }

    int GetCursorPrice()
    {
        float price = cursorBasePrice * Mathf.Pow(cursorPriceMultiplier, cursorsOwned);
        return Mathf.FloorToInt(price);
    }

    void RefreshShopUI()
    {
        if (cursorsOwned >= maxCursors)
        {
            if (buyCursorButton != null)
                buyCursorButton.gameObject.SetActive(false);

            if (cursorPriceText != null)
                cursorPriceText.gameObject.SetActive(false);
           return;
        }

        if (buyCursorButton != null)
            buyCursorButton.gameObject.SetActive(true);

        if (cursorPriceText != null)
            cursorPriceText.gameObject.SetActive(true);

        int price = GetCursorPrice();

        if (cursorPriceText != null)
            cursorPriceText.text = "+  " + price;

        if (buyCursorButton != null)
            buyCursorButton.interactable = (cookies >= price);
    }

    void RefreshWinUI()
    {
        if (winButton == null)
            return;

        winButton.interactable = (cookies >= winTarget);
    }

    public void WinGame()
    {
        if (hasWon)
            return;

        if (cookies < winTarget)
            return;

        hasWon = true;
        if (youWinPanel != null)
            youWinPanel.SetActive(true);
    }

    public void BuyCursor()
    {
        if (cursorsOwned >= maxCursors)
            return;

        int price = GetCursorPrice();
        if (cookies < price)
            return;
        cookies -= price;
        cursorsOwned++;

        RecalculateCookiesPerSecond();

        SpawnCursor();
        RepositionCursors();

        RefreshUI();
        RefreshShopUI();
        RefreshWinUI();
    }

    void SpawnCursor()
    {
        if (cursorPrefab == null)
            return;

        if (cookieTransform == null)
            return;

        GameObject obj = Instantiate(cursorPrefab, cookieTransform.position, Quaternion.identity);

        CursorAutoClick cursor = obj.GetComponent<CursorAutoClick>();
        if (cursor == null)
            cursor = obj.AddComponent<CursorAutoClick>();

        cursor.radius = cursorOrbitRadius;
        cursor.orbitSpeed = cursorOrbitSpeed;

        spawnedCursors.Add(cursor);
    }

    void RepositionCursors()
    {
        if (cookieTransform == null)
            return;

        int total = spawnedCursors.Count;
        if (total <= 0)
            return;

        float start = cursorsCenterAngle - ((total - 1) * cursorsAngleSpacing * 0.5f);

        for (int i = 0; i < total; i++)
        {
            float angle = start + (i * cursorsAngleSpacing);

            if (spawnedCursors[i] != null)
                spawnedCursors[i].Initialize(cookieTransform, angle);
        }
    }
}

