using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace BezierSolution
{
	[AddComponentMenu( "Bezier Solution/Bezier Generate" )]
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	public class BezierGenerateMesh : MonoBehaviour
	{
		public enum RotationMode { No = 0, UseSplineNormals = 1, UseEndPointRotations = 2 };

#pragma warning disable 0649
		[SerializeField]
		private BezierSpline m_spline;
		public BezierSpline spline
		{
			get { return m_spline; }
			set
			{
				if( m_spline != value )
				{
					if( m_spline )
						m_spline.onSplineChanged -= OnSplineChanged;

					m_spline = value;

					if( m_spline && isActiveAndEnabled )
					{
						m_spline.onSplineChanged -= OnSplineChanged;
						m_spline.onSplineChanged += OnSplineChanged;

						OnSplineChanged( m_spline, DirtyFlags.All );
					}
				}
			}
		}

    [SerializeField] Mesh2D shape2D;
    [Range(2, 64)]
    [SerializeField] int ringCount = 8;

		[SerializeField]
		[Range( 0f, 1f )]
		private float m_normalizedT = 0f;

		[SerializeField]
		[Range( 0f, 10f )]
		public float scale = 1f;
		public float normalizedT
		{
			get { return m_normalizedT; }
			set
			{
				value = Mathf.Clamp01( value );

				if( m_normalizedT != value )
				{
					m_normalizedT = value;

					if( isActiveAndEnabled )
						OnSplineChanged( m_spline, DirtyFlags.All );
				}
			}
		}

#if UNITY_EDITOR
		[Header( "Other Settings" )]
		[SerializeField]
		private bool executeInEditMode = false;

		[SerializeField, HideInInspector]
		private BezierSpline prevSpline;
#endif
#pragma warning restore 0649

		Mesh mesh;

		private void OnEnable()
		{
			if( m_spline )
			{
				m_spline.onSplineChanged -= OnSplineChanged;
				m_spline.onSplineChanged += OnSplineChanged;

				OnSplineChanged( m_spline, DirtyFlags.All );
			}
		}

    void Awake() {

        mesh = new Mesh();
        mesh.name = "Segment";

        GetComponent<MeshFilter>().sharedMesh = mesh;

				GenerateMesh();

				var mc = gameObject.GetComponent<MeshCollider>();
				mc.sharedMesh = mesh;
    }
    void GenerateMesh() {

        mesh.Clear();

        // Vertices

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
				OrientedPoint op = GetBezierPoint(0);

        for (int ring = 0; ring < ringCount+1; ring++) {

            float t = (ring) / (float)(ringCount-1);

						op = GetBezierPoint(t);

            for (int i = 0; i < shape2D.VertexCount; i++) {

                verts.Add( op.LocalToWorld( shape2D.vertices[i].point * scale ));
                normals.Add( op.LocalToWorldVector( shape2D.vertices[i].normal ));
            }
        }

        // Triangles

        List<int> triIndices = new List<int>();
				List<int> quadIndices = new List<int>();
				List<int> lineIndices = new List<int>();
				

        for(int ring = 0; ring < ringCount-1; ring++) {
            int rootIndex = ring * shape2D.VertexCount;
            int rootIndexNext = (ring+1) * shape2D.VertexCount;

            for(int line = 0; line < shape2D.LineCount; line+=2) {

                int lineIndexA = shape2D.lineIndices[line];
                int lineIndexB = shape2D.lineIndices[line+1];

								// Debug.Log(lineIndexA + " " + lineIndexB);
								

                int currentA = rootIndex + lineIndexA;
                int currentB = rootIndex + lineIndexB;
                int nextA = rootIndexNext + lineIndexA;
                int nextB = rootIndexNext + lineIndexB;

								// lineIndices.Add(currentA);
								// lineIndices.Add(nextA);
								// lineIndices.Add(currentB);
								// lineIndices.Add(nextB);



								// quadIndices.Add(currentA);
								// quadIndices.Add(nextA);
								// quadIndices.Add(nextB);
								// quadIndices.Add(currentB);

                triIndices.Add(currentA);
                triIndices.Add(nextA);
                triIndices.Add(nextB);

                triIndices.Add(currentA);
                triIndices.Add(nextB);
                triIndices.Add(currentB);


            }


        }

        // Debug.Log(triIndices.ToArray().Length);

        mesh.SetVertices( verts );
        mesh.SetNormals( normals );
        mesh.SetTriangles( triIndices, 0);
				// mesh.SetIndices(lineIndices, MeshTopology.LineStrip, 0);

				


    }

		private void OnDisable()
		{
			if( m_spline )
				m_spline.onSplineChanged -= OnSplineChanged;
		}
		

#if UNITY_EDITOR
		private void OnValidate()
		{
			UnityEditor.Undo.RecordObject( transform, "Modify BezierAttachment" );

			BezierSpline _spline = m_spline;
			m_spline = prevSpline;
			spline = prevSpline = _spline;

			if( isActiveAndEnabled )
				OnSplineChanged( m_spline, DirtyFlags.All );
		}

    private void Update() {
			GenerateMesh();
		}

		private void LateUpdate()
		{
			if( transform.hasChanged )
				OnSplineChanged( m_spline, DirtyFlags.All );
		}
#endif

		private void OnSplineChanged( BezierSpline spline, DirtyFlags dirtyFlags )
		{
	#if UNITY_EDITOR
				if( !executeInEditMode && !UnityEditor.EditorApplication.isPlaying )
					return;
	#endif

			RefreshInternal( dirtyFlags );
		}

		public void Refresh()
		{
			RefreshInternal( DirtyFlags.All );
		}

    OrientedPoint GetBezierPoint( float t ) {
        return new OrientedPoint(spline.GetPoint(t), spline.GetTangent(t), spline.GetNormal(t));
    }


		private void RefreshInternal( DirtyFlags dirtyFlags )
		{
			if( !m_spline || m_spline.Count < 2 )
				return;

			// if( !m_updatePosition && m_updateRotation == RotationMode.No )
			// 	return;

			BezierSpline.Segment segment = m_spline.GetSegmentAt( m_normalizedT );

			// switch( m_updateRotation )
			// {
			// 	case RotationMode.UseSplineNormals:
			// 		if( m_rotationOffset == Vector3.zero )
			// 			transform.rotation = Quaternion.LookRotation( segment.GetTangent(), segment.GetNormal() );
			// 		else
			// 			transform.rotation = Quaternion.LookRotation( segment.GetTangent(), segment.GetNormal() ) * Quaternion.Euler( m_rotationOffset );

			// 		break;
			// 	case RotationMode.UseEndPointRotations:
			// 		if( m_rotationOffset == Vector3.zero )
			// 			transform.rotation = Quaternion.LerpUnclamped( segment.point1.rotation, segment.point2.rotation, segment.localT );
			// 		else
			// 			transform.rotation = Quaternion.LerpUnclamped( segment.point1.rotation, segment.point2.rotation, segment.localT ) * Quaternion.Euler( m_rotationOffset );

			// 		break;
			// }

			// if( m_updatePosition && ( dirtyFlags & DirtyFlags.SplineShapeChanged ) == DirtyFlags.SplineShapeChanged )
			// {
			// 	if( m_positionOffset == Vector3.zero )
			// 		transform.position = segment.GetPoint();
			// 	else
			// 		transform.position = segment.GetPoint() + transform.rotation * m_positionOffset;
			// }

#if UNITY_EDITOR
			transform.hasChanged = false;
#endif
		}
	}
}