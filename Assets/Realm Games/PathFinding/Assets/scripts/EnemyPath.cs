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
using System.Collections;
using System;
using System.Collections.Generic;

namespace RealmGames.PathFinding
{
    [Serializable]
    public class EnemyPath
    {
        public int pathId;
        public Vector3 start;
        public Vector3 goal;
        public Vector3[] points;

        public EnemyPath() { }

        public EnemyPath(int id, Vector3 s, Vector3 e)
        {
            this.pathId = id;
            this.start = s;
            this.goal = e;
        }

        public Vector3[] Optimize()
        {
            List<Vector3> segments = new List<Vector3>();
            List<Vector3> current = new List<Vector3>();
            Vector3 last = Vector3.zero;
            Vector3 dir = Vector3.zero;
            Vector3 start = Vector3.zero;

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 point = points[i];

                if (current.Count == 0)
                {
                    start = point;
                    last = point;
                    current.Add(point);
                }
                else if (current.Count == 1)
                {
                    dir = (last - point).normalized;
                    last = point;
                    current.Add(point);
                }
                else
                {
                    Vector3 direction = (last - point).normalized;

                    if (direction != dir)
                    {
                        segments.Add(start);
                        segments.Add(last);

                        current.Clear();

                        start = last;
                        current.Add(last);
                    }

                    dir = (last - point).normalized;
                    last = point;
                    current.Add(point);
                }
            }

            if (current.Count == 1)
            {
                segments.Add(current[0]);
            }
            else if (current.Count > 1)
            {
                segments.Add(current[0]);
                segments.Add(current[current.Count - 1]);
            }

            return segments.ToArray();
        }


    }
}