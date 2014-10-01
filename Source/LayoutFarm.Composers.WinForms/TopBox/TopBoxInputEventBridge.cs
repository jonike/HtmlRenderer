﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Drawing;

namespace LayoutFarm
{

    class TopBoxInputEventBridge
    {

        CanvasEventsStock eventStock = new CanvasEventsStock();
        RenderElement currentMouseActiveElement = null;
        RenderElement currentDragingElement = null;

        int globalXOfCurrentUI = 0;
        int globalYOfCurrentUI = 0;

        int currentXDistanceFromDragPoint = 0;
        int currentYDistanceFromDragPoint = 0;

        readonly HitPointChain hitPointChain = new HitPointChain();

        UIHoverMonitorTask hoverMonitoringTask;
        public event EventHandler CurrentFocusElementChanged;

        int msgChainVersion;

        MyTopWindowRenderBox topwin;
        RenderElement currentKeyboardFocusedElement;
        RootGraphic rootGraphic;


        public TopBoxInputEventBridge()
        {


        }
        public void Bind(MyTopWindowRenderBox topwin)
        {
            this.topwin = topwin;
            this.rootGraphic = topwin.Root;
            this.hoverMonitoringTask = new UIHoverMonitorTask(this.topwin, OnMouseHover);
#if DEBUG
            hitPointChain.dbugHitTracker = this.rootGraphic.dbugHitTracker;
#endif
        }
         

        public RenderElement CurrentKeyboardFocusedElement
        {
            get
            {

                return this.currentKeyboardFocusedElement;
            }
            set
            {

                if (value != null && !(value.Focusable))
                {
                    return;
                }
                if (currentKeyboardFocusedElement != null)
                {
                    if (currentKeyboardFocusedElement == value)
                    {
                        return;
                    }

                    UIFocusEventArgs focusEventArg = eventStock.GetFreeFocusEventArgs(value, currentKeyboardFocusedElement);
                    focusEventArg.SetWinRoot(topwin);
                    eventStock.ReleaseEventArgs(focusEventArg);
                }
                currentKeyboardFocusedElement = value;
                if (currentKeyboardFocusedElement != null)
                {
                    UIFocusEventArgs focusEventArg = eventStock.GetFreeFocusEventArgs(value, currentKeyboardFocusedElement);
                    focusEventArg.SetWinRoot(topwin);
                    Point globalLocation = value.GetGlobalLocation();
                    globalXOfCurrentUI = globalLocation.X;
                    globalYOfCurrentUI = globalLocation.Y;
                    focusEventArg.SetWinRoot(topwin);

                    IEventListener ui = value.GetController() as IEventListener;
                    if (ui != null)
                    {

                    }
                    eventStock.ReleaseEventArgs(focusEventArg);
                    if (CurrentFocusElementChanged != null)
                    {
                        CurrentFocusElementChanged.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    globalXOfCurrentUI = 0;
                    globalYOfCurrentUI = 0;
                }
            }
        }

        internal RenderElement CurrentMouseFocusedElement
        {
            get
            {
                return currentMouseActiveElement;
            }
        }
        public void ClearAllFocus()
        {
            CurrentKeyboardFocusedElement = null;
            this.currentDragingElement = null;
        }


        public void OnDoubleClick(UIMouseEventArgs e)
        {

            RenderElement hitElement = HitTestCoreWithPrevChainHint(e.X, e.Y, UIEventName.DblClick);
            if (currentMouseActiveElement != null)
            {
                e.TranslateCanvasOrigin(globalXOfCurrentUI, globalYOfCurrentUI);
                e.Location = hitPointChain.CurrentHitPoint;
                e.SourceRenderElement = currentMouseActiveElement;

                IEventListener ui = currentMouseActiveElement.GetController() as IEventListener;
                if (ui != null)
                {
                }
                e.TranslateCanvasOriginBack();

            }
            hitPointChain.SwapHitChain();
        }
        public void OnMouseWheel(UIMouseEventArgs e)
        {

            if (currentMouseActiveElement != null)
            {
                IEventListener ui = currentMouseActiveElement.GetController() as IEventListener;
                if (ui != null)
                {
                    ui.ListenMouseEvent(UIMouseEventName.MouseWheel, e);
                }
            }
        }
        public void OnMouseDown(UIMouseEventArgs e)
        {

#if DEBUG
            if (this.rootGraphic.dbugEnableGraphicInvalidateTrace)
            {
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("MOUSEDOWN");
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
            }
#endif  
            msgChainVersion = 1;
            int local_msgVersion = 1;
            RenderElement hitElement = HitTestCoreWithPrevChainHint(e.X, e.Y, UIEventName.MouseDown);
            if (hitElement == this.topwin || hitElement == null)
            {
                hitPointChain.SwapHitChain();
                return;
            }
            DisableGraphicOutputFlush = true;

            e.TranslateCanvasOrigin(globalXOfCurrentUI, globalYOfCurrentUI);
            e.Location = hitPointChain.CurrentHitPoint;
            e.SourceRenderElement = hitElement;


            currentMouseActiveElement = hitElement;


            IEventListener ui = hitElement.GetController() as IEventListener;
            if (ui != null)
            {
                ui.ListenMouseEvent(UIMouseEventName.MouseDown, e);
            }
            e.TranslateCanvasOriginBack();
#if DEBUG
            RootGraphic visualroot = this.rootGraphic;
            if (visualroot.dbug_RecordHitChain)
            {
                visualroot.dbug_rootHitChainMsg.Clear();
                int i = 0;
                foreach (HitPointChain.HitPair hp in hitPointChain.HitPairIter)
                {

                    RenderElement ve = hp.elem;
                    ve.dbug_WriteOwnerLayerInfo(visualroot, i);
                    ve.dbug_WriteOwnerLineInfo(visualroot, i);

                    string hit_info = new string('.', i) + " [" + i + "] "
                        + "(" + hp.point.X + "," + hp.point.Y + ") "
                        + ve.dbug_FullElementDescription();
                    visualroot.dbug_rootHitChainMsg.AddLast(new dbugLayoutMsg(ve, hit_info));
                    i++;
                }
            }
#endif
            hitPointChain.SwapHitChain();
            if (hitElement.ParentVisualElement == null)
            {
                currentMouseActiveElement = null;
                return;
            }

            if (local_msgVersion != msgChainVersion)
            {
                return;
            }


            if (hitElement.Focusable)
            {
                this.CurrentKeyboardFocusedElement = hitElement;
                //e.WinTop.CurrentKeyboardFocusedElement = hitElement;
            }
            DisableGraphicOutputFlush = false;
            FlushAccumGraphicUpdate();

#if DEBUG
            visualroot.dbugHitTracker.Write("stop-mousedown");
            visualroot.dbugHitTracker.Play = false;
#endif

        }
        RenderElement HitTestCoreWithPrevChainHint(int x, int y, UIEventName hitEvent)
        {
            hitPointChain.SetVisualRootStartTestPoint(x, y);
            RenderElement commonElement = hitPointChain.HitTestOnPrevChain();
            if (commonElement == null)
            {
                commonElement = this.topwin;
            }
            commonElement.HitTestCore(hitPointChain);
            return hitPointChain.CurrentHitElement;
        }
        bool DisableGraphicOutputFlush
        {
            get { return this.rootGraphic.DisableGraphicOutputFlush; }
            set { this.rootGraphic.DisableGraphicOutputFlush = value; }
        }
        void FlushAccumGraphicUpdate()
        {
            this.rootGraphic.FlushAccumGraphicUpdate(this.topwin);
        }
        public void OnMouseMove(UIMouseEventArgs e)
        {

            RenderElement hitElement = HitTestCoreWithPrevChainHint(e.X, e.Y, UIEventName.MouseMove);

            hoverMonitoringTask.Reset();
            hoverMonitoringTask.SetEnable(true, this.topwin);

            if (hitElement != currentMouseActiveElement)
            {
                DisableGraphicOutputFlush = true;
                {
                    if (RenderElement.IsTestableElement(currentMouseActiveElement))
                    {
                        Point prevElementGlobalLocation = currentMouseActiveElement.GetGlobalLocation();
                        e.TranslateCanvasOrigin(prevElementGlobalLocation);
                        e.Location = hitPointChain.PrevHitPoint;
                        e.SourceRenderElement = currentMouseActiveElement;

                        IEventListener ui = currentMouseActiveElement.GetController() as IEventListener;
                        if (ui != null)
                        {
                            ui.ListenMouseEvent(UIMouseEventName.MouseLeave, e);
                        }

                        e.TranslateCanvasOriginBack();
                        currentMouseActiveElement = null;
                    }


                    if (RenderElement.IsTestableElement(hitElement))
                    {

                        currentMouseActiveElement = hitElement;


                        e.TranslateCanvasOrigin(hitPointChain.LastestElementGlobalX, hitPointChain.LastestElementGlobalY);
                        e.Location = hitPointChain.CurrentHitPoint;
                        e.SourceRenderElement = hitElement;

                        IEventListener ui = hitElement.GetController() as IEventListener;
                        if (ui != null)
                        {
                            ui.ListenMouseEvent(UIMouseEventName.MouseEnter, e);
                        }

                        e.TranslateCanvasOriginBack();

                    }
                }
                DisableGraphicOutputFlush = false;
                FlushAccumGraphicUpdate();
            }
            else if (hitElement != null)
            {
                DisableGraphicOutputFlush = true;
                {
                    e.TranslateCanvasOrigin(hitPointChain.LastestElementGlobalX, hitPointChain.LastestElementGlobalY);
                    e.Location = hitPointChain.CurrentHitPoint;
                    e.SourceRenderElement = hitElement;

                    IEventListener ui = hitElement.GetController() as IEventListener;
                    if (ui != null)
                    {
                        ui.ListenMouseEvent(UIMouseEventName.MouseMove, e);
                    }

                    e.TranslateCanvasOriginBack();
                }
                DisableGraphicOutputFlush = false;
                FlushAccumGraphicUpdate();
            }

            hitPointChain.SwapHitChain();
        }
        void OnMouseHover(object sender, EventArgs e)
        {
            RenderElement hitElement = HitTestCoreWithPrevChainHint(hitPointChain.LastestRootX, hitPointChain.LastestRootY, UIEventName.MouseHover);
            if (hitElement != null && RenderElement.IsTestableElement(hitElement))
            {
                DisableGraphicOutputFlush = true;
                Point hitElementGlobalLocation = hitElement.GetGlobalLocation();

                UIMouseEventArgs e2 = new UIMouseEventArgs();
                e2.WinTop = this.topwin;
                e2.Location = hitPointChain.CurrentHitPoint;
                e2.SourceRenderElement = hitElement;
                IEventListener ui = hitElement.GetController() as IEventListener;
                if (ui != null)
                {
                    ui.ListenMouseEvent(UIMouseEventName.MouseHover, e2);
                }

                DisableGraphicOutputFlush = false;
                FlushAccumGraphicUpdate();
            }
            hitPointChain.SwapHitChain();
            hoverMonitoringTask.SetEnable(false, this.topwin);
        }
        public void OnDragStart(UIDragEventArgs e)
        {

#if DEBUG
            if (this.rootGraphic.dbugEnableGraphicInvalidateTrace)
            {
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("START_DRAG");
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
            }
#endif


            currentXDistanceFromDragPoint = 0;
            currentYDistanceFromDragPoint = 0;
            currentDragingElement = HitTestCoreWithPrevChainHint(
                hitPointChain.LastestRootX,
                hitPointChain.LastestRootY,
                UIEventName.DragStart);

            if (currentDragingElement != null && currentDragingElement != this.topwin)
            {
                DisableGraphicOutputFlush = true;
                Point globalLocation = currentDragingElement.GetGlobalLocation();
                e.TranslateCanvasOrigin(globalLocation);
                e.Location = hitPointChain.CurrentHitPoint;
                e.DragingElement = currentDragingElement;
                e.SourceRenderElement = currentDragingElement;



                IEventListener ui = currentDragingElement.GetController() as IEventListener;
                if (ui != null)
                {
                    ui.ListenDragEvent(UIDragEventName.DragStart, e);
                }
                e.TranslateCanvasOriginBack();
                DisableGraphicOutputFlush = false;
                FlushAccumGraphicUpdate();
                hitPointChain.ClearDragHitElements();
            }
            hitPointChain.SwapHitChain();
        }
        public void OnDrag(UIDragEventArgs e)
        {

#if DEBUG
            this.rootGraphic.dbugEventIsDragging = true;
#endif

            if (currentDragingElement == null)
            {

                return;
            }
            else
            {
            }

            //--------------
            currentXDistanceFromDragPoint += e.XDiff;
            currentYDistanceFromDragPoint += e.YDiff;


            DisableGraphicOutputFlush = true;

            Point globalDragingElementLocation = currentDragingElement.GetGlobalLocation();
            e.TranslateCanvasOrigin(globalDragingElementLocation);
            e.SourceRenderElement = currentDragingElement;
            Point dragPoint = hitPointChain.PrevHitPoint;
            dragPoint.Offset(currentXDistanceFromDragPoint, currentYDistanceFromDragPoint);
            e.Location = dragPoint;
            e.DragingElement = currentDragingElement;

            IEventListener ui = currentDragingElement.GetController() as IEventListener;
            if (ui != null)
            {
                ui.ListenDragEvent(UIDragEventName.Dragging, e);
            }
            e.TranslateCanvasOriginBack();

            if (currentDragingElement.HasDragBroadcastable)
            {
                BroadcastDragHitEvents(e);
            }

            FlushAccumGraphicUpdate();
        }


        void BroadcastDragHitEvents(UIDragEventArgs e)
        {


            //Point globalDragingElementLocation = currentDragingElement.GetGlobalLocation();
            //Rectangle dragRect = currentDragingElement.GetGlobalRect();

            //VisualDrawingChain drawingChain = this.WinRootPrepareRenderingChain(dragRect);

            //List<RenderElement> selVisualElements = drawingChain.selectedVisualElements;
            //int j = selVisualElements.Count;
            //LinkedList<RenderElement> underlyingElements = new LinkedList<RenderElement>();
            //for (int i = j - 1; i > -1; --i)
            //{

            //    if (selVisualElements[i].ListeningDragEvent)
            //    {
            //        underlyingElements.AddLast(selVisualElements[i]);
            //    }
            //}

            //if (underlyingElements.Count > 0)
            //{
            //    foreach (RenderElement underlyingUI in underlyingElements)
            //    {

            //        if (underlyingUI.IsDragedOver)
            //        {   
            //            hitPointChain.RemoveDragHitElement(underlyingUI);
            //            underlyingUI.IsDragedOver = false;
            //        }
            //    }
            //}
            //UIDragEventArgs d_eventArg = UIDragEventArgs.GetFreeDragEventArgs();

            //if (hitPointChain.DragHitElementCount > 0)
            //{
            //    foreach (RenderElement elem in hitPointChain.GetDragHitElementIter())
            //    {
            //        Point globalLocation = elem.GetGlobalLocation();
            //        d_eventArg.TranslateCanvasOrigin(globalLocation);
            //        d_eventArg.SourceVisualElement = elem;
            //        var script = elem.GetController();
            //        if (script != null)
            //        {
            //        }
            //        d_eventArg.TranslateCanvasOriginBack();
            //    }
            //}
            //hitPointChain.ClearDragHitElements();

            //foreach (RenderElement underlyingUI in underlyingElements)
            //{

            //    hitPointChain.AddDragHitElement(underlyingUI);
            //    if (underlyingUI.IsDragedOver)
            //    {
            //        Point globalLocation = underlyingUI.GetGlobalLocation();
            //        d_eventArg.TranslateCanvasOrigin(globalLocation);
            //        d_eventArg.SourceVisualElement = underlyingUI;

            //        var script = underlyingUI.GetController();
            //        if (script != null)
            //        {
            //        }

            //        d_eventArg.TranslateCanvasOriginBack();
            //    }
            //    else
            //    {
            //        underlyingUI.IsDragedOver = true;
            //        Point globalLocation = underlyingUI.GetGlobalLocation();
            //        d_eventArg.TranslateCanvasOrigin(globalLocation);
            //        d_eventArg.SourceVisualElement = underlyingUI;

            //        var script = underlyingUI.GetController();
            //        if (script != null)
            //        {
            //        }

            //        d_eventArg.TranslateCanvasOriginBack();
            //    }
            //}
            //UIDragEventArgs.ReleaseEventArgs(d_eventArg);
        }
        public void OnDragStop(UIDragEventArgs e)
        {


#if DEBUG
            this.rootGraphic.dbugEventIsDragging = false;
#endif
            if (currentDragingElement == null)
            {
                return;
            }

            DisableGraphicOutputFlush = true;

            Point globalDragingElementLocation = currentDragingElement.GetGlobalLocation();
            e.TranslateCanvasOrigin(globalDragingElementLocation);

            Point dragPoint = hitPointChain.PrevHitPoint;
            dragPoint.Offset(currentXDistanceFromDragPoint, currentYDistanceFromDragPoint);
            e.Location = dragPoint;

            e.SourceRenderElement = currentDragingElement;
            var script = currentDragingElement.GetController() as IEventListener;
            if (script != null)
            {
                script.ListenDragEvent(UIDragEventName.DragStop, e);
            }

            e.TranslateCanvasOriginBack();

            UIDragEventArgs d_eventArg = new UIDragEventArgs();
            if (hitPointChain.DragHitElementCount > 0)
            {
                foreach (RenderElement elem in hitPointChain.GetDragHitElementIter())
                {
                    Point globalLocation = elem.GetGlobalLocation();
                    d_eventArg.TranslateCanvasOrigin(globalLocation);
                    d_eventArg.SourceRenderElement = elem;
                    d_eventArg.DragingElement = currentDragingElement;

                    var script2 = elem.GetController();
                    if (script2 != null)
                    {
                    }

                    d_eventArg.TranslateCanvasOriginBack();
                }
            }

            hitPointChain.ClearDragHitElements();


            currentDragingElement = null;
            DisableGraphicOutputFlush = false;
            FlushAccumGraphicUpdate();

        }
        public void OnGotFocus(UIFocusEventArgs e)
        {

            if (currentMouseActiveElement != null)
            {

            }

        }
        public void OnLostFocus(UIFocusEventArgs e)
        {

        }
        public void OnMouseUp(UIMouseEventArgs e)
        {

#if DEBUG

            if (this.rootGraphic.dbugEnableGraphicInvalidateTrace)
            {
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("MOUSEUP");
                this.rootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
            }

#endif

            RenderElement hitElement = HitTestCoreWithPrevChainHint(e.X, e.Y, UIEventName.MouseUp);
            if (hitElement != null)
            {
                DisableGraphicOutputFlush = true;

                Point globalLocation = hitElement.GetGlobalLocation();
                e.TranslateCanvasOrigin(globalLocation);
                e.Location = hitPointChain.CurrentHitPoint;

                e.SourceRenderElement = hitElement;
                IEventListener ui = hitElement.GetController() as IEventListener;
                if (ui != null)
                {
                    ui.ListenMouseEvent(UIMouseEventName.MouseUp, e);
                }
                e.TranslateCanvasOriginBack();

                DisableGraphicOutputFlush = false;

                if (hitElement.Focusable)
                {
                    this.CurrentKeyboardFocusedElement = hitElement; 
                    //e.WinTop.CurrentKeyboardFocusedElement = hitElement;
                }
                FlushAccumGraphicUpdate();
            }

            hitPointChain.SwapHitChain();
        }
        public void OnKeyDown(UIKeyEventArgs e)
        {
            var visualroot = this.rootGraphic;
            e.IsShiftKeyDown = e.Shift;
            e.IsAltKeyDown = e.Alt;
            e.IsCtrlKeyDown = e.Control;

            if (currentKeyboardFocusedElement != null)
            {

                e.TranslateCanvasOrigin(globalXOfCurrentUI, globalYOfCurrentUI);
                e.SourceRenderElement = currentKeyboardFocusedElement;
                IEventListener ui = currentKeyboardFocusedElement.GetController() as IEventListener;
                if (ui != null)
                {
                    ui.ListenKeyEvent(UIKeyEventName.KeyDown, e);
                }
                e.TranslateCanvasOriginBack();
            }
        }
        public void OnKeyUp(UIKeyEventArgs e)
        {
            var visualroot = this.rootGraphic;
            e.IsShiftKeyDown = e.Shift;
            e.IsAltKeyDown = e.Alt;
            e.IsCtrlKeyDown = e.Control;

            if (currentKeyboardFocusedElement != null)
            {
                e.TranslateCanvasOrigin(globalXOfCurrentUI, globalYOfCurrentUI);
                e.SourceRenderElement = currentKeyboardFocusedElement;

                IEventListener ui = currentKeyboardFocusedElement.GetController() as IEventListener;
                if (ui != null)
                {
                    ui.ListenKeyEvent(UIKeyEventName.KeyUp, e);
                }

                e.TranslateCanvasOriginBack();
            }


        }
        public void OnKeyPress(UIKeyPressEventArgs e)
        {

            if (currentKeyboardFocusedElement != null)
            {

                e.TranslateCanvasOrigin(globalXOfCurrentUI, globalYOfCurrentUI);
                e.SourceRenderElement = currentKeyboardFocusedElement;
                IEventListener ui = currentKeyboardFocusedElement.GetController() as IEventListener;
                if (ui != null)
                {
                    ui.ListenKeyPressEvent(e);
                }
                e.TranslateCanvasOriginBack();
            }
        }

        public bool OnProcessDialogKey(UIKeyEventArgs e)
        {

            bool result = false;
            if (currentKeyboardFocusedElement != null)
            {
                e.TranslateCanvasOrigin(globalXOfCurrentUI, globalYOfCurrentUI);

                e.SourceRenderElement = currentKeyboardFocusedElement;


                IEventListener ui = currentKeyboardFocusedElement.GetController() as IEventListener;
                if (ui != null)
                {
                    result = ui.ListenProcessDialogKey(e);
                }

                if (result && currentKeyboardFocusedElement != null)
                {

                    currentKeyboardFocusedElement.InvalidateGraphic();

                }
                e.TranslateCanvasOriginBack();
            }

            return result;
        }

    }
}