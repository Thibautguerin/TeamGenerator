using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : MonoBehaviour {

    public enum STATE {
        OPEN = 0,
        CLOSED = 1,
        WALL = 2,
        SECRET = 3,
        WEAKENED = 4,
        BLOCKED = 5
    }

    public const string PLAYER_NAME = "Player";

    Utils.ORIENTATION _orientation = Utils.ORIENTATION.NONE;
	public Utils.ORIENTATION Orientation { get { return _orientation; } }

	STATE _state = STATE.OPEN;
	public STATE State { get { return _state; } }
	public GameObject closedGo = null;
    public GameObject openGo = null;
    public GameObject wallGo = null;
    public GameObject secretGo = null;
    public GameObject weakenedGo = null;
    public GameObject blockedGO = null;

	private Room _room = null;

	public void Awake()
	{
		_room = GetComponentInParent<Room>();
	}

	public void Start()
    {
        
	}

    public void SetRotation(Utils.ORIENTATION orientation)
    {
        _orientation = orientation;
        transform.rotation = Quaternion.Euler(0, 0, -Utils.OrientationToAngle(orientation));
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent != Player.Instance.gameObject.transform)
            return;

        switch (_state) {
            case STATE.CLOSED:
                if (Player.Instance.KeyCount > 0)
                {
                    Player.Instance.KeyCount--;
                    SetState(STATE.OPEN);
					Room nextRoom = GetNextRoom();
					if(nextRoom)
					{
						Door[] doors = nextRoom.GetComponentsInChildren<Door>(true);
						foreach(Door door in doors)
						{
							if (_orientation == Utils.OppositeOrientation(door.Orientation) && door._state == STATE.CLOSED)
							{
								door.SetState(STATE.OPEN);
							}
						}
					}
				}
                break;
        }
    }

	private Room GetNextRoom()
	{
		Vector2Int dir = Utils.OrientationToDir(_orientation);
		Room nextRoom = Room.allRooms.Find(x => x.position == _room.position + dir/* * new Vector2Int(11, 9)*/);
		return nextRoom;
	} 

    public void SetState(STATE state)
    {
        if (closedGo) { closedGo.SetActive(false); }
        if (openGo) { openGo.SetActive(false); }
        if (wallGo) { wallGo.SetActive(false); }
        if (secretGo) { secretGo.SetActive(false); }
        if (weakenedGo) { weakenedGo.SetActive(false); }
        if (blockedGO) blockedGO.SetActive(false);
        _state = state;
        switch(_state)
        {
            case STATE.CLOSED:
                if (closedGo) { closedGo.SetActive(true); }
                break;
            case STATE.OPEN:
                if (openGo) { openGo.SetActive(true); }
                break;
            case STATE.WALL:
                if (wallGo) { wallGo.SetActive(true); }
                break;
            case STATE.SECRET:
                if (secretGo) { secretGo.SetActive(true); }
                break;
            case STATE.WEAKENED:
                if (weakenedGo) { weakenedGo.SetActive(true); }
                break;
            case STATE.BLOCKED:
                if (blockedGO) blockedGO.SetActive(true);
                break;
        }
    }

}
