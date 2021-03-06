using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour {

    public bool isStartRoom = false;
	public Vector2Int position = Vector2Int.zero;
	public float time;

	private TilemapGroup _tilemapGroup;

	public static List<Room> allRooms = new List<Room>();

	public RoomType type;
	public DoorsInfo info;
	private uint canBit =0;
	private uint mustBit=0;
	private List<Door> doorList = new List<Door>();
	[HideInInspector]
	public bool isComeBack = false;
	private bool bossKilled = false;


    void Awake()
    {
		_tilemapGroup = GetComponentInChildren<TilemapGroup>();
		allRooms.Add(this);
	}

	void SetupDoor()
    {
		List<Door> doors = GetComponentsInChildren<Door>().ToList();
		for (int i = 0; i < 4; i++)
		{
			if ((canBit & 1 << i) == 0)
				continue;
			Door doorToAdd = doors[0];
			for (int j = 1; j < doors.Count; j++)
			{
				switch (i)
				{
					case 0:
						if (doors[j].transform.position.x < doorToAdd.transform.position.x)
							doorToAdd = doors[j];
						break;
					case 1:
						if (doors[j].transform.position.x > doorToAdd.transform.position.x)
							doorToAdd = doors[j];
						break;
					case 2:
						if (doors[j].transform.position.y > doorToAdd.transform.position.y)
							doorToAdd = doors[j];
						break;
					case 3:
						if (doors[j].transform.position.y < doorToAdd.transform.position.y)
							doorToAdd = doors[j];
						break;
				}
			}
			Utils.ORIENTATION orientation = Utils.ORIENTATION.NONE;
            switch (i)
            {
				case 0:
					orientation = Utils.ORIENTATION.WEST;
					break;
				case 1:
					orientation = Utils.ORIENTATION.EAST;
					break;
				case 2:
					orientation = Utils.ORIENTATION.NORTH;
					break;
				case 3:
					orientation = Utils.ORIENTATION.SOUTH;
					break;
            }
			doorToAdd.SetRotation(orientation);
			doorList.Add(doorToAdd);
			doors.Remove(doorToAdd);
		}
	}

	void SetupBit()
    {
		canBit = 0;
		canBit += (uint)(info.leftDoors.canBeDoor ? 1 : 0) << 0;
		canBit += (uint)(info.rightDoors.canBeDoor ? 1 : 0) << 1;
		canBit += (uint)(info.upDoors.canBeDoor ? 1 : 0) << 2;
		canBit += (uint)(info.bottomDoors.canBeDoor ? 1 : 0) << 3;
		mustBit = 0;
		mustBit += (uint)(info.leftDoors.mustBeDoor ? 1 : 0) << 0;
		mustBit += (uint)(info.rightDoors.mustBeDoor ? 1 : 0) << 1;
		mustBit += (uint)(info.upDoors.mustBeDoor ? 1 : 0) << 2;
		mustBit += (uint)(info.bottomDoors.mustBeDoor ? 1 : 0) << 3;
	}

	private void OnDestroy()
	{
		allRooms.Remove(this);
	}

	void Start () {
        if(isStartRoom)
        {
            OnEnterRoom();
        }
    }
	
	public void OnEnterRoom()
    {
		if (bossKilled && isStartRoom)
			Hud.Instance.Victory();
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        Bounds cameraBounds = GetWorldRoomBounds();
        cameraFollow.SetBounds(cameraBounds);
		Player.Instance.EnterRoom(this);
    }


	public Bounds GetLocalRoomBounds()
    {
		Bounds roomBounds = new Bounds(Vector3.zero, Vector3.zero);
		if (_tilemapGroup == null)
			return roomBounds;

		foreach (STETilemap tilemap in _tilemapGroup.Tilemaps)
		{
			Bounds bounds = tilemap.MapBounds;
			roomBounds.Encapsulate(bounds);
		}
		return roomBounds;
    }

    public Bounds GetWorldRoomBounds()
    {
        Bounds result = GetLocalRoomBounds();
        result.center += transform.position;
        return result;
    }

	public bool Contains(Vector3 position)
	{
		position.z = 0;
		return (GetWorldRoomBounds().Contains(position));
	}

	public bool CheckDoor(uint doorBit)
    {
		SetupBit();

		if ((mustBit & doorBit) == mustBit && (canBit & doorBit) == doorBit)
			return true;
		return false;
    }

	public void InitDoor(List<Door.STATE> doorsState)
    {
		SetupBit();
		SetupDoor();

		int doorIndex = 0;
		for (int i = 0; i < doorsState.Count; i++)
        {
			if ((canBit & 1 << i) == 0)
				continue;
			doorList[doorIndex].SetState(doorsState[i]);
			doorIndex++;
		}
    }

	public void UpdateRoomAfterBoss(bool shouldClose)
    {
		bossKilled = true;
        foreach (var item in doorList)
        {
			if (shouldClose && item.State == Door.STATE.OPEN)
				item.SetState(Door.STATE.BLOCKED);
			if (item.State == Door.STATE.WEAKENED)
				item.SetState(Door.STATE.OPEN);
			if (shouldClose && item.State == Door.STATE.CLOSED)
				item.SetState(Door.STATE.OPEN);
        }
    }

	[System.Serializable]
	public struct DoorsInfo
    {
		public doorInfo upDoors;
		public doorInfo leftDoors;
		public doorInfo bottomDoors;
		public doorInfo rightDoors;
	}

	[System.Serializable]
	public struct doorInfo
    {
		public bool canBeDoor;
		public bool mustBeDoor;
    }
}
