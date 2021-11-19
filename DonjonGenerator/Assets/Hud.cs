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
    public Text TimerText;
    public float timer;

    private void Awake()
    {
        Instance = this;
        ActivateTimer(70);
    }

    public void Update()
    {
        if (Player.Instance == null)
            return;
        if(TimerText.IsActive())
        {
            timer -= Time.deltaTime;
            if(timer<0)
                SceneManager.LoadScene("SampleScene");
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
}
