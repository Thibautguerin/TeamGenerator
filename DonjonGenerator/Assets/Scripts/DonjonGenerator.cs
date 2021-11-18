using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    COMEBACK,
    SECRET
}

public enum RoomType
{
    START,
    NORMAL,
    KEY,
    BOSS,
    HARDROOM,
    SECRET,
    COMEBACK
}

public class RoomNode
{
    public PathType pathType = PathType.PRINCIPAL;
    public RoomType roomType = RoomType.NORMAL;
    public bool isComeBackPath = false;

    public Vector2Int position;
    public uint doors;
    public uint lastdoorAdded;
    public List<Door.STATE> doorsState = new List<Door.STATE>(4)
        {Door.STATE.WALL, Door.STATE.WALL, Door.STATE.WALL, Door.STATE.WALL};

    public List<RoomNode> linkedRooms = new List<RoomNode>(4)
        {null, null, null, null};

    public RoomNode previous;
    public RoomNode next;

    
    public void AddDoor(DoorPosition doorPosition, RoomNode roomNode, bool changePrevNext, bool isPrevious)
    {
        if ((doors & 1 << (int)doorPosition) != 1 << (int)doorPosition)
        {
            doors += (uint)(1 << (int)doorPosition);
            lastdoorAdded = (uint)(1 << (int)doorPosition);
            linkedRooms[(int)doorPosition] = roomNode;

            if (changePrevNext)
            {
                if (isPrevious)
                {
                    previous = roomNode;
                }
                else
                {
                    next = roomNode;
                }
            }
        }
    }

    public void RemoveDoor(DoorPosition doorPosition)
    {
        if ((doors & 1 << (int)doorPosition) == 1 << (int)doorPosition)
        {
            doors -= (uint)(1 << (int)doorPosition);
            linkedRooms[(int)doorPosition] = null;
        }
    }

    public void RemoveLastDoorAdded()
    {
        switch (lastdoorAdded)
        {
            case 1 << 0:
                RemoveDoor(DoorPosition.LEFT);
                break;
            case 1 << 1:
                RemoveDoor(DoorPosition.RIGHT);
                break;
            case 1 << 2:
                RemoveDoor(DoorPosition.TOP);
                break;
            case 1 << 3:
                RemoveDoor(DoorPosition.BOTTOM);
                break;
            default:
                break;
        }
    }

    public void ChangeDoorState(DoorPosition doorPosition, Door.STATE newDoorState)
    {
        doorsState[(int)doorPosition] = newDoorState;
    }
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
    [Range(0, 100), Header("Hard Room")]
    public int hardRoomProbability = 20;
    [Header("Seed")]
    public bool useSeed = false;
    public int seed = 0;

    System.Random random;
    
    Dictionary<Vector2Int, RoomNode> roomsDico = new Dictionary<Vector2Int, RoomNode>();

    RoomNode firstRoom;
    RoomNode lastRoom;
    RoomManager roomM;

    private void Awake()
    {
        if (useSeed)
        {
            random = new System.Random(seed);
        }
        else
        {
            random = new System.Random();
        }
        nbRoomsAfterStartMax++;
        nbRoomsBetweenDoorsMax++;
        nbRoomsSecondaryPathMax++;
        nbRoomsBeforeBossMax++;
        hardRoomProbability--;
        roomM = GetComponent<RoomManager>();
    }

    private void Start()
    {
        Generate();
        DisplayDonjon();
        SpawnRoom();
    }

    public void Generate()
    {
        firstRoom = null;
        lastRoom = null;
        roomsDico.Clear();

        int currentNbDoors = 0;
        int nbRooms = random.Next(nbRoomsAfterStartMin, nbRoomsAfterStartMax);
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

        firstRoom = newRoom;
        lastRoom = newRoom;
        roomsDico.Add(newRoom.position, newRoom);
    }

    public bool GeneratePrincipalRoom(ref int currentNbDoors, ref int nbRoomsBetweenDoors, ref int currentNbRoomsBetweenDoors)
    {
        bool positionUsed = true;
        int nbIter = 0;
        DoorPosition randomDirection = DoorPosition.BOTTOM;
        Vector2Int newPosition = new Vector2Int();

        while (positionUsed && nbIter <= 20)
        {
            randomDirection = (DoorPosition)random.Next(0, 4);
            switch (randomDirection)
            {
                case DoorPosition.LEFT:
                    newPosition = lastRoom.position - new Vector2Int(1, 0);
                    break;
                case DoorPosition.RIGHT:
                    newPosition = lastRoom.position + new Vector2Int(1, 0);
                    break;
                case DoorPosition.TOP:
                    newPosition = lastRoom.position + new Vector2Int(0, 1);
                    break;
                case DoorPosition.BOTTOM:
                    newPosition = lastRoom.position - new Vector2Int(0, 1);
                    break;
                default:
                    break;
            }

            if (roomsDico.ContainsKey(newPosition))
            {
                nbIter++;
                continue;
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

        lastRoom.AddDoor(randomDirection, newRoom, true, false);
        newRoom.AddDoor(GetInverseDoorPosition(randomDirection), lastRoom, true, true);

        if (currentNbRoomsBetweenDoors >= nbRoomsBetweenDoors && currentNbDoors < nbDoors)
        {
            // Mettre une porte
            lastRoom.ChangeDoorState(randomDirection, Door.STATE.CLOSED);
            newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.CLOSED);

            currentNbRoomsBetweenDoors = 0;
            currentNbDoors++;
            if (currentNbDoors < nbDoors)
                nbRoomsBetweenDoors = random.Next(nbRoomsBetweenDoorsMin, nbRoomsBetweenDoorsMax);
            else
                nbRoomsBetweenDoors = random.Next(nbRoomsBeforeBossMin, nbRoomsBeforeBossMax);
        }
        else if (currentNbRoomsBetweenDoors >= nbRoomsBetweenDoors && currentNbDoors == nbDoors)
        {
            // Mettre le Boss
            newRoom.roomType = RoomType.BOSS;
            lastRoom.ChangeDoorState(randomDirection, Door.STATE.OPEN);
            newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.OPEN);

            currentNbRoomsBetweenDoors = 0;
            nbRoomsBetweenDoors = random.Next(nbRoomsBetweenDoorsMin, nbRoomsBetweenDoorsMax);
            currentNbDoors++;
        }
        else
        {
            lastRoom.ChangeDoorState(randomDirection, Door.STATE.OPEN);
            newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.OPEN);
        }
        currentNbRoomsBetweenDoors++;

        lastRoom = newRoom;
        roomsDico.Add(newRoom.position, newRoom);
        return true;
    }

    public bool GenerateSecondaryPath()
    {
        RoomNode currentPrincipalRoom = lastRoom;

        int findDoor = 0;

        while (currentPrincipalRoom != null)
        {
            if (currentPrincipalRoom.doorsState.Contains(Door.STATE.CLOSED))
            {
                ++findDoor;
            }
            
            if (findDoor == 4)
            {
                return false;
            }

            if (findDoor == 2 || findDoor == 3)
            {
                Dictionary<Vector2Int, RoomNode> tmpRoomsDico = new Dictionary<Vector2Int, RoomNode>();

                RoomNode currentRoom = currentPrincipalRoom;
                int randomLenght = random.Next(nbRoomsSecondaryPathMin, nbRoomsSecondaryPathMax);

                bool secondaryPathFinded = true;

                for (int i = 1; i <= randomLenght; i++)
                {
                    bool positionUsed = true;
                    int nbIter = 0;
                    DoorPosition randomDirection = DoorPosition.BOTTOM;
                    Vector2Int newPosition = new Vector2Int();

                    while (positionUsed && nbIter <= 15)
                    {
                        randomDirection = (DoorPosition)random.Next(0, 4);
                        switch (randomDirection)
                        {
                            case DoorPosition.LEFT:
                                newPosition = currentRoom.position - new Vector2Int(1, 0);
                                break;
                            case DoorPosition.RIGHT:
                                newPosition = currentRoom.position + new Vector2Int(1, 0);
                                break;
                            case DoorPosition.TOP:
                                newPosition = currentRoom.position + new Vector2Int(0, 1);
                                break;
                            case DoorPosition.BOTTOM:
                                newPosition = currentRoom.position - new Vector2Int(0, 1);
                                break;
                            default:
                                break;
                        }

                        if (roomsDico.ContainsKey(newPosition) || tmpRoomsDico.ContainsKey(newPosition))
                        {
                            nbIter++;
                            continue;
                        }

                        positionUsed = false;
                    }
                    if (positionUsed)
                    {
                        if (i > 1)
                        {
                            currentRoom.RemoveLastDoorAdded();
                        }
                        secondaryPathFinded = false;
                        break;
                    }

                    RoomNode newRoom = new RoomNode();

                    newRoom.position = newPosition;
                    newRoom.pathType = PathType.SECONDARY;

                    if (i == 1)
                    {
                        currentRoom.AddDoor(randomDirection, newRoom, false, false);
                    }
                    else
                    {
                        currentRoom.AddDoor(randomDirection, newRoom, true, false);
                    }
                    newRoom.AddDoor(GetInverseDoorPosition(randomDirection), currentRoom, true, true);

                    currentRoom.ChangeDoorState(randomDirection, Door.STATE.OPEN);
                    newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.OPEN);

                    if (i == randomLenght)
                    {
                        newRoom.roomType = RoomType.KEY;
                    }
                    tmpRoomsDico.Add(newRoom.position, newRoom);

                    currentRoom = newRoom;
                }

                if (secondaryPathFinded)
                {
                    foreach (KeyValuePair<Vector2Int, RoomNode> tmpRoom in tmpRoomsDico)
                    {
                        roomsDico.Add(tmpRoom.Key, tmpRoom.Value);
                    }
                    findDoor = findDoor == 2 ? 0 : 1;
                }
            }
            currentPrincipalRoom = currentPrincipalRoom.previous;
        }

        if (findDoor >= 2)
        {
            return false;
        }

        return true;
    }

    public bool GenerateComeBackPath()
    {
        RoomNode currentRoom = lastRoom;
        lastRoom.isComeBackPath = true;

        while (currentRoom != firstRoom)
        {
            bool badPosition = true;
            int nbIter = 0;
            DoorPosition randomDirection = DoorPosition.BOTTOM;
            Vector2Int newPosition = new Vector2Int();

            RoomNode roomNodeTmp = null;

            List<KeyValuePair<float, Vector2Int>> shortestDistance = new List<KeyValuePair<float, Vector2Int>>();
            if (currentRoom.previous != null)
            {
                if (currentRoom.previous.position - currentRoom.position != new Vector2Int(-1, 0))
                {
                    shortestDistance.Add(new KeyValuePair<float, Vector2Int>(Vector2.Distance(firstRoom.position, currentRoom.position + new Vector2(-1, 0)), new Vector2Int(-1, 0)));
                }
                if (currentRoom.previous.position - currentRoom.position != new Vector2Int(1, 0))
                {
                    shortestDistance.Add(new KeyValuePair<float, Vector2Int>(Vector2.Distance(firstRoom.position, currentRoom.position + new Vector2(1, 0)), new Vector2Int(1, 0)));
                }
                if (currentRoom.previous.position - currentRoom.position != new Vector2Int(0, 1))
                {
                    shortestDistance.Add(new KeyValuePair<float, Vector2Int>(Vector2.Distance(firstRoom.position, currentRoom.position + new Vector2(0, 1)), new Vector2Int(0, 1)));
                }
                if (currentRoom.previous.position - currentRoom.position != new Vector2Int(0, -1))
                {
                    shortestDistance.Add(new KeyValuePair<float, Vector2Int>(Vector2.Distance(firstRoom.position, currentRoom.position + new Vector2(0, -1)), new Vector2Int(0, -1)));
                }
            }
            else
            {
                shortestDistance.Add(new KeyValuePair<float, Vector2Int>(Vector2.Distance(firstRoom.position, currentRoom.position + new Vector2Int(-1, 0)), new Vector2Int(-1, 0)));
                shortestDistance.Add(new KeyValuePair<float, Vector2Int>(Vector2.Distance(firstRoom.position, currentRoom.position + new Vector2Int(1, 0)), new Vector2Int(1, 0)));
                shortestDistance.Add(new KeyValuePair<float, Vector2Int>(Vector2.Distance(firstRoom.position, currentRoom.position + new Vector2Int(0, 1)), new Vector2Int(0, 1)));
                shortestDistance.Add(new KeyValuePair<float, Vector2Int>(Vector2.Distance(firstRoom.position, currentRoom.position + new Vector2Int(0, -1)), new Vector2Int(0, -1)));
            }
            shortestDistance.Sort((x, y) => { return x.Key.CompareTo(y.Key);});

            while (badPosition && nbIter <= 20)
            {
                roomNodeTmp = null;
                int randomNb = random.Next(0, 100);

                Vector2Int orientation;
                if (shortestDistance.Count == 3)
                {
                    // 70 20 10
                    if (randomNb <= 69)
                    {
                        orientation = shortestDistance[0].Value;
                    }
                    else if (randomNb <= 89)
                    {
                        orientation = shortestDistance[1].Value;
                    }
                    else
                    {
                        orientation = shortestDistance[2].Value;
                    }
                }
                else
                {
                    // 60 25 10 5
                    if (randomNb <= 59)
                    {
                        orientation = shortestDistance[0].Value;
                    }
                    else if (randomNb <= 84)
                    {
                        orientation = shortestDistance[1].Value;
                    }
                    else if (randomNb <= 94)
                    {
                        orientation = shortestDistance[2].Value;
                    }
                    else
                    {
                        orientation = shortestDistance[3].Value;
                    }
                }
                newPosition = currentRoom.position + orientation;

                if (orientation == new Vector2Int(-1, 0))
                {
                    randomDirection = DoorPosition.LEFT;
                }
                else if (orientation == new Vector2Int(1, 0))
                {
                    randomDirection = DoorPosition.RIGHT;
                }
                else if (orientation == new Vector2Int(0, 1))
                {
                    randomDirection = DoorPosition.TOP;
                }
                else
                {
                    randomDirection = DoorPosition.BOTTOM;
                }

                if ((roomsDico.TryGetValue(newPosition, out roomNodeTmp) && currentRoom == lastRoom)
                    || (roomNodeTmp != null && roomNodeTmp.isComeBackPath))
                {
                    nbIter++;
                    continue;
                }

                badPosition = false;
            }
            if (badPosition)
            {
                // Recommencer la génération
                return false;
            }
            else if (roomNodeTmp != null)
            {
                roomNodeTmp.isComeBackPath = true;

                roomNodeTmp.AddDoor(GetInverseDoorPosition(randomDirection), currentRoom, true, true);
                if (currentRoom.pathType == PathType.COMEBACK)
                {
                    currentRoom.AddDoor(randomDirection, roomNodeTmp, true, false);
                    currentRoom.ChangeDoorState(randomDirection, Door.STATE.WEAKENED);
                    roomNodeTmp.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.WEAKENED);
                }
                else if (currentRoom.doorsState[(int)randomDirection] == Door.STATE.WALL || roomNodeTmp.doorsState[(int)GetInverseDoorPosition(randomDirection)] == Door.STATE.WALL)
                {
                    currentRoom.ChangeDoorState(randomDirection, Door.STATE.WEAKENED);
                    roomNodeTmp.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.WEAKENED);
                }

                currentRoom = roomNodeTmp;
            }
            else
            {
                RoomNode newRoom = new RoomNode();

                newRoom.position = newPosition;
                newRoom.pathType = PathType.COMEBACK;
                newRoom.roomType = RoomType.COMEBACK;
                newRoom.isComeBackPath = true;

                if (currentRoom.pathType == PathType.COMEBACK)
                {
                    currentRoom.AddDoor(randomDirection, newRoom, true, false);
                }
                else
                {
                    currentRoom.AddDoor(randomDirection, newRoom, false, false);
                }
                newRoom.AddDoor(GetInverseDoorPosition(randomDirection), currentRoom, true, true);

                currentRoom.ChangeDoorState(randomDirection, Door.STATE.WEAKENED);
                newRoom.ChangeDoorState(GetInverseDoorPosition(randomDirection), Door.STATE.WEAKENED);

                roomsDico.Add(newRoom.position, newRoom);
                currentRoom = newRoom;
            }
        }
        return true;
    }

    public bool GenerateSecretRoom()
    {
        bool secretRoomGenerated = false;
        RoomNode secretRoom = new RoomNode();
        bool canCreatHardRoom = true;

        foreach (KeyValuePair<Vector2Int, RoomNode> room in roomsDico)
        {
            if (room.Value == firstRoom || room.Value == lastRoom)
            {
                continue;
            }

            bool canTryToCreateHardRoom = false;
            Vector2Int direction = new Vector2Int();

            if ((room.Value.doors & 1 << 0) == 1 << 0
                && (room.Value.doors & 1 << 2) == 1 << 2
                && (room.Value.doors & 1 << 1) != 1 << 1
                && (room.Value.doors & 1 << 3) != 1 << 3)
            {
                canTryToCreateHardRoom = true;
                direction = new Vector2Int(-1, 1);
            }
            else if ((room.Value.doors & 1 << 0) != 1 << 0
                && (room.Value.doors & 1 << 2) == 1 << 2
                && (room.Value.doors & 1 << 1) == 1 << 1
                && (room.Value.doors & 1 << 3) != 1 << 3)
            {
                canTryToCreateHardRoom = true;
                direction = new Vector2Int(1, 1);
            }
            else if ((room.Value.doors & 1 << 0) != 1 << 0
                && (room.Value.doors & 1 << 2) != 1 << 2
                && (room.Value.doors & 1 << 1) == 1 << 1
                && (room.Value.doors & 1 << 3) == 1 << 3)
            {
                canTryToCreateHardRoom = true;
                direction = new Vector2Int(1, -1);
            }
            else if ((room.Value.doors & 1 << 0) == 1 << 0
                && (room.Value.doors & 1 << 2) != 1 << 2
                && (room.Value.doors & 1 << 1) != 1 << 1
                && (room.Value.doors & 1 << 3) == 1 << 3)
            {
                canTryToCreateHardRoom = true;
                direction = new Vector2Int(-1, -1);
            }

            if (canTryToCreateHardRoom)
            {
                if (!canCreatHardRoom)
                {
                    canCreatHardRoom = true;
                    continue;
                }

                int rand = random.Next(0, 100);

                if (rand <= hardRoomProbability)
                {
                    room.Value.roomType = RoomType.HARDROOM;
                    canCreatHardRoom = false;
                    if (!secretRoomGenerated && !roomsDico.ContainsKey(room.Key + direction))
                    {
                        int rand1 = random.Next(0, 100);

                        if (rand1 <= 33)
                        {

                            // Create Secret Room
                            secretRoom.position = room.Key + direction;
                            secretRoom.pathType = PathType.SECRET;
                            secretRoom.roomType = RoomType.SECRET;

                            RoomNode room1;
                            RoomNode room2;

                            if (roomsDico.TryGetValue(room.Key + new Vector2Int(direction.x, 0), out room1) && roomsDico.TryGetValue(room.Key + new Vector2Int(0, direction.y), out room2))
                            {
                                room1.AddDoor(GetDoorPosition(new Vector2Int(0, direction.y)), secretRoom, false, false);
                                room2.AddDoor(GetDoorPosition(new Vector2Int(direction.x, 0)), secretRoom, false, false);
                                secretRoom.AddDoor(GetInverseDoorPosition(GetDoorPosition(new Vector2Int(0, direction.y))), room1, true, true);
                                secretRoom.AddDoor(GetInverseDoorPosition(GetDoorPosition(new Vector2Int(direction.x, 0))), room2, true, true);

                                room1.ChangeDoorState(GetDoorPosition(new Vector2Int(0, direction.y)), Door.STATE.SECRET);
                                room2.ChangeDoorState(GetDoorPosition(new Vector2Int(direction.x, 0)), Door.STATE.SECRET);
                                secretRoom.ChangeDoorState(GetInverseDoorPosition(GetDoorPosition(new Vector2Int(0, direction.y))), Door.STATE.SECRET);
                                secretRoom.ChangeDoorState(GetInverseDoorPosition(GetDoorPosition(new Vector2Int(direction.x, 0))), Door.STATE.SECRET);

                                secretRoomGenerated = true;
                            }
                        }
                    }
                }
            }
        }
        if (!secretRoomGenerated)
        {
            foreach (KeyValuePair<Vector2Int, RoomNode> room in roomsDico)
            {
                if (room.Value.roomType == RoomType.HARDROOM)
                {
                    Vector2Int direction = new Vector2Int();

                    if ((room.Value.doors & 1 << 0) == 1 << 0
                        && (room.Value.doors & 1 << 2) == 1 << 2
                        && (room.Value.doors & 1 << 1) != 1 << 1
                        && (room.Value.doors & 1 << 3) != 1 << 3)
                    {
                        direction = new Vector2Int(-1, 1);
                    }
                    else if ((room.Value.doors & 1 << 0) != 1 << 0
                        && (room.Value.doors & 1 << 2) == 1 << 2
                        && (room.Value.doors & 1 << 1) == 1 << 1
                        && (room.Value.doors & 1 << 3) != 1 << 3)
                    {
                        direction = new Vector2Int(1, 1);
                    }
                    else if ((room.Value.doors & 1 << 0) != 1 << 0
                        && (room.Value.doors & 1 << 2) != 1 << 2
                        && (room.Value.doors & 1 << 1) == 1 << 1
                        && (room.Value.doors & 1 << 3) == 1 << 3)
                    {
                        direction = new Vector2Int(1, -1);
                    }
                    else if ((room.Value.doors & 1 << 0) == 1 << 0
                        && (room.Value.doors & 1 << 2) != 1 << 2
                        && (room.Value.doors & 1 << 1) != 1 << 1
                        && (room.Value.doors & 1 << 3) == 1 << 3)
                    {
                        direction = new Vector2Int(-1, -1);
                    }

                    if (!roomsDico.ContainsKey(room.Key + direction))
                    {
                        // Create Secret Room
                        secretRoom.position = room.Key + direction;
                        secretRoom.pathType = PathType.SECRET;
                        secretRoom.roomType = RoomType.SECRET;

                        RoomNode room1;
                        RoomNode room2;

                        if (roomsDico.TryGetValue(room.Key + new Vector2Int(direction.x, 0), out room1) && roomsDico.TryGetValue(room.Key + new Vector2Int(0, direction.y), out room2))
                        {
                            room1.AddDoor(GetDoorPosition(new Vector2Int(0, direction.y)), secretRoom, false, false);
                            room2.AddDoor(GetDoorPosition(new Vector2Int(direction.x, 0)), secretRoom, false, false);
                            secretRoom.AddDoor(GetInverseDoorPosition(GetDoorPosition(new Vector2Int(0, direction.y))), room1, true, true);
                            secretRoom.AddDoor(GetInverseDoorPosition(GetDoorPosition(new Vector2Int(direction.x, 0))), room2, true, true);

                            room1.ChangeDoorState(GetDoorPosition(new Vector2Int(0, direction.y)), Door.STATE.SECRET);
                            room2.ChangeDoorState(GetDoorPosition(new Vector2Int(direction.x, 0)), Door.STATE.SECRET);
                            secretRoom.ChangeDoorState(GetInverseDoorPosition(GetDoorPosition(new Vector2Int(0, direction.y))), Door.STATE.SECRET);
                            secretRoom.ChangeDoorState(GetInverseDoorPosition(GetDoorPosition(new Vector2Int(direction.x, 0))), Door.STATE.SECRET);

                            secretRoomGenerated = true;
                            break;
                        }
                    }
                }
            }
        }

        if (!secretRoomGenerated)
        {
            return false;
        }
        roomsDico.Add(secretRoom.position, secretRoom);
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

    public DoorPosition GetDoorPosition(Vector2Int direction)
    {
        if (direction == new Vector2Int(-1, 0))
        {
            return DoorPosition.LEFT;
        }
        else if (direction == new Vector2Int(1, 0))
        {
            return DoorPosition.RIGHT;
        }
        else if (direction == new Vector2Int(0, 1))
        {
            return DoorPosition.TOP;
        }
        else
        {
            return DoorPosition.BOTTOM;
        }
    }

    public void DisplayDonjon()
    {
        Vector2Int positionMin = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int positionMax = new Vector2Int(int.MinValue, int.MinValue);

        foreach (KeyValuePair<Vector2Int, RoomNode> room in roomsDico)
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

            if (room.Value.position.x < positionMin.x)
                positionMin.x = room.Value.position.x;
            if (room.Value.position.y < positionMin.y)
                positionMin.y = room.Value.position.y;
            if (room.Value.position.x > positionMax.x)
                positionMax.x = room.Value.position.x;
            if (room.Value.position.y > positionMax.y)
                positionMax.y = room.Value.position.y;
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
                RoomNode roomNode;
                if (roomsDico.TryGetValue(new Vector2Int(x, mapAbsSize.y - y) + positionMin, out roomNode))
                {
                    map += "[";

                    if (roomNode.doorsState.Contains(Door.STATE.CLOSED) && roomNode.next != null && roomNode.previous != null)
                    {
                        map += "<color=orange>";
                    }
                    else if (roomNode.pathType == PathType.COMEBACK)
                    {
                        map += "<color=black>";
                    }
                    else
                    {
                        switch (roomNode.roomType)
                        {
                            case RoomType.START:
                                map += "<color=green>";
                                break;
                            case RoomType.NORMAL:
                                map += "<color=yellow>";
                                break;
                            case RoomType.HARDROOM:
                                map += "<color=yellow>";
                                break;
                            case RoomType.KEY:
                                map += "<color=blue>";
                                break;
                            case RoomType.BOSS:
                                map += "<color=red>";
                                break;
                            case RoomType.SECRET:
                                map += "<color=purple>";
                                break;
                            default:
                                break;
                        }
                    }

                    if (roomNode.next != null)
                    {
                        Vector2Int dir = roomNode.next.position - roomNode.position;
                        if (roomNode.roomType != RoomType.HARDROOM)
                        {
                            if (dir == new Vector2Int(1, 0))
                            {
                                map += "→</color>";
                            }
                            else if (dir == new Vector2Int(-1, 0))
                            {
                                map += "←</color>";
                            }
                            else if (dir == new Vector2Int(0, 1))
                            {
                                map += "↑</color>";
                            }
                            else
                            {
                                map += "↓</color>";
                            }
                        }
                        else
                        {
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
                    }
                    else
                    {
                        map += "◆</color>";
                    }

                    if (roomNode.isComeBackPath)
                    {
                        map += "<color=black>" + ((int)roomNode.pathType + 1) + "</color>]\t";
                    }
                    else
                    {
                        map += ((int)roomNode.pathType + 1) + "]\t";
                    }
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

    private void SpawnRoom()
    {
        foreach (var item in roomsDico)
        {
            Room temp = roomM.FindRoomfromNode(item.Value);
            Vector3Int position = (Vector3Int)item.Key;
            position.x *= 11;
            position.y *= 9;
            temp.transform.position = position;
            temp.position = item.Key;
        }
    }
    // Pour le chemin de retour -> probabilité qui augment pour aller vers la salle start

    // fermer random les salles principales et secondaires (1 chance sur 2 ou 3)
}
