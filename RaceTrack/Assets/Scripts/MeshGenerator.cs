using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{

        //DATA

        private List<Vector3> pointsList = new List<Vector3>(); // to store our points

        private List<int> trianglesList = new List<int>(); // to store the index of the points that make up a triangle

        private List<Vector3> normalsList = new List<Vector3>(); // to store the normals for each point

        private List<int>[] submeshList = new List<int>[] {}; // to store an array of submeshes - a submesh a group of triangles that make up a submesh

        private List<Vector2> uvList = new List<Vector2>(); // to  store a list of uv coordinates - just provide a value between 0 - 1


        //Constructor
        public MeshGenerator(int submeshCount){

            submeshList = new List<int>[submeshCount];

            for(int i=0; i < submeshCount; i++){

              submeshList[i] = new List<int>();      

            }
        }


        // Functions / Methods

        public void CreateTriangle(Vector3 point1, Vector3 point2, Vector3 point3, int submeshIndex){

            // calculating the normal i.e. the surface / face of a triangle which is the perpendicular angle of a triangle
            Vector3 normal = Vector3.Cross(point2 - point1, point3 - point1).normalized;

            //get the indexes of the next avaialbe 3 locations in the points list
            int point1Index = pointsList.Count;
            int point2Index = pointsList.Count + 1;
            int point3Index = pointsList.Count + 2;


            // points
            pointsList.Add(point1);
            pointsList.Add(point2);
            pointsList.Add(point3);

            // added index to our triangles list
            trianglesList.Add(point1Index);
            trianglesList.Add(point2Index);
            trianglesList.Add(point3Index);


            //add these points to our submesh
            submeshList[submeshIndex].Add(point1Index);
            submeshList[submeshIndex].Add(point2Index);
            submeshList[submeshIndex].Add(point3Index);

            // add normals
            normalsList.Add(normal);
            normalsList.Add(normal);
            normalsList.Add(normal);


            //add uv Coordinates - value between  0 - 1
            uvList.Add(new Vector2(0,0));
            uvList.Add(new Vector2(0,1));
            uvList.Add(new Vector2(1,1));

        }

        public Mesh CreateMesh(){

            Mesh mesh = new Mesh();

            mesh.vertices = pointsList.ToArray();

            mesh.triangles = trianglesList.ToArray();

            mesh.normals = normalsList.ToArray();

            mesh.uv = uvList.ToArray();

            mesh.subMeshCount = submeshList.Length;

            for(int i = 0; i < submeshList.Length; i++){
                
                if(submeshList[i].Count < 3){
                    mesh.SetTriangles(new int[3]{0,0,0}, i);
                }else{
                   mesh.SetTriangles(submeshList[i].ToArray(), i);    
                }
                
            }

            return mesh;

        }

}
