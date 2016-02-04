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
using Grasshopper;
using System.Windows.Forms;
using System.Drawing;

namespace BetaSpeckle
{
    public class BetaSpeckleComponent : GH_Component
    {

        /// 
        /// HIC SVNT DRACONES (aka bad code)
        /// 

        // IMPORTANT VARIABLES

        // Part 0: GH doc + folders
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        string folderLocation = @"c:\temp";

        // Part 1: lists and stuff

        // holds all the possible slider combinations: list[ [p1v1, p2v1, p3v1], [p1v2, p2v1, p3v1], ... ]
        List<List<double>> sliderValues = new List<List<double>>();

        // this lot - no idea; it's late.
        List<string> sliderNames;
        List<List<double>> myMatrix;
        List<List<System.Object>> geometries;
        List<string> geometrySets;
        List<KeyValuePair> kvpairs = new List<KeyValuePair>();
        List<SuperProperty> myProperties = new List<SuperProperty>();

        // Part 2: other params

        // How many values we split up a double slider range( [min, max], MAXVALUES )
        public int MAXVALUES = 6;
        public string STATUS = "waiting";
        public int instanceCount = -1;
        public int currentCount = 0;
        public string currentInstanceName = "";

        // Part 3: random
        public Form _form;
        private bool solve = false;





        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BetaSpeckleComponent()
          : base("Beta.Speckle", "BSpk",
              "Export a Beta.Speckle Archive",
              "Params", "Util")
        {
        
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("SLD", "SLD", "Parametric measures: The sliders that you want to define your parametric space with.", GH_ParamAccess.list);
            pManager.AddGenericParameter("PRF", "PRF", "Performance measures: values that you evaluate your design by (area, cost, element number, etc.). ", GH_ParamAccess.list);
            pManager.AddGenericParameter("OBJ", "OBJ", "Dynamic geometry: can be breps, (coloured) meshes, curves and points.", GH_ParamAccess.list);
            pManager.AddGenericParameter("STA", "STA", "Static geometry: objects that do not change (ie, context)", GH_ParamAccess.list);
     
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("INFO", "INFO", "You should really check here for information", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // setup basics
            Component = this;
            GrasshopperDocument = Instances.ActiveCanvas.Document;

            // create a folder that we're going to dump data to
            checkAndMakeFolder();

            geometries = new List<List<System.Object>>();
            Kvpairs = new List<KeyValuePair>();

            myProperties = new List<SuperProperty>();

            foreach (IGH_Param param in Component.Params.Input[3].Sources)
            {

                SuperProperty myProperty = new SuperProperty(getPanelName(param));
                myProperties.Add(myProperty);

            }

            // form init
            _form = new Form();
            _form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            _form.Size = new Size(300, 50);
            _form.StartPosition = FormStartPosition.Manual;
            _form.Text = "SPKGENERATOR 0.1.0";
            _form.FormClosed += FormClosed;
            Grasshopper.GUI.GH_WindowsFormUtil.CenterFormOnEditor(_form, true);

            Button button = new Button();
            button.Text = "Create Export! WARNING: Rhino and GH will freeze until all is done.";
            button.Tag = Component;
            button.Dock = DockStyle.Bottom;
            button.Click += ButtonClick;

            _form.Controls.Add(button);
            _form.Show(Grasshopper.Instances.DocumentEditor);

            Grasshopper.Instances.DocumentEditor.FormShepard.RegisterForm(_form);

            DA.SetData(0, folderLocation);            
        }

 
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{22a288b4-b95d-4e77-86ae-090f92bd95f2}"); }
        }

        internal List<KeyValuePair> Kvpairs
        {
            get
            {
                return kvpairs;
            }

            set
            {
                kvpairs = value;
            }
        }

        public void checkAndMakeFolder()
        {

            string folderpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folderpath += @"\Speckle Models\" + GrasshopperDocument.DisplayName;

            if ((!System.IO.Directory.Exists(folderpath)))
            {
                System.IO.Directory.CreateDirectory(folderpath);
            }
            else
            {
                folderpath += "-" + (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }

            folderLocation = folderpath;

        }

        #region Form events & Co.

        private void ButtonClick(object sender, EventArgs e)
        {
            ///TODO
            /// 
            geometries = new List<List<System.Object>>();
            geometrySets = new List<string>();
            sliderNames = new List<string>();

            STATUS = "generating";
            currentCount = 0;
            Button button = sender as Button;

            if (button == null)
                return;

            IGH_Component component = button.Tag as IGH_Component;

            if (component == null)
                return;

            GH_Document doc = component.OnPingDocument();

            if (doc == null)
            {
                button.FindForm().Close();
                return;
            }

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
            instanceCount = myMatrix.Count;

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

            //button.Text = geometries.Count + " <<?";
            STATUS = "done";
        }

        private void FormClosed(object sender, EventArgs e)
        {
            _form = null;
        }

        #endregion

        /// <summary>
        /// This here handles generating all the possible combinations of the input sliders.
        /// Parametric Model > Big Data 
        /// </summary>

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
                return myParam.NickName + "," + myParam.VolatileData.get_Branch(0)[0].ToString();
            }
            else return null;
        }


        public static string getPanelVal(IGH_Param param)
        {

            Grasshopper.Kernel.Special.GH_Panel myParam = param as Grasshopper.Kernel.Special.GH_Panel;

            if (myParam != null)
                return myParam.VolatileData.get_Branch(0)[0].ToString();

            return null;

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
        
        public static System.Dynamic.ExpandoObject translateGeometry(List<System.Object> inputObjects, String name, IGH_Component component)
        {
            dynamic myInstance = new System.Dynamic.ExpandoObject();
            myInstance.metadata = new System.Dynamic.ExpandoObject();
            myInstance.metadata.verion = "1.0";
            myInstance.metadata.type = "Object";
            myInstance.metadata.generator = "Instance Export";

            // super important - name will link it to the correct group in three js
            myInstance.metadata.name = name;
            myInstance.metadata.properties = new List<String>();

            foreach (IGH_Param param in component.Params.Input[3].Sources)
            {
                var myprop = getPanelNameAndVal(param);
                if (myprop != null)
                    myInstance.metadata.properties.Add(myprop);
            }

            myInstance.geometries = new List<System.Object>();

            foreach (System.Object myObj in inputObjects)
            {
                if (myObj is GH_Mesh)
                {
                    GH_Mesh tempers = (GH_Mesh) myObj;
                    SuperMesh mesh = new SuperMesh(tempers);
                    myInstance.geometries.Add(mesh);

                }
                else if ((myObj is Curve) || (myObj is GH_Line))
                {
                    Curve tempers = (Curve)myObj;
                    Curve myCrv = tempers;

                    if (myCrv.Degree == 1)
                    {
                        Polyline tempP; myCrv.TryGetPolyline(out tempP);
                        SuperPolyline polyline = new SuperPolyline(tempP, false);
                        myInstance.geometries.Add(polyline);

                    }
                    else if ((myCrv.Degree == 2) || (myCrv.Degree == 3))
                    {
                        bool isClosed = myCrv.IsClosed;
                        int mainSegmentCount = 0, subSegmentCount = 1;
                        double maxAngleRadians = 0, maxChordLengthRatio = 0, maxAspectRatio = 0, tolerance = 0.1, minEdgeLength = 0, maxEdgeLength = 0;
                        bool keepStartPoint = true;

                        PolylineCurve p = myCrv.ToPolyline(mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint);
                        Polyline pp; p.TryGetPolyline(out pp);
                        myInstance.geometries.Add(new SuperPolyline(pp, isClosed));

                    }
                }
                else if (myObj is Point3d)
                {
                    //Rhino.Geometry.Point3d tempers = (Rhino.Geometry.Point3d)myObj;
                    GH_Point tempers = (GH_Point)myObj;
                    SuperPoint point = new SuperPoint(tempers);
                    myInstance.geometries.Add(point);

                }
                else if ((myObj is Brep) || (myObj is Surface))
                {
                    Brep myFutureBrep = null;
                    GH_Convert.ToBrep(myObj, ref myFutureBrep, GH_Conversion.Primary);

                    Mesh[] myMeshes = Mesh.CreateFromBrep(myFutureBrep, MeshingParameters.Smooth);

                    if (myMeshes == null || myMeshes.Length == 0)
                    {
                        // TODO throw an error
                    }

                    Mesh brep_mesh = new Mesh();
                    foreach (Mesh tempmesh in myMeshes)
                        brep_mesh.Append(tempmesh);

                    GH_Mesh temporal = new GH_Mesh(brep_mesh);

                    SuperMesh mesh = new SuperMesh(temporal);
                    myInstance.geometries.Add(mesh);

                }
                else
                {
                    myInstance.geometries.Add("error - undefined type");
                }
            }
            return myInstance;
        }

        #region doubleclickhandler

        public override void CreateAttributes()
        {
            m_attributes = new BetaSpeckleComponentAttributes(this);
        }

        public class BetaSpeckleComponentAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
        {

            public BetaSpeckleComponentAttributes(BetaSpeckleComponent owner) : base(owner)
            {
            }

            public override Grasshopper.GUI.Canvas.GH_ObjectResponse RespondToMouseDoubleClick(Grasshopper.GUI.Canvas.GH_Canvas sender, Grasshopper.GUI.GH_CanvasMouseEvent e)
            {
                if ((ContentBox.Contains(e.CanvasLocation)))
                {
                    BetaSpeckleComponent sc = (BetaSpeckleComponent)Owner;
                    sc.solve = true;
                    sc.ExpireSolution(true);
                    return Grasshopper.GUI.Canvas.GH_ObjectResponse.Handled;
                }

                return Grasshopper.GUI.Canvas.GH_ObjectResponse.Ignore;
            }
        }

        #endregion

    } // class end
}
