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
    internal class SuperPoint : SPK_Object
    {
        public SuperPoint(GH_Point pp, string guid) : base()
        { 
            data.type = "SPKL_Point";

            data.parentGuid = guid;

            Point3d p = pp.Value;

            data.uvs = "";
            data.normals = "";
            data.faces = "";
            data.vertices = new List<double>();

            data.vertices.Add(Math.Round(p.Y * 1, 3));
            data.vertices.Add(Math.Round(p.Z * 1, 3));
            data.vertices.Add(Math.Round(p.X * 1, 3));

            string hashText = this.type + this.parentGuid + p.ToString();
            myHash = sha256_hash(hashText);
        }
    }
}