﻿// 2015,2014 ,BSD, WinterDev 
using System.Collections.Generic;
using System.Globalization;
using PixelFarm.Drawing;
using LayoutFarm.WebDom;
using LayoutFarm.Css;
using LayoutFarm.Composers;
namespace LayoutFarm.HtmlBoxes
{
    class CssIsolateBox : CssBox
    {
        public CssIsolateBox(BoxSpec spec, RootGraphic rootgfx)
            : base(spec, rootgfx)
        {

        } 
    } 

    class RenderElementBridgeCssBox : CssBox
    {
        LayoutFarm.RenderElement containerElement;
        public RenderElementBridgeCssBox(BoxSpec spec,
            LayoutFarm.RenderElement containerElement,
            RootGraphic rootgfx)
            : base(spec, rootgfx)
        {
            this.containerElement = containerElement;
        } 
        protected override void InvalidateBubbleUp(Rectangle clientArea)
        {
            //send to container element
            this.containerElement.InvalidateGraphicBounds(clientArea);
        }
        public LayoutFarm.RenderElement ContainerElement
        {
            get { return this.containerElement; }
        }
        protected override CssBox GetGlobalLocationImpl(out float globalX, out float globalY)
        {
            Point p = containerElement.GetGlobalLocation();
            globalX = p.X;
            globalY = p.Y;
            return this;
        }
    }



}
