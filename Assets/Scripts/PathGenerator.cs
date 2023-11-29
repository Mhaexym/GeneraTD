using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;

public class PathGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject node;
    public List<GameObject> nodeOut;
    private List<GameObject> nodes = new List<GameObject>();
    private Tilemap tilemap;
    public TileBase[] pathtiles;
    private int center;
    private enum TI {N, E, S, W, NW, NE, SE, SW, NWinner, NEinner, SEinner, SWinner}
    private void Start()
    {
        center = (int)this.transform.position.x;
        tilemap = this.GetComponentInParent<Tilemap>();
        GenerateNodes(nodes);
        nodeOut = nodes;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
    
    private void GenerateNodes(List<GameObject> nodes)
    {
        string prev = "";
        Vector3 curr = new Vector3();
        if (nodes.Count == 0)
        {
            int rand_y = center + Random.Range(-8, 9);
            Vector3 start = new Vector3(center-18, rand_y, 300);
            nodes.Add((GameObject) Instantiate(node, start, Quaternion.identity));
            nodes[0].name = "SpawnPosition";
            Vector3 second = new Vector3(center-15, rand_y, 300);
            nodes.Add((GameObject) Instantiate(node, second, Quaternion.identity));
            nodes[1].name = "W";
            curr = second;
        }
        
        while(true)
        {
            int caseNumber;
            var cnums = "";
            while(true)
            {
                bool nodeInPlace = false;
                caseNumber = Random.Range(1, 5);
                cnums += caseNumber;
                if (curr.x <= (center - 12) && caseNumber == 1) continue;
                if (curr.y <= (center - 6) && caseNumber == 3) continue;
                if (curr.y >= (center + 6) && caseNumber == 4) continue;
                int legal = 0;
                foreach (GameObject n in nodes)
                {
                    if(n == null) break;
                    Vector3 nodepos = n.transform.position;
                    switch (caseNumber)
                    {
                        case 1:
                            if (nodepos.x == curr.x - 3 && nodepos.y == curr.y) nodeInPlace = true;
                            break;
                        case 2:
                            if (nodepos.x == curr.x + 3 && nodepos.y == curr.y) nodeInPlace = true;
                            break;
                        case 3:
                            if (nodepos.y == curr.y - 3 && nodepos.x == curr.x) nodeInPlace = true;
                            break;
                        case 4:
                            if (nodepos.y == curr.y + 3 && nodepos.x == curr.x) nodeInPlace = true;
                            break;
                    }
                    if(nodeInPlace) break;
                    Vector3 tempcurr = curr;
                    switch (caseNumber)
                    {
                        case 1:
                            tempcurr.x -= 3;
                            break;
                        case 2:
                            tempcurr.x += 3;
                            break;
                        case 3:
                            tempcurr.y -= 3;
                            break;
                        case 4:
                            tempcurr.y += 3;
                            break;
                    }
                    // Problem here: if we can still find at least 1 option, we can take it,
                    // but if that takes us on a path of only 1 option until we suddenly have 0
                    // (like a line on the edge of the map), we cannot get out of it. 
                    legal = 0;
                    for (int i = 1; i < 5; i++)
                    {
                        switch (i)
                        {
                            case 1:
                                if (nodepos.x == tempcurr.x - 3 && nodepos.y == tempcurr.y) legal += 1;
                                else if (tempcurr.x - 3 <= center - 12) legal += 1;
                                break;
                            case 2:
                                if (nodepos.x == tempcurr.x + 3 && nodepos.y == tempcurr.y) legal+=1;
                                break;
                            case 3:
                                if (nodepos.y == tempcurr.y - 3 && tempcurr.x == tempcurr.x) legal+=1;
                                else if (tempcurr.y - 3 <= center - 6) legal += 1;
                                break;
                            case 4:
                                if (nodepos.y == tempcurr.y + 3 && tempcurr.x == tempcurr.x) legal+=1;
                                else if (tempcurr.y + 3 >= center + 6) legal += 1;
                                break;
                        }
                    }
                }

                if (legal >= 3)
                {
                    Debug.Log("Goes Wrong Here");
                    continue;
                }
                if (!nodeInPlace) break;
            }
            Debug.Log(cnums + " for node on coordinates " + curr);
            int nextX = new int();
            int nextY = new int();
            switch(caseNumber)
            {
                case 1:
                    nextX = (int) curr.x - 3;
                    nextY = (int) curr.y;
                    prev = "E";
                    break;
                case 2:
                    nextX = (int) curr.x + 3;
                    nextY = (int) curr.y;
                    prev = "W";
                    break;
                case 3:
                    nextX = (int) curr.x;
                    nextY = (int) curr.y - 3;
                    prev = "N";
                    break;
                case 4:
                    nextX = (int) curr.x;
                    nextY = (int) curr.y + 3;
                    prev = "S";
                    break;
            }

            Vector3 nextNodePos = new Vector3(nextX, nextY, 300);
            nodes.Add((GameObject) Instantiate(node, nextNodePos, Quaternion.identity));
            nodes.Last().name = prev;
            curr = nextNodePos;
            if (curr.x > center + 15) break;
        }
        int index = 1;
        foreach (GameObject n in nodes)
        {
            n.transform.SetParent(this.transform);
            Vector3Int tilePlace = Vector3Int.FloorToInt(n.transform.position);
            if (n == nodes[0]) continue;
            if (index+1 < nodes.Count)
            {
                var p = nodes[index].name;
                var q = nodes[index + 1].name;
                //Debug.Log(p+q+" This is the current switch on index " + index);
                index++;
                switch (p + q) // Tile placer on nodes
                {
                    case "WW": //horizontal paths
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        break;
                    case "EE": //horizontal paths
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.x += 2;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        break;
                    
                    case "NN": //vertical paths
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.x += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        break;
                    case "SS": //vertical paths
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.y -= 2;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.x += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        break;
                    
                    case "WN": //top-right corner
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.NE]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.SWinner]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        break;
                    case "SE": //top-right corner
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.NE]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.SWinner]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 2;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.x += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        break;
                    
                    case "WS": //bottom-right corner
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.SE]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.NWinner]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        break;
                    case "NE": //bottom-right corner
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.SE]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.NWinner]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.x += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        break;
                    
                    case "NW": //bottom-left corner
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.NEinner]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.SW]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.x += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        break;
                    case "ES": //bottom-left corner
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.NEinner]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.SW]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.x += 2;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        break;
                    
                    case "SW": //top-left corner
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.SEinner]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.NW]);
                        tilePlace.y -= 2;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.x += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.E]);
                        break;
                    case "EN": //top-left corner
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.SEinner]);
                        tilePlace.x -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.W]);
                        tilePlace.y += 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.NW]);
                        tilePlace.x += 2;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.N]);
                        tilePlace.y -= 1;
                        tilemap.SetTile(tilePlace, pathtiles[(int)TI.S]);
                        break;
                    default:
                        Debug.Log("This does not work!!");
                        break;
                }
            }
        }
        nodes.Last().name = "EndPosition";
    }
}

