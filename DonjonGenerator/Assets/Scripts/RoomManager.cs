using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public List<Room> Rooms = new List<Room>();
    public float closePercent = 0.8f;
    private List<Room> StartRooms = new List<Room>();
    private List<Room> NormalRooms = new List<Room>();
    private List<Room> KeyRooms = new List<Room>();
    private List<Room> BossRooms = new List<Room>();
    private List<Room> HardRooms = new List<Room>();
    private List<Room> SecretRooms = new List<Room>();
    private List<Room> ComeBackRooms = new List<Room>();
    [HideInInspector]
    public float timeSum = 0;

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
                case RoomType.HARDROOM:
                    HardRooms.Add(item);
                    break;
                case RoomType.SECRET:
                    SecretRooms.Add(item);
                    break;
                case RoomType.COMEBACK:
                    ComeBackRooms.Add(item);
                    break;
            }
        }
        //RoomNode room = new RoomNode();
        //room.roomType = RoomType.NORMAL;
        //room.doors = 1 << 0 + 1 << 2;
        //room.doorsState = new List<Door.STATE>(4)
        //{Door.STATE.CLOSED, Door.STATE.CLOSED, Door.STATE.CLOSED, Door.STATE.CLOSED};
        //FindRoomfromNode(room);
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
            case RoomType.HARDROOM:
                selectedRoom = HardRooms;
                break;
            case RoomType.SECRET:
                selectedRoom = SecretRooms;
                break;
            case RoomType.COMEBACK:
                selectedRoom = ComeBackRooms;
                break;
        }
        foreach (Room room in selectedRoom)
        {
            if (room.CheckDoor(node.doors))
                possibleRoom.Add(room);
        }
        if (possibleRoom.Count == 0)
            Debug.LogError("Room not found door : "+ node.doors+" type : "+node.roomType);
        int r = DonjonGenerator.Instance.random.Next(0, possibleRoom.Count - 1);
        Room instRoom = Instantiate(possibleRoom[r]);
        instRoom.InitDoor(node.doorsState);
        instRoom.isComeBack = node.isComeBackPath;
        if (node.isComeBackPath)
            timeSum += instRoom.time;
        return instRoom;
    }

    public void UpdateAllRoom()
    {
        foreach (var item in Room.allRooms)
        {
            if (item.isComeBack)
                item.UpdateRoomAfterBoss(false);
            else
            {
                item.UpdateRoomAfterBoss(Random.value < closePercent);
            }
        }
        Hud.Instance.ActivateTimer(timeSum);
    }
}
