using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject pathToken;
    [SerializeField] private GameObject endsToken;
    [SerializeField] private GameObject openToken;
    [SerializeField] private GameObject closedToken;
    [SerializeField] private float drawingSpeed;

    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    private Node[,] NodeMatrix;
    private int startPosx, startPosy;
    private int endPosx, endPosy;

    private List<Node> openNodeList;
    private List<Node> closeNodeList;

    void Awake()
    {
        Instance = this;
        Calculs.CalculateDistances(Panel, Size);
    }
    private void Start()
    {
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        }
        while (endPosx == startPosx || endPosy == startPosy);

        NodeMatrix = new Node[Size, Size];
        CreateNodes();

        StartCoroutine(DoAStarCalculations());
    }
    public void CreateNodes()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                // Assigna nodes al tauler
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i, j));
                // Assigna heurística a cada node
                NodeMatrix[i, j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i, j], endPosx, endPosy);
            }
        }

        Node initialNode = new Node(startPosx, startPosy, new Vector2(startPosx, startPosy));
        SetWays(initialNode, startPosx, startPosy);

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
                SetWays(NodeMatrix[i, j], i, j);
        }
    }

    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x > 0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (x < Size - 1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (y > 0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y < Size - 1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x < Size - 1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

    public IEnumerator DoAStarCalculations()
    {
        Node startNode = NodeMatrix[startPosx, startPosy];
        Node endNode = NodeMatrix[endPosx, endPosy];

        openNodeList = new List<Node>();
        closeNodeList = new List<Node>();

        startNode.AcumulatedCost = 0;
        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            // Tria node amb millor cost de la llista oberta (cost acumulat + heurística)
            Node current = openNodeList.OrderBy(n => n.FinalCost).ToList()[0];

            Instantiate(closedToken, current.RealPosition, Quaternion.identity);
            yield return new WaitForSeconds(drawingSpeed);

            if (current == endNode) break;

            openNodeList.Remove(current);
            closeNodeList.Add(current);

            // Posa per explorar els nodes veïns
            foreach (Way way in current.WayList)
            {
                Node surroundingNode = way.NodeDestiny;

                if (!closeNodeList.Contains(surroundingNode))
                {
                    float candidateAcumCost = current.AcumulatedCost + way.Cost;

                    if (!openNodeList.Contains(surroundingNode) || candidateAcumCost < surroundingNode.AcumulatedCost)
                    {
                        surroundingNode.AcumulatedCost = candidateAcumCost;
                        surroundingNode.NodeParent = current;

                        if (!openNodeList.Contains(surroundingNode))
                        {
                            openNodeList.Add(surroundingNode);

                            Instantiate(openToken, surroundingNode.RealPosition, Quaternion.identity);
                            yield return new WaitForSeconds(drawingSpeed);
                        }

                    }
                }
            }
        }

        StartCoroutine(DrawFinalPath(startNode, endNode));
    }

    private IEnumerator DrawFinalPath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node temp = endNode;

        while (temp != null)
        {
            path.Add(temp);
            temp = temp.NodeParent;
        }
        path.Reverse();

        foreach (Node node in path)
        {
            Instantiate(pathToken, node.RealPosition, Quaternion.identity);
            yield return new WaitForSeconds(drawingSpeed);
        }

        Instantiate(endsToken, startNode.RealPosition, Quaternion.identity);
        Instantiate(endsToken, endNode.RealPosition, Quaternion.identity);
    }
}