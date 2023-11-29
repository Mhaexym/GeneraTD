using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class PathManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject node;
    public List<GameObject> nodeOut;
    private List<GameObject> nodes = new List<GameObject>();
    private Tilemap tilemap;
    public TileBase[] pathtiles;
    public int centerX;
    private int centerY; //Never changes.
    private enum TI {N, E, S, W, NW, NE, SE, SW, NWinner, NEinner, SEinner, SWinner}

    private bool justMoved = false;
    public void addToCenterX(int i)
    {
        if ((int) Camera.main.transform.position.x > centerX)
        {
            centerX += i;
            justMoved = true;
        }
    }

    public void AddPath()
    {
        if ((int)Camera.main.transform.position.x == centerX && justMoved)
        {
            justMoved = false;
            Debug.Log("It does work!");
            Vector3 start = nodes.Last().transform.position;
            nodes.Last().name = "W";
            if (start.x < centerX - 15
            ) //Exception case whenever previous EndPosition is not FULLY in the current map range. 
            {
                start.x += 3;
                nodes.Add(Instantiate(node, start, Quaternion.identity));
                nodes.Last().name = "W";
            }

            GenerateNodes(nodes, start);
            nodeOut = nodes;
        }
    }
    private void Start()
    {
        centerX = (int)this.transform.position.x;
        centerY = (int)this.transform.position.y;
        tilemap = this.GetComponentInParent<Tilemap>();
        GenerateNodes(nodes, InitializePath(nodes));
        nodeOut = nodes;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
    
    private List<char> CheckActions(int x, int y, List<Vector2Int> nodes)
    {
        List<char> actions = new List<char>{'U', 'R', 'D', 'L'};
        foreach(Vector2Int v in nodes)
        {
            int step = 3;
            int down = y - step;
            int up = y + step;
            int right = x + step;
            int left = x - step;
            if (v.x == right && v.y == y) actions.Remove('R');
            if (v.x == left && v.y == y || left < centerX - 15) actions.Remove('L');
            if (v.y == up && v.x == x || up > centerY + 8) actions.Remove('U');
            if (v.y == down && v.x == x || down < centerY - 8) actions.Remove('D');
        }

        return actions;
    }

    private Vector2Int TakeAction(int x, int y, char action)
    {
        switch (action)
        {
            case 'U':
                y += 3;
                break;
            case 'D':
                y -= 3;
                break;
            case 'L':
                x -= 3;
                break;
            case 'R':
                x += 3;
                break;
        }
        
        return new Vector2Int(x,y);
    }

    private Vector3 InitializePath(List<GameObject> nodes) // For running it the first time, sets spawn and second tile and returns the location (Vector3) of that second tile.
    {
        Vector3 curr = new Vector3();
        if (nodes.Count == 0) // Redundant check, not sure if necessary.
        {
            int rand_y = centerY + Random.Range(-8, 9);
            Vector3 start = new Vector3(centerX-18, rand_y, 300);
            nodes.Add((GameObject) Instantiate(node, start, Quaternion.identity));
            nodes[0].name = "W";
            Vector3 second = new Vector3(centerX-15, rand_y, 300);
            nodes.Add((GameObject) Instantiate(node, second, Quaternion.identity));
            nodes[1].name = "W";
            curr = second;
        }

        return curr;
    }
    private void GenerateNodes(List<GameObject> nodes, Vector3 s) //taking in the nodeslist to add to, and the starting position from which to generate the path.
    {
        //Initialisation of loop variables
        string currentpath = "";
        Stack<string> branches = new Stack<string>();
        Stack<string> bannedActionBranches = new Stack<string>();
        string bannedactions = "";
        char action;
        Vector2Int currCoords = new Vector2Int((int) s.x, (int) s.y);
        Stack<Vector2Int> branchCoords = new Stack<Vector2Int>();
        List<Vector2Int> nodeCoords = new List<Vector2Int>();
        nodeCoords.Add(currCoords);
        Stack<int> branchLengths = new Stack<int>();
        int branchLength = 0;
        int max = 20000;
        int j = 0;
        bool justPoppedBack = false;
        
        //Loop to find a valid path with some randomness built in. 
        while (j < max)
        {
            if (currCoords.x > centerX + 15) break; //We've finished.
            List<char> possActions = CheckActions(currCoords.x, currCoords.y, nodeCoords);
            foreach (var c in bannedactions)
            {
                possActions.Remove(c);
            }

            if (possActions.Count > 1)
            {
                string debug = "";
                foreach (var c in possActions) debug += c;
                //ALL OF THIS CODE SHOULD CREATE NEW STACK ELEMENTS FOR ALL BRANCH STACKERS. 
                branches.Push(currentpath);
                branchCoords.Push(currCoords);
                //We need a new branch to be formed here everytime, that also means a new branchlength
                branchLengths.Push(branchLength);
                branchLength = 0;
                action = possActions[Random.Range(0, possActions.Count)];
                Debug.Log("Found multiple options in " + currCoords + " with currentpath " + currentpath + " and banned actions are " + bannedactions + " leading to only possible actions: " + debug + " and we chose action " + action);
                currCoords = TakeAction(currCoords.x, currCoords.y, action);
                nodeCoords.Add(currCoords);
                currentpath += action;
                
                bannedactions += action;
                bannedActionBranches.Push(bannedactions);
                bannedactions = "";
                
            }
            else if (possActions.Count == 1)
            {
                //IT IS WRONG THAT IT SHOULD NOT CREATE BRANCHES, AS ONE OPTION LEFT WITH MULTIPLE OPTIONS BANNED IS STILL A BRANCH!! 
                //So: 
                
                action = possActions[0];
                Debug.Log("Found only a single option in " + currCoords + " with currentpath " + currentpath + " and banned actions are " + bannedactions + " leading to only possible actions: " + possActions[0]);
                if (justPoppedBack)
                {
                    bannedactions += possActions[0];
                    bannedActionBranches.Push(bannedactions);
                    bannedactions = "";
                    branches.Push(currentpath);
                    branchCoords.Push(currCoords);
                    branchLengths.Push(branchLength);
                    justPoppedBack = false;
                }
                currCoords = TakeAction(currCoords.x, currCoords.y, action);
                nodeCoords.Add(currCoords);
                //No branch needs to be formed as there is only one possible path, we do update the branchlength though. 
                branchLength = branchLengths.Pop();
                branchLength++;
                branchLengths.Push(branchLength);
                currentpath += action;
            }
            else // Oftewel fokking 0 mogelijkheden G 
            {
                currentpath = branches.Pop();
                bannedactions = bannedActionBranches.Pop();
                currCoords = branchCoords.Pop();
                justPoppedBack = true;
                Debug.Log(branchLengths.Count);
                if (branchLengths.Count != 0)
                {
                    Debug.Log(branchLengths.Peek() + " for nodeCoordinate list " + nodeCoords.Count + " On node coordinates " + currCoords);
                }
                nodeCoords = nodeCoords.GetRange(0, nodeCoords.Count -1 - (branchLengths.Pop() -1));
                branchLength = 0;
            }
            j++;
        }
        Debug.Log(currentpath);
        foreach (char c in currentpath)
        {
            switch (c) // Node creation based on found path 
            {
                case 'U':
                    s.y += 3;
                    nodes.Add((GameObject) Instantiate(node, s, Quaternion.identity));
                    nodes.Last().name = "S";
                    break;
                case 'D':
                    s.y -= 3;
                    nodes.Add((GameObject) Instantiate(node, s, Quaternion.identity));
                    nodes.Last().name = "N";
                    break;
                case 'L':
                    s.x -= 3;
                    nodes.Add((GameObject) Instantiate(node, s, Quaternion.identity));
                    nodes.Last().name = "E";
                    break;
                case 'R':
                    s.x += 3;
                    nodes.Add((GameObject) Instantiate(node, s, Quaternion.identity));
                    nodes.Last().name = "W";
                    break;
            }
            
        }
        int index = 0;
        foreach (GameObject n in nodes)
        {
            n.transform.SetParent(this.transform);
            Vector3Int tilePlace = Vector3Int.FloorToInt(n.transform.position);
            if (index < nodes.Count)
            {
                string p;
                string q;
                if (index == nodes.Count - 1)
                {
                    p = "W";
                    q = "W";
                }  //exception case for last node
                else
                {
                    p = nodes[index].name;
                    q = nodes[index + 1].name;
                }
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

        nodes.First().name = "SpawnPosition";
        nodes.Last().name = "EndPosition";
    }
}

