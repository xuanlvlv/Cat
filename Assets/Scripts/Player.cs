using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 玩家控制
/// </summary>
public class Player : TileEntity
{
    public GameScene gameScene;

    public LayerMask obstacleLayer; // 障碍物的层
    private float checkRadius = 0.05f; // 碰撞检测的半径

    // 玩家数字，0表示无限大(∞)
    public int playerNumber = 0;   
    public GameObject numberObj;
    private SpriteRenderer numberSprite;
    public List<Sprite> numberSprites;
    // 无限大图标的索引
    [SerializeField] private int infinitySymbolIndex = 0;

    private OperatorType operatorType;
    public OperatorType OperatorType { get { return operatorType; } set { operatorType = value; } }

    // 获取移动方向
    Vector3Int direction = Vector3Int.zero;
    
    // 移动冷却时间，防止连续移动过快
    private float moveCooldown = 0.15f;
    private float lastMoveTime = 0f;
    
    // 是否正在移动的标志
    private bool isMoving = false;
    private Vector3 targetPosition;
    [SerializeField] private float moveSpeed = 8f;

    void Start()
    {
        gameScene = GameObject.Find("GameScene").GetComponent<GameScene>();
        numberSprite = numberObj.GetComponent<SpriteRenderer>();
        
        // 初始化无限大数字
        playerNumber = 0;
        targetPosition = transform.position;
        
        // 初始化游戏数据
        InitializeGameData();
    }
    
    /// <summary>
    /// 初始化游戏数据
    /// </summary>
    private void InitializeGameData()
    {
        // 确保UIManager已初始化
        if (UIManager.Instance != null)
        {
            // 开始新回合，重置心情值和播放数
            UIManager.Instance.StartNewRound();
        }
    }

    void Update()
    {
        // 如果正在移动动画中，先完成移动
        if (isMoving)
        {
            MoveAnimation();
            return;
        }
        
        // 检测按键输入
        DetectKeyboardInput();
        
        // 更新数字显示
        UpdateNumberSprite();
    }
    
    // 更新数字显示
    private void UpdateNumberSprite()
    {
        // 根据玩家数字显示对应的精灵
        if (playerNumber == 0)
        {
            // 无限大使用索引0的精灵
            numberSprite.sprite = numberSprites[0];
        }
        else if (playerNumber >= 0 && playerNumber < numberSprites.Count)
        {
            numberSprite.sprite = numberSprites[playerNumber];
        }
    }

    /// <summary>
    /// 检测键盘输入并移动
    /// </summary>
    private void DetectKeyboardInput()
    {
        // 如果冷却时间未到，不处理输入
        if (Time.time < lastMoveTime + moveCooldown)
            return;

        Vector3Int moveDirection = Vector3Int.zero;

        // 检测WASD键或方向键
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection = Vector3Int.up;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection = Vector3Int.down;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection = Vector3Int.left;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection = Vector3Int.right;
        }

        // 如果有方向输入，尝试移动
        if (moveDirection != Vector3Int.zero)
        {
            direction = moveDirection;
            
            // 先检查前方是否有数字方块可以交互
            if (TryInteractWithTile(direction))
            {
                lastMoveTime = Time.time;
            }
            else
            {
                // 如果没有可交互方块，尝试移动
                if (TryMove(direction))
                {
                    lastMoveTime = Time.time;
                }
            }
        }
    }

    /// <summary>
    /// 尝试与方向上的方块交互
    /// </summary>
    /// <param name="direction">移动方向</param>
    /// <returns>是否成功交互</returns>
    private bool TryInteractWithTile(Vector3Int direction)
    {
        Vector3 targetPosition = transform.position + new Vector3(direction.x, direction.y, 0);
        
        // 在目标位置查找精灵对象
        GameObject targetObject = GetObjectAtPosition(targetPosition);
        
        // 如果目标位置没有对象，直接返回false
        if (targetObject == null)
            return false;

        // 检查是否是数字方块
        if (targetObject.TryGetComponent<NumTile>(out var numTile))
        {
            // 无限大可以与任何数字交互，获得对方的数字
            if (playerNumber == -1)
            {
                // 播放消除音效
                // TODO: 添加消除音效

                // 增加播放数
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.AddViews(numTile.num);
                }

                // 记录目标位置
                Vector3 oldPosition = targetObject.transform.position;

                // 删除目标方块
                Destroy(targetObject);

                // 更新玩家数字并移动
                playerNumber = numTile.num;

                // 设置目标位置并启动移动动画
                StartMoving(oldPosition);

                return true;
            }

            // 只有当玩家数字大于等于方块数字时才能交互
            if (playerNumber >= numTile.num || playerNumber == 0)
            {
                // 增加播放数
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.AddViews(numTile.num);
                }

                // 根据运算符处理数字
                switch (operatorType)
                {
                    case OperatorType.Add:
                        playerNumber += numTile.num;
                        break;

                    case OperatorType.Sub:
                        playerNumber -= numTile.num;
                        break;

                    default:
                        Debug.LogWarning("未知的运算符类型");
                        return false;
                }

                // 处理运算结果
                ProcessNumberResult();

                // 播放消除音效
                // TODO: 添加消除音效

                // 记录目标位置
                Vector3 oldPosition = targetObject.transform.position;

                // 删除目标方块
                Destroy(targetObject);

                // 设置目标位置并启动移动动画
                StartMoving(oldPosition);

                return true;
            }

            // 玩家数字小于方块数字，不能交互
            return false;
        }
        else if (targetObject.TryGetComponent<PropTile>(out var propTile))
        {
            // 处理道具方块
            propTile.Explode();

            // 设置目标位置并启动移动动画
            StartMoving(propTile.transform.position);

            return true;
        }
        else if (targetObject.TryGetComponent<SpecialTile>(out var specialTile))
        {
            specialTile.TriggerEffect();

            // 修正：使用specialTile而不是propTile
            StartMoving(specialTile.transform.position);

            return true;
        }
        // 其他类型的方块，无法交互
        return false;
    }
    
    /// <summary>
    /// 处理数字运算结果，应用游戏规则
    /// </summary>
    private void ProcessNumberResult()
    {
        if (playerNumber <= 0)
        {
            playerNumber = Mathf.Abs(playerNumber);
        }

        // 结果等于10时，变成无限大
        if (playerNumber == 10)
        {
            playerNumber = 0; // 使用0表示无限大
        }
        
        // 结果大于10时，取个位数
        if (playerNumber > 10)
        {
            playerNumber = playerNumber % 10;
        }
    }

    /// <summary>
    /// 开始移动到指定位置
    /// </summary>
    private void StartMoving(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
    }
    
    /// <summary>
    /// 移动动画
    /// </summary>
    private void MoveAnimation()
    {
        // 使用MoveTowards平滑移动到目标位置
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
        
        // 当到达目标位置时停止移动
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    /// <summary>
    /// 尝试移动到指定方向
    /// </summary>
    /// <param name="direction">移动方向</param>
    /// <returns>是否成功移动</returns>
    private bool TryMove(Vector3Int direction)
    {
        Vector3 newPosition = transform.position + new Vector3(direction.x, direction.y, 0);
        
        // 检查目标位置是否有障碍物或墙壁
        if (IsPositionBlocked(newPosition))
        {
            return false;
        }
        
        // 设置目标位置并启动移动动画
        StartMoving(newPosition);
        return true;
    }
    
    /// <summary>
    /// 检查位置是否被阻挡
    /// </summary>
    private bool IsPositionBlocked(Vector3 position)
    {
        // 检查是否有障碍物
        Collider2D obstacle = Physics2D.OverlapCircle(position, checkRadius, obstacleLayer);
        if (obstacle != null)
        {
            return true;
        }
        
        // 检查是否有墙壁或其他不可交互的方块
        GameObject obj = GetObjectAtPosition(position);
        if (obj != null)
        {
            // 墙壁总是阻挡移动
            if (obj.GetComponent<WallTile>() != null)
            {
                return true;
            }
            
            // 检查数字方块，只有当玩家数字比方块数字小时才阻挡
            NumTile numTile = obj.GetComponent<NumTile>();
            if (numTile != null && playerNumber != -1 && playerNumber < numTile.num)
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// 在指定位置查找游戏对象
    /// </summary>
    private GameObject GetObjectAtPosition(Vector3 position)
    {
        // 使用射线检测找到位置上的游戏对象
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        return hit.collider != null ? hit.collider.gameObject : null;
    }
}

