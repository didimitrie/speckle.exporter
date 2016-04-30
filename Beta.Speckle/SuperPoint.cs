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
    internal class SuperPoint : SPK_Object
    {
        public SuperPoint(GH_Point pp, string guid, BetaSpeckleComponent parent) : base()
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

            hashText = this.type + this.parentGuid + p.ToString();

            parent.addToBBox(p.Y, p.Z, p.X);
        }
    }
}