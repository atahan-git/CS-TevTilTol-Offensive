using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PrefabAlongSpline : MonoBehaviour {

	#if UNITY_EDITOR


	public GameObject myPrefab;
	public float prefabSize = 1f;
	public float prefabHeight = 1.05f;
	public Quaternion prefabRotation = Quaternion.identity;
	//public Vector3 prefabOffset = Vector3.zero;

	GameObject[] myObjects = new GameObject[0];
	int size = 0;

	public bool editModeRender = false;

	//-------------------------------------------------------- INSTANTIATE STUFF

	void Start () {
		/*foreach (Transform chld in transform) {
			DestroyImmediate (chld.gameObject);
		}
		foreach (Transform chld in transform) {
			DestroyImmediate (chld.gameObject);
		}*/

		if (transform.childCount == 0) {

			DestroyOldObjects ();
			OnValidate ();
		}
	}

	public void Update (){
		if (Application.isPlaying)
			this.enabled = false;

		//DestroyOldObjects ();
		OnValidate ();
	}

	int n = 0;
	public void OnValidate () {
		if (myPrefab == null)
			return;

		if (transform.childCount != 0) {
			if (Application.isEditor && !editModeRender && !Application.isPlaying)
				return;
		}

		//print ("on validate");
		n = 0;
		try{
		DestroyOldObjects ();
		}catch{
		}
		Vector3 p0 = points[0];
		for (int i = 1; i < points.Length; i += 1) {
			//print (i + " " + i);
			Vector3 p1 = points[i];
			CheckSize (transform.TransformPoint(p0), transform.TransformPoint(p1), i);
			p0 = p1;
		}


		//ResizeObjects ();
	}

	//public GameObject debugObj;

	int oldSize;
	float distance;
	float oldDistance;
	void CheckSize (Vector3 pointA, Vector3 pointB, int count) {
		//print ("check size");
		distance = Vector3.Distance (pointA, pointB);

		//ShearEffect.s.ParallelShear (debugObj, distance, 2f);

		size = Mathf.RoundToInt((distance)/prefabSize);

		//print (distance + " " + size);

		//if (size != oldSize) {
		DrawPrefabs (pointA, pointB, size, count);
		//} else if (distance != oldDistance) {
			//DrawPrefabs (pointA, pointB, size);
			//ResizeObjects (pointA, pointB);
		//}

		oldSize = size;
		oldDistance = distance;
	}

	void DrawPrefabs (Vector3 pointA, Vector3 pointB, int mySize, int count) {

		if (pointA.y > pointB.y) {
			Vector3 temp = pointB;
			pointB = pointA;
			pointA = temp;
		}


		GameObject preParent = new GameObject ();
		preParent.transform.parent = transform;
		preParent.name = "Section " + count.ToString ();
		preParent.transform.position = pointA;
		preParent.transform.rotation = Quaternion.LookRotation (pointA - pointB);
		
		myObjects = new GameObject[mySize];
		for (int i = 0; i < mySize; i++) {
			//print (mySize + " " + i);
			myObjects [i] = (GameObject)Instantiate (myPrefab, Vector3.zero, Quaternion.identity);
			myObjects [i].name = myObjects [i].name + " " + n.ToString();
			n++;
			myObjects [i].transform.parent = preParent.transform;
		}

		ResizeObjects (pointA, pointB, preParent);
	}


	void ResizeObjects (Vector3 pointA, Vector3 pointB, GameObject preParent) {
		Quaternion objRot = Quaternion.LookRotation (pointA - pointB);
		for (int i = 0; i < size; i++) {
			myObjects [i].transform.position = Vector3.Lerp (pointA, pointB, (1f / ((float)size)) * ((float)i + 1));
			//myObjects [i].transform.position += transform.position;
			//print ((1f / ((float)size + 1f)) * ((float)i + 1));
			myObjects [i].transform.rotation = objRot;
			//myObjects [i].transform.position += myObjects [i].transform.InverseTransformDirection (prefabOffset);
			//myObjects [i].transform.localRotation *= prefabRotation;
			float s = distance / (prefabSize * (float)size);
			myObjects [i].transform.localScale = new Vector3 (1f,1f,s);
		}

		if (pointA.y != pointB.y)
			ParallelShear (preParent, Vector3.Distance (pointA, pointB), prefabHeight);
	}


	public void DestroyOldObjects () {

		var children = new List<GameObject>();
		foreach (Transform child in transform) children.Add(child.gameObject);
		try{
	#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall+=()=>
			{
				children.ForEach(child => DestroyImmediate(child));
			};
				//children.ForEach(child => DestroyImmediate(child));
	#endif
				children.ForEach(child => Destroy(child));
		}
		catch{
		}
			
	}

	//-------------------------------------------------------- SHEAR STUFF

	/*
	 * 									C
	 * 							O		|
	 * 					_		| -		|
	 * 			_				|    -	|
	 * 		A---------------------------B
	 * 							|
	 * 							D
	 */


	void ParallelShear (GameObject obj, float sizeX, float sizeY){

		Vector3 a = obj.transform.position;
		Vector3 b = obj.transform.position + (-obj.transform.forward * sizeX);

		Vector3 c = b + (obj.transform.up * sizeY);

		//Debug.DrawLine (a, b, Color.red);
		//Debug.DrawLine (b, c, Color.green);

		Vector3 d = new Vector3 (c.x, a.y, c.z);

		GameObject firstp = new GameObject ();
		GameObject secondp = new GameObject ();
		firstp.transform.parent = transform;
		secondp.transform.parent = transform;
		firstp.name = "Parallel Correction";
		secondp.name = obj.name + " Size Correction";

		firstp.transform.position = a;
		secondp.transform.position = a;
		firstp.transform.LookAt (c, obj.transform.up);
		secondp.transform.LookAt (new Vector3 (c.x, a.y, c.z), Vector3.up);
		//firstp.transform.rotation = Quaternion.LookRotation (a - c);
		firstp.transform.parent = secondp.transform;

		//math
		float bc = Vector3.Distance (b, c);
		float ac = Vector3.Distance (a, c);
		float oc = (bc * bc) / ac;

		Vector3 o = Vector3.Lerp (c, a, oc / ac);

		float cd = Vector3.Distance (c, d);
		float similarityMultiplier = cd / oc;
		float ad = Vector3.Distance (a, d);
		float og = ad / similarityMultiplier;

		float ob = Vector3.Distance (o, b);
		Vector3 g = Vector3.Lerp (o, b, og / ob);

		float gb = Vector3.Distance (g, b);


		Vector3 m = new Vector3 (d.x, b.y, d.z);
		Vector3 e = new Vector3 (b.x, a.y, b.z);
		float ae = Vector3.Distance (a, e);
		float bm = Vector3.Distance (b, m);
		float gd = Vector3.Distance (g, d);
		float gm = Vector3.Distance (g, m);

		//actual doing it

		firstp.transform.localScale = new Vector3 (1, og / ob, 1);
		//Debug.DrawLine (o, g, Color.blue);
		//Debug.DrawLine (g, b, Color.white);


		Quaternion tempRot = obj.transform.rotation;
		obj.transform.SetParent (firstp.transform, false);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.rotation = tempRot;
		secondp.transform.localScale = new Vector3 (1, ((gd - gm) / gd), (ae / (ae - bm)));

		float _gc = ((gd - gm) / gd) * Vector3.Distance (g, c);

		obj.transform.localScale = new Vector3 (1, prefabHeight / _gc, 1);
	}











	//-------------------------------------------------------- LINE STUFF

	public Vector3[] points = new Vector3[2];

	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 2;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 1;
		}
		return transform.TransformPoint(Line.GetPoint(
			points[i], points[i + 1], t));
	}

	public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 2;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 1;
		}
		return transform.TransformPoint(Line.GetFirstDerivative(
			points[i], points[i + 1],  t)) - transform.position;
	}

	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

	public int CurveCount {
		get {
			return (points.Length - 1) / 1;
		}
	}

	public void Reset () {
		points = new Vector3[] {
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f)
		};
	}

	public void AddCurve () {
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 1);
		point.x += 1f;
		points[points.Length - 1] = point;
	}

	#endif
}

public static class Line {

	public static Vector3 GetPoint (Vector3 p0, Vector3 p1, float t) {
		t = Mathf.Clamp01(t);

		return Vector3.Lerp (p0, p1, t);
		/*float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * p0 +
			3f * oneMinusT * oneMinusT * t * p1 +
			3f * oneMinusT * t * t * p2 +
			t * t * t * p3;*/
	}

	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			Vector3.Normalize(p1 - p0);
	}
		
}
