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

/// 
/// HIC SVNT DRACONES (aka bad code)
/// 

using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Newtonsoft.Json;
using System.Linq;
using Grasshopper.Kernel.Types;
using System.IO;
using System.IO.Compression;
using Grasshopper;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Text;
using Ionic.Zip;
using System.Diagnostics;
using Grasshopper.GUI.Canvas;
using GH_IO.Serialization;
using System.Drawing.Drawing2D;
using Grasshopper.GUI;

namespace BetaSpeckle
{
    public class BetaSpeckleComponent : GH_Component
    {

        /// 
        /// HIC SVNT DRACONES (aka really bad code)
        /// not sure whether to rewrite or keep patching :/
        /// 

        // IMPORTANT VARIABLES

        // Part 0: GH doc + folders
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        string FOLDERLOCATION = @"c:\temp";
        string GHDEFNAME = "";

        // Part 1: lists and stuff
        // holds all the possible slider combinations: list[ [p1v1, p2v1, p3v1], [p1v2, p2v1, p3v1], ... ]
        List<List<double>> sliderValues = new List<List<double>>();

        // this lot - no idea; it's late.
        List<string> sliderNames;
        List<List<double>> myMatrix;
        List<List<System.Object>> geometries;
        List<string> geometrySets;
        List<SuperKVP> kvpairs = new List<SuperKVP>();
        List<SuperProperty> myProperties = new List<SuperProperty>();

        dynamic OUTFILE;

        // Part 2: other params

        // How many values we split up a double slider range( [min, max], MAXVALUES )
        int MAXVALUES = 6;
        int INSTANCECOUNT = -1;
        int currentCount = 0;
        string currentInstanceName = "";

        // Part 3: random
        bool EMERGENCY_BREAK = false;
        public bool SOLVE = false;
        bool ALLOWLARGE = false;
        bool PATHISSET = false;
        bool allowDefExport = true;

        // Part X: Testing hashes
        HashSet<string> myHashSet = new HashSet<string>();

        public BetaSpeckleComponent()
          : base("Beta.Speckle", "BSpk",
              "Export a Beta.Speckle Archive",
              "Params", "Util")
        {
            myHashSet = new HashSet<string>();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("SLD", "SLD", "Parametric measures: The sliders that you want to define your parametric space with.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("PRF", "PRF", "Performance measures: values that you evaluate your design by (area, cost, element number, etc.). ", GH_ParamAccess.tree);
            pManager.AddGenericParameter("OBJ", "OBJ", "Dynamic geometry: can be breps, (coloured) meshes, curves and points.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("STA", "STA", "Static geometry: objects that do not change (ie, context)", GH_ParamAccess.tree);


            //pManager[0].Optional = true;
            pManager[1].Optional = true;
            //pManager[2].Optional = true;
            pManager[3].Optional = true;

            pManager[0].WireDisplay = GH_ParamWireDisplay.faint;
            pManager[1].WireDisplay = GH_ParamWireDisplay.faint;
            pManager[2].WireDisplay = GH_ParamWireDisplay.faint;
            pManager[3].WireDisplay = GH_ParamWireDisplay.faint;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("test", "test", "test", GH_ParamAccess.list);
        }

        #region Menu Items
        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            GH_DocumentObject.Menu_AppendItem(menu, @"User Guide - Give it a read!", openHelp);
            GH_DocumentObject.Menu_AppendSeparator(menu);
            GH_DocumentObject.Menu_AppendItem(menu, @"Export!", menuExport);
            GH_DocumentObject.Menu_AppendSeparator(menu);
            GH_DocumentObject.Menu_AppendItem(menu, @"Allow Defintion Export (good for debugging)", allowGHExport, true, allowDefExport);
            GH_DocumentObject.Menu_AppendSeparator(menu);
            GH_DocumentObject.Menu_AppendItem(menu, @"Github Source | MIT License", gotoGithub);
            GH_DocumentObject.Menu_AppendSeparator(menu);
            GH_DocumentObject.Menu_AppendItem(menu, @"The Bartlett UCL", gotoBartlett);
            GH_DocumentObject.Menu_AppendItem(menu, @"InnoChain", gotoInnochain);
            GH_DocumentObject.Menu_AppendItem(menu, @"Questions: @idid", gotoTwitter);

            return true;
        }

        private void openHelp(Object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/didimitrie/speckle.exporter/wiki/User-Guide");
        }


        private void allowGHExport(Object sender, EventArgs e)
        {
            allowDefExport = !allowDefExport;
        }

        private void menuExport(Object sender, EventArgs e)
        {
            this.SOLVE = true;
            this.Export();
            this.ExpireSolution(true);
        }

        private void gotoGithub(Object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/didimitrie/speckle.exporter");
        }

        private void gotoBartlett(Object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://www.bartlett.ucl.ac.uk/");
        }

        private void gotoInnochain(Object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://www.innochain.net");
        }

        private void gotoTwitter(Object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://twitter.com/idid");
        }
        #endregion

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            Component = this;

            GrasshopperDocument = Instances.ActiveCanvas.Document;

            GHDEFNAME = RemoveSpecialCharacters(GrasshopperDocument.DisplayName.ToString());
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            myMatrix = constructMatrixFromSliders((GH_Component)Component);
            INSTANCECOUNT = myMatrix.Count;
            if(INSTANCECOUNT > 4242)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "There are " + INSTANCECOUNT + " instances. That's quite a lot. Try reducing the parameter space!\nSpeckle WILL work but you might have a long wait ahead.");
            }

            this.Message = "# Instances:\n" + myMatrix.Count;

            if (!SOLVE)
            {
                // part of some obscure ritual code
                geometries = new List<List<System.Object>>();
                kvpairs = new List<SuperKVP>();

                myProperties = new List<SuperProperty>();

                foreach (IGH_Param param in Component.Params.Input[1].Sources)
                {
                    SuperProperty myProperty = new SuperProperty(getPanelName(param));
                    myProperties.Add(myProperty);
                }

                EMERGENCY_BREAK = false;


                List<System.Object> inputObjects = new List<System.Object>();

                this.Message = "# Instances:\n" + myMatrix.Count;

                // critical - if not set we don't know where to sthap
                INSTANCECOUNT = myMatrix.Count;

            }
            else
            {
                // sanity checks
                if (EMERGENCY_BREAK) return;

                // running through the iterations - so store and save

                List<System.Object> geoms = new List<System.Object>();
                List<string> guids = new List<string>();

                foreach (IGH_Param param in Component.Params.Input[2].Sources)
                {
                    foreach (Object myObj in param.VolatileData.AllData(true))
                    {
                        geoms.Add(myObj); // these are the object geometries
                        guids.Add(param.InstanceGuid.ToString()); // these are the guids of the parent componenets
                    }
                }

                string path = Path.Combine(FOLDERLOCATION, currentInstanceName + ".json");


                writeFile(JsonConvert.SerializeObject(translateGeometry(geoms, guids, currentInstanceName, Component), Newtonsoft.Json.Formatting.None), path);

                // get the key value pairs nicely wrapped up
                SuperKVP myKVP = getCurrentKVP(currentInstanceName);
                kvpairs.Add(myKVP);

                this.Message = currentCount + " / " + INSTANCECOUNT;
                string[] splitvals = myKVP.values.Split(',');

                int counter = 0;

                foreach (SuperProperty prop in myProperties)
                {
                    prop.addValue(splitvals[counter]);
                    counter++;
                }

                currentCount++;

                this.Message = currentCount + "\n---\n" + INSTANCECOUNT;
                //that means we have calculated all the required instances
                if (currentCount == INSTANCECOUNT)
                {
                    SOLVE = false;
                    string pathh = "";

                    // write the static geom file

                    List<Object> staticGeo = new List<Object>();

                    foreach (IGH_Param param in Component.Params.Input[3].Sources)
                        foreach (Object myObj in param.VolatileData.AllData(true))
                            staticGeo.Add(myObj); // these are the object geometries

                    pathh = Path.Combine(FOLDERLOCATION, "static.json");
                    writeFile(JsonConvert.SerializeObject(translateGeometry(staticGeo, new List<string>(), "staticGeo", Component), Newtonsoft.Json.Formatting.None), pathh);


                    OUTFILE = new System.Dynamic.ExpandoObject();
                    OUTFILE.meta = "ParametricSpaceExporter";
                    OUTFILE.parameters = new List<dynamic>();
                    OUTFILE.propNames = new List<string>();
                    OUTFILE.kvpairs = kvpairs;

                    foreach (IGH_Param param in Component.Params.Input[1].Sources)
                    {
                        //Print(getPanelNameAndVal(param));
                        var myprop = getPanelNameAndVal(param);
                        if (myprop != null)
                        {
                            string[] pops = myprop.Split(',');
                            OUTFILE.propNames.Add(pops[0]);
                        }
                    }

                    // populate the sliders
                    int k = 0;
                    foreach (List<double> mySliderVars in sliderValues)
                    {
                        dynamic Slider = new System.Dynamic.ExpandoObject();
                        Slider.name = sliderNames.ElementAt(k++);
                        Slider.values = mySliderVars;
                        OUTFILE.parameters.Add(Slider);
                    }

                    OUTFILE.properties = myProperties;

                    pathh = Path.Combine(FOLDERLOCATION, "params.json");

                    writeFile(JsonConvert.SerializeObject(OUTFILE, Newtonsoft.Json.Formatting.None), pathh);

                    // copy/write the gh defintion in the folder   

                    if (allowDefExport)
                    { 
                        string ghSavePath = Path.Combine(FOLDERLOCATION, "def.ghx");
                        GH_Archive myArchive = new GH_Archive();
                        myArchive.Path = ghSavePath;
                        myArchive.AppendObject(GrasshopperDocument, "Definition");
                        myArchive.WriteToFile(ghSavePath, true, false);
                    }

                    // zip things up
                    string startPath = FOLDERLOCATION;

                    using (ZipFile zip = new ZipFile())
                    {
                        zip.AddDirectory(@startPath);
                        zip.Save(startPath + @"\" + GHDEFNAME + ".zip");
                    }

                    // delete the garbage data

                    System.IO.DirectoryInfo myDir = new DirectoryInfo(FOLDERLOCATION);

                    foreach (FileInfo file in myDir.GetFiles())
                    {
                        if (!(file.Extension == ".zip"))
                            file.Delete();
                    }

                    // open an explorer window to the location of the archive. 
                    Process.Start("explorer.exe", FOLDERLOCATION);

                    DA.SetDataList(0, myHashSet);

                    PATHISSET = false;
                    FOLDERLOCATION = "";
                    myHashSet = new HashSet<string>();
                    Component.ExpireSolution(true);
                }

            }

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.SPK_Icon_07;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{22a288b4-b95d-4e77-86ae-090f92bd95f2}"); }
        }

        #region utils: write file, etc.

        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == '\\' || c == ':' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public SuperKVP getCurrentKVP(string name)
        {

            SuperKVP myKVP;

            string values = "";

            foreach (IGH_Param param in Component.Params.Input[1].Sources)
            {
                var myprop = getPanelVal(param);

                if (myprop != null)
                {

                    values += myprop + ",";

                }

            }

            myKVP = new SuperKVP(name, values);

            return myKVP;

        }

        public void writeFile(string what, string name)
        {
            string path = Path.GetFullPath(name);


            System.IO.StreamWriter file = new System.IO.StreamWriter(path);
            file.WriteLine(what);
            file.Close();
        }



        #endregion

        #region Actual SOLVER

        public void Export()
        {

            if (INSTANCECOUNT > 4242)
            {

                string message = "Warning: There are " + INSTANCECOUNT + " iterations.\nIt might take a lot of time and / or Rhino might crash.\nAre you sure you want to proceed?";

                DialogResult largeinstancewarning = MessageBox.Show(message, "Warning", MessageBoxButtons.YesNo);
                if (largeinstancewarning == DialogResult.No)
                {
                    EMERGENCY_BREAK = true;
                    return;
                }

            }

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select an empty folder where the export file should be saved.";
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK)
            {

                string[] files = Directory.GetFiles(fbd.SelectedPath);

                if (files.Length > 0)
                    System.Windows.Forms.MessageBox.Show("This is not an empty folder!");
                else
                {
                    this.PATHISSET = true;
                    this.FOLDERLOCATION = fbd.SelectedPath;
                    GHDEFNAME = new DirectoryInfo(fbd.SelectedPath).Name;
                    EMERGENCY_BREAK = false;
                }
            }

            if (result == DialogResult.Cancel)
            {
                EMERGENCY_BREAK = true;
                return;
            }

          

            // Sanity checks
            IGH_Component component = Component;
            GH_Document doc = GrasshopperDocument;

            if (!PATHISSET)
                return;
            if (component == null)
                return;
            if (doc == null)
                return;


            /// flushing up them arrays
            geometries = new List<List<System.Object>>();
            geometrySets = new List<string>();
            sliderNames = new List<string>();

            currentCount = 0;

            // Collect all sliders that are plugged into the first input parameter of the script component.
            List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();

            foreach (IGH_Param param in component.Params.Input[0].Sources)
            {
                Grasshopper.Kernel.Special.GH_NumberSlider slider = param as Grasshopper.Kernel.Special.GH_NumberSlider;
                if (slider != null)
                {
                    sliders.Add(slider);
                    sliderNames.Add(slider.NickName);
                }
            }

            if (sliders.Count == 0)
                return;

            // generate the Matrix File
            myMatrix = constructMatrixFromSliders((GH_Component)component);

            // update the instance count
            INSTANCECOUNT = myMatrix.Count;

            // go generate stuff
            foreach (List<double> instance in myMatrix)
            {
                // pause the solver while set up the sliders
                doc.Enabled = false;

                // update the currentInstanceName - top level var
                currentInstanceName = "";

                foreach (double tempvar in instance)
                {
                    currentInstanceName += tempvar + ",";
                }

                // set sliders up

                for (int i = 0; i < sliders.Count; i++)
                {
                    Grasshopper.Kernel.Special.GH_NumberSlider mySlider = sliders[i];
                    mySlider.Slider.Value = (decimal)instance[i];
                }

                //renable solver
                doc.Enabled = true;

                //compute new solution
                doc.NewSolution(false);
            }

        }



        #endregion

        #region combinatorics

        public static List<List<T>> AllCombinationsOf<T>(List<List<T>> lists)
        {
            // need array bounds checking etc for production
            var combinations = new List<List<T>>();

            // prime the data
            foreach (var value in lists[0])
                combinations.Add(new List<T> { value });

            foreach (var set in lists.Skip(1))
                combinations = AddExtraSet(combinations, set);

            return combinations;
        }


        private static List<List<T>> AddExtraSet<T>(List<List<T>> combinations, List<T> set)
        {
            var newCombinations = from value in set
                                  from combination in combinations
                                  select new List<T>(combination) { value };

            return newCombinations.ToList();
        }

        #endregion

        #region panel handlers (used for performance measures). 

        public static string getPanelName(IGH_Param param)
        {

            Grasshopper.Kernel.Special.GH_Panel myParam = param as Grasshopper.Kernel.Special.GH_Panel;

            if (myParam != null)
            {
                return myParam.NickName;
            }

            else return null;

        }

        public static string getPanelNameAndVal(IGH_Param param)
        {
            Grasshopper.Kernel.Special.GH_Panel myParam = param as Grasshopper.Kernel.Special.GH_Panel;
            if (myParam != null)
            {
                string panelVal = "";

                foreach (Object myObj in myParam.VolatileData.AllData(true))
                {
                    GH_String temporary = myObj as GH_String;
                    string myString = temporary.Value;
                    double mydbl = 0;
                    try
                    {
                        mydbl = Double.Parse(myString);
                    }
                    catch
                    {
                    }

                    panelVal += mydbl;
                }

                return myParam.NickName + "," + panelVal;

            }
            else return null;
        }

        public static string getPanelVal(IGH_Param param)
        {

            Grasshopper.Kernel.Special.GH_Panel myParam = param as Grasshopper.Kernel.Special.GH_Panel;

            string panelVal = "";

            foreach (Object myObj in myParam.VolatileData.AllData(true))
            {
                GH_String temporary = myObj as GH_String;
                string myString = temporary.Value;
                double mydbl = 0;
                try
                {
                    mydbl = Double.Parse(myString);
                }
                catch
                {
                }

                panelVal += mydbl;
            }

            return panelVal;

        }


        #endregion

        /// <summary>
        /// Creates a list of lists holding up all the possible slider combinations. No idea why it returns something. 
        /// </summary>

        public List<List<double>> constructMatrixFromSliders(GH_Component component)
        {
            List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();

            foreach (IGH_Param param in component.Params.Input[0].Sources)
            {
                Grasshopper.Kernel.Special.GH_NumberSlider slider = param as Grasshopper.Kernel.Special.GH_NumberSlider;
                if (slider != null)
                    sliders.Add(slider);
            }

            if (sliders.Count == 0)
                return null;

            sliderValues = new List<List<double>>();

            for (int k = 0; k < sliders.Count; k++)
            {

                List<double> mySliderValues = new List<double>();

                Grasshopper.Kernel.Special.GH_NumberSlider slider = sliders[k];

                if (slider.Slider.Type == Grasshopper.GUI.Base.GH_SliderAccuracy.Integer)
                {
                    // TODO restrict to MAXVALUES for int sliders
                    int min, max;

                    min = (int)slider.Slider.Minimum;
                    max = (int)slider.Slider.Maximum;

                    for (int j = min; j <= max; j++)
                        mySliderValues.Add((double)Math.Round((double)j, 0));
                }

                else if (slider.Slider.Type == Grasshopper.GUI.Base.GH_SliderAccuracy.Float)
                {
                    double min, max;

                    min = (double)slider.Slider.Minimum;
                    max = (double)slider.Slider.Maximum;

                    double absRange = max - min;
                    double increment = absRange / (MAXVALUES - 1);

                    for (double j = min; j <= max; j += increment)
                        mySliderValues.Add((double)Convert.ToDouble(Convert.ToString(Math.Round(j, 2)))); //really, really, really stupid

                }

                sliderValues.Add(mySliderValues);
            }

            return AllCombinationsOf(sliderValues);
        }

        /// <summary>
        /// This one does many things, which essentially boil down to translating the geometry + performance measures into a spk instance.
        /// This instance subsequently gets json-ed out to a file
        /// </summary>

        public System.Dynamic.ExpandoObject translateGeometry(List<System.Object> inputObjects, List<string> guids, String name, IGH_Component component)
        {
            dynamic myInstance = new System.Dynamic.ExpandoObject();
            myInstance.metadata = new System.Dynamic.ExpandoObject();
            myInstance.metadata.verion = "1.0";
            myInstance.metadata.type = "Object";
            myInstance.metadata.generator = "Instance Export";

            // super important - name will link it to the correct group in three js
            myInstance.metadata.name = name;
            myInstance.metadata.properties = new List<String>();

            foreach (IGH_Param param in component.Params.Input[1].Sources)
            {
                var myprop = getPanelNameAndVal(param);
                if (myprop != null)
                    myInstance.metadata.properties.Add(myprop);
            }

            myInstance.geometries = new List<System.Object>();

            int k = 0;

            foreach (System.Object myObj in inputObjects)
            {
                string myguid = "000";

                if (name != "staticGeo")
                    myguid = guids[k];

                k++;
                SPK_Object myObject = translateGeometryItem(myObj, myguid, name);

                myHashSet.Add(myObject.myHash);

                myInstance.geometries.Add( myObject );
            }

            return myInstance;
        }


        public SPK_Object translateGeometryItem(object myObj, string myguid, string name)
        {
            if (myObj != null)
            {
                
                if (myObj is GH_Mesh)
                {
                    GH_Mesh tempers = (GH_Mesh)myObj;
                    SuperMesh mesh = new SuperMesh(tempers, myguid);
                     return mesh;

                }
                else if ((myObj is GH_Curve))
                {
                    GH_Curve tempers = (GH_Curve)myObj;
                    Curve myCrv = tempers.Value;

                    if (myCrv.Degree == 1)
                    {
                        Polyline tempP; myCrv.TryGetPolyline(out tempP);
                        SuperPolyline polyline = new SuperPolyline(tempP, false, myguid);
                        return polyline;

                    }
                    else if ((myCrv.Degree == 2) || (myCrv.Degree == 3))
                    {
                        bool isClosed = myCrv.IsClosed;
                        int mainSegmentCount = 0, subSegmentCount = 1;
                        double maxAngleRadians = 0, maxChordLengthRatio = 0, maxAspectRatio = 0, tolerance = 0.1, minEdgeLength = 0, maxEdgeLength = 0;
                        bool keepStartPoint = true;

                        PolylineCurve p = myCrv.ToPolyline(mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint);
                        Polyline pp; p.TryGetPolyline(out pp);
                        return new SuperPolyline(pp, isClosed, myguid);

                    }
                }
                else if (myObj is Point3d)
                {
                    GH_Point tempers = (GH_Point)myObj;
                    SuperPoint point = new SuperPoint(tempers, myguid);
                    return point;

                }
                else if ((myObj is GH_Brep) || (myObj is GH_Surface))
                {
                    Mesh[] myMeshes;

                    Brep myFutureBrep = null;

                    GH_Convert.ToBrep(myObj, ref myFutureBrep, GH_Conversion.Primary);

                    if (myFutureBrep != null)
                    {
                        myMeshes = Mesh.CreateFromBrep(myFutureBrep, MeshingParameters.Smooth);

                        if (myMeshes == null || myMeshes.Length == 0)
                        {
                            // TODO throw an error
                        }

                        Mesh brep_mesh = new Mesh();
                        foreach (Mesh tempmesh in myMeshes)
                            brep_mesh.Append(tempmesh);

                        GH_Mesh temporal = new GH_Mesh(brep_mesh);

                        SuperMesh mesh = new SuperMesh(temporal, myguid);
                        return mesh;
                    }
                }
                else if (myObj is GH_Circle)
                {
                    GH_Circle myCircle = myObj as GH_Circle;
                    //NurbsCurve mycrv = myCircle.Value.ToNurbsCurve();
                    NurbsCurve mycrv = NurbsCurve.CreateFromCircle(myCircle.Value);


                    int mainSegmentCount = 30, subSegmentCount = 1;
                    double maxAngleRadians = 0, maxChordLengthRatio = 0, maxAspectRatio = 0, tolerance = 0.1, minEdgeLength = 0, maxEdgeLength = 0;
                    bool keepStartPoint = true;

                    if (mycrv != null)
                    {
                        PolylineCurve p = mycrv.ToPolyline(mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint);

                        Polyline pp; p.TryGetPolyline(out pp);
                        if (pp != null)
                            return new SuperPolyline(pp, true, myguid);    
                    }
                    
                }
                else if (myObj is GH_Line)
                {
                    GH_Line myLine = myObj as GH_Line;

                    return new SuperPolyline(myLine, myguid);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        #region doubleclickhandler

        public override void CreateAttributes()
        {
            m_attributes = new BetaSpeckleComponentAttributes(this);
        }

      

        #endregion

    } // class end
}
