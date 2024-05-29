using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMapObjects
{
    public abstract class moRenderer
    {
        /// <summary>
        /// 获取渲染类型
        /// </summary>
        public abstract moRendererTypeConstant RendererType {  get; }
        public abstract moRenderer Clone();
    }
}
