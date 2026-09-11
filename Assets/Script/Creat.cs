using UnityEngine;
using Unity;
using System.Collections;
using System.Collections.Generic;
public class Creat : MonoBehaviour
{
    public GameObject MainCamera;
    [Min(1)]public int x;
    [Min(1)]public int y;
    [Min(0.01f)]public float stepDelay = 0.5f;
    public Node nodeprefab;
    public List<Node> nodes = new List<Node>(); 
    bool isSearching = false;
    void Start()
    {
        if(stepDelay <= 0f)
        {
            stepDelay = 0.5f;
        }
        MainCamera.transform.position = new Vector3(x, x+y, y);
        for(int i = 1; i <= x; i++)
        {
            for(int j = 1; j<=y; j++)
            {
                Node node =Instantiate(nodeprefab,new Vector3(i*2,0,j*2),Quaternion.identity);
                node.i = i;             // 缓存行列坐标，GetNeighbors 直接取用，无需按索引反解
                node.j = j;
                node.isInGrid = true;   // 标记为网格节点，允许运行时点击切换障碍
                nodes.Add(node);
            }
        }
    }
    void Update()
    {
        // 演示途中按 0：中断当前搜索并清空所有痕迹
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            StopAllCoroutines();
            isSearching = false;
            ResetNodes();
            return;
        }
        // 一段搜索没跑完之前忽略新按键，避免多段搜索叠加显示
        if(isSearching)
        {
            return;
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResetNodes();
            // 迷宫生成后 (1,1) 与 (x,y) 可能正好落在墙上，起终点改为自动挑可通行节点
            Node startNode = GetFirstNode();
            startNode.isStart = true;
            Node endNode = GetLastNode();
            endNode.isEnd = true;
            isSearching = true;
            StartCoroutine(DFS(startNode));
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ResetNodes();
            Node startNode = GetFirstNode();
            startNode.isStart = true;
            Node endNode = GetLastNode();
            endNode.isEnd = true;
            isSearching = true;
            StartCoroutine(BFS(startNode));
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            ResetNodes();
            Node startNode = GetFirstNode();
            startNode.isStart = true;
            Node endNode = GetLastNode();
            endNode.isEnd = true;
            isSearching = true;
            StartCoroutine(FindShortestPath(startNode, endNode));
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Creat_Maze();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // 与 1/2/3 一致：先清痕迹再置忙，Prim 内部还会把整张网格重置为全墙
            ResetNodes();
            isSearching = true;
            StartCoroutine(Prim(GetFirstNode()));
        }
    }
    // 按行列坐标(i, j)取节点，越界返回 null
    Node GetNode(int i, int j)
    {
        if(i < 1 || i > x || j < 1 || j > y)
        {
            return null;
        }
        return nodes[(i - 1) * y + (j - 1)];
    }
    // 第一个可通行节点：迷宫生成后 (1,1) 往往是墙，各算法改从它起步
    Node GetFirstNode()
    {
        foreach(Node node in nodes)
        {
            if(!node.isBarrier)
            {
                return node;
            }
        }
        return GetNode(1, 1);   // 全图皆墙时退回左上角，交给各算法自身的障碍判断兜底
    }
    // 最后一个可通行节点：作为终点，保证起点到终点之间有足够长的演示路径
    Node GetLastNode()
    {
        for(int k = nodes.Count - 1; k >= 0; k--)
        {
            if(!nodes[k].isBarrier)
            {
                return nodes[k];
            }
        }
        return GetNode(x, y);
    }
    // 取节点上下左右四个相邻节点（越界的已过滤）
    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        // 节点自身缓存的坐标，越界或未入网格（i/j 为 0）时 GetNode 返回 null，不会产生邻居
        if(node == null)
        {
            return neighbors;
        }
        Node up = GetNode(node.i, node.j + 1);
        Node down = GetNode(node.i, node.j - 1);
        Node left = GetNode(node.i - 1, node.j);
        Node right = GetNode(node.i + 1, node.j);
        if(up != null)
        {
            neighbors.Add(up);
        }
        if(down != null)
        {
            neighbors.Add(down);
        }
        if(left != null)
        {
            neighbors.Add(left);
        }
        if(right != null)
        {
            neighbors.Add(right);
        }
        return neighbors;
    }
    // 深度优先搜索：优先沿一条路径深入，走不通再回溯（每 stepDelay 秒推进一步）
    IEnumerator DFS(Node node)
    {
        if(node != null && !node.isFound && !node.isBarrier)
        {
            Stack<Node> stack = new Stack<Node>();
            node.isFound = true;
            stack.Push(node);
            while(stack.Count > 0)
            {
                Node current = stack.Pop();
                if(current.isEnd)
                {
                    break;
                }
                foreach(Node neighbor in GetNeighbors(current))
                {
                    if(neighbor.isFound || neighbor.isBarrier)
                    {
                        continue;
                    }
                    if(neighbor.isEnd)
                    {
                        break;
                    }
                    neighbor.isFound = true;
                    stack.Push(neighbor);
                }
                // 每探索完一个节点，停 stepDelay 秒再走下一步
                yield return new WaitForSeconds(stepDelay);
            }
        }
        isSearching = false;
    }
    // 广度优先搜索：按层向外扩散（每 stepDelay 秒推进一步）
    IEnumerator BFS(Node node)
    {
        if(node != null && !node.isFound && !node.isBarrier)
        {
            Queue<Node> queue = new Queue<Node>();
            node.isFound = true;
            queue.Enqueue(node);
            while(queue.Count > 0)
            {
                Node current = queue.Dequeue();
                if(current.isEnd)
                {
                    break;
                }
                foreach(Node neighbor in GetNeighbors(current))
                {
                    if(neighbor.isFound || neighbor.isBarrier)
                    {
                        continue;
                    }
                    neighbor.isFound = true;
                    queue.Enqueue(neighbor);
                }
                // 每扩散完一层中的一个节点，停 stepDelay 秒再走下一步
                yield return new WaitForSeconds(stepDelay);
            }
        }
        isSearching = false;
    }
    // 清空上一轮的搜索痕迹（保留障碍标记），让每次按键都能重新演示
    void ResetNodes()
    {
        foreach(Node node in nodes)
        {
            node.isFound = false;
            node.isPath = false;
            node.parent = null;
            node.isStart = false;
            node.isEnd = false;
        }
    }
    // 最短路径：BFS 逐层扩散先找到终点，再从终点沿 parent 一步步回溯点亮整条路径
    IEnumerator FindShortestPath(Node start, Node end)
    {
        if(start != null && end != null && !start.isBarrier && !end.isBarrier)
        {
            Queue<Node> queue = new Queue<Node>();
            start.parent = null;
            start.isFound = true;
            queue.Enqueue(start);
            bool reachable = false;
            while(queue.Count > 0)
            {
                Node current = queue.Dequeue();
                if(current.isEnd)
                {
                    reachable = true;
                    break;
                }
                foreach(Node neighbor in GetNeighbors(current))
                {
                    if(neighbor.isFound || neighbor.isBarrier)
                    {
                        continue;
                    }
                    neighbor.isFound = true;
                    neighbor.parent = current;
                    queue.Enqueue(neighbor);
                }
                // 搜索阶段：每步停 stepDelay 秒，能看清扩散过程
                yield return new WaitForSeconds(stepDelay);
            }
            if(reachable)
            {
                // 回溯阶段：从终点沿 parent 退回起点，边退边点亮
                int step = 0;
                for(Node current = end; current != null; current = current.parent)
                {
                    current.isPath = true;
                    step++;
                    yield return new WaitForSeconds(stepDelay);
                }
                Debug.Log("最短路径步数：" + (step - 1));
            }
            else
            {
                Debug.Log("未找到从起点到终点的路径");
            }
        }
        isSearching = false;
    }
    // 生成迷宫地图（按键 4）：先把整张网格重置为全墙，再以「奇数行、奇数列的交叉点」为格，
    // 用随机化深度优先（回溯法）从左上角格出发逐格凿墙，
    // 生成结果四邻域连通、无环、无孤立格，可直接配合 1/2/3 的搜索演示
    void Creat_Maze()
    {
        // 与 1/2/3/5 一致：先清掉上一轮留下的起点/终点/路径痕迹
        ResetNodes();
        // 1) 全墙起步：每次按 4 都是重新凿一张新迷宫，不会残留上一次的障碍标记
        foreach(Node node in nodes)
        {
            node.isBarrier = true;
        }
        Node root = GetNode(1, 1);
        if(root == null)
        {
            return;   // 网格为空（x 或 y 为 0）时直接返回
        }
        // 2) 起点格并入迷宫，isFound 在生成期间临时充当「已并入迷宫」标记
        root.isBarrier = false;
        root.isFound = true;
        Stack<Node> stack = new Stack<Node>();
        stack.Push(root);
        int cellCount = 1;
        while(stack.Count > 0)
        {
            Node current = stack.Peek();
            // 3) 收集还没并入迷宫的「隔一道墙」的相邻格（行列坐标各相差 2）
            List<Node> unvisited = new List<Node>();
            foreach(Node cell in GetCellNeighbors(current))
            {
                if(!cell.isFound)
                {
                    unvisited.Add(cell);
                }
            }
            if(unvisited.Count == 0)
            {
                stack.Pop();    // 四周的格都已在迷宫里，回溯到上一个格
                continue;
            }
            // 4) 随机挑一个相邻格，打通两格正中间的那道墙
            Node next = unvisited[Random.Range(0, unvisited.Count)];
            Node wall = GetNode((current.i + next.i) / 2, (current.j + next.j) / 2);
            if(wall != null)
            {
                wall.isBarrier = false;
            }
            next.isBarrier = false;
            next.isFound = true;
            stack.Push(next);
            cellCount++;
        }
        // 5) 清掉生成期间留下的临时标记：isFound/isPath 要留给后面的 1/2/3 搜索使用
        int passable = 0;
        foreach(Node node in nodes)
        {
            node.isFound = false;
            node.isPath = false;
            node.parent = null;
            if(!node.isBarrier)
            {
                passable++;
            }
        }
        Debug.Log("迷宫生成完成：连通格数 " + cellCount + "，可通行节点 " + passable);
    }
    // 随机化 Prim 迷宫：先把整张网格重置为全墙，再从左上的「格」起步，
    // 每步随机挑一个「待选前沿」格，打通它与已连通格之间的那道墙（每 stepDelay 秒凿一步）
    // 迷宫以「奇数行、奇数列的交叉点」为格，格与格之间的偶数行/列为墙，
    // 生成结果四邻域连通且无环无孤立格，可直接配合 1/2/3 的搜索演示
    IEnumerator Prim(Node startNode)
    {
        ResetNodes();
        // 1) 全墙起步：迷宫完全从零开始凿，保证每次按 5 都能看到完整生成过程
        foreach(Node node in nodes)
        {
            node.isBarrier = true;
        }
        // 2) 起点格：格必须落在奇数行列上，传进来的节点若是偶数坐标就向内收一格
        int ci = startNode != null ? startNode.i : 1;
        int cj = startNode != null ? startNode.j : 1;
        if(ci % 2 == 0)
        {
            ci = ci - 1;
        }
        if(cj % 2 == 0)
        {
            cj = cj - 1;
        }
        Node root = GetNode(ci, cj);
        if(root == null)
        {
            root = GetNode(1, 1);
        }
        // 3) 起点格并入迷宫（isFound 标记「已连通」），它的隔墙邻居全部进入待选前沿
        root.isBarrier = false;
        root.isFound = true;
        List<Node> frontier = new List<Node>();
        AddFrontierCells(root, frontier);
        int cellCount = 1;      // 已并入迷宫的格数
        Node lastPicked = null; // 上一步凿开的格，用来取消品红高亮
        yield return new WaitForSeconds(stepDelay);

        while(frontier.Count > 0)
        {
            // 4) 在前沿里随机挑一个格（随机化 Prim：等概率随机挑边，而不是挑最小权重边）
            int pick = Random.Range(0, frontier.Count);
            Node cell = frontier[pick];
            frontier.RemoveAt(pick);
            if(cell.isFound)
            {
                continue;   // 已被别的分支先凿通，跳过（前沿里允许有重复格）
            }
            // 5) 在这个格周围已连通的格里随机挑一个，打通两格正中间的那道墙
            List<Node> linkedCells = new List<Node>();
            foreach(Node neighbor in GetCellNeighbors(cell))
            {
                if(neighbor.isFound)
                {
                    linkedCells.Add(neighbor);
                }
            }
            if(linkedCells.Count == 0)
            {
                continue;   // 兜底：理论上不会出现，防御性跳过
            }
            Node from = linkedCells[Random.Range(0, linkedCells.Count)];
            Node wall = GetNode((cell.i + from.i) / 2, (cell.j + from.j) / 2);
            if(wall != null)
            {
                wall.isBarrier = false;  // 打通两道墙之间的通道
            }
            cell.isBarrier = false;      // 打通这个格本身
            cell.isFound = true;
            cellCount++;
            // 6) 高亮当前凿开的格，并把它的隔墙邻居补进前沿
            if(lastPicked != null)
            {
                lastPicked.isPath = false;
            }
            cell.isPath = true;
            lastPicked = cell;
            AddFrontierCells(cell, frontier);
            // 每凿通一个格，停 stepDelay 秒再凿下一个
            yield return new WaitForSeconds(stepDelay);
        }
        if(lastPicked != null)
        {
            lastPicked.isPath = false;
        }
        Debug.Log("Prim 迷宫生成完成：连通格数 " + cellCount);
        isSearching = false;
    }
    // 取某个格「隔一道墙」的四个相邻格（即行列坐标各相差 2 的节点）
    List<Node> GetCellNeighbors(Node cell)
    {
        List<Node> cells = new List<Node>();
        if(cell == null)
        {
            return cells;
        }
        Node up = GetNode(cell.i, cell.j + 2);
        Node down = GetNode(cell.i, cell.j - 2);
        Node left = GetNode(cell.i - 2, cell.j);
        Node right = GetNode(cell.i + 2, cell.j);
        if(up != null)
        {
            cells.Add(up);
        }
        if(down != null)
        {
            cells.Add(down);
        }
        if(left != null)
        {
            cells.Add(left);
        }
        if(right != null)
        {
            cells.Add(right);
        }
        return cells;
    }
    // 把某个格的隔墙邻居加入待选前沿（已并入迷宫的、以及已在前沿里的都不重复加）
    void AddFrontierCells(Node cell, List<Node> frontier)
    {
        foreach(Node neighbor in GetCellNeighbors(cell))
        {
            if(neighbor.isFound || frontier.Contains(neighbor))
            {
                continue;
            }
            frontier.Add(neighbor);
        }
    }
}
