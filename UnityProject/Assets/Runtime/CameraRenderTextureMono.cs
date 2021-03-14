using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraRenderTextureMono : MonoBehaviour
{
    public static Mesh fullScreenMesh{
        get
        {
            if (m_mesh != null)
            {
                return m_mesh;
            }
            m_mesh = new Mesh();
            m_mesh.vertices = new Vector3[]
            {
                new Vector3(-1, -1, 0),
                new Vector3(-1, 1, 0),
                new Vector3(1, 1, 0),
                new Vector3(1, -1, 0),
            };
            m_mesh.uv = new Vector2[]
            {
                new Vector2(0,1), 
                new Vector2(0,0), 
                new Vector2(1,0), 
                new Vector2(1,1), 
            };
            m_mesh.SetIndices(new int[]{0,1,2,3},MeshTopology.Quads,0);
            return m_mesh;
        }
    }

    private static Mesh m_mesh;
    private static Vector4[] corners = new Vector4[4];
    private RenderTexture _renderTexture;

    private Camera _camera;

    public Transform _cubeTrans;

    public Mesh _cubeMesh;

    public Material _cubeMat;
    public Material _skyboxMaterial;
    // Start is called before the first frame update
    void Start()
    {
        _renderTexture = new RenderTexture(Screen.width,Screen.height,24);
        _camera = Camera.current;
        _cubeMesh = _cubeTrans.GetComponent<MeshFilter>().mesh;
        _cubeMat = _cubeTrans.GetComponent<MeshRenderer>().material;
    }

    private void DrawSkybox(Camera cam)
    {
        corners[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.farClipPlane));
        corners[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.farClipPlane));
        corners[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.farClipPlane));
        corners[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.farClipPlane));
        _skyboxMaterial.SetVectorArray("_Corner",corners);
        _skyboxMaterial.SetPass(0);
        Graphics.DrawMeshNow(fullScreenMesh, Matrix4x4.identity);
    }



    private void OnPostRender()
    {
        _camera = Camera.current;
        Graphics.SetRenderTarget(_renderTexture);
        GL.Clear(true,true,Color.gray);
        //start draw call
        _cubeMat.color = new Color(0,0.5f,0.8f);
        _cubeMat.SetPass(0);
        Graphics.DrawMeshNow(_cubeMesh,_cubeTrans.localToWorldMatrix);
        //end draw call
        Graphics.Blit(_renderTexture,_camera.targetTexture);
//        _camera.targetTexture = _renderTexture;
        DrawSkybox(_camera);
    }
}
