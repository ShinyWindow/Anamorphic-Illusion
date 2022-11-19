using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class OffCenterCamera : MonoBehaviour {

	/*
	 * Anamorphic camera defined by the 3 corners of the screen.
	 */

	Camera cam; //Camera of the 'projector'

	public Transform EyePosition;

    public Transform BottomLeftCornerTransform;
    public Transform BottomRightCornerTransform;
    public Transform TopLeftCornerTransform;

    Vector3 BottomLeftCorner;
	Vector3 BottomRightCorner;
	Vector3 TopLeftCorner;

	/** The fourth corner of the screen, based on others corners' position. Read-only. */
	public Vector3 TopRightCorner{
		get {
			return new Vector3 (TopLeftCorner.x + (BottomRightCorner.x - BottomLeftCorner.x),
				BottomRightCorner.y + (TopLeftCorner.y - BottomLeftCorner.y),
				BottomRightCorner.z + (TopLeftCorner.z - BottomLeftCorner.z));
		}
	}

	/** The width of the screen */
	public float Width {
		get {
			return (BottomRightCorner - BottomLeftCorner).magnitude;
		}
		set {
			Vector3 vecWidth = BottomRightCorner - BottomLeftCorner;
			float scale = value / vecWidth.magnitude;
			vecWidth *= (1-scale);

			TopLeftCorner += vecWidth/2;
			BottomLeftCorner += vecWidth/2;
			BottomRightCorner -= vecWidth/2;
		}
	}

	/** The height of the screen */
	public float Height {
		get {
			return (TopLeftCorner - BottomLeftCorner).magnitude;
		}
		set {
			Vector3 vecHeight = TopLeftCorner - BottomLeftCorner;
			float scale = value / vecHeight.magnitude;
			vecHeight *= (1-scale);

			TopLeftCorner -= vecHeight/2;
			BottomLeftCorner += vecHeight/2;
			BottomRightCorner += vecHeight/2;
		}
	}

	// Use this for initialization
	void Start () {
		
        cam = GetComponent<Camera>();
	}

	// Update is called once per frame
	public void Update () {
		//calculate projection
		Matrix4x4 genProjection = GeneralizedPerspectiveProjection(
			BottomLeftCorner, BottomRightCorner,
			TopLeftCorner, transform.position,
			cam.nearClipPlane, cam.farClipPlane);
		cam.projectionMatrix = genProjection;
	}

	private void FixedUpdate()
	{
		transform.position = EyePosition.position;
		BottomLeftCorner = BottomLeftCornerTransform.position;
		TopLeftCorner = TopLeftCornerTransform.position;
		BottomRightCorner = BottomRightCornerTransform.position;
		//cam.projectionMatrix = cam.projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
	}

    // Off-center camera
    public static Matrix4x4 GeneralizedPerspectiveProjection(Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pe, float near, float far){

		// Modify vectors to fix bugs
		pa = new Vector3(pa.x, pa.y, pa.z);
		pb = new Vector3(pb.x, pb.y, pb.z);
		pc = new Vector3(pc.x, pc.y, pc.z);

		Vector3 va, vb, vc;
		Vector3 vr, vu, vn;

		float left, right, bottom, top, eyedistance;

		Matrix4x4 transformMatrix;
		Matrix4x4 projectionM;
		Matrix4x4 eyeTranslateM;
		Matrix4x4 finalProjection;

		///Calculate the orthonormal for the screen (the screen coordinate system
		vr = pb - pa;
		vr.Normalize();
		vu = pc - pa;
		vu.Normalize();
		vn = Vector3.Cross(vr, vu);
		vn.Normalize();

		//Calculate the vector from eye (pe) to screen corners (pa, pb, pc)
		va = pa-pe;
		vb = pb-pe;
		vc = pc-pe;

		//Get the distance from the eye to the screen plane
		eyedistance = -(Vector3.Dot(vn, va));
		//Debug.Log ("dist : " + eyedistance);

		//Get the variables for the off center projection
		left = Vector3.Dot(vr, va)*near/eyedistance;
		//Debug.Log ("left : " + left);
		right  = Vector3.Dot(vr, vb)*near/eyedistance;
		//Debug.Log ("right : " + right);
		bottom  = Vector3.Dot(vu, va)*near/eyedistance;
		top = Vector3.Dot(vu, vc)*near/eyedistance;


		//Get this projection
		projectionM = PerspectiveOffCenter(left, right, bottom, top, near, far);
		//Fill in the transform matrix
		transformMatrix = new Matrix4x4();
		transformMatrix[0, 0] = vr.x;
		transformMatrix[0, 1] = vr.y;
		transformMatrix[0, 2] = vr.z;
		transformMatrix[0, 3] = 0;
		transformMatrix[1, 0] = vu.x;
		transformMatrix[1, 1] = vu.y;
		transformMatrix[1, 2] = vu.z;
		transformMatrix[1, 3] = 0;
		transformMatrix[2, 0] = vn.x;
		transformMatrix[2, 1] = vn.y;
		transformMatrix[2, 2] = vn.z;
		transformMatrix[2, 3] = 0;
		transformMatrix[3, 0] = 0;
		transformMatrix[3, 1] = 0;
		transformMatrix[3, 2] = 0;
		transformMatrix[3, 3] = 1;


		//Multiply all together
		finalProjection = new Matrix4x4();
		// INFO : I added the last matrix to rotate the camera 180° around Z
		finalProjection = Matrix4x4.identity * projectionM * transformMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,180), Vector3.one);
        //finally return
        return finalProjection;
	}

	static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far) {
		float x = 2.0F * near / (right - left);
		float y = 2.0F * near / (top - bottom);
		float a = (right + left) / (right - left);
		float b = (top + bottom) / (top - bottom);
		float c = -(far + near) / (far - near);
		float d = -(2.0F * far * near) / (far - near);
		float e = -1.0F;

		Matrix4x4 m = new Matrix4x4();
		m[0, 0] = x;
		m[0, 1] = 0;
		m[0, 2] = a;
		m[0, 3] = 0;
		m[1, 0] = 0;
		m[1, 1] = y;
		m[1, 2] = b;
		m[1, 3] = 0;
		m[2, 0] = 0;
		m[2, 1] = 0;
		m[2, 2] = c;
		m[2, 3] = d;
		m[3, 0] = 0;
		m[3, 1] = 0;
		m[3, 2] = e;
		m[3, 3] = 0;
		return m;
	}

	public void OnDrawGizmos(){

		float size = 0.1f;
		Gizmos.color = Color.red;
		Gizmos.DrawSphere (BottomLeftCorner, size);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere (BottomRightCorner, size);
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere (TopLeftCorner, size);
		Gizmos.color = Color.gray;
		Gizmos.DrawSphere (TopRightCorner, size);

		/*

		Gizmos.color = Color.magenta;
		Gizmos.DrawLine (transform.position, transform.position + va); 
		Gizmos.DrawLine (transform.position, transform.position + vb); 
		Gizmos.DrawLine (transform.position, transform.position + vc); 

		Gizmos.color = Color.red;
		Gizmos.DrawLine (BottomLeftCorner, BottomLeftCorner + vr); 
		Gizmos.color = Color.green;
		Gizmos.DrawLine (BottomLeftCorner, BottomLeftCorner + vu); 
		Gizmos.color = Color.blue;
		Gizmos.DrawLine (BottomLeftCorner, BottomLeftCorner + vn); 

		*/
	}

}