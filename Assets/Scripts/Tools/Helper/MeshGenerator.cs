using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    
    public static Mesh Circle(float radius, int segments, Vector3 normal)
    {
        var mesh = new Mesh();
        normal.Normalize();

        var vertices = new Vector3[segments + 1];
        var triangles = new int[3 * segments];

        vertices[segments] = new Vector3();

        Quaternion rotation = Quaternion.FromToRotation(Vector3.back, normal);

        for (int i = 0; i < segments; i++)
        {
            float alpha = i * 2 * Mathf.PI / segments;
            vertices[i] = rotation * new Vector3(radius * Mathf.Cos(alpha), radius * Mathf.Sin(alpha), 0);

            triangles[3 * i] = segments;
            triangles[3 * i + 2] = i;
            triangles[3 * i + 1] = (i + 1) % segments;

        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.name = "Circle";

        return mesh;
    }

    public static Mesh Cube (float width, float height, float depth, Pivot pivot)
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        float bottomX = pivot == Pivot.Center ? -height / 2 : (pivot == Pivot.Top ? -height : 0);

        Vector3 up = new Vector3(0, height, 0);
        Vector3 right = new Vector3(width, 0, 0);
        Vector3 forward = new Vector3(0, 0, depth);

        Vector3 v00 = (-up - right - forward) / 2;
        v00.y = bottomX;

        // Bottom
        vertices.Add(v00);
        vertices.Add(v00 + forward);
        vertices.Add(v00 + forward + right);
        vertices.Add(v00 + right);
        AddQuad(triangles, 0, 1, 2, 3, true);

        var v01 = v00 + up;

        // Top
        vertices.Add(v01);
        vertices.Add(v01 + forward);
        vertices.Add(v01 + forward + right);
        vertices.Add(v01 + right);
        AddQuad(triangles, 4, 5, 6, 7, false);

        // Front
        vertices.Add(v00);
        vertices.Add(v00 + up);
        vertices.Add(v00 + up + right);
        vertices.Add(v00 + right);
        AddQuad(triangles, 8, 9, 10, 11, false);

        // Back
        v01 = v00 + forward;
        vertices.Add(v01);
        vertices.Add(v01 + up);
        vertices.Add(v01 + up + right);
        vertices.Add(v01 + right);
        AddQuad(triangles, 12, 13, 14, 15, true);

        // Left
        vertices.Add(v00);
        vertices.Add(v00 + up);
        vertices.Add(v00 + up + forward);
        vertices.Add(v00 + forward);
        AddQuad(triangles, 16, 17, 18, 19, true);

        // Right
        v01 = v00 + right;
        vertices.Add(v01);
        vertices.Add(v01 + up);
        vertices.Add(v01 + up + forward);
        vertices.Add(v01 + forward);
        AddQuad(triangles, 20, 21, 22, 23, false);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.name = "Cube";

        return mesh;
    }

    public static Mesh Cylinder (float radius, float height, int segments, Pivot pivot)
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        float bottomX = pivot == Pivot.Center ? -height / 2 : (pivot == Pivot.Top ? -height : 0);

        for (int i = 0; i < segments; i++)
        {
            float alpha = i * 2 * Mathf.PI / segments;
            var v = new Vector3(radius * Mathf.Cos(alpha), bottomX, radius * Mathf.Sin(alpha));
            vertices.Add(v);
            vertices.Add(v);

            v = new Vector3(radius * Mathf.Cos(alpha), bottomX + height, radius * Mathf.Sin(alpha));
            vertices.Add(v);
            vertices.Add(v);
        }

        for (int i = 0; i < segments; i++)
        {
            if (i < segments - 2)
            {
                // Bottom
                AddTriangle(triangles, 0, 4 * (i + 1), 4 * (i + 2), false);

                // Top
                AddTriangle(triangles, 2, 4 * (i + 1) + 2, 4 * (i + 2) + 2, true);    
            }

            // Side
            AddQuad(triangles, 4 * i + 1, 4 * i + 3, (4 * (i + 1) + 3) % (4 * segments), (4 * (i + 1) + 1) % (4 * segments), false);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.name = "Cylinder";

        return mesh;
    }

    public static Mesh Tetrahedron (float edge, float height, Vector3 up)
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, up);

        float h = edge * Mathf.Sqrt(3) / 2;

        var v0 = new Vector3(-edge/2, 0, -h/3);
        var v1 = new Vector3(0, 0, 2 * h / 3);
        var v2 = new Vector3(edge / 2, 0, -h / 3);
        var v3 = new Vector3(0, height, 0);

        AddTriangle(vertices, triangles, v0, v1, v2, true);
        AddTriangle(vertices, triangles, v0, v1, v3, false);
        AddTriangle(vertices, triangles, v1, v2, v3, false);
        AddTriangle(vertices, triangles, v2, v0, v3, false);

        ApplyRotation(vertices, rotation);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.name = "Tetrahedron";

        return mesh;
    }

    public static Mesh Cone(float radius, float height, int segments)
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (int i = 0; i < segments; i++)
        {
            float alpha = i * 2 * Mathf.PI / segments;
            var v = new Vector3(radius * Mathf.Cos(alpha), 0, radius * Mathf.Sin(alpha));
            vertices.Add(v);
            vertices.Add(v);
        }

        for (int i = 0; i < segments; i++)
        {
            vertices.Add(new Vector3(0, height, 0));    
        }

        for (int i = 1; i <= segments - 2; i++)
        {
            // Bottom
            AddTriangle(triangles, 0, 2 * i, 2 * i + 2, false);

            // Side
            AddTriangle(triangles, 2 * i - 1, 2 * i + 1 , 2 * segments + i - 1, true);
        }

        AddTriangle(triangles, 2 * segments - 3, 2 * segments - 1, vertices.Count - 2, true);
        AddTriangle(triangles, 2 * segments - 1, 1, vertices.Count - 1, true);


        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.name = "Cone";

        return mesh;
    }

    public static Mesh Quad(float width, float height, Vector3 normal, Vector2Int pivotPosition = default)
    {
        var mesh = new Mesh();

        var pivotOffset = new Vector3(pivotPosition.x * width/2, pivotPosition.y * height/2, 0);
        var vertices = new Vector3[] {
            new Vector3(-width/2, -height/2, 0) -pivotOffset,
            new Vector3(width/2, -height/2, 0)  - pivotOffset,
            new Vector3(width/2, height/2, 0)  - pivotOffset,
            new Vector3(-width/2, height/2, 0)  - pivotOffset,
        };
        var triangles = new int[] { 0, 2, 1, 0, 3, 2 };

        Quaternion rotation = Quaternion.FromToRotation(Vector3.back, normal);
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = rotation * vertices[i];
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mesh.name = "Custom Quad (" + width + "," + height + ")";

        return mesh;
    }

    public static Mesh Sphere (float radius, int longitudes, int latitudes)
    {
        Mesh mesh = new Mesh();

        #region Vertices
        Vector3[] vertices = new Vector3[(longitudes + 1) * latitudes + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        vertices[0] = Vector3.up * radius;
        for (int lat = 0; lat < latitudes; lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (latitudes + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= longitudes; lon++)
            {
                float a2 = _2pi * (float)(lon == longitudes ? 0 : lon) / longitudes;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[lon + lat * (longitudes + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -radius;
        #endregion

        #region Normales        
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normales[n] = vertices[n].normalized;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.up;
        uvs[uvs.Length - 1] = Vector2.zero;
        for (int lat = 0; lat < latitudes; lat++)
            for (int lon = 0; lon <= longitudes; lon++)
                uvs[lon + lat * (longitudes + 1) + 1] = new Vector2((float)lon / longitudes, 1f - (float)(lat + 1) / (latitudes + 1));
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < longitudes; lon++)
        {
            triangles[i++] = lon + 2;
            triangles[i++] = lon + 1;
            triangles[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < latitudes - 1; lat++)
        {
            for (int lon = 0; lon < longitudes; lon++)
            {
                int current = lon + lat * (longitudes + 1) + 1;
                int next = current + longitudes + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        //Bottom Cap
        for (int lon = 0; lon < longitudes; lon++)
        {
            triangles[i++] = vertices.Length - 1;
            triangles[i++] = vertices.Length - (lon + 2) - 1;
            triangles[i++] = vertices.Length - (lon + 1) - 1;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        return mesh;
    }

    static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 v1, Vector3 v2, Vector3 v3, bool ccw)
    {
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        AddTriangle(triangles, vertices.Count - 3, vertices.Count - 2, vertices.Count - 1, ccw);
    }

    static void AddTriangle(List<int> triangles, int v1, int v2, int v3, bool ccw)
    {
        if (ccw)
        {
            triangles.Add(v1);
            triangles.Add(v3);
            triangles.Add(v2);
        }
        else
        {
            triangles.Add(v1);
            triangles.Add(v2);
            triangles.Add(v3);
        }
    }

    static void AddQuad(List<int> triangles, int v1, int v2, int v3, int v4, bool ccw)
    {
        if (ccw)
        {
            triangles.Add(v1);
            triangles.Add(v4);
            triangles.Add(v3);

            triangles.Add(v1);
            triangles.Add(v3);
            triangles.Add(v2);
        } 
        else
        {
            triangles.Add(v1);
            triangles.Add(v2);
            triangles.Add(v3);

            triangles.Add(v1);
            triangles.Add(v3);
            triangles.Add(v4);
        }
    }

    static void ApplyRotation (List<Vector3> vertices, Quaternion rotation)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = rotation * vertices[i];
        }
    }
}

public enum Pivot
{
    Center,
    Bottom,
    Top
}