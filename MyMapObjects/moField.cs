using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMapObjects
{
    public class moField
    {
        #region 字段

        private string _Name = "";
        private string _AliasName = "";
        private moValueTypeConstant _ValueType = moValueTypeConstant.dInt32; // 值类型
        private Int32 _Length = 0; // 长度

        #endregion 构造函数

        public moField(string name)
        {
            _Name = name;
            _AliasName = name;
        }

        public moField(string name, moValueTypeConstant valueType)
        {
            _Name = name;
            _AliasName = name;
            _ValueType = valueType;
        }

        #region 属性

        /// <summary>
        /// 获取名称
        /// </summary>
        public string Name
        {
            get { return _Name; }
        }

        /// <summary>
        /// 获取或设置别名
        /// </summary>
        public string AliasName
        {
            get { return _AliasName; }
            set { _AliasName = value; }
        }

        /// <summary>
        /// 获取值类型
        /// </summary>
        public moValueTypeConstant ValueType
        {
            get { return _ValueType; }
        }

        /// <summary>
        /// 获取字段长度
        /// </summary>
        public Int32 Length
        {
            get { return _Length; }
        }

        #endregion

        #region 方法

        public moField Clone()
        {
            moField sField = new moField(_Name, _ValueType);
            sField._AliasName = _AliasName;
            sField._Length = _Length;
            return sField;
        }

        #endregion
    }
}
