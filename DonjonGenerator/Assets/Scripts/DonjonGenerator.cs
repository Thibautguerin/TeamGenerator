using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorPosition
{
    LEFT,
    RIGHT,
    TOP,
    BOTTOM
}

// Doors position bitset
//1 << 0 = LEFT
//1 << 1 = RIGHT
//1 << 2 = TOP
//1 << 3 = BOTTOM

public enum PathType
{
    PRINCIPAL,
    SECONDARY,
    ALTERNATIVE,
    SECRET
}

public enum RoomType
{
    START,
    NORMAL,
    KEY,
    BOSS
}

public class RoomNode
{
    public PathType pathType = PathType.PRINCIPAL;
    public RoomType roomType = RoomType.NORMAL;

    public Vector2Int position;
    public uint doors;
    public uint lastdoorAdded;
    public List<Door.STATE> doorsState = new List<Door.STATE>(4)
        {Door.STATE.WALL, Door.STATE.WALL, Door.STATE.WALL, Door.STATE.WALL};

    public LinkedListNode<RoomNode> nextSecondary;

    public LinkedListNode<RoomNode> previousComeBack;
    public LinkedListNode<RoomNode> nextComeBack;

    public void AddDoor(DoorPosition doorPosition)
    {
        if ((doors & 1 << (int)doorPosition) != 1 << (int)doorPosition)
        {
            doors += (uint)(1 << (int)doorPosition);
            lastdoorAdded = (uint)(1 << (int)doorPosition);
        }
    }

    public void RemoveDoor(DoorPosition doorPosition)
    {
        if ((doors & 1 << (int)doorPosition) == 1 << (int)doorPosition)
        {
            doors -= (uint)(1 << (int)doorPosition);
        }
    }

    public void RemoveLastDoorAdded()
    {
        if ((doors & lastdoorAdded) == lastdoorAdded)
        {
            doors -= lastdoorAdded;
        }
    }

    public void ChangeDoorState(DoorPosition doorPosition, Door.STATE newDoorState)
    {
        doorsState[(int)doorPosition] = newDoorState;
    }

    //public Room defaultRoom;
    //public Room modifiedRoom;
}

public class DonjonGenerator : MonoBehaviour
{
    public int nbDoors = 2;
    [Header("After Start")]
    public int nbRoomsAfterStartMin = 4;
    public int nbRoomsAfterStartMax = 6;
    [Header("Principal Paths")]
    public int nbRoomsBetweenDoorsMin = 3;
    public int nbRoomsBetweenDoorsMax = 5;
    [Header("Secondary Paths")]
    public int nbRoomsSecondaryPathMin = 2;
    public int nbRoomsSecondaryPathMax = 3;
    [Header("Before Boss")]
    public int nbRoomsBeforeBossMin = 4;
    public int nbRoomsBeforeBossMax = 6;
    

    LinkedList<RoomNode> primaryRooms = new LinkedList<RoomNode>();
    List<LinkedList<RoomNode>> secondaryRooms = new List<LinkedList<RoomNode>>();
    List<LinkedListNode<RoomNode>> comeBackRooms = new List<LinkedListNode<RoomNode>>();
    LinkedListNode<RoomNode> secretRoom;

    Dictionary<Vector2Int, LinkedListNode<RoomNode>> roomsDico = new Dictionary<Vector2Int, LinkedListNode<RoomNode>>();

    private void Awake()
    {
        nbRoomsAfterStartMax++;
        nbRoomsBetweenDoorsMax++;
        nbRoomsSecondaryPathMax++;
        nbRoomsBeforeBossMax++;
    }

    private void Start()
    {
        Generate();
        DisplayDonjon();
    }

    public void Generate()
    {
        primaryRooms.Clear();
        secondaryRooms.Clear();
        roomsDico.Clear();
        comeBackRooms.Clear();
        secretRoom = null;

        int currentNbDoors = 0;
        int nbRooms = Random.Range(nbRoomsAfterStartMin, nbRoomsAfterStartMax);
        int currentNbRooms = 0;

        bool noError = true;

        GenerateFirstRoom(ref currentNbDoors, ref nbRooms, ref currentNbRooms);
        
        while (currentNbDoors <= nbDoors && noError)
        {
            noError = GeneratePrincipalRoom(ref currentNbDoors, ref nbRooms, ref currentNbRooms);
        }

        if (noError)
        {
            noError = GenerateSecondaryPath();
            if (noError)
            {
                noError = GenerateComeBackPath();
                if (noError)
                {
                    noError = GenerateSecretRoom();
                }
            }
        }

        if (!noError)
        {
            Debug.Log("<color=red>FAIL: Restart Generation !</color>");
            Generate();
        }
    }

    public void GenerateFirstRoom(ref int currentNbDoors, ref int nbRoomsBetweenDoors, ref int currentNbRoomsBetweenDoors)
    {
        RoomNode newRoom = new RoomNode();

        newRoom.pathType = PathType.PRINCIPAL;
        newRoom.roomType = RoomType.START;
        newRoom.position = new Vector2Int(0, 0);

        currentNbRoomsBetweenDoors++;

        primaryRooms.AddLast(newRoom);
        roomsDico.Add(newRoom.position, primaryRooms.Last);
    }

    public bool GeneratePrincipalRoom(ref int currentNbDoors, ref int nbRoomsBetweenDoors, ref int currentNbRoomsBetweenDoors)
    {
        bool positionUsed = true;
        int nbIter = 0;
        DoorPosition randomDirection = DoorPosition.BOTTOM;
        Vector2Int newPosition = new Vector2Int();

        while (positionUsed && nbIter <= 20)
        {
            randomDirection = (DoorPosition)Random.Range(0, 4);
            switch (randomDirection)
            {
                case DoorPosition.LEFT:
                    newPosition = primaryRooms.Last.Value.position - new Vector2Int(1, 0);
                    if (roomsDico.ContainsKey(newPosition))
                    {
                        nbIter++;
                        continue;
                    }
                    break;
                case DoorPosition.RIGHT:
                    newPosition = primaryRooms.Last.Value.position + new Vector2Int(1, 0);
                    if (roomsDico.ContainsKey(newPosition))
                    {
                        nbIter++;
                        continue;
                    }
                    break;
                case DoorPosition.TOP:
                    newPosition = primaryRooms.Last.Value.position + new Vector2Int(0, 1);
                    if (roomsDico.ContainsKey(newPosition))
                    {
                        nbIter++;
                        continue;
                    }
                    break;
                case DoorPosition.BOTTOM:
                    newPosition = primaryRooms.Last.Value.position - new Vector2Int(0, 1);
                    if (roomsDico.ContainsKey(newPosition))
                    {
                        nbIter++;
                        continue;
                    }
                    break;
                default:
                    break;
            }
            positionUsed = false;
        }
        if (positionUsed)
        {
            // Recommencer la génération
            return false;
        }

        RoomNode newRoom = new RoomNode();
        
        newRoom.position = newPosition;
        newRoom.pathType = PathType.PRINCIPAL;

        primaryRooms.Last.Value.AddDoor(randomDirection);
        newRoom.AddDoor(GetInverseDoorPosition(randomDirection));

        if (currentNbRoomsBetweenDoors >= nbRoomsBetweenDoors && currentNbDoors < nbDoors)
        {
            // Mettre une porte
            primaryRooms.Last.Value.ChangeDoorState(randomDirection, Door.STATE.CLOSED);
            newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.CLOSED);

            currentNbRoomsBetweenDoors = 0;
            currentNbDoors++;
            if (currentNbDoors < nbDoors)
                nbRoomsBetweenDoors = Random.Range(nbRoomsBetweenDoorsMin, nbRoomsBetweenDoorsMax);
            else
                nbRoomsBetweenDoors = Random.Range(nbRoomsBeforeBossMin, nbRoomsBeforeBossMax);
        }
        else if (currentNbRoomsBetweenDoors >= nbRoomsBetweenDoors && currentNbDoors == nbDoors)
        {
            // Mettre le Boss
            newRoom.roomType = RoomType.BOSS;
            primaryRooms.Last.Value.ChangeDoorState(randomDirection, Door.STATE.OPEN);
            newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.OPEN);

            currentNbRoomsBetweenDoors = 0;
            nbRoomsBetweenDoors = Random.Range(nbRoomsBetweenDoorsMin, nbRoomsBetweenDoorsMax);
            currentNbDoors++;
        }
        else
        {
            primaryRooms.Last.Value.ChangeDoorState(randomDirection, Door.STATE.OPEN);
            newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.OPEN);
        }
        currentNbRoomsBetweenDoors++;

        primaryRooms.AddLast(newRoom);
        roomsDico.Add(newRoom.position, primaryRooms.Last);
        return true;
    }

    public bool GenerateSecondaryPath()
    {
        LinkedListNode<RoomNode> currentPrincipalRoom = primaryRooms.Last;

        int findDoor = 0;

        while (currentPrincipalRoom != null)
        {
            if (currentPrincipalRoom.Value.doorsState.Contains(Door.STATE.CLOSED))
            {
                ++findDoor;
            }
            
            if (findDoor == 4)
            {
                return false;
            }

            if (findDoor == 2 || findDoor == 3)
            {
                LinkedList<RoomNode> tmpSecondaryRooms = new LinkedList<RoomNode>();
                Dictionary<Vector2Int, LinkedListNode<RoomNode>> tmpRoomsDico = new Dictionary<Vector2Int, LinkedListNode<RoomNode>>();

                LinkedListNode<RoomNode> currentRoom = currentPrincipalRoom;
                int randomLenght = Random.Range(nbRoomsSecondaryPathMin, nbRoomsSecondaryPathMax);

                bool secondaryPathFinded = true;

                for (int i = 1; i <= randomLenght; i++)
                {
                    bool positionUsed = true;
                    int nbIter = 0;
                    DoorPosition randomDirection = DoorPosition.BOTTOM;
                    Vector2Int newPosition = new Vector2Int();

                    while (positionUsed && nbIter <= 15)
                    {
                        randomDirection = (DoorPosition)Random.Range(0, 4);
                        switch (randomDirection)
                        {
                            case DoorPosition.LEFT:
                                newPosition = currentRoom.Value.position - new Vector2Int(1, 0);
                                if (roomsDico.ContainsKey(newPosition) || tmpRoomsDico.ContainsKey(newPosition))
                                {
                                    nbIter++;
                                    continue;
                                }
                                break;
                            case DoorPosition.RIGHT:
                                newPosition = currentRoom.Value.position + new Vector2Int(1, 0);
                                if (roomsDico.ContainsKey(newPosition) || tmpRoomsDico.ContainsKey(newPosition))
                                {
                                    nbIter++;
                                    continue;
                                }
                                break;
                            case DoorPosition.TOP:
                                newPosition = currentRoom.Value.position + new Vector2Int(0, 1);
                                if (roomsDico.ContainsKey(newPosition) || tmpRoomsDico.ContainsKey(newPosition))
                                {
                                    nbIter++;
                                    continue;
                                }
                                break;
                            case DoorPosition.BOTTOM:
                                newPosition = currentRoom.Value.position - new Vector2Int(0, 1);
                                if (roomsDico.ContainsKey(newPosition) || tmpRoomsDico.ContainsKey(newPosition))
                                {
                                    nbIter++;
                                    continue;
                                }
                                break;
                            default:
                                break;
                        }
                        positionUsed = false;
                    }
                    if (positionUsed)
                    {
                        // Aller à la node précédente
                        if (i > 1)
                        {
                            currentRoom.Value.RemoveLastDoorAdded();
                            currentRoom.Value.nextSecondary = null;
                        }
                        secondaryPathFinded = false;
                        break;
                    }

                    // ajouter une room (secondary path)

                    RoomNode newRoom = new RoomNode();

                    newRoom.position = newPosition;
                    newRoom.pathType = PathType.SECONDARY;

                    currentRoom.Value.AddDoor(randomDirection);
                    newRoom.AddDoor(GetInverseDoorPosition(randomDirection));

                    currentRoom.Value.ChangeDoorState(randomDirection, Door.STATE.OPEN);
                    newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.OPEN);

                    if (i == randomLenght)
                    {
                        newRoom.roomType = RoomType.KEY;
                    }
                    tmpSecondaryRooms.AddLast(newRoom);
                    tmpRoomsDico.Add(newRoom.position, tmpSecondaryRooms.Last);

                    if (i == 1) // première salle du chemin secondaire
                    {
                        currentRoom.Value.nextSecondary = primaryRooms.Last;
                        tmpSecondaryRooms.AddFirst(currentRoom.Value);
                    }

                    currentRoom = tmpSecondaryRooms.Last;
                }

                if (secondaryPathFinded)
                {
                    secondaryRooms.Add(tmpSecondaryRooms);
                    foreach (KeyValuePair<Vector2Int, LinkedListNode<RoomNode>> tmpRoom in tmpRoomsDico)
                    {
                        roomsDico.Add(tmpRoom.Key, tmpRoom.Value);
                    }
                    findDoor = findDoor == 2 ? 0 : 1;
                }
            }
            currentPrincipalRoom = currentPrincipalRoom.Previous;
        }

        if (findDoor >= 2)
        {
            return false;
        }

        return true;
    }

    public bool GenerateComeBackPath()
    {
        return true;
    }

    public bool GenerateSecretRoom()
    {
        return true;
    }

    public DoorPosition GetInverseDoorPosition(DoorPosition doorPosition)
    {
        switch (doorPosition)
        {
            case DoorPosition.LEFT:
                return DoorPosition.RIGHT;
            case DoorPosition.RIGHT:
                return DoorPosition.LEFT;
            case DoorPosition.TOP:
                return DoorPosition.BOTTOM;
            case DoorPosition.BOTTOM:
                return DoorPosition.TOP;
            default:
                return DoorPosition.LEFT;
        }
    }

    public void DisplayDonjon()
    {
        Vector2Int positionMin = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int positionMax = new Vector2Int(int.MinValue, int.MinValue);

        foreach (KeyValuePair<Vector2Int, LinkedListNode<RoomNode>> room in roomsDico)
        {
            /*string Doors = "";
            for (int i = 0; i < room.Value.Value.doorPosition.Count; ++i)
            {
                Doors += room.Value.Value.doorPosition[i] + ": " + room.Value.Value.doorState[i] + "\n";
            }

            Debug.Log("----[" + room.Key + "]----\n" +
                "Room Type: " + room.Value.Value.roomType + "\n" +
                "Path Type: " + room.Value.Value.pathType + "\n" +
                "Doors:\n" +
                Doors +
                "---------------");*/

            if (room.Value.Value.position.x < positionMin.x)
                positionMin.x = room.Value.Value.position.x;
            if (room.Value.Value.position.y < positionMin.y)
                positionMin.y = room.Value.Value.position.y;
            if (room.Value.Value.position.x > positionMax.x)
                positionMax.x = room.Value.Value.position.x;
            if (room.Value.Value.position.y > positionMax.y)
                positionMax.y = room.Value.Value.position.y;
        }

        Vector2Int mapSize = (positionMax - positionMin);
        Vector2Int mapAbsSize = mapSize;

        mapAbsSize.x = Mathf.Abs(mapAbsSize.x);
        mapAbsSize.y = Mathf.Abs(mapAbsSize.y);

        string map = "";

        for (int y = 0; y <= mapAbsSize.y; ++y)
        {
            for (int x = 0; x <= mapAbsSize.x; ++x)
            {
                LinkedListNode<RoomNode> roomNode;
                if (roomsDico.TryGetValue(new Vector2Int(x, mapAbsSize.y - y) + positionMin, out roomNode))
                {
                    map += "[";

                    if (roomNode.Value.doorsState.Contains(Door.STATE.CLOSED) && roomNode.Next != null && roomNode.Previous != null)
                    {
                        map += "<color=orange>";
                    }
                    else
                    {
                        switch (roomNode.Value.roomType)
                        {
                            case RoomType.START:
                                map += "<color=green>";
                                break;
                            case RoomType.NORMAL:
                                map += "<color=yellow>";
                                break;
                            case RoomType.KEY:
                                map += "<color=blue>";
                                break;
                            case RoomType.BOSS:
                                map += "<color=red>";
                                break;
                            default:
                                break;
                        }
                    }

                    if (roomNode.Next != null)
                    {
                        Vector2Int dir = roomNode.Next.Value.position - roomNode.Value.position;
                        if (dir == new Vector2Int(1, 0))
                        {
                            map += "►</color>";
                        }
                        else if (dir == new Vector2Int(-1, 0))
                        {
                            map += "◄</color>";
                        }
                        else if (dir == new Vector2Int(0, 1))
                        {
                            map += "▲</color>";
                        }
                        else
                        {
                            map += "▼</color>";
                        }
                    }
                    else
                    {
                        map += "◆</color>";
                    }

                    map += ((int)roomNode.Value.pathType + 1) + "]\t";
                }
                else
                {
                    map += ". . . . .\t";
                }
            }
            map += "\n";
        }

        Debug.Log(map);
    }

    // Pour le chemin de retour -> probabilité qui augment pour aller vers la salle start
}
