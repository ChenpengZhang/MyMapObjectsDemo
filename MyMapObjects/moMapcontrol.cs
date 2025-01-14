﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyMapObjects
{
    public partial class moMapcontrol : UserControl
    {
        #region 字段

        //设计时属性变量
        private Color _SelectionColor = Color.Cyan;
        private Color _FlashColor = Color.Green;
        //运行时属性变量
        private moLayers _Layers = new moLayers(); // 图层集合
        private moProjectionCS _ProjectionCS; // 投影坐标系统
        //模块级变量
        private moMapDrawingReference mMapDrawingReference; // 地图-屏幕坐标转换对象
        private Bitmap mBufferMap1 = new Bitmap(10, 10); // 缓冲位图，仅含要素和注记
        private Bitmap mBufferMap2 = new Bitmap(10, 10); // 缓冲位图，含所有要素，注记
        //跟踪图形（选择图形，描绘图形等）
        private Bitmap mBufferMap3 = new Bitmap(10, 10); // 在屏幕上移动BufferMap2时，先将2绘制到3上，再将3绘制到屏幕上，以降低轻触屏幕带来的闪烁感
        private moSimpleMarkerSymbol mSelectedPointSymbol; // 选择点符号
        private moSimpleLineSymbol mSelectedLineSymbol; // 选择线符号
        private moSimpleFillSymbol mSelectedFillSymbol; // 选择面符号
        private moShapeFlashControler mFlashControler = new moShapeFlashControler(); // 闪烁控制器
        private moSimpleMarkerSymbol mFlashPointSymbol; // 闪烁点符号
        private moSimpleLineSymbol mFlashLineSymbol; // 闪烁线符号
        private moSimpleFillSymbol mFlashFillSymbol; // 闪烁面符号

        #endregion

        #region 构造函数

        public moMapcontrol()
        {
            CreateDefaultProjectionCS(); // 创建默认投影坐标系统
            CreateMapDrawingReference(); // 新建地图-屏幕坐标转换对象
            InitializeComponent();
            ResizeBufferMaps(); // 调整缓冲区位图的尺寸
            InitializeSymbols(); // 初始化符号
            mFlashControler.NeedClearFlashShapes += MFlashControler_NeedClearFlashShapes;
            mFlashControler.NeedDrawFlashShapes += MFlashControler_NeedDrawFlashShapes;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 绘制选择要素使用的颜色
        /// </summary>
        [Browsable(true), Description("获取或设置选择要素的颜色")]
        public Color SelectionColor
        {
            get { return _SelectionColor; }
            set { _SelectionColor = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(true), Description("获取或设置选择闪烁图形的颜色")]
        public Color FlashColor
        {
            get { return _FlashColor; } 
            set { _FlashColor = value; }
        }

        [Browsable(false)]
        public moProjectionCS ProjectionCS 
        {
            get { return _ProjectionCS; } 
            set { _ProjectionCS = value; mMapDrawingReference.mpu = _ProjectionCS.ToMeters(1); }
        }

        [Browsable(false)]
        public moLayers Layers { get { return _Layers; } }

        /// <summary>
        /// 获取地图比例尺
        /// </summary>
        [Browsable(false)]
        public double MapScale { get { return mMapDrawingReference.MapScale; } }

        /// <summary>
        /// 获取控件左上点X的偏移
        /// </summary>
        [Browsable(false)]
        public double MapOffsetX { get { return mMapDrawingReference.OffsetX; } }

        /// <summary>
        /// 获取控件左上点Y的偏移
        /// </summary>
        [Browsable(false)]
        public double MapOffsetY { get { return mMapDrawingReference.OffsetY; } }

        #endregion

        #region 方法

        /// <summary>
        /// 获取地图窗口对应的地图范围
        /// </summary>
        /// <returns></returns>
        public moRectangle GetExtent()
        {
            //定义变量
            double sMinX = double.MaxValue, sMaxX = double.MinValue;
            double sMinY = double.MaxValue, sMaxY = double.MinValue;
            moRectangle sExtent;
            //如果工作区为空，则返回空矩形
            Rectangle sClientRect = this.ClientRectangle;
            if (sClientRect.IsEmpty == true)
            {
                sExtent = new moRectangle(sMinX, sMaxX, sMinY, sMaxY);
                return sExtent;
            }
            //定义工作区左上点和右下点的屏幕坐标
            moPoint sTopLeftScreenPoint = new moPoint(0, 0);
            moPoint sBottomRightScreenPoint = new moPoint(sClientRect.Width, sClientRect.Height);
            //获取工作区左上点和右下点的地图坐标
            moPoint sTopLeftMapPoint = mMapDrawingReference.ToMapPoint(sTopLeftScreenPoint.X, sTopLeftScreenPoint.Y);
            moPoint sBottomRightMapPoint = mMapDrawingReference.ToMapPoint(sBottomRightScreenPoint.X, sBottomRightScreenPoint.Y);
            //定义范围矩形
            sMinX = sTopLeftMapPoint.X;
            sMaxX = sBottomRightMapPoint.X;
            sMinY = sBottomRightMapPoint.Y;
            sMaxY = sTopLeftMapPoint.Y;
            sExtent = new moRectangle(sMinX, sMaxX, sMinY, sMaxY);
            return sExtent;
        }

        /// <summary>
        /// 获取地图范围
        /// </summary>
        /// <returns></returns>
        public moRectangle GetFullExtent()
        {
            //（1）新建一个空矩形
            double sMinX = double.MaxValue, sMaxX = double.MinValue;
            double sMinY = double.MaxValue, sMaxY = double.MinValue;
            moRectangle sFullExtent;
            //（2）如果图层数量为0，则返回空矩形
            Int32 sLayerCount = _Layers.Count;
            if (sLayerCount == 0)
            {
                sFullExtent = new moRectangle(sMinX, sMaxX, sMinY, sMaxY);
                return sFullExtent;
            }
            //（3）计算范围矩形
            for (Int32 i = 0; i <= sLayerCount - 1; i++)
            {
                moMapLayer sLayer = _Layers.GetItem(i);
                moRectangle sExtent = sLayer.Extent;
                if (sExtent.IsEmpty == false)
                {
                    if (sExtent.MinX < sMinX)
                        sMinX = sExtent.MinX;
                    if (sExtent.MaxX > sMaxX)
                        sMaxX = sExtent.MaxX;
                    if (sExtent.MinY < sMinY)
                        sMinY = sExtent.MinY;
                    if (sExtent.MaxY > sMaxY)
                        sMaxY = sExtent.MaxY;
                }
            }
            sFullExtent = new moRectangle(sMinX, sMaxX, sMinY, sMaxY);
            return sFullExtent;
        }

        /// <summary>
        /// 在窗口内选中地图全部范围
        /// </summary>
        public void FullExtent()
        {
            moRectangle sFullExtent = GetFullExtent();
            if(!sFullExtent.IsEmpty)
            {
                Rectangle sClientRect = this.ClientRectangle;
                mMapDrawingReference.ZoomExtentToWindow(sFullExtent, sClientRect.Width, sClientRect.Height);
                this.UseWaitCursor = true;
                DrawBufferMap1();
                DrawBufferMap2();
                this.UseWaitCursor = false;
                Refresh();
                // 触发事件
                if (MapScaleChanged != null)
                    MapScaleChanged(this);
            }
        }

        /// <summary>
        /// 设置地图视图
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="mapScale"></param>
        public void SetView(double offsetX, double offsetY, double mapScale)
        {
            mMapDrawingReference.SetView(offsetX, offsetY, mapScale);
            //触发事件
            if (MapScaleChanged != null)
                MapScaleChanged(this);
        }

        /// <summary>
        /// 以指定中心和指定系数进行缩放
        /// </summary>
        /// <param name="center"></param>
        /// <param name="ratio"></param>
        public void ZoomByCenter(moPoint center, double ratio)
        {
            mMapDrawingReference.ZoomByCenter(center, ratio);
            this.UseWaitCursor = true;
            DrawBufferMap1();
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
            //触发事件
            if (MapScaleChanged != null)
                MapScaleChanged(this);
        }

        /// <summary>
        /// 在窗口内显示指定范围
        /// </summary>
        /// <param name="extent"></param>
        public void ZoomToExtent(moRectangle extent)
        {
            double sWindowWidth = this.ClientRectangle.Width;
            double sWindowHeight = this.ClientRectangle.Height;
            mMapDrawingReference.ZoomExtentToWindow(extent, sWindowWidth, sWindowHeight);
            this.UseWaitCursor = true;
            DrawBufferMap1();
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
            //触发事件
            if (MapScaleChanged != null)
                MapScaleChanged(this);
        }

        /// <summary>
        /// 将地图平移指定量
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public void PanDelta(double deltaX, double deltaY)
        {
            mMapDrawingReference.PanDelta(deltaX, deltaY);
            this.UseWaitCursor = true;
            DrawBufferMap1();
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
        }

        /// <summary>
        /// 将屏幕坐标转换为地图坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public moPoint ToMapPoint(double x, double y)
        {
            return mMapDrawingReference.ToMapPoint(x, y);
        }

        /// <summary>
        /// 将地图坐标转换为屏幕坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public moPoint FromMapPoint(double x, double y)
        {
            return mMapDrawingReference.FromMapPoint(x, y);
        }

        /// <summary>
        /// 将屏幕距离转换为地图距离
        /// </summary>
        /// <param name="dis"></param>
        /// <returns></returns>
        public double ToMapDistance(double dis)
        {
            return mMapDrawingReference.ToMapDistance(dis);
        }

        /// <summary>
        /// 将地图距离转换为屏幕距离
        /// </summary>
        /// <param name="dis"></param>
        /// <returns></returns>
        public double FromMapDistance(double dis)
        {
            return mMapDrawingReference.FromMapDistance(dis);
        }

        public void RedrawMap()
        {
            this.UseWaitCursor = true;
            DrawBufferMap1();
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
        }

        /// <summary>
        /// 重绘跟踪图形
        /// </summary>
        public void RedrawTrackingShapes()
        {
            this.UseWaitCursor = true;
            DrawBufferMap2();
            this.UseWaitCursor = false;
            Refresh();
        }

        /// <summary>
        /// 将地图图像移动到指定位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void PanMapImageTo(float x, float y)
        {
            Graphics g = Graphics.FromImage(mBufferMap3);
            g.Clear(Color.White);
            g.DrawImage(mBufferMap2, x, y);
            g = Graphics.FromHwnd(this.Handle);
            g.DrawImage(mBufferMap3, 0, 0);
            g.Dispose();
        }

        /// <summary>
        /// 对指定的图形数组闪烁
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="times"></param>
        /// <param name="interval"></param>
        public void FlashShapes(moShape[] shapes, Int32 times, Int32 interval)
        {
            mFlashControler.StartFlash(shapes, times, interval);
        }

        public moUserDrawingTool GetDrawingTool()
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            moUserDrawingTool sDrawingTool = CreateDrawingTool(g);
            return sDrawingTool;
        }

        /// <summary>
        /// 根据指定选择盒和选择方式进行选择
        /// </summary>
        /// <param name="selectingBox"></param>
        /// <param name="tolerance"></param>
        /// <param name="selectMethod"></param>
        public void SelectByBox(moRectangle selectingBox, double tolerance, Int32 selectMethod)
        {
            for (Int32 i = 0; i < _Layers.Count; i++)
            {
                moMapLayer sLayer = _Layers.GetItem(i);
                if (sLayer.Visible && sLayer.Selectable)
                {
                    moFeatures sFeatures = sLayer.SearchByBox(selectingBox, tolerance);
                    sLayer.ExecuteSelect(sFeatures, 0);
                }
                else
                {
                    sLayer.SelectedFeatures.Clear();
                }
            }
        }

        #endregion

        #region 事件

        public delegate void MapScaleChangedHandle(object sender);
        /// <summary>
        /// 地图比例尺发生了变化
        /// </summary>
        [Browsable(true), Description("地图比例尺发生了变化")]
        public event MapScaleChangedHandle MapScaleChanged;
        
        public delegate void AfterTrackingLayerDrawHandle(object sender, moUserDrawingTool drawTool);
        /// <summary>
        /// 跟踪曾图形绘制完毕
        /// </summary>
        [Browsable(true), Description("跟踪层图形绘制完毕")]
        public event AfterTrackingLayerDrawHandle AfterTrackingLayerDraw;

        #endregion

        #region 模板和相关对象的事件处理

        //母版刷新
        private void moMapcontrol_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(mBufferMap2, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
        }

        //模板尺寸变化
        private void moMapcontrol_Resize(object sender, EventArgs e)
        {
            if (mMapDrawingReference == null) { return; }
            ResizeBufferMaps();
            RedrawMap();
        }

        //需要绘制闪烁图形
        private void MFlashControler_NeedDrawFlashShapes(object sender, moShape[] shapes)
        {
            double sMapScale = mMapDrawingReference.MapScale;
            double dpm = mMapDrawingReference.dpm;
            double mpu = mMapDrawingReference.mpu;
            moRectangle sExtent = GetExtent();
            Graphics g = Graphics.FromImage(mBufferMap2);
            Int32 sShapeCount = shapes.Length;
            for (Int32 i = 0; i <= sShapeCount - 1; i++)
            {
                if (shapes[i].GetType() == typeof(moPoint))
                {
                    moPoint sPoint = (moPoint)shapes[i];
                    moMapDrawingTools.DrawPoint(g, sExtent, sMapScale, dpm, mpu, sPoint, mFlashPointSymbol);
                }
                else if (shapes[i].GetType() == typeof(moRectangle))
                {
                    moRectangle sRect = (moRectangle)shapes[i];
                    moMapDrawingTools.DrawRectangle(g, sExtent, sMapScale, dpm, mpu, sRect, mFlashFillSymbol);
                }
                else if (shapes[i].GetType() == typeof(moMultiPolyline))
                {
                    moMultiPolyline sMultiPolyline = (moMultiPolyline)shapes[i];
                    moMapDrawingTools.DrawMultiPolyline(g, sExtent, sMapScale, dpm, mpu, sMultiPolyline, mFlashLineSymbol);
                }
                else if (shapes[i].GetType() == typeof(moMultiPolygon))
                {
                    moMultiPolygon sMultiPolygon = (moMultiPolygon)shapes[i];
                    moMapDrawingTools.DrawMultiPolygon(g, sExtent, sMapScale, dpm, mpu, sMultiPolygon, mFlashFillSymbol);
                }
            }
            g.Dispose();
            Refresh();
        }

        //需要清除闪烁图形
        private void MFlashControler_NeedClearFlashShapes(object sender)
        {
            RedrawTrackingShapes();
        }

        #endregion

        #region 私有函数

        private void CreateDefaultProjectionCS()
        {
            string sProjCSName = "Beijing54 Lambert Conformal Conic 2SP";
            string sGeoCSName = "Beijing 1954";
            string sDatumName = "Beijing 1954";
            string sSpheroidName = "Krassowsky_1940";
            double sSemiMajor = 6378245;
            double sInverseFlattening = 298.3;
            double sOriginLatitude = 0;
            double sCentralMeridian = 105;
            double sFalseEasting = 0;
            double sFalseNorthing = 0;
            double sScaleFactor = 1;
            double sStandardParallelOne = 30;
            double sStandardParallelTwo = 62;
            moLinearUnitConstant sLinearUnit = moLinearUnitConstant.Meter;
            moProjectionTypeConstant sProjType = moProjectionTypeConstant.Lambert_Conformal_Conic_2SP;
            _ProjectionCS = new moProjectionCS(sProjCSName, sGeoCSName, sDatumName, sSpheroidName, sSemiMajor,
                sInverseFlattening, sProjType, sOriginLatitude, sCentralMeridian, sFalseEasting,
                sFalseNorthing, sScaleFactor, sStandardParallelOne, sStandardParallelTwo, sLinearUnit);
        }

        //创建地图-屏幕坐标转换对象
        private void CreateMapDrawingReference()
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            double dpm = g.DpiX / 0.0254;
            g.Dispose();
            double mpu = _ProjectionCS.ToMeters(1);
            mMapDrawingReference = new moMapDrawingReference(0, 0, 1000000, dpm, mpu);
        }

        //调整缓冲位图大小
        private void ResizeBufferMaps()
        {
            Rectangle sClientRectangle = this.ClientRectangle;
            if (sClientRectangle.Width > 0 && sClientRectangle.Height > 0)
            {
                if (sClientRectangle.Width != mBufferMap1.Width || sClientRectangle.Height != mBufferMap1.Height)
                {
                    mBufferMap1 = new Bitmap(sClientRectangle.Width, sClientRectangle.Height);
                    mBufferMap2 = new Bitmap(sClientRectangle.Width, sClientRectangle.Height);
                    mBufferMap3 = new Bitmap(sClientRectangle.Width, sClientRectangle.Height);
                }
            }
        }

        //初始化符号
        private void InitializeSymbols()
        {
            //选择点符号
            mSelectedPointSymbol = new moSimpleMarkerSymbol();
            mSelectedPointSymbol.Color = _SelectionColor;
            mSelectedPointSymbol.Size = 3;
            //选择线符号
            mSelectedLineSymbol = new moSimpleLineSymbol();
            mSelectedLineSymbol.Color = _SelectionColor;
            mSelectedLineSymbol.Size = 3000 / mMapDrawingReference.dpm;
            //选择面符号
            mSelectedFillSymbol = new moSimpleFillSymbol();
            mSelectedFillSymbol.Color = Color.Transparent;
            mSelectedFillSymbol.Outline.Color = _SelectionColor;
            mSelectedFillSymbol.Outline.Size = 3000 / mMapDrawingReference.dpm;
            //闪烁点符号
            mFlashPointSymbol = new moSimpleMarkerSymbol();
            mFlashPointSymbol.Color = _FlashColor;
            mFlashPointSymbol.Style = moSimpleMarkerSymbolStyleConstant.SolidCircle;
            mFlashPointSymbol.Size = 3;
            //闪烁线符号
            mFlashLineSymbol = new moSimpleLineSymbol();
            mFlashLineSymbol.Color = _FlashColor;
            mFlashLineSymbol.Size = 1.5;
            //闪烁面符号
            mFlashFillSymbol = new moSimpleFillSymbol();
            mFlashFillSymbol.Color = _FlashColor;
            mFlashFillSymbol.Outline.Color = Color.Black;
            mFlashFillSymbol.Outline.Size = 0.35;
        }

        private void DrawBufferMap1()
        {
            //（1）获取地图窗口的范围
            moRectangle sExtent = GetExtent();
            if (sExtent.IsEmpty == true)
                return;
            //（2）绘制所有图层的要素，采用倒序
            double sMapScale = mMapDrawingReference.MapScale;
            double dpm = mMapDrawingReference.dpm;
            double mpu = mMapDrawingReference.mpu;
            Graphics g = Graphics.FromImage(mBufferMap1);
            g.Clear(Color.White);
            Int32 sLayerCount = _Layers.Count;
            for (Int32 i = sLayerCount - 1; i >= 0; i--)
            {
                moMapLayer sLayer = _Layers.GetItem(i);
                if (sLayer.Visible == true)
                {
                    sLayer.DrawFeatures(g, sExtent, sMapScale, dpm, mpu);
                }
            }
            //（3）绘制所有图层的注记，依然倒序
            List<RectangleF> sPlacedLabelExtents = new List<RectangleF>();
            for (Int32 i = sLayerCount - 1; i >= 0; i--)
            {
                moMapLayer sLayer = _Layers.GetItem(i);
                if (sLayer.Visible == true)
                {
                    sLayer.DrawLabels(g, sExtent, sMapScale, dpm, mpu, sPlacedLabelExtents);
                }
            }
            g.Dispose();
        }

        private void DrawBufferMap2()
        {
            //（1）获取地图窗口的范围
            moRectangle sExtent = GetExtent();
            if (sExtent.IsEmpty == true)
                return;
            //（2）绘制缓冲位图1
            Graphics g = Graphics.FromImage(mBufferMap2);
            g.Clear(Color.White);
            Rectangle sRect = new Rectangle(0, 0, mBufferMap1.Width, mBufferMap1.Height);
            g.DrawImage(mBufferMap1, sRect, sRect, GraphicsUnit.Pixel);
            //（3）绘制所有图层的选择要素，采用倒序
            double sMapScale = mMapDrawingReference.MapScale;
            double dpm = mMapDrawingReference.dpm;
            double mpu = mMapDrawingReference.mpu;
            Int32 sLayerCount = _Layers.Count;
            for (Int32 i = sLayerCount - 1; i >= 0; i--)
            {
                moMapLayer sLayer = _Layers.GetItem(i);
                if (sLayer.ShapeType == moGeometryTypeConstant.Point)
                {
                    sLayer.DrawSelectedFeatures(g, sExtent, sMapScale, dpm, mpu, mSelectedPointSymbol);
                }
                else if (sLayer.ShapeType == moGeometryTypeConstant.MultiPolyline)
                {
                    sLayer.DrawSelectedFeatures(g, sExtent, sMapScale, dpm, mpu, mSelectedLineSymbol);
                }
                else if (sLayer.ShapeType == moGeometryTypeConstant.MultiPolygon)
                {
                    sLayer.DrawSelectedFeatures(g, sExtent, sMapScale, dpm, mpu, mSelectedFillSymbol);
                }
            }
            //（4）触发事件，以便用户程序继续绘图
            if (AfterTrackingLayerDraw != null)
            {
                //新建绘图工具
                moUserDrawingTool sDrawingTool = CreateDrawingTool(g);
                AfterTrackingLayerDraw(this, sDrawingTool);
            }
            g.Dispose();
        }

        //生成一个用户绘图工具
        private moUserDrawingTool CreateDrawingTool(Graphics g)
        {
            moRectangle sExtent = GetExtent();
            double sMapScale = mMapDrawingReference.MapScale;
            double dpm = mMapDrawingReference.dpm;
            double mpu = mMapDrawingReference.mpu;
            moUserDrawingTool sDrawingTool = new moUserDrawingTool(g, sExtent, sMapScale, dpm, mpu);
            return sDrawingTool;
        }

        #endregion
    }
}
