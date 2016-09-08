﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.DebugUtil;

namespace Utility.Polygon2D
{
    public class Contour : IEnumerable<Vector2>
    {
        public int VertexCount { get { return verticies.Count; } }
        public Bounds Bounds { get { if (!areBoundsValid) CalcBounds(); return bounds; } }
        public bool IsEmpty { get { return verticies.Count == 0; } }
        public List<Vector2> verticies;

        private Bounds bounds;
        private bool areBoundsValid;

        public Contour(params Vector2[] verticies)
        {
            this.verticies = new List<Vector2>(verticies);
            CalcBounds();
        }

        public Contour(List<Vector2> verticies)
        {
            this.verticies = new List<Vector2>();
            this.verticies.AddRange(verticies);
            CalcBounds();
        }

        public void AddVertex(Vector2 v)
        {
            verticies.Add(v);
            bounds.max = Vector2.Max(bounds.max, v);
            bounds.min = Vector2.Min(bounds.min, v);
        }

        public void RemoveVertexAt(int pos)
        {
            if (pos < 0 || pos >= verticies.Count || verticies.Count == 1)// The remove of the last vertex is not allowed!
                return;
            verticies.RemoveAt(pos);
            areBoundsValid = false;
        }

        public Vector2 this[int key]
        {
            get { return verticies[key]; }
        }

        public void RemoveAllPointEdges()
        {
            //Removes: edges with length = 0
            for (int i = 0; i < verticies.Count - 1; i++)
            {
                if (verticies[i] == verticies[i + 1])
                {
                    verticies.RemoveAt(i);
                    i--;
                }
            }
            if (verticies[0] == verticies[verticies.Count - 1])
                verticies.RemoveAt(verticies.Count - 1);
        }

        public bool IsSolid()
        {
            return CalcArea() >= 0;
        }

        public float CalcArea()
        {
            float area = 0;
            int j = verticies.Count - 1;

            for (int i = 0; i < verticies.Count; i++)
            {
                area = area + (verticies[j].x + verticies[i].x) * (verticies[j].y - verticies[i].y);
                j = i;
            }
            return area / 2;
        }

        public void DrawDebugInfo(bool withSpheres = false)
        {
            Vector2 prev = verticies[verticies.Count-1];
            foreach (Vector2 vert in verticies)
            {
                DrawArrow.ForGizmo(prev, vert - prev);
                if(withSpheres)
                Gizmos.DrawWireSphere(prev, 0.5f);
                else
                    Gizmos.DrawWireSphere(prev, 0.2f);
                prev = vert;
            }
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            return ((IEnumerable<Vector2>)verticies).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Vector2>)verticies).GetEnumerator();
        }

        private void CalcBounds()
        {
            areBoundsValid = true;
            bounds.min = verticies[0];
            bounds.max = verticies[0];
            for (int iVert = 0; iVert < verticies.Count; iVert++)
            {
                bounds.max = Vector2.Max(bounds.max, verticies[iVert]);
                bounds.min = Vector2.Min(bounds.min, verticies[iVert]);
            }
        }
    }
}
