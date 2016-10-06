﻿using UnityEngine;
using System.Collections.Generic;
using Utility;

namespace NavMesh2D
{
    public class NavigationData2D : ScriptableObject
    {
        const float mapPointMaxDeviation = 3;
        const float mapPointInstantAcceptDeviation = 0.01f;

        //identifiers
        public new string name;
        public int version;

        public NavNode[] nodes; // sorted by x Value. Min -> Max

        /*public NavRayCastHit RayCast(Vector2 pos, Vector2 dir, float length)
                {
                    Vector2 endPos = dir * length + pos;
                    float minX = Mathf.Min(pos.x, endPos.x);
                    float maxX = Mathf.Max(pos.x, endPos.x);

                    float minY = Mathf.Min(pos.y, endPos.y);
                    float maxY = Mathf.Min(pos.y, endPos.y);

                    float m = (dir.x == 0) ? 0 : dir.x / dir.y;

                    foreach (NavNode nn in nodes)
                    {
                        if (nn.max.x < minX)
                            continue;
                        if (nn.min.x > maxX)
                            break;
                        if (nn.min.y > maxY || nn.max.y < minY)
                            continue;

                        if (dir.x != 0) // m != 0
                        {
                            float startY = m * nn.min.x + pos.y;
                            float endY = m * nn.max.x + pos.y;

                            if (startY > endY)
                            {
                                if (startY > maxY)
                                    continue;
                                if (endY < minY)
                                    continue;
                            }
                            else
                            {
                                if (startY < maxY)
                                    continue;
                                if (endY > minY)
                                    continue;
                            }
                        }

                        //Bounds passed all tests. A intersection is possible

                    }
                }
        }*/

        public bool TryMapPoint(Vector2 point, out Vector2 nearestPoint)
        {
            NavNode map_cNavNode;
            float map_minDist = float.MaxValue;
            float dist;
            Vector2 cPoint;
            nearestPoint = Vector2.zero;

            for (int iNavNode = 0; iNavNode < nodes.Length; iNavNode++)
            {
                map_cNavNode = nodes[iNavNode];

                //Extended bounds test
                if (map_cNavNode.min.x - mapPointMaxDeviation > point.x || map_cNavNode.max.x + mapPointMaxDeviation < point.x
                || map_cNavNode.min.y - mapPointMaxDeviation > point.y || map_cNavNode.max.y + mapPointMaxDeviation < point.y)
                {
                    Bounds b = new Bounds();
                    b.SetMinMax(map_cNavNode.min, map_cNavNode.max);
                    DebugExtension.DebugBounds(b, Color.red);
                    //Failed test
                    continue;
                }

                if (map_cNavNode.isClosed && map_cNavNode.Contains(point))
                {
                    //maybe later check children, not implemented though
                    Bounds b = new Bounds();
                    b.SetMinMax(map_cNavNode.min, map_cNavNode.max);
                    DebugExtension.DebugBounds(b, Color.green);
                    return false;
                }

                if (map_cNavNode.TryFindClosestPointOnContour(point, out dist, out cPoint))
                {
                    Debug.Log(dist);
                    if (dist < map_minDist)
                    {
                        
                        nearestPoint = cPoint;
                        if (dist <= mapPointInstantAcceptDeviation)
                        {
                            return true;
                        }
                        map_minDist = dist;
                    }
                }
            }

            return map_minDist != float.MaxValue;
        }

        public void DrawForDebug()
        {
            int counter = 1;
            foreach (NavNode n in nodes)
            {
                n.VisualDebug(counter++);
            }
        }
    }

    public class NavNode
    {
        const float maxDeviationInside = 0.1f;
        const float maxDeviationOutside = 0.001f;

        public readonly Vector2 min;
        public readonly Vector2 max;
        public readonly bool isClosed;
        public readonly int hierachyIndex; // 0 = hole, 1 = solid, 2 = hole, 3 = solid, ...

        public bool IsSolid { get { return hierachyIndex % 2 == 0; } }

        NavNodeLink[] links;
        public NavVert[] verts;


        public NavNode(NavVert[] verts, Bounds bounds, bool isClosed, int hierachyIndex)
        {
            this.verts = verts;
            min = bounds.min;
            max = bounds.max;
            this.isClosed = isClosed;
            this.hierachyIndex = hierachyIndex;
        }

        public bool Contains(Vector2 point)
        {
            Debug.Assert(isClosed);

            if (min.x > point.x || max.x < point.x || min.y > point.y || max.y < point.y)
            {
                //Bound test failed
                return false;
            }

            bool inside = false;
            NavVert cVert = verts[verts.Length - 1];
            for (int iEdge = 0; iEdge < verts.Length; iEdge++)
            {
                if ((verts[iEdge].PointB.y > point.y) != (cVert.PointB.y > point.y) &&
                    point.x < (cVert.PointB.x - verts[iEdge].PointB.x) * (point.y - verts[iEdge].PointB.y) / (cVert.PointB.y - verts[iEdge].PointB.y) + verts[iEdge].PointB.x)
                {
                    inside = !inside;
                }
                cVert = verts[iEdge];
            }
            return inside;
        }

        public bool TryFindClosestPointOnContour(Vector2 point, out float distance, out Vector2 nearestPoint)
        {
            distance = float.MaxValue;
            nearestPoint = Vector2.zero;
            NavVert cVert = (isClosed) ? verts[verts.Length - 1] : verts[0];
            for (int iEdge = isClosed ? 0 : 1; iEdge < verts.Length; iEdge++)
            {
                DebugExtension.DebugPoint(verts[iEdge].PointB, Color.red);
                float lineSide = Mathf.Sign((verts[iEdge].PointB.x - cVert.PointB.x) * (point.y - cVert.PointB.y) - (verts[iEdge].PointB.y - cVert.PointB.y) * (point.x - cVert.PointB.x));
                if (lineSide == 0)
                {
                    distance = 0;
                    nearestPoint = point;
                    DebugExtension.DebugPoint(nearestPoint);
                    return true;
                }
                if (lineSide == 1)
                {
                    cVert = verts[iEdge];
                    DebugExtension.DebugPoint(nearestPoint, Color.red);
                    continue;
                }

                //Point is on right side. Now calculate distance.
                Vector2 AP = point - cVert.PointB;       //Vector from A to P   
                Vector2 AB = verts[iEdge].PointB - cVert.PointB;
                float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
                float dis = Mathf.Clamp(ABAPproduct / AB.sqrMagnitude, 0, 1); //The normalized "distance" from a to your closest point  

                AP = AB * dis + cVert.PointB;
                dis = (AP - point).sqrMagnitude;
                if (distance > dis)
                {
                    distance = dis;
                    nearestPoint = AP;
                }
                cVert = verts[iEdge];
            }
            if (distance == float.MaxValue)
            {
                return false;
            }
            distance = Mathf.Sqrt(distance);
            DebugExtension.DebugPoint(nearestPoint);
            return true;
        }

        public void VisualDebug(int colorId)
        {
            if (verts.Length == 0)
                Debug.Log("Node [" + colorId + "] is empty.");
            if (verts.Length == 1)
                Debug.Log("Node [" + colorId + "] is too small. " + verts[0].PointB);

            for (int iVert = 0; iVert < verts.Length - 1; iVert++)
            {
                if (verts[iVert].PointB == verts[iVert + 1].PointB)
                {
                    Debug.Log("Node [" + colorId + "] has edge with zero length. " + verts[iVert].PointB + ", totalVertCount = " + verts.Length);
                    DebugExtension.DebugPoint(verts[iVert].PointB, Color.magenta);
                }
                //float angleGrad = (1 / Mathf.PI) * verts[iVert].slopeAngleBC;
                DebugExtension.DebugArrow(verts[iVert].PointB, verts[iVert + 1].PointB - verts[iVert].PointB, DifferentColors.GetColor(colorId));
                //Debug.DrawLine(verts[iVert].PointB, verts[iVert + 1].PointB, DifferentColors.GetColor(colorId));
                DebugExtension.DebugCircle(verts[iVert].PointB, Vector3.forward, DifferentColors.GetColor(colorId), 0.2f);
            }
            if (isClosed)
            {
                //Debug.DrawLine(verts[verts.Length - 1].PointB, verts[0].PointB, DifferentColors.GetColor(colorId));
                //float angleGrad = (1 / Mathf.PI) * verts[verts.Length - 1].slopeAngleBC;
                DebugExtension.DebugArrow(verts[verts.Length - 1].PointB, verts[0].PointB - verts[verts.Length - 1].PointB, DifferentColors.GetColor(colorId));
            }
            DebugExtension.DebugCircle(verts[verts.Length - 1].PointB, Vector3.forward, DifferentColors.GetColor(colorId), 0.2f);
        }
    }

    public class NavNodeLink
    {
        public enum LinkType { NotAccessible, Jump, Ladder, };
        NavNode target;
        PathRequirements[] isReachableFrom;
        Vector2 startPoint;
        Vector2 endPoint;
        LinkType linkType;
    }

    public class NavVert
    {
        public Vector2 PointB { get { return pointB; } }

        public DynamicObstruction firstObstruction;

        public readonly float angleABC;
        public readonly float slopeAngleBC;
        public readonly float distanceBC;

        Vector2 pointB; // a -> b -> c
        NavNodeLink[] links;

        public NavVert(Vector2 point, float angleABC, float slopeAngleBC, float distanceBC)
        {
            this.pointB = point;
            this.angleABC = angleABC;
            this.slopeAngleBC = slopeAngleBC;
            this.distanceBC = distanceBC;
        }

        public NavVert(Vector2 point)
        {
            this.pointB = point;
        }
    }

    public class DynamicObstruction
    {

    }

    public class PathRequirements
    {
        float minSlope;
        bool useRelativeSlopeAngle;
        bool isPathUnblocked;

    }

    class NavAgent
    {
        float maxWalkableSlope;
        bool useRelativeSlopeAngle;
        float height;
    }

    class NavRayCastHit
    {

    }
}
