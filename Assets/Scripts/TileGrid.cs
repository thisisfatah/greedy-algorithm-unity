using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    public class TileGrid : MonoBehaviour
    {
        private const int TileWeight_Default = 1;
        private const int TileWeight_Expensive = 50;
        private const int TileWeight_Infinity = int.MaxValue;

        public int Rows;
        public int Cols;
        public GameObject TilePrefab;

        private Vector2 endVal = new Vector2(10,14);

        public Color TileColor_Default = new Color(0.86f, 0.83f, 0.83f);
        public Color TileColor_Expensive = new Color(0.19f, 0.65f, 0.43f);
        public Color TileColor_Infinity = new Color(0.37f, 0.37f, 0.37f);
        public Color TileColor_Start = Color.green;
        public Color TileColor_End = Color.red;
        public Color TileColor_Path = new Color(0.73f, 0.0f, 1.0f);
        public Color TileColor_Visited = new Color(0.75f, 0.55f, 0.38f);
        public Color TileColor_Frontier = new Color(0.4f, 0.53f, 0.8f);

        public Tile[] Tiles { get; private set; }

		private Tile start;
		private Tile end;

        private Tile endPrev;

        private List<Tile> pathTileList = new List<Tile>();
        private int currentPathIndex;

        [SerializeField] private GameObject enemyObj;

		[SerializeField] private GameObject playerObj;

		private void Awake()
        {
            Tiles = new Tile[Rows * Cols];
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Tile tile = new Tile(this, r, c, TileWeight_Default);
					tile.IsWalkingable = true;
					tile.InitGameObject(transform, TilePrefab);

                    int index = GetTileIndex(r, c);
                    Tiles[index] = tile;
                }
            }


            CreateExpensiveArea(3, 3, 9, 1, TileWeight_Expensive);
            CreateExpensiveArea(3, 9, 1, 9, TileWeight_Expensive);
            ResetGrid();

            for (int i = 0; i < Tiles.Length; i++)
            {
                Debug.Log($"Tile({Tiles[i].Row}, {Tiles[i].Col})");
            }

			start = GetTile(9, 2);
			end = GetTile((int)endVal.y, (int)endVal.x);
		}

		private void Start()
		{
			enemyObj.transform.position = start.Pos;
			playerObj.transform.position = end.Pos;

		}

		private void Update()
        {
            if (!GameManager.IsGameReady) return;
            
			if (Input.GetKeyDown(KeyCode.W))
			{
                ResetGrid();
                endVal.y -= 1;
                if(endVal.y < 0)
                {
                    endVal.y = 0;
                }
                Mathf.Abs(endVal.y);
				end = GetTile((int)endVal.y, (int)endVal.x);
				playerObj.transform.position = end.Pos;
			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				ResetGrid();
                endVal.y += 1;
				if (endVal.y >= Rows - 1)
				{
					endVal.y = Rows - 1;
				}
				Mathf.Abs(endVal.y);
				end = GetTile((int)endVal.y, (int)endVal.x);
				playerObj.transform.position = end.Pos;
			}
            else if(Input.GetKeyDown(KeyCode.D))
            {
				ResetGrid();
				endVal.x += 1;
				if (endVal.x >= Rows - 1)
				{
					endVal.x = Rows - 1;
				}
				Mathf.Abs(endVal.x);
				end = GetTile((int)endVal.y, (int)endVal.x);
				playerObj.transform.position = end.Pos;
			}
            else if (Input.GetKeyDown(KeyCode.A))
            {
				ResetGrid();
				endVal.x -= 1;
				if (endVal.x < 0)
				{
					endVal.x = 0;
				}
				Mathf.Abs(endVal.x);
				end = GetTile((int)endVal.y, (int)endVal.x);
				playerObj.transform.position = end.Pos;
			}

			if (end != endPrev)
			{
				endPrev = end;
				pathTileList = FindPath(start, end, PathFinder.FindPath_GreedyBestFirstSearch);
			}

			HandleMovement();
        }

        private void HandleMovement()
        {
            if(pathTileList.Count > 0)
            {
                Vector2 targetPos = pathTileList[currentPathIndex].Pos;
                if(Vector2.Distance((Vector2)enemyObj.transform.position, targetPos) > 0.1f)
                {
                    Vector2 moveDir = (targetPos - (Vector2)enemyObj.transform.position).normalized;

                    float distance = Vector2.Distance((Vector2)enemyObj.transform.position, targetPos);
					enemyObj.transform.position = enemyObj.transform.position + new Vector3(moveDir.x, moveDir.y, 0) * 2f * Time.deltaTime;
                }
                else
                {
                    currentPathIndex++;
                    
					if (currentPathIndex >= pathTileList.Count)
                    {
                        GameManager.Instance.GameOver();
                        RestartGame();
						pathTileList.Clear();
                    }
                    else
                    {
						start = pathTileList[currentPathIndex];
					}
                }
            }
        }

        private void RestartGame()
        {
			endVal = new Vector2(10, 14);
			start = GetTile(9, 2);
			end = GetTile((int)endVal.y, (int)endVal.x);

			enemyObj.transform.position = start.Pos;
			playerObj.transform.position = end.Pos;
		}


        private void CreateExpensiveArea(int row, int col, int width, int height, int weight)
        {
            for (int r = row; r < row + height; r++)
            {
                for (int c = col; c < col + width; c++)
                {
                    Tile tile = GetTile(r, c);
                    if (tile != null)
                    {
                        tile.IsWalkingable = false;
                        tile.Weight = weight;
                    }
                }
            }
        }

        private void ResetGrid()
        {
            foreach (var tile in Tiles)
            {
                tile.Cost = 0;
                tile.PrevTile = null;
                tile.SetText("");

                switch (tile.Weight)
                {
                    case TileWeight_Default:
                        tile.SetColor(TileColor_Default);
                        break;
                    case TileWeight_Expensive:
                        tile.SetColor(TileColor_Expensive);
                        break;
                    case TileWeight_Infinity:
                        tile.SetColor(TileColor_Infinity);
                        break;
                }
            }
        }

        private List<Tile> FindPath(Tile start, Tile end, Func<TileGrid, Tile, Tile, List<IVisualStep>, List<Tile>> pathFindingFunc)
        {
            ResetGrid();

            currentPathIndex = 0;
            pathTileList.Clear();

			List<IVisualStep> steps = new List<IVisualStep>();
            List<Tile> paths = pathFindingFunc(this, start, end, steps);
            List<Tile> result = new List<Tile>();

			foreach (var step in steps)
            {
                step.Execute();
            }

			foreach (var path in paths)
			{
				result.Add(path);
			}
			pathTileList.AddRange(result);


			return result;
		}

        public Tile GetTile(int row, int col)
        {
            if (!IsInBounds(row, col))
            {
                return null;
            }

            return Tiles[GetTileIndex(row, col)];
        }

        public IEnumerable<Tile> GetNeighbors(Tile tile)
        {
            Tile right = GetTile(tile.Row, tile.Col + 1);
            if (right != null)
            {
                yield return right;
            }

            Tile up = GetTile(tile.Row - 1, tile.Col);
            if (up != null)
            {
                yield return up;
            }

            Tile left = GetTile(tile.Row, tile.Col - 1);
            if (left != null)
            {
                yield return left;
            }

            Tile down = GetTile(tile.Row + 1, tile.Col);
            if (down != null)
            {
                yield return down;
            }
        }

        private bool IsInBounds(int row, int col)
        {
            bool rowInRange = row >= 0 && row < Rows;
            bool colInRange = col >= 0 && col < Cols;
            return rowInRange && colInRange;
        }

        private int GetTileIndex(int row, int col)
        {
            return row * Cols + col;
        }
    }
}
