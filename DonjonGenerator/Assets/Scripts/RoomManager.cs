using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public List<Room> Rooms = new List<Room>();
    private List<Room> StartRooms = new List<Room>();
    private List<Room> NormalRooms = new List<Room>();
    private List<Room> KeyRooms = new List<Room>();
    private List<Room> BossRooms = new List<Room>();

    private void Awake()
    {
        foreach (var item in Rooms)
        {
            switch (item.type)
            {
                case RoomType.START:
                    StartRooms.Add(item);
                    break;
                case RoomType.NORMAL:
                    NormalRooms.Add(item);
                    break;
                case RoomType.KEY:
                    KeyRooms.Add(item);
                    break;
                case RoomType.BOSS:
                    BossRooms.Add(item);
                    break;
            }
        }
    }

    public Room FindRoomfromNode(RoomNode node)
    {
        List<Room> possibleRoom = new List<Room>();
        List<Room> selectedRoom = NormalRooms;
        switch (node.roomType)
        {
            case RoomType.START:
                selectedRoom = StartRooms;
                break;
            case RoomType.KEY:
                selectedRoom = KeyRooms;
                break;
            case RoomType.BOSS:
                selectedRoom = BossRooms;
                break;
        }
        foreach (Room room in selectedRoom)
        {
            if (room.CheckDoor(node.doors))
                possibleRoom.Add(room);
        }
        int r = Random.Range(0, possibleRoom.Count - 1);
        Room instRoom = Instantiate(possibleRoom[r]);
        instRoom.InitDoor(node.doorsState);
        return instRoom;
    }
}
