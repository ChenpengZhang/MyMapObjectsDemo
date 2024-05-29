using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyMapObjectsDemo
{
    public partial class frmLayerRenderer : Form
    {
        #region 字段

        private MyMapObjects.moMapLayer _Layer;
        private MyMapObjects.moRenderer _Renderer;

        #endregion

        #region 方法

        //设置图层
        internal void SetDate(MyMapObjects.moMapLayer layer)
        {
            _Layer = layer;
        }

        internal void GetData(out MyMapObjects.moRenderer renderer)
        {
            renderer = _Renderer;
        }

        #endregion

        #region 字段

        #endregion

        public frmLayerRenderer()
        {
            InitializeComponent();
        }

        private void frmLayerRenderer_Load(object sender, EventArgs e)
        {
            // 这时已经获得了输入的图层对象_Layer，为接下来的用户交互做相应的准备工作
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // 1. 根据用户交互的结果新建一个Renderer的实例，并赋给_Renderer变量
            // 2. 关闭窗体，并返回对话框结果
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
