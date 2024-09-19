using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct minNmax
{
    public Vector3 min;
    public Vector3 max;

    public minNmax(Vector3 minPoints, Vector3 maxPoints)
    {
        min = minPoints;
        max = maxPoints;
    }
}

public static class GeometryAnalyzer
{
    public static float getObjectSize(GameObject obj)
    {
        minNmax result = new minNmax(obj.transform.position, obj.transform.position);
        float minY = obj.transform.position.y;
        float maxY = obj.transform.position.y;

        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            minY = Mathf.Min(minY, renderer.bounds.min.y);
            maxY = Mathf.Max(maxY, renderer.bounds.max.y);
        }
        float size = maxY - minY;
        //Debug.Log("MinY=" + minY + "  maxY=" + maxY + "  size=" + size);
        return size;
    }

    public static minNmax getMinNmax(GameObject obj)
    {
        minNmax result = new minNmax(obj.transform.position, obj.transform.position);
        Vector3[] vertices;
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            if (meshFilter.mesh.isReadable)
            {
                vertices = obj.GetComponent<MeshFilter>().mesh.vertices;
                if (vertices.Length > 0)
                {
                    Vector3 v1 = obj.transform.TransformPoint(vertices[0]);
                    float xmin = v1.x;
                    float xmax = xmin;
                    float ymin = v1.y;
                    float ymax = ymin;
                    float zmin = v1.z;
                    float zmax = zmin;

                    foreach (Vector3 v in vertices)
                    {
                        Vector3 worldV = obj.transform.TransformPoint(v);
                        if (worldV.x < xmin)
                        {
                            xmin = worldV.x;
                        }
                        else if (xmax < worldV.x)
                        {
                            xmax = worldV.x;
                        }
                        if (worldV.y < ymin)
                        {
                            ymin = worldV.y;
                        }
                        else if (ymax < worldV.y)
                        {
                            ymax = worldV.y;
                        }
                        if (worldV.z < zmin)
                        {
                            zmin = worldV.z;
                        }
                        else if (zmax < worldV.z)
                        {
                            zmax = worldV.z;
                        }
                    }
                    Vector3 minVec = new Vector3(xmin, ymin, zmin);
                    Vector3 maxVec = new Vector3(xmax, ymax, zmax);
                    minNmax verticesMinNMax = new minNmax(minVec, maxVec);
                    result = compareMinNmax(result, verticesMinNMax);
                }
            }
        }
        foreach (Transform t in obj.transform)
        {
            result = compareMinNmax(result, getMinNmax(t.gameObject));
        }
        return result;
    }

    public static minNmax getLocalMinNmax(GameObject obj)
    {
        minNmax result = new minNmax(obj.transform.position, obj.transform.position);
        Vector3[] vertices;
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            if (meshFilter.mesh.isReadable)
            {
                vertices = obj.GetComponent<MeshFilter>().mesh.vertices;
                if (vertices.Length > 0)
                {
                    Vector3 v1 = vertices[0];
                    float xmin = v1.x;
                    float xmax = xmin;
                    float ymin = v1.y;
                    float ymax = ymin;
                    float zmin = v1.z;
                    float zmax = zmin;

                    foreach (Vector3 v in vertices)
                    {
                        if (v.x < xmin)
                        {
                            xmin = v.x;
                        }
                        else if (xmax < v.x)
                        {
                            xmax = v.x;
                        }
                        if (v.y < ymin)
                        {
                            ymin = v.y;
                        }
                        else if (ymax < v.y)
                        {
                            ymax = v.y;
                        }
                        if (v.z < zmin)
                        {
                            zmin = v.z;
                        }
                        else if (zmax < v.z)
                        {
                            zmax = v.z;
                        }
                    }
                    Vector3 minVec = new Vector3(xmin, ymin, zmin);
                    Vector3 maxVec = new Vector3(xmax, ymax, zmax);
                    minNmax verticesMinNMax = new minNmax(minVec, maxVec);

                    result = compareMinNmax(result, verticesMinNMax);
                }
            }
        }
        foreach (Transform t in obj.transform)
        {
            result = compareMinNmax(result, getMinNmax(t.gameObject));
        }

        return result;
    }

    public static minNmax getRelativMinNmax(GameObject obj, Transform reference)
    {
        minNmax result = new minNmax(reference.InverseTransformPoint(obj.transform.position), reference.InverseTransformPoint(obj.transform.position));
        Vector3[] vertices;
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            if (meshFilter.mesh.isReadable)
            {
                vertices = obj.GetComponent<MeshFilter>().mesh.vertices;
                if (vertices.Length > 0)
                {
                    Vector3 v1 = reference.InverseTransformPoint(vertices[0]);
                    float xmin = v1.x;
                    float xmax = xmin;
                    float ymin = v1.y;
                    float ymax = ymin;
                    float zmin = v1.z;
                    float zmax = zmin;

                    foreach (Vector3 v in vertices)
                    {
                        Vector3 rv = reference.InverseTransformPoint(v);
                        if (rv.x < xmin)
                        {
                            xmin = rv.x;
                        }
                        else if (xmax < rv.x)
                        {
                            xmax = rv.x;
                        }
                        if (rv.y < ymin)
                        {
                            ymin = rv.y;
                        }
                        else if (ymax < rv.y)
                        {
                            ymax = rv.y;
                        }
                        if (rv.z < zmin)
                        {
                            zmin = rv.z;
                        }
                        else if (zmax < rv.z)
                        {
                            zmax = rv.z;
                        }
                    }
                    Vector3 minVec = new Vector3(xmin, ymin, zmin);
                    Vector3 maxVec = new Vector3(xmax, ymax, zmax);
                    minNmax verticesMinNMax = new minNmax(minVec, maxVec);

                    result = compareMinNmax(result, verticesMinNMax);
                }
            }
        }
        foreach (Transform t in obj.transform)
        {
            result = compareMinNmax(result, getMinNmax(t.gameObject));
        }

        return result;
    }

    public static minNmax compareMinNmax(minNmax e1, minNmax e2)
    {
        Vector3 minVect = new Vector3(Mathf.Min(e1.min.x, e2.min.x), Mathf.Min(e1.min.y, e2.min.y), Mathf.Min(e1.min.z, e2.min.z));
        Vector3 maxVect = new Vector3(Mathf.Max(e1.max.x, e2.max.x), Mathf.Max(e1.max.y, e2.max.y), Mathf.Max(e1.max.z, e2.max.z));
        return new minNmax(minVect, maxVect);
    }

    public static float getHorizontalRadius(GameObject obj)
    {
        float r = 0f;
        minNmax extremities = getMinNmax(obj);
        float deltaX = Mathf.Abs(extremities.max.x - extremities.min.x);
        float deltaZ = Mathf.Abs(extremities.max.z - extremities.min.z);

        r = Mathf.Max(deltaX, deltaZ);
        return r;
    }

    public static float getMaxLocalRadius(GameObject obj)
    {
        float r = 0f;
        minNmax extremities = getLocalMinNmax(obj);
        float deltaX = Mathf.Abs(extremities.max.x - extremities.min.x);
        float deltaY = Mathf.Abs(extremities.max.y - extremities.min.y);
        float deltaZ = Mathf.Abs(extremities.max.z - extremities.min.z);

        r = Mathf.Max(deltaX, deltaY, deltaZ); ;
        return r;
    }

    public static float getLocalRadius(GameObject obj)
    {
        float r = 0f;
        minNmax extremities = getLocalMinNmax(obj);
        float deltaX = Mathf.Abs(extremities.max.x - extremities.min.x);
        float deltaZ = Mathf.Abs(extremities.max.z - extremities.min.z);

        r = Mathf.Max(deltaX, deltaZ);
        return r;
    }



    public static Vector3 getCenterPoint(GameObject obj)
    {
        minNmax mNm = getMinNmax(obj);
        float x = (mNm.min.x + mNm.max.x) / 2;
        float y = (mNm.min.y + mNm.max.y) / 2;
        float z = (mNm.min.z + mNm.max.z) / 2;

        return new Vector3(x, y, z);
    }

    public static Vector3 getLocalCenterPoint(GameObject obj)
    {
        minNmax mNm = getLocalMinNmax(obj);
        float x = (mNm.min.x + mNm.max.x) / 2;
        float y = (mNm.min.y + mNm.max.y) / 2;
        float z = (mNm.min.z + mNm.max.z) / 2;

        return new Vector3(x, y, z);
    }

    public static float eulerToRadian(float eulerAngle)
    {
        return eulerAngle / 180f;
    }

    public static float getScaleAdjustment(minNmax oldScale, minNmax newScale)
    {
        float x = (oldScale.max.x - oldScale.min.x) / (newScale.max.x - newScale.min.x);
        float y = (oldScale.max.y - oldScale.min.y) / (newScale.max.y - newScale.min.y);
        float z = (oldScale.max.z - oldScale.min.z) / (newScale.max.z - newScale.min.z);
        return Mathf.Min(x, y, z);
    }
}
