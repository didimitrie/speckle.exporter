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

using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BetaSpeckle
{
    internal class SuperMesh
    {
        public System.Guid uuid = System.Guid.NewGuid();
        public string type = "SPKL_Mesh";
        public dynamic data = new System.Dynamic.ExpandoObject();

        public SuperMesh(GH_Mesh myMesh)
        {

            data.uvs = "";
            data.normals = "";

            Mesh actualMesh = myMesh.Value;
            
            // COLOURS (if any)
            if (actualMesh.VertexColors.Count > 0)
            {
                data.vertexColors = new List<int>();
                type = "SPKL_ColorMesh";

                foreach (System.Drawing.Color c in actualMesh.VertexColors)
                {
                    c.ToString();
                    data.vertexColors.Add(ColorToInt(c));
                }
            }

            //  VERTICES
            data.vertices = new List<double>();
            foreach (Point3d vertex in actualMesh.Vertices)
            {
                data.vertices.Add(Math.Round(vertex.Y * 1, 3));
                data.vertices.Add(Math.Round(vertex.Z * 1, 3));
                data.vertices.Add(Math.Round(vertex.X * 1, 3));
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
        }

        public int ColorToInt(System.Drawing.Color color)
        {
            string c = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return Convert.ToInt32(c, 16);
        }
    }
}