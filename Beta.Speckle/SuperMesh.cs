/*
 * Beta.Speckle GH Exporter Component
 * Copyright (C) 2016 Dimitrie A. Stefanescu (@idid) / The Bartlett School of Architecture, UCL
 * 
 */

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BetaSpeckle
{
    [Serializable]
    internal class SuperMesh : SPK_Object 
    {

        public SuperMesh(GH_Mesh myMesh, string guid, BetaSpeckleComponent parent) : base()
        {
            data.type = "SPKL_Mesh";

            data.parentGuid = guid;

            data.uvs = "";
            data.normals = "";

            hashText = this.type + this.parentGuid;

            Mesh actualMesh = myMesh.Value;

            int i = 0, count = 25, mod;

            // COLOURS (if any)
            if (actualMesh.VertexColors.Count > 0)
            {

                hashText += "colours";

                data.vertexColors = new List<int>();
                data.type = "SPKL_ColorMesh";

                i = 0;
                mod = actualMesh.VertexColors.Count / count;

                foreach (System.Drawing.Color c in actualMesh.VertexColors)
                {
                    c.ToString();
                    data.vertexColors.Add(ColorToInt(c));
                    if (mod < 2)
                        hashText += ColorToInt(c).ToString();
                    else if (i % mod == 0)
                        hashText += ColorToInt(c).ToString();
                    i++;
                }
            }

            i = 0;
            mod = actualMesh.Vertices.Count / count;

            data.vertices = new List<double>();

            hashText += actualMesh.Vertices.Count.ToString();

            foreach (Point3d vertex in actualMesh.Vertices)
            {
                data.vertices.Add(Math.Round(vertex.Y, 3));
                data.vertices.Add(Math.Round(vertex.Z, 3));
                data.vertices.Add(Math.Round(vertex.X, 3));

                parent.addToBBox(vertex.Y, vertex.Z, vertex.X);

                if (mod < 2)
                    hashText += vertex.ToString();
                else if (i % mod == 0)
                    hashText += vertex.ToString();

                i++;

            }

            // FACES
            data.faces = new List<int>();
            foreach (MeshFace face in actualMesh.Faces)
            {
                if (face.IsTriangle)
                {
                    data.faces.Add(0);
                    data.faces.Add(face.A);
                    data.faces.Add(face.B);
                    data.faces.Add(face.C);
                }
                else
                {
                    // some reason quad meshes say nono to shadows in threejs :(
                    data.faces.Add(0);
                    data.faces.Add(face.A);
                    data.faces.Add(face.B);
                    data.faces.Add(face.C);
                    data.faces.Add(0);
                    data.faces.Add(face.A);
                    data.faces.Add(face.C);
                    data.faces.Add(face.D);
                }
            }

            //myHash = sha256_hash(hashText);
        }

        public int ColorToInt(System.Drawing.Color color)
        {
            string c = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return Convert.ToInt32(c, 16);
        }
    }
}