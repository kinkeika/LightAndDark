﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using NavMesh2D;

public class NavData2DVisualizerWindow : EditorWindow
{

    [MenuItem("Pathfinding/NavData2DVisualizer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(NavData2DVisualizerWindow));
    }

    public static void SceneDrawNavData2D(NavigationData2D nav2d)
    {
        if (nav2d == null || nav2d.nodes == null)
            return;

        for (int iNode = 0; iNode < nav2d.nodes.Length; iNode++)
        {
            NavNode nn = nav2d.nodes[iNode];
            Handles.color = Utility.DifferentColors.GetColor(iNode);
            for (int iVert = 0; iVert < nn.verts.Length - 1; iVert++)
            {
                Handles.DrawLine(nn.verts[iVert].PointB, nn.verts[iVert + 1].PointB);
                Handles.DrawWireDisc(nn.verts[iVert].PointB, Vector3.forward, 0.1f);
            }
            Handles.DrawWireDisc(nn.verts[nn.verts.Length - 1].PointB, Vector3.forward, 0.1f);
            if (nn.isClosed)
            {
                Handles.DrawLine(nn.verts[nn.verts.Length - 1].PointB, nn.verts[0].PointB);

            }
        }
    }

    public static void SceneDrawNavData2D(RawNavigationData2D nav2d)
    {
        if (nav2d == null || nav2d.nodes == null)
            return;

        for (int iNode = 0; iNode < nav2d.nodes.Length; iNode++)
        {
            RawNavNode nn = nav2d.nodes[iNode];
            Handles.color = Utility.DifferentColors.GetColor(iNode);
            for (int iVert = 0; iVert < nn.verts.Length - 1; iVert++)
            {
                Handles.DrawLine(nn.verts[iVert].PointB, nn.verts[iVert + 1].PointB);
                Handles.DrawWireDisc(nn.verts[iVert].PointB, Vector3.forward, 0.1f);
            }
            Handles.DrawWireDisc(nn.verts[nn.verts.Length - 1].PointB, Vector3.forward, 0.1f);
            if (nn.isClosed)
            {
                Handles.DrawLine(nn.verts[nn.verts.Length - 1].PointB, nn.verts[0].PointB);

            }
        }
    }

    NavigationData2D navData2d;
    RawNavigationData2D rawNavData2d;

    void OnGUI()
    {
        EditorGUILayout.LabelField("NavData Visualizer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        navData2d = (NavigationData2D)EditorGUILayout.ObjectField("NavData2d", navData2d, typeof(NavigationData2D), false);
        rawNavData2d = (RawNavigationData2D)EditorGUILayout.ObjectField("RawNavData2d", rawNavData2d, typeof(RawNavigationData2D), false);
        if (EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (navData2d != null)
            SceneDrawNavData2D(navData2d);
        else
            SceneDrawNavData2D(rawNavData2d);
    }

    void OnEnable()
    {
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        // Add (or re-add) the delegate.
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }

    void OnDestroy()
    {
        // When the window is destroyed, remove the delegate
        // so that it will no longer do any drawing.
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.RepaintAll();
    }
}
