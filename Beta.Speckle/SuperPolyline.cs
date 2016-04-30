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
    internal class SuperPolyline : SPK_Object
    {

        private Point3d[] mypoints;

        public SuperPolyline(Polyline p, bool isClosed, string guid, BetaSpeckleComponent parent)  : base()
        {

            data.type = "SPKL_Polyline";

            data.parentGuid = guid;
            data.uvs = "";
            data.normals = "";
            data.faces = "";
            data.isClosed = isClosed;
            data.vertices = new List<double>();

            Point3d[] pts = p.ToArray();
            mypoints = pts;

            hashText = this.type + this.parentGuid + data.isClosed;
           
            int i = 0, count = 25 - 1, mod;
            mod = pts.Length / count;

            foreach (Point3d myPoint in pts)
            {
                data.vertices.Add(Math.Round(myPoint.Y * 1, 3));
                data.vertices.Add(Math.Round(myPoint.Z * 1, 3));
                data.vertices.Add(Math.Round(myPoint.X * 1, 3));

                parent.addToBBox(myPoint.Y, myPoint.Z, myPoint.X);

                if (mod < 2)
                    hashText += myPoint.ToString();
                else if (i % mod == 0)
                    hashText += myPoint.ToString();
            }
        }

        public SuperPolyline(GH_Line line, string guid, BetaSpeckleComponent parent) : base() 
        {
            this.myGeometry = line.Value;

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

            parent.addToBBox(start.Y, start.Z, start.X);
            parent.addToBBox(end.Y, end.Z, end.X);

            hashText = this.type + this.parentGuid + data.isClosed.toString();
            hashText += start.ToString() + end.ToString();
        }
        
    }
}