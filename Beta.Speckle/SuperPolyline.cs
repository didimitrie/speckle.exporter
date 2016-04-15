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
    internal class SuperPolyline
    {
        public System.Guid uuid = System.Guid.NewGuid();
        public string type = "SPKL_Polyline";
        public dynamic data = new System.Dynamic.ExpandoObject();
        public string parentGuid = "undefined";

        public SuperPolyline(Polyline p, bool isClosed, string guid)
        {
            parentGuid = guid;
            data.uvs = "";
            data.normals = "";
            data.faces = "";
            data.isClosed = isClosed;
            data.vertices = new List<double>();

            Point3d[] pts = p.ToArray();

            foreach (Point3d myPoint in pts)
            {
                data.vertices.Add(Math.Round(myPoint.Y * 1, 3));
                data.vertices.Add(Math.Round(myPoint.Z * 1, 3));
                data.vertices.Add(Math.Round(myPoint.X * 1, 3));
            }
        }

        public SuperPolyline(GH_Line line, string guid)
        {
            parentGuid = guid;
            data.uvs = "";
            data.normals = "";
            data.faces = "";
            data.vertices = new List<double>();

            Point3d start = line.Value.From;
            Point3d end = line.Value.To;

            data.vertices.Add(Math.Round(start.Y * 1, 3));
            data.vertices.Add(Math.Round(start.Z * 1, 3));
            data.vertices.Add(Math.Round(start.X * 1, 3));

            data.vertices.Add(Math.Round(end.Y * 1, 3));
            data.vertices.Add(Math.Round(end.Z * 1, 3));
            data.vertices.Add(Math.Round(end.X * 1, 3));

        }
    }
}