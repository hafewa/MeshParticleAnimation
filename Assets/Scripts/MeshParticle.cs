using System.Collections.Generic;
using UnityEngine;

public class MeshParticle : MonoBehaviour
{
    [SerializeField] private GameObject mModel;
    [SerializeField] private float mTime = 1f;
    [SerializeField] private bool mFinishcheck = true;
    [SerializeField] private int mCheckCount;
    [SerializeField] private bool[] mCheck;

    [Space]

    [SerializeField] private Transform mChest;
    [SerializeField] private Vector3[] mTargetPos;

    [Header("Prefab About")]
    [SerializeField] private Material mMaterial;
    [SerializeField] private List<Vector3> mTrianglesCenter;
    [SerializeField] private Vector3[] mNormal;
    [SerializeField] private Vector3[] mPrefabMeshVertices;
    //   [SerializeField] private Vector2[] prefabMeshUV;
    [SerializeField] private int[] mPrefabMeshTriangles;
    [SerializeField] private Vector3[] mVerticePoint;

    private Mesh mPrefabMesh;
    private MeshRenderer mMeshRenderer;
    private MeshFilter mMeshFilter;
    GameObject[] mParentGameobject;
    private int mLenght;
    private int t;
    private int tt;
    private int[] mTriangles;
    private float mDistance;

    private Vector3[] mVertices;
    private Vector3 mPosOne, mPosTwo, mPosThree, mCenter;
    private Vector3 mObjectCenter;
    private MeshFilter mPrefabMeshFilter;
    private void PrefabAssigned()
    {
        mPrefabMeshFilter = mModel.GetComponent<MeshFilter>();

        mPrefabMesh = mPrefabMeshFilter.mesh;

        mPrefabMeshVertices = mPrefabMesh.vertices;
        // prefabMeshUV = prefabMesh.uv;
        mPrefabMeshTriangles = mPrefabMesh.triangles;

        mLenght = mPrefabMeshTriangles.Length;
        print(mLenght);

        mVertices = new Vector3[mLenght];
        mTriangles = new int[6];
        mTargetPos = new Vector3[mLenght / 3];
        mCheck = new bool[mLenght / 3];

        mVerticePoint = new Vector3[mLenght];

        mParentGameobject = new GameObject[769];

        for (int i = 0; i < mLenght; i++)
        {
            mVerticePoint[i] = mModel.transform.TransformPoint(mPrefabMeshVertices[mPrefabMeshTriangles[i]]);
        }
    }


    private void CreateTriangle()
    {
        for (int i = 0; i < mLenght; i += 3)
        {
            mVertices[i] = mVerticePoint[i];
            mVertices[i + 1] = mVerticePoint[i + 1];
            mVertices[i + 2] = mVerticePoint[i + 2];

            //uv[i] = VerticePoint1[i];
            //uv[i + 1] = VerticePoint1[i + 1];
            //uv[i + 2] = VerticePoint1[i + 2];

            mTrianglesCenter.Add((mVerticePoint[i] + mVerticePoint[i + 1] + mVerticePoint[i + 2]) / 3);

            mTriangles[0] = 0;
            mTriangles[1] = 1;
            mTriangles[2] = 2;
            mTriangles[3] = 2;
            mTriangles[4] = 1;
            mTriangles[5] = 0;

        }
        tt = mTrianglesCenter.Count;

        for (int i = 0; i < tt; i++)
        {
            mObjectCenter += mTrianglesCenter[i];

            if (i == tt - 1)
                mObjectCenter = mObjectCenter / tt;
        }

        for (int a = 0; a < mLenght / 3; a++)
        {
            if (a != 0)
                t += 3;

            // meshFilter.mesh.RecalculateNormals(); aynı işlevi görüyor 
            mNormal = new Vector3[3];
            mNormal[0] = GetNormal(mVertices[t], mVertices[t + 1], mVertices[t + 2]);
            mNormal[1] = GetNormal(mVertices[t], mVertices[t + 1], mVertices[t + 2]);
            mNormal[2] = GetNormal(mVertices[t], mVertices[t + 1], mVertices[t + 2]);

            mParentGameobject[a] = new GameObject("ParentTriangle");

            Mesh newMesh = new Mesh();

            Vector3[] test = new Vector3[3];

            test[0] = mVertices[t];
            test[1] = mVertices[t + 1];
            test[2] = mVertices[t + 2];

            newMesh.vertices = test;

            //newMesh.uv = uv;
            newMesh.triangles = mTriangles;
            newMesh.normals = mNormal;

            GameObject gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));

            mPosOne = gameObject.transform.TransformPoint(mVertices[t]);
            mPosTwo = gameObject.transform.TransformPoint(mVertices[t + 1]);
            mPosThree = gameObject.transform.TransformPoint(mVertices[t + 2]);

            mCenter = (mPosOne + mPosTwo + mPosThree) / 3;
            mParentGameobject[a].transform.position = mCenter;
            gameObject.transform.parent = mParentGameobject[a].transform;
            mMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            mMeshFilter = gameObject.GetComponent<MeshFilter>();
            mMeshFilter.mesh = newMesh;
            mMeshRenderer.material = mMaterial;
            // Düzgün aydınlatma olmuyor.
            //meshFilter.mesh.RecalculateNormals();
            //  parentGameobject.SetActive(false);

            mTargetPos[a] = mTrianglesCenter[a] + (1 * ((mTrianglesCenter[a] - mObjectCenter).normalized));
            mParentGameobject[a].transform.position = mChest.position;

        }
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }


    private void Awake()
    {
        PrefabAssigned();
    }

    private void Start()
    {
        CreateTriangle();
    }

    private void Update()
    {

        for (int i = 0; i < tt; i++)
        {
            if (mFinishcheck)
                mParentGameobject[i].transform.position = Vector3.Lerp(mParentGameobject[i].transform.position, mTrianglesCenter[i], 5 * Time.deltaTime);
            else
            {
                mDistance = Vector3.Distance(mParentGameobject[i].transform.position, mTargetPos[i]);
                mParentGameobject[i].transform.position = Vector3.MoveTowards(mParentGameobject[i].transform.position, mTargetPos[i], (mDistance / mTime) * Time.deltaTime);
            }

            if (i == 0)
                mCheckCount = 0;

            if (Vector3.Distance(mParentGameobject[i].transform.position, mTargetPos[i]) < 0.01f)
            {
                mCheck[i] = true;
            }

            if (mCheck[i] == true)
            {
                mCheckCount++;

                if (mCheckCount == tt - 1)
                {
                    mFinishcheck = true;
                }
            }
        }
    }
}

