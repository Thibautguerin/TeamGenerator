using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    public static Hud Instance = null;

    public RectTransform heartBar;
    public GameObject heartPrefab;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public Text TimerText;
    public float timer;
    private bool stopTimer = false;

    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        if (Player.Instance == null)
            return;
        if(TimerText.IsActive() && !stopTimer)
        {
            timer -= Time.deltaTime;
            if (timer < 0) {
                Attack attack = new Attack();
                attack.damages = 10;
                Player.Instance.ApplyHit(attack);
            }
            UpdateTimerText();
        }
        while (heartBar.childCount < Player.Instance.life && Player.Instance.life > 0)
        {
            AddHearth();
        }
        while (Player.Instance.life < heartBar.childCount && Player.Instance.life > 0)
        {
            RemoveHearth();
        }
    }

    public void AddHearth()
    {
        GameObject heart = GameObject.Instantiate(heartPrefab);
        heart.transform.SetParent(heartBar);
    }

    public void RemoveHearth()
    {
        if (heartBar.childCount == 0)
            return;
        Transform heart = heartBar.GetChild(0);
        heart.SetParent(null);
        GameObject.Destroy(heart.gameObject);
    }

    public void ActivateTimer(float time)
    {
        TimerText.gameObject.SetActive(true);
        timer = time;
        UpdateTimerText();
    }

    public void UpdateTimerText()
    {
        int second = (int)timer;
        int minute = second / 60;
        second %= 60;
        string middle = second >= 10 ? ":" : ":0";
        TimerText.text = minute + middle + second;
    }
    public void Victory()
    {
        victoryPanel.SetActive(true);
        stopTimer = true;
    }
    public void Defeat()
    {
        defeatPanel.SetActive(true);
        stopTimer = true;
    }

    public void NewLevel()
    {
        Destroy(DonjonGenerator.Instance.gameObject);
        SceneManager.LoadScene("SampleScene");
    }

    public void Restart()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
