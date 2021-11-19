using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountain : MonoBehaviour
{
    public Player.MovementParameters speedBoost;
    public Player.MovementParameters stunnedSpeedBoost;

    public float boostDuration = 6;
    private float currentDuration = 0;

    private Player.MovementParameters saveDefaultSpeed;
    private Player.MovementParameters saveStunnedSpeed;

    private bool boostActivated = false;
    private bool canTakeLife = true;

    private void Update()
    {
        if (boostActivated)
        {
            currentDuration = Mathf.Min(boostDuration, currentDuration + Time.deltaTime);
            if (currentDuration == boostDuration)
            {
                Player.Instance.defaultMovement = saveDefaultSpeed;
                Player.Instance.stunnedMovement = saveStunnedSpeed;
                boostActivated = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            saveDefaultSpeed = new Player.MovementParameters(Player.Instance.defaultMovement);
            saveStunnedSpeed = new Player.MovementParameters(Player.Instance.stunnedMovement);

            Player.Instance.defaultMovement = speedBoost;
            Player.Instance.stunnedMovement = stunnedSpeedBoost;

            if (canTakeLife)
            {
                Player.Instance.life++;
                canTakeLife = false;
            }

            currentDuration = 0;
            boostActivated = true;
        }
    }
}
