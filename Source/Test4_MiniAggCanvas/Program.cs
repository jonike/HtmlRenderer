﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
namespace Test4_AggCanvasBox
{
    static class Program
    {
        static LayoutFarm.Dev.FormDemoList formDemoList;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //temp
            //var startPars = new LayoutFarm.UI.GdiPlus.MyWinGdiPortalSetupParameters();
            //var platform = LayoutFarm.UI.GdiPlus.MyWinGdiPortal.Start(startPars);
            formDemoList = new LayoutFarm.Dev.FormDemoList();
            formDemoList.LoadDemoList(typeof(Program));
            Application.Run(formDemoList);
            //LayoutFarm.UI.GdiPlus.MyWinGdiPortal.End();
        }
    }
}
