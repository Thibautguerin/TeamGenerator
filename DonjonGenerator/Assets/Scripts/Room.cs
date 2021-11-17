using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public bool isStartRoom = false;
	public Vector2Int position = Vector2Int.zero;

	private TilemapGroup _tilemapGroup;

	public static List<Room> allRooms = new List<Room>();

	public RoomType type;
	public DoorsInfo info;
	private uint canBit =0;
	private uint mustBit=0;
	private List<Door> doorList = new List<Door>();


    void Awake()
    {
		_tilemapGroup = GetComponentInChildren<TilemapGroup>();
		allRooms.Add(this);
	}

	void SetupDoor()
    {
		Door[] doors = GetComponentsInChildren<Door>();
		for (int i = 0; i < 4; i++)
		{
			if ((canBit & 1 << i) == 0)
				continue;
			Door doorToAdd = doors[0];
			for (int j = 1; j < 4; j++)
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
						if (doors[j].transform.position.y > doorToAdd.transform.position.x)
							doorToAdd = doors[j];
						break;
					case 3:
						if (doors[j].transform.position.y < doorToAdd.transform.position.x)
							doorToAdd = doors[j];
						break;
				}
			}
			doorList.Add(doorToAdd);
		}
	}

	void SetupBit()
    {
		canBit += (uint)(info.leftDoors.canBeDoor ? 1 : 0) << 0;
		canBit += (uint)(info.rightDoors.canBeDoor ? 1 : 0) << 1;
		canBit += (uint)(info.upDoors.canBeDoor ? 1 : 0) << 2;
		canBit += (uint)(info.bottomDoors.canBeDoor ? 1 : 0) << 3;
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
