// using UnityEngine;

// public class BezierMeshGenerator : MonoBehaviour
// {
//     public Vector3[] points; // Array of control points for the Bezier curve
//     public int resolution = 10; // Number of segments to generate in the mesh

//     void Start()
//     {
//         // Create an array of vertices for the mesh
//         Vector3[] vertices = new Vector3[(resolution + 1) * 4];

//         // Generate the vertices for the mesh
//         for (int i = 0; i <= resolution; i++)
//         {
//             // Calculate the position of the vertex along the Bezier curve
//             float t = (float)i / resolution;
//             Vector3 point = Bezier.GetPoint(points, t);

//             // Set the position of the vertex
//             vertices[i * 4] = point;
//             vertices[i * 4 + 1] = point;
//             vertices[i * 4 + 2] = point;
//             vertices[i * 4 + 3] = point;
//         }

//         // Create an array of triangles for the mesh
//         int[] triangles = new int[resolution * 6];
//         for (int i = 0; i < resolution; i++)
//         {
//             triangles[i * 6] = i * 4;
//             triangles[i * 6 + 1] = i * 4 + 1;
//             triangles[i * 6 + 2] = i * 4 + 4;
//             triangles[i * 6 + 3] = i * 4 + 4;
//             triangles[i * 6 + 4] = i * 4 + 1;
//             triangles[i * 6 + 5] = i * 4 + 5;
//         }

//         // Create a new mesh and populate it with the generated vertices and triangles
//         Mesh mesh = new Mesh();
//         mesh.vertices = vertices;
//         mesh.triangles = triangles;

//         // Assign the mesh to the mesh filter of the game object
//         GetComponent<MeshFilter>().mesh = mesh;
//     }
// }
