using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPGeneration : MonoBehaviour
{
    public int rows, columns;
    public int minRoomSize, maxRoomSize, minRoomHeight, maxRoomHeight;

    public int zoneWidth;
    public int zoneHeight;

    public int amountOfZones = 1;
    public Rect[,] zones;
    public bool zoned = false;

    private static List<Rect> sectionList = new List<Rect>();
    private static List<Room> roomList = new List<Room>();
    private static List<Rect> corridorList = new List<Rect>();

    public GameObject floorTile;
    public GameObject wallTile;
    public GameObject[,] floorPositions;

    int counter = 0;
    public Rect mapSize;


    private void Start()
    {
        Section initialSection = new Section(new Rect(0, 0, rows, columns));
        Partition(initialSection);
        initialSection.CreateRoom();


        floorPositions = new GameObject[rows, columns];
        zones = new Rect[rows, columns];
        InitialiseZones(initialSection.rect);
        mapSize = initialSection.rect;


         DrawRooms(initialSection);
         DrawWalls(initialSection);

    }

    private void OnDrawGizmos()
    {
        /*float mostLeftSection = 5;
        float mostRightSection = 0;
        bool notRun = true;
        Rect leftMost = new Rect(-1, -1, 0, 0); // i.e null
        Rect rightMost = new Rect(-1, -1, 0, 0); // i.e null

        if (notRun)
        {
            foreach(Rect rect in sectionList)
            {
                if (rect.y < mostLeftSection) { leftMost = rect; }
                if (rect.y > mostRightSection) { rightMost = rect; }
                counter++;
                if (counter >= sectionList.Count) { notRun = false; }
            }
        }*/

        Gizmos.color = new Color(1, 0, 1, 0.2f);
        Gizmos.DrawCube(mapSize.center, mapSize.size);

        foreach (Rect rect in sectionList)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(rect.center, rect.size);
        }

        if(zoned == true)
        {
            foreach (Rect zone in zones)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawWireCube(zone.center, zone.size);
            }
        }

        foreach (Room room in roomList)
        {
            Gizmos.color = new Color(room.r, room.g, room.b, 0.5f);
            Gizmos.DrawCube(room.rect.center, room.rect.size);
        }
        foreach (Rect rect in corridorList)
        {
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawCube(rect.center, rect.size);
        }
    }

    public void DrawRooms(Section section)
    {
        if (section == null)
        {
            return;
        }
        if (section.IsLeaf())
        {
            for (int i = (int)section.room.rect.x; i <= section.room.rect.xMax; i++)
            {
                for (int j = (int)section.room.rect.y; j <= section.room.rect.yMax; j++)
                {
                    //if (section.room.rect.y == section.room.corridor.y && section.room.rect.x == section.room.corridor.x)
                    //{
                    //    print("Doorway here");
                    //}
                    //else
                    {
                        GameObject instance = Instantiate(floorTile, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(transform);
                        print("Room X: " + section.room.rect.x + "Room Y: " + section.room.rect.y + "Doorway X: " + section.room.corridor.x + "Doorway Y: " + section.room.corridor.y);
                    }
                }
            }
        }
        else
        {
            DrawRooms(section.left);
            DrawRooms(section.right);
        }
    }

    public void DrawWalls(Section section)
    {
        int heightValue = 1;
        if (section == null)
        {
            return;
        }
        if (section.IsLeaf())
        {
            int roomHeight = Random.Range(minRoomHeight, maxRoomHeight);
            for (int n = 1; n < roomHeight; n++)
            {
                for (int i = (int)section.room.rect.x; i <= section.room.rect.xMax; i++)
                {
                    //Draw top
                    GameObject instance = Instantiate(wallTile, new Vector3(i, section.room.rect.yMax, n), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(transform);
                }
                for (int i = (int)section.room.rect.x; i <= section.room.rect.xMax; i++)
                {
                    //Draw bottom
                    GameObject instance = Instantiate(wallTile, new Vector3(i, section.room.rect.yMin, n), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(transform);
                }
                for (int j = (int)section.room.rect.yMin; j <= section.room.rect.yMax; j++)
                {
                    //Draw left
                    GameObject instance = Instantiate(wallTile, new Vector3(section.room.rect.xMin, j, n), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(transform);
                    heightValue++;
                }
                for (int j = (int)section.room.rect.yMin; j <= section.room.rect.yMax; j++)
                {
                    //Draw right
                    GameObject instance = Instantiate(wallTile, new Vector3(section.room.rect.xMax, j, n), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(transform);
                    heightValue++;
                }
            }
        }
        else
        {
            DrawWalls(section.left);
            DrawWalls(section.right);
        }
    }

    public class Room
    {
        public Rect rect;
        public int height;
        public Rect corridor;
        public bool hasRandomised = false;
        public float b;
        public float r;
        public float g;

        public Color color;

        public Room(Rect trect, Color tcolor = default(Color))
        {
            rect = trect;
            color = tcolor;
        }
    }


    public class Section
    {
        public Section left, right;
        public Rect rect;
        public Room room = new Room(new Rect(-1, -1, 0, 0)); // i.e null
        public Rect corridor = new Rect(-1, -1, 0, 0); // i.e null

        public int sectionID;
        public static int sectionIDCounter = 0;

        public void CreateRoom()
        {
            if (left != null)
            {
                left.CreateRoom();
            }
            if (right != null)
            {
                right.CreateRoom();
            }
            if (IsLeaf())
            {
                int roomWidth = (int)Random.Range(rect.width / 2, rect.width - 2);
                int roomHeight = (int)Random.Range(rect.height / 2, rect.height - 2);
                int roomX = (int)Random.Range(1, rect.width - roomWidth - 1);
                int roomY = (int)Random.Range(1, rect.height - roomHeight - 1);

                room = new Room(new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight));

                float corridorX = Random.Range(room.rect.xMin + 1, room.rect.xMax - 1);
                float corridorY;

                if (Random.Range(0.0f, 1.0f) > 0.5f)
                {
                    corridorY = room.rect.yMax - 1;
                }
                else
                {
                    corridorY = room.rect.yMin;
                }
      
                corridorX = Mathf.RoundToInt(corridorX);
                corridorY = Mathf.RoundToInt(corridorY);

                corridor = new Rect(corridorX, corridorY, 1, 1);
                room.corridor = corridor;
                roomList.Add(room);
                corridorList.Add(corridor);
            }
        }

        public void CreateCorridor()
        {

        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public Section(Rect trect)
        {
            rect = trect;
            sectionID = sectionIDCounter;
            sectionIDCounter++;
        }

        public bool Split(int minRoomSize, int maxRoomSize)
        {
            //If this has already split
            if (IsLeaf() == false)
            {
                //Stop splitting
                return false;
            }
            bool splitHorizontally;
            if (rect.width / rect.height >= 1.25)
            {
                splitHorizontally = false;
            }
            else if (rect.height / rect.width >= 1.25)
            {
                splitHorizontally = true;
            }
            else
            {
                splitHorizontally = Random.Range(0.0f, 1.0f) > 0.5f;
            }

            if (Mathf.Min(rect.height, rect.width) / 2 < minRoomSize)
            {
                return false;
            }

            if (splitHorizontally == true)
            {
                int splitValue = Random.Range(minRoomSize, (int)rect.width - minRoomSize);

                left = new Section(new Rect(rect.x, rect.y, rect.width, splitValue));
                right = new Section(new Rect(rect.x, rect.y + splitValue, rect.width, rect.height - splitValue));
                sectionList.Add(left.rect);
                sectionList.Add(right.rect);
            }
            else
            {
                int splitValue = Random.Range(minRoomSize, (int)rect.height - minRoomSize);

                left = new Section(new Rect(rect.x, rect.y, splitValue, rect.height));
                right = new Section(new Rect(rect.x + splitValue, rect.y, rect.width - splitValue, rect.height));
                sectionList.Add(left.rect);
                sectionList.Add(right.rect);
            }
            return true;
        }
    }

    public void Partition(Section section)
    {
        if (section.IsLeaf() == true)
        {
            if (section.rect.width > maxRoomSize
                || section.rect.height > maxRoomSize
                || Random.Range(0.0f, 1.0f) > 0.25)
            {

                if (section.Split(minRoomSize, maxRoomSize))
                {
                    Partition(section.left);
                    Partition(section.right);
                }
            }
        }
    }

    public class Zone
    {

    }

    void InitialiseZones(Rect section)
    {
        zoneHeight = (columns) / amountOfZones;
        zoneWidth = (rows) / amountOfZones;

        for (int i = (int)section.x; i < section.xMax / zoneWidth; i++)
        {
            for (int j = (int)section.y; j < section.yMax / zoneHeight; j++)
            {
                zones[i, j] = new Rect(i * zoneWidth, j * zoneHeight, zoneWidth, zoneHeight);
            }
        }
        zoned = true;
    }
}
