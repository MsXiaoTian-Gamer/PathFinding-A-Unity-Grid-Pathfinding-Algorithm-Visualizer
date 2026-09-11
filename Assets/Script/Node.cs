using UnityEngine;

public class Node : MonoBehaviour
{
    public GameObject nodeprefab;
    public bool isFound = false;
    public bool isBarrier = false;
    public bool isStart = false;
    public bool isEnd = false;
    public bool isPath = false;
    // 是否属于网格中的节点（场景里那个模板节点不会被标记，不参与点击切换）
    public bool isInGrid = false;
    // 网格中的行列坐标，生成时由 Creat 赋值，供 GetNeighbors 直接取用
    public int i = 0;
    public int j = 0;
    // Prim 最小生成树：节点权重、连到生成树的最小边代价、是否处于待选前沿
    public int weight = 1;
    public float key = float.MaxValue;
    public bool isFrontier = false;
    public Node parent = null;
    public Vector3 position;
    public Color defaultColor = Color.white;
    private Renderer rend;
    public void Start()
    {
        nodeprefab = this.gameObject;
        position = nodeprefab.transform.position;
        rend = transform.GetComponent<Renderer>();
        defaultColor = rend.material.color;
    }
    public void Update()
    {
        if(isStart)
        {
            rend.material.color = Color.blue;
        }
        else if(isEnd)
        {
            rend.material.color = Color.yellow;
        }
        else if(isBarrier)
        {
            rend.material.color = Color.red;
        }
        else if(isPath)
        {
            rend.material.color = Color.magenta;
        }
        else if(isFrontier)
        {
            rend.material.color = Color.cyan;   // Prim：待选前沿
        }
        else if(isFound)
        {
            rend.material.color = Color.green;
        }
        else
        {
            rend.material.color = defaultColor;
        }
    }
    // 运行时点击节点：在「障碍 / 非障碍」之间切换
    // 节点自带 BoxCollider，主相机射线命中碰撞体时 Unity 会自动调用本事件
    public void OnMouseDown()
    {
        if(!isInGrid)
        {
            return;
        }
        // 只翻转障碍标记，不碰 isFound/isPath/parent，避免干扰进行中的搜索协程
        // 颜色在 Update 里按 isBarrier 优先级即时刷新为红色
        isBarrier = !isBarrier;
    }
}
