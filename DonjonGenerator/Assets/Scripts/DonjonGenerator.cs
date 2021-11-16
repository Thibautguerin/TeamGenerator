using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Difficulty
{
    PEACEFULL,
    EASY,
    MEDIUM,
    HARD,
    BOSS
}

enum DoorPosition
{
    LEFT,
    RIGHT,
    TOP,
    BOTTOM
}

enum PathType
{
    PRINCIPAL,
    SECONDARY,
    ALTERNATIVE,
    HIDDEN
}

enum RoomType
{
    NORMAL,
    KEY
}

class RoomNode
{
    PathType pathType;
    RoomType roomType;

    Vector2Int position;
    Difficulty difficulty;
    List<DoorPosition> doorsPosition;

    List<RoomNode> nextRooms;
    List<RoomNode> previousRooms;

    RoomNode modified;
}

public class DonjonGenerator : MonoBehaviour
{
    Dictionary<Vector2Int, RoomNode> Rooms;
    int nbDoorsBeforeTheBoss = 2;
    int nbRoomsBetweenDoorsMin = 1;
    int nbRoomsBetweenDoorsMax = 3;
}
