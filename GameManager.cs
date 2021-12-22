using System.Collections;                       //line  162 ~ 205   BfsPathFinding
using System.Collections.Generic;               //line  300 ~ 439   AstarPathFinding
using UnityEngine;                              //line  442 ~ 528   DfsPathFinding
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;                 // 다른 스크립트에서 참조하는 부분이 많아 싱글톤으로 구현
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    public struct Node
    {
        public Vector2 pos;
        public Vector2 parent;

        public int fcost;

        public int gcost;
        public int hcost;

    };
    
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform targetPos;
    [SerializeField] private Transform enemyPfb;

    [SerializeField] private Camera Cam;

    private float width;
    private float height;

    public int stage;
    private int count;

    public int score;
    public int life;
    public int gold;

    public int enemyCount;


    private bool onPlay;

    private bool isfind;
    

    private List<Node> closeNodes = new List<Node>();         
    private List<Node> pathNodes = new List<Node>();

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        
        width = targetPos.position.x - startPos.position.x;
        height = targetPos.position.y - startPos.position.y;

        onPlay = false;

        Node cur;
        cur.pos = new Vector2(startPos.position.x,startPos.position.y);
        cur.parent = new Vector2(0, 0); 

        for (int i = 0; i < width + 1; i++)                                                         // x,y 각각 맵 크기 + 1 만큼 closeNode List에 추가함
        {
            AddCloseNode(new Vector2(startPos.position.x + i, startPos.position.y - 1));
            AddCloseNode(new Vector2(startPos.position.x + i, startPos.position.y + height + 1));
        }

        for (int i = 0; i < height + 1; i++)
        {
            AddCloseNode(new Vector2(startPos.position.x -1, startPos.position.y + i));
            AddCloseNode(new Vector2(startPos.position.x + width + 1, startPos.position.y + i));
        }

    }
    // Update is called once per frame
    void Update()
    {


        if(onPlay)
        {
            Spawn();
            onPlay = false;
        }

        
    }
    
    private List<Node> PathFinding(Vector2 startPos)        // pathfinding / bfs방식으로 시작노드부터 주변의 노드들을 한칸씩 최종목적지를 찾을때 까지 탐색
    {
        sw.Start();
        int count = 0;
        List<Vector2> dir;
        List<Node> final;
        Queue<Node> queue = new Queue<Node>();
        List<Node> visited = new List<Node>();

        visited.Add(InitPathFinding(startPos));

        queue.Enqueue(visited[0]);
        
        var dir1 = new List<Vector2> {new Vector2(1,0) , new Vector2(0, 1), new Vector2(-1, 0) , new Vector2(0, -1) };
        var dir2 = new List<Vector2> { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };

        while (true)
        {

            Node cur;
            Node last;

            cur = queue.Dequeue();
            last = cur;

            dir = dir1;
            if (count % 2 == 0)
                dir = dir2;


            foreach (var i in dir)
            {
                if(cur.pos == new Vector2(targetPos.position.x,targetPos.position.y))
                {
                    final = AddFinalPath(cur, visited);
                    sw.Stop();
                    Debug.Log(final.Count);
                    Debug.Log(sw.ElapsedMilliseconds.ToString() + "ms");
                    return final;
                }

                cur.pos += i;

                if (NodeinList(closeNodes,cur))
                {
                    cur = last;
                    continue;
                }
                if (NodeinList(visited, cur))
                {
                    cur = last;
                    continue;
                }

                cur.parent = last.pos;
                queue.Enqueue(cur);

                if (!NodeinList(visited, cur))
                    visited.Add(cur);
                
                cur = last;              
            }

            count++;

        }


    }
    private Node InitPathFinding(Vector2 startPos)              // path를 찾기전 list를 초기화함
    {
        pathNodes.Clear();

        Node cur = InitNode(startPos);

        pathNodes.Add(cur);

        return cur;
    }
    private List<Node> AddFinalPath(Node n, List<Node> v)       // 최종 path를 저장하고 이를 return
    {
        List<Node> p = new List<Node>();
        p.Clear();
        p.Add(n);
        bool t = true ;
        while (t)
        {
            Node c = v.Find(x => x.pos == p[p.Count - 1].parent);

            if (c.pos == new Vector2(startPos.position.x, startPos.position.y))
            {
                t = false;
            }
            
            p.Add(c);
        }     

        p.Reverse();

        return p;
    }
    public List<Vector2> Path(Vector2 startPos)                 // 최종 path를 vector2의 list로 return
    {
        pathNodes = PathFinding(startPos);

        List<Vector2> p = new List<Vector2>();

        for (int i = 0; i < pathNodes.Count; i++)
        {
                p.Add(pathNodes[i].pos);
        }
        return p;
    }

    public bool OnMap(Vector2 pos)                                  // 위치를 매개변수로 받아 그 위치가 맵위에 있으면 true 아니면 false를 return
    {
        if(startPos.position.x <= pos.x && pos.x <= targetPos.position.x)
        {
            if (startPos.position.y <= pos.y && pos.y <= targetPos.position.y)
            {
                return true;
            }
        }

        return false;

    }
    
    private bool NodeinList(List<Node> ls, Node n)                  // 리스트안에 포지션이 겹치면 true 아니면 false를 return
    {
        List<Node> List;
        
        List = ls.FindAll(x => x.pos == n.pos);
        
        for(int i = 0; i < List.Count; i++)
        {
            if (List[i].pos == n.pos)
                return true;
        }

        return false;
    }
    private void Spawn()                                                    // 적을 스폰함
    {
        Instantiate(enemyPfb, startPos.position, Quaternion.identity);

    }

    
    public void AddCloseNode(Vector2 node)             // 유닛이 배치된 노드를 추가
    {
        Node n = InitNode(node);

        closeNodes.Add(n);
    }

    public void DeleteCloseNode(Vector2 node)           // 유닛이 배치된 노드를 삭제
    {
        Node n;
        n.pos = node;
        n.parent = new Vector2(0, 0);

        List<Node> List;

        List = closeNodes.FindAll(x => x.pos == n.pos);

        for (int i = 0; i < List.Count; i++)
        {
            if (List[i].pos == n.pos)
                closeNodes.Remove(List[i]);
        }

    }


    public Camera GetMainCam()                          // 메인 카메라를 return
    {
        return Cam;
    }

    public bool GetOnPlay()                             // 게임이 플레이중이면 true 아니면 false를 return
    {
        return onPlay;
    }

    public void GameStart()                             // 게임 시작 버튼 onPlay를 true로 변환
    {
        if (onPlay == false)
        {
            onPlay = true;
            
        }
    }

    private Node InitNode(Vector2 pos)
    {
        Node n;

        n.pos = pos;
        n.parent = new Vector2(0, 0);
        n.fcost = 0;
        n.hcost = 0;
        n.gcost = 0;

        return n;
    }




    public List<Vector2> AStartPath(Vector2 startpos)
    {
        sw.Start();
        List<Node> openList = new List<Node>();
        List<Node> closeList = new List<Node>();
        

        Node startNode = InitNode(startpos);

        openList.Add(startNode);


        int i = 20000;
        int j = 0;

        while (i > 0)
        {
            closeList.Add(openList[LowCostIndex(openList)]);
            if (openList.Count > 0) { 
                openList.Remove(closeList[closeList.Count - 1]);
                j++;
            }

            if (closeList[closeList.Count - 1].pos == new Vector2(targetPos.position.x, targetPos.position.y))
            {
                sw.Stop();
                Debug.Log(sw.ElapsedMilliseconds.ToString() + "ms");
                return GetFinalList(closeList,startpos,targetPos.position);
            }
            i--;
            openList.AddRange(AddNeighborNodes(closeList[closeList.Count - 1], openList, closeList));
        }


        return GetFinalList(closeList, startpos, targetPos.position);

    }

    private int LowCostIndex(List<Node> l)
    {
        int idx = 0;

        for (int i = 0; i < l.Count; i++)
        {
            if (l[i].fcost < l[idx].fcost)
                idx = i;
        }

        return idx;
    }

    private List<Node> AddNeighborNodes(Node startNode, List<Node> op, List<Node> cl)
    {


        List<Node> nodes = new List<Node>();
        Node n;
        Node cur;

        n = startNode;
        cur = n;

        var dir = new List<Vector2> { new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1) };

        foreach(var i in dir)
        {
            

            n.pos += i;

            if (NodeinList(closeNodes, n))
            {
                n = cur;
                continue;
            }
            if (NodeinList(op, n))
            {
                n = cur;
                continue;
            }
            if (NodeinList(cl, n))
            {
                n = cur;
                continue;
            }

            n = SetNode(cur, n);
            nodes.Add(n);
      
        }

        return nodes;
    }

    private List<Vector2> GetFinalList(List<Node> p, Vector2 start, Vector2 end)
    {
        List<Vector2> final = new List<Vector2>();
        Node n = p.Find(x => x.pos == end);

        int i = 0;
        while (i < 50)
        {
            n = p.Find(x => x.pos == n.parent);

            if (n.pos == start) i = 50;

            final.Add(n.pos);
            i++;
        }
        final.Reverse();
        return final;
    }

    private Node SetNode(Node lastNode, Node newNode)
    {
        
        Node n = new Node
        {
            pos = newNode.pos,
            parent = lastNode.pos
        };

        n.hcost = GetHCost(n);
        n.gcost = lastNode.gcost + 10;
        n.fcost = n.gcost + n.hcost;


        return n;
    }

    private int GetHCost(Node n)
    {
        int length_x = (int)(targetPos.position.x - n.pos.x);
        int length_y = (int)(targetPos.position.y - n.pos.y);

        int sum = (int)(Mathf.Pow(length_x, 2) + Mathf.Pow(length_y, 2));
        int result = (int)Mathf.Sqrt(sum);

        return result;
    }


    public List<Vector2> DfsPath(Vector2 startpos)
    {
        sw.Start();
        List<Vector2> final = new List<Vector2>();
        List<Node> path = new List<Node>();
        List<Node> visited = new List<Node>();
        Node cur = new Node() { pos = startpos };

        Dfsloop(path, visited, cur, startpos);

        for (int i = 0; i < path.Count; i++)
        {
            final.Add(path[i].pos);
        }
        Debug.Log(path.Count);
        sw.Stop();
        Debug.Log(sw.ElapsedMilliseconds.ToString() + "ms");
        return final;

    }

    private void Dfsloop(List<Node> p, List<Node> v,  Node cur, Vector2 startpos)
    {
        if (!(cur.pos == new Vector2(targetPos.position.x, targetPos.position.y)))
        {
            var dir = SetDirection(startpos, targetPos.position);
            Node last = cur;

            foreach (var i in dir)
            {
                if (isfind == true) continue;
                cur.pos += i;
                cur.parent = cur.pos;

                if (NodeinList(p, cur))
                {
                    cur = last;
                    continue;
                }

                if (NodeinList(closeNodes, cur))
                {
                    cur = last;
                    continue;
                }

                if (NodeinList(v,cur))
                {
                    cur = last;
                    continue;
                }

                p.Add(cur);
                v.Add(cur);
                Dfsloop(p, v, cur, startpos);
            }
            if (isfind == false)
            {
                p.Remove(cur);
                cur = p[p.Count - 1];
                Dfsloop(p, v, cur, startpos);
            }
        }
        else
            isfind = true;
    }

    private List<Vector2> SetDirection(Vector2 start, Vector2 end)
    {

        if((end.x - start.x > 0) && (end.y - start.y) > 0){
            return new List<Vector2>{ new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1) };
        }
        else if ((end.x - start.x > 0) && (end.y - start.y < 0))
        {
            return new List<Vector2> { new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1) };
        }
        else if ((end.x - start.x < 0) && (end.y - start.y > 0))
        {
            return new List<Vector2> { new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1) };
        }
        else
        {
            return new List<Vector2> { new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1) };
        }

    }

}
