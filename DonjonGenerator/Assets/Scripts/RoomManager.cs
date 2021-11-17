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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
