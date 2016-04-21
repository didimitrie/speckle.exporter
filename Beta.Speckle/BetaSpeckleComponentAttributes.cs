using Grasshopper.GUI.Canvas;
using System;
using System.Drawing;

namespace BetaSpeckle
{

    public class BetaSpeckleComponentAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {

        private RectangleF bounds;


        public BetaSpeckleComponent CurrentOwner
        {
            get;
            private set;
        }

        public override RectangleF Bounds
        {
            get
            {
                return this.bounds;
            }
            set
            {
                this.bounds = value;
            }
        }

        public BetaSpeckleComponentAttributes(BetaSpeckleComponent owner) : base(owner)
        {
            this.CurrentOwner = owner;
        }

        protected override void Layout()
        {
            base.Layout();
            this.bounds.Inflate(new SizeF(7f, 30f));
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
        }

        public override Grasshopper.GUI.Canvas.GH_ObjectResponse RespondToMouseDoubleClick(Grasshopper.GUI.Canvas.GH_Canvas sender, Grasshopper.GUI.GH_CanvasMouseEvent e)
        {
            if ((ContentBox.Contains(e.CanvasLocation)))
            {
                CurrentOwner.SOLVE = true;

                // hit the export straight away, where we handle the folder selection! 
                CurrentOwner.Export();
                CurrentOwner.ExpireSolution(true);

                return Grasshopper.GUI.Canvas.GH_ObjectResponse.Handled;
            }

            return Grasshopper.GUI.Canvas.GH_ObjectResponse.Ignore;
        }
    }



}
