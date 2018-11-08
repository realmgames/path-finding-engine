/*
    MIT License

    Copyright (c) 2018 William Herrera

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using UnityEngine;
using System.Collections.Generic;

namespace RealmGames.PathFinding
{
    public delegate bool CanTraverse(Vector2Int cellPosition);
    public delegate Vector2Int[] Moves(Vector2Int cellPosition);

    public class PathFindingEngine
    {
        private RectInt m_bounds;
        private Vector2Int m_offset;
        private bool[] m_visited;
        private CanTraverse m_canTraverse;
        private Moves m_moves;

        public PathFindingEngine(RectInt bounds, CanTraverse canTraverse, Moves moves)
        {
            m_bounds = bounds;
            m_offset = new Vector2Int(-bounds.position.x, -bounds.position.y);
            m_canTraverse = canTraverse;
            m_moves = moves;
            m_visited = new bool[bounds.width * bounds.height];
        }

        public PathFindingEngine(Vector2Int size, CanTraverse canTraverse, Moves moves) : this(new RectInt(Vector2Int.zero, size),
                                                                                  canTraverse,
                                                                                  moves) { }

        public PathFindingEngine(Vector2Int size, CanTraverse canTraverse) : this(size, canTraverse, RectangleMovement) { }

        public static Vector2Int[] RectangleMovement(Vector2Int p)
        {
            return new Vector2Int[] {
                Vector2Int.right + p,
                Vector2Int.left + p,
                Vector2Int.up + p,
                Vector2Int.down + p
            };
        }

        public static Vector2Int[] DiagonalMovement(Vector2Int p)
        {
            return new Vector2Int[] {
                Vector2Int.right + p,
                Vector2Int.left + p,
                Vector2Int.up + p,
                Vector2Int.down + p,
                Vector2Int.up + Vector2Int.left + p,
                Vector2Int.up + Vector2Int.right + p,
                Vector2Int.down + Vector2Int.left + p,
                Vector2Int.down + Vector2Int.right + p,
            };
        }

        public static Vector2Int[] HexagonMovement(Vector2Int p)
        {
            if (p.y % 2 == 0)
            {
                return new Vector2Int[]
                {
                    Vector2Int.right + p,
                    Vector2Int.left + p,
                    new Vector2Int(-1, 1) + p,
                    new Vector2Int( 0, 1) + p,
                    new Vector2Int(-1,-1) + p,
                    new Vector2Int( 0,-1) + p,
                };
            }
            else
            {
                return new Vector2Int[]
                {
                    Vector2Int.right + p,
                    Vector2Int.left + p,
                    new Vector2Int( 0, 1) + p,
                    new Vector2Int( 1, 1) + p,
                    new Vector2Int( 0,-1) + p,
                    new Vector2Int( 1,-1) + p,
                };
            }
        }

        private int PosToIndex(Vector2Int p)
        {
            int x = p.x + m_offset.x;
            int y = p.y + m_offset.y;
            return x + (y * m_bounds.width);
        }

        public bool InBounds(Vector2Int p)
        {
            return m_bounds.Contains(p);
        }

        public bool HasVisited(Vector2Int p)
        {
            return m_visited[PosToIndex(p)];
        }

        void MarkVisited(Vector2Int p)
        {
            m_visited[PosToIndex(p)] = true;
        }

        int Cost(CanTraverse canTraverse, Vector2Int p)
        {
            int cost = 0;

            Vector2Int[] moves = m_moves(p);

            foreach(Vector2Int move in moves)
                    cost += (InBounds(move) && canTraverse(move)) ? 0 : 2;
            
            return cost;
        }

        void ClearVisited() {
            for (int i = 0; i < m_visited.Length; i++)
                m_visited[i] = false;
        }
 
        void VisitNode(CanTraverse canTraverse, PathNode current, List<PathNode> nodes, Vector2Int goal)
        {
            int step = current.step + 1;

            Vector2Int[] moves = m_moves(current.position);

            foreach (Vector2Int move in moves)
            {
                if (InBounds(move) && canTraverse(move) && !HasVisited(move))
                {
                    MarkVisited(move);

                    int cost = Cost(canTraverse, move);

                    nodes.Add(new PathNode(current, move, step + cost, goal));
                }
            }
        }

        void WalkNode(CanTraverse canTraverse, Vector2Int current, List<Vector2Int> nodes)
        {
            Vector2Int[] moves = m_moves(current);

            foreach (Vector2Int move in moves)
            {
                if (InBounds(move) && canTraverse(move) && !HasVisited(move))
                {
                    MarkVisited(move);
                    nodes.Add(move);
                }
            }
        }

        public void WalkAllNodes(Vector2Int start)
        {
            ClearVisited();

            List<Vector2Int> nodes = new List<Vector2Int>
            {
                start
            };

            while (nodes.Count > 0)
            {
                Vector2Int current = nodes[0]; nodes.RemoveAt(0);

                WalkNode(m_canTraverse, current, nodes);
            }
        }

        public PathNode FindPath(Vector2Int start, Vector2Int goal)
        {
            ClearVisited();

            //Debug.Log("FINDING PATH FROM: " + start + "->" + goal);

            List<PathNode> nodes = new List<PathNode>();
            PathNode start_node = new PathNode(null, start, 0, goal);
            PathNode goal_node = null;

            nodes.Add(start_node);

            while (nodes.Count > 0)
            {
                PathNode current = nodes[0]; nodes.Remove(current);

                if (current.position.x == goal.x && current.position.y == goal.y)
                {
                    goal_node = current;
                    //Debug.Log("FOUND GOAL NODE!");
                    break;
                }

                //Debug.Log("VISITING NODE: " + current);

                VisitNode(m_canTraverse, current, nodes, goal);

                nodes.Sort(delegate (PathNode a, PathNode b)
                {
                    if (a.heuristic > b.heuristic) return 1;
                    else if (a.heuristic < b.heuristic) return -1;
                    else return 0;
                });

                //Debug.Log("### SORT RESULT ###");
                //foreach(PathNode n in nodes) Debug.Log("heuristic: " + n.heuristic);
            }

            return goal_node;
        }
    }
}