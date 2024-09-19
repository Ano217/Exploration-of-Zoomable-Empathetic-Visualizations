//source: https://github.com/MahmoudSaberAmin/Unity_Moph_Mesh_Easy
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Vertex Pair maps two verices to each other, used to find the nearest vertex to a given vertex.
/// </summary>
public class VertexPair
{
    public Vector3 Vertex1 { get; set; }
    public Vector3 Vertex2 { get; set; }

     public VertexPair(Vector3 vertex1, Vector3 vertex2)
    {
        Vertex1 = vertex1;
        Vertex2 = vertex2;
    }
}


/// <summary>
/// Morpher is responsible for morphing between two meshe.
/// It uses the slider value to determine the percentage of the morphing.
/// </summary>
public class Morpher : MonoBehaviour
{
    public bool IsDeforming = true;

    [Tooltip("Mesh should be read/write enabled from the model import settings")]
    private Mesh _oldMesh;
    [Tooltip("Mesh should be read/write enabled from the model import settings")]
    private Mesh _newMesh;

    private Material _oldMat;
    private Material _newMat;

    private MeshFilter _meshFilter;
    private Renderer _renderer;

    [SerializeField] private GameObject _oldObject;
    [SerializeField] private GameObject _newObject;

    [Range(0f, 1f)]
    [SerializeField] private float _slider;


    private Vector3[] _oldVertices;
    private Vector3[] _newVertices;

    private int[] _oldTriangles;
    private int[] _newTriangles;

    private List<Vector3> _finalVertices;

    private Mesh _interpolatedMesh;
    private List<VertexPair> _pairsOfVertices1;
    private List<VertexPair> _pairsOfVertices2;


    private Material _finalMaterial;

    void Start()
    {
        _renderer = _oldObject.GetComponent<Renderer>();
        _oldMat = _renderer.sharedMaterial;
        
        _meshFilter = _oldObject.GetComponent<MeshFilter>();
        _oldMesh = _meshFilter.sharedMesh;

        var newRenderer = _newObject.GetComponent<Renderer>();
        _newMat = newRenderer.sharedMaterial;
        var newMeshFilter = _newObject.GetComponent<MeshFilter>();
        _newMesh = newMeshFilter.sharedMesh;

        _interpolatedMesh = new Mesh();
        _interpolatedMesh.MarkDynamic();

        if (_meshFilter!=null)
        {
            _meshFilter.mesh = _interpolatedMesh;
        }

        _oldVertices = _oldMesh.vertices;
        _newVertices = _newMesh.vertices;

        _oldTriangles = _oldMesh.triangles;
        _newTriangles = _newMesh.triangles;

        _finalVertices = new List<Vector3>(_oldVertices);

        CreatePairs1();
        CreatePairs2();

        _finalMaterial = new Material(_slider< 0.5? _oldMat:_newMat);
        Deform();
        IsDeforming = false;
    }

    /// <summary>
    /// Create pairs of vertices, each pair contains a vertex from the old mesh and a vertex from the new mesh.
    /// Vertices are mapped by the nearest one to each other.
    /// </summary>
    private void CreatePairs1()
    {
        _pairsOfVertices1 = new List<VertexPair>();

        for (int i = 0; i < _oldVertices.Length; i++)
        {
            var oldVertex = _oldVertices[i];

            var nearestToOldVertex = _newVertices.OrderBy(v => Vector3.Distance(v, oldVertex)).FirstOrDefault();

            _pairsOfVertices1.Add(new VertexPair(oldVertex, nearestToOldVertex));
        }
    }


    /// <summary>
    /// Create pairs of vertices, each pair contains a vertex from the new mesh and a vertex from the old mesh.
    /// Vertices are mapped by the nearest one to each other.
    /// </summary>
    private void CreatePairs2()
    {
        _pairsOfVertices2 = new List<VertexPair>();

        for (int i = 0; i < _newVertices.Length; i++)
        {
            var newVertex = _newVertices[i];

            var nearestToNewVertex = _oldVertices.OrderBy(v => Vector3.Distance(v, newVertex)).FirstOrDefault();

            _pairsOfVertices2.Add(new VertexPair(newVertex, nearestToNewVertex));
        }
    }


    void Update()
    {
        if (IsDeforming)
        {
            Deform();
        }
    }

    public void SetSlider(float slider)
    {
        _slider = slider;
    }

    public bool alterSlider(float delta)
    {
        _slider = Mathf.Max(0, Mathf.Min(_slider + delta, 1));
        return delta<0 ? _slider==0 : _slider==1;
    }

    public void setObjects(GameObject oldShape, GameObject newShape)
    {
        _oldObject = oldShape;
        _newObject = newShape;
    }


    /// <summary>
    /// Deform the mesh based on the slider value.
    /// </summary>
    private void Deform()
    {
        if (_slider< 0.5f)
        {
            _finalVertices = _pairsOfVertices1.Select(p => Vector3.Lerp(p.Vertex1, p.Vertex2, _slider)).ToList();
        }
        else
        {
            _finalVertices = _pairsOfVertices2.Select(p => Vector3.Lerp(p.Vertex2, p.Vertex1, _slider)).ToList();
        }

        _interpolatedMesh.Clear();

        _interpolatedMesh.SetVertices(_finalVertices);
        _interpolatedMesh.triangles = _slider < 0.5f ? _oldTriangles : _newTriangles;


        _interpolatedMesh.bounds = _slider < 0.5f ? _oldMesh.bounds : _newMesh.bounds;
        _interpolatedMesh.uv = _slider < 0.5f ? _oldMesh.uv : _newMesh.uv;
        _interpolatedMesh.uv2 = _slider < 0.5f ? _oldMesh.uv2 : _newMesh.uv2;
        _interpolatedMesh.uv3 = _slider < 0.5f ? _oldMesh.uv3 : _newMesh.uv3;

        _interpolatedMesh.RecalculateNormals();

        _finalMaterial.Lerp(_oldMat, _newMat, _slider);
        _finalMaterial.SetTexture("_MainTex", _slider < 0.5f ?_oldMat.GetTexture("_MainTex"): _newMat.GetTexture("_MainTex"));
        //_finalMaterial.SetTexture("_BumpMap", _slider < 0.5f ? _oldMat.GetTexture("_BumpMap") : _newMat.GetTexture("_BumpMap"));
        //_finalMaterial.SetTexture("_MetallicGlossMap", _slider < 0.5f ? _oldMat.GetTexture("_MetallicGlossMap") : _newMat.GetTexture("_MetallicGlossMap"));
        //_finalMaterial.SetTexture("_OcclusionMap", _slider < 0.5f ? _oldMat.GetTexture("_OcclusionMap") : _newMat.GetTexture("_OcclusionMap"));
        //_finalMaterial.SetTexture("_EmissionMap", _slider < 0.5f ? _oldMat.GetTexture("_EmissionMap") : _newMat.GetTexture("_EmissionMap"));
        
        _renderer.material = _finalMaterial;
    }

}