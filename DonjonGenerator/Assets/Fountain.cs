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
                if (Player.Instance._state == Player.STATE.STUNNED)
                {
                    Player.Instance._currentMovement = saveStunnedSpeed;
                }
                else
                {
                    Player.Instance._currentMovement = saveDefaultSpeed;
                }
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
            saveDefaultSpeed = Player.Instance.defaultMovement;
            saveStunnedSpeed = Player.Instance.stunnedMovement;

            if (Player.Instance._state == Player.STATE.STUNNED)
            {
                Player.Instance._currentMovement = stunnedSpeedBoost;
            }
            else
            {
                Player.Instance._currentMovement = speedBoost;
            }
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
