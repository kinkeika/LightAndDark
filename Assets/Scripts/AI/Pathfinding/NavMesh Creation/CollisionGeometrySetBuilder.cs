﻿using UnityEngine;
using System.Collections.Generic;
using System;

namespace NavMesh2D.Core
{
    public class CollisionGeometrySetBuilder
    {
        public static CollisionGeometrySet Build(IEnumerable<Collider2D> collider, int circleVertCount)
        {
            CollisionGeometrySet result = new CollisionGeometrySet();
            List<Vector2> inOutVerts = new List<Vector2>(10); //Just a guess
            float anglePerCircleVert = (Mathf.PI * 2) / circleVertCount;

            foreach (Collider2D col in collider)
            {
                Type cTyp = col.GetType();

                //Sort out any edge collider, as they will be processed differently.
                if (cTyp == typeof(EdgeCollider2D))
                {
                    //LoadEdgeColliderVerts((EdgeCollider2D)collider[iCol], inOutVerts);
                    //result.AddEdge(inOutVerts);
                    //Ignore it for the time being!!
                }
                else
                {
                    if (cTyp == typeof(BoxCollider2D))
                        LoadBoxColliderVerts((BoxCollider2D)col, inOutVerts);
                    else if (cTyp == typeof(CircleCollider2D))
                        LoadCircleColliderVerts((CircleCollider2D)col, inOutVerts, circleVertCount, anglePerCircleVert);
                    else
                        LoadPolygonColliderVerts((PolygonCollider2D)col, inOutVerts);

                    result.AddCollider(inOutVerts);
                }
                inOutVerts.Clear();
            }
            return result;
        }

        private static void LoadBoxColliderVerts(BoxCollider2D collider, List<Vector2> inOutVerts)
        {
            Vector2 halfSize = collider.size / 2;
            inOutVerts.Add(collider.transform.TransformPoint(halfSize + collider.offset));
            inOutVerts.Add(collider.transform.TransformPoint(new Vector2(-halfSize.x, halfSize.y) + collider.offset));
            inOutVerts.Add(collider.transform.TransformPoint(-halfSize + collider.offset));
            inOutVerts.Add(collider.transform.TransformPoint(new Vector2(halfSize.x, -halfSize.y) + collider.offset));
        }

        private static void LoadCircleColliderVerts(CircleCollider2D collider, List<Vector2> inOutVerts, int circleVertCount, float anglePerCircleVert)
        {
            for (int i = 0; i < circleVertCount; i++)
            {
                inOutVerts.Add(collider.transform.TransformPoint(new Vector2(collider.radius * Mathf.Cos(anglePerCircleVert * i) + collider.offset.x, collider.radius * Mathf.Sin(anglePerCircleVert * i) + collider.offset.y)));
            }
        }

        private static void LoadPolygonColliderVerts(PolygonCollider2D collider, List<Vector2> inOutVerts)
        {
            Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
            for (int iVert = 0; iVert < collider.points.Length; iVert++)
            {
                inOutVerts.Add(localToWorld.MultiplyPoint(collider.points[iVert] + collider.offset));
            }
        }

        private static void LoadEdgeColliderVerts(EdgeCollider2D collider, List<Vector2> inOutVerts)
        {
            Matrix4x4 localToWorld = collider.transform.localToWorldMatrix;
            for (int iVert = 0; iVert < collider.points.Length; iVert++)
            {
                inOutVerts.Add(localToWorld.MultiplyPoint(collider.points[iVert] + collider.offset));
            }
        }
    }
}
