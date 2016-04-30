/*
 * Beta.Speckle GH Exporter Component
 * Copyright (C) 2016 Dimitrie A. Stefanescu (@idid) / The Bartlett School of Architecture, UCL
 *
 * This program is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program.  If not, see <http://www.gnu.org/licenses/>.
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
            this.myGeometry = myMesh.Value;

            data.type = "SPKL_Mesh";

            data.parentGuid = guid;

            data.uvs = "";
            data.normals = "";

            string hashText = this.type + this.parentGuid;

            Mesh actualMesh = myMesh.Value;
            
            // COLOURS (if any)
            if (actualMesh.VertexColors.Count > 0)
            {

                hashText += "colours";

                data.vertexColors = new List<int>();
                data.type = "SPKL_ColorMesh";

                foreach (System.Drawing.Color c in actualMesh.VertexColors)
                {
                    c.ToString();
                    data.vertexColors.Add(ColorToInt(c));
                }
            }

            // hashing logic:
            // if mesh.vertices.length > 50 sample 50
            // else sample all
            // -> need a modifier
            // Vertices.Count / 50 = 

            //  VERTICES
            int k = 0, i = 0;
            data.vertices = new List<double>();

            hashText += actualMesh.Vertices.Count.ToString();

            foreach (Point3d vertex in actualMesh.Vertices)
            {
                data.vertices.Add(Math.Round(vertex.Y, 3));
                data.vertices.Add(Math.Round(vertex.Z, 3));
                data.vertices.Add(Math.Round(vertex.X, 3));

                parent.addToBBox(vertex.Y, vertex.Z, vertex.X);

                if (k++ < 50)
                    hashText += Math.Round(vertex.Y, 3).ToString() + Math.Round(vertex.Z, 3).ToString() + Math.Round(vertex.X, 3).ToString();
                    //hashText += vertex.ToString();
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