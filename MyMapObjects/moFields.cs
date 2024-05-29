using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMapObjects
{
    public class moFields
    {
        #region 字段

        private List<moField> _Fields;
        private string _PrimaryField = ""; // 主字段
        private bool _ShowAlias = false; // 是否显示别名

        #endregion

        #region 构造函数

        public moFields()
        {
            _Fields = new List<moField>();
        }

        #endregion 属性

        public Int32 Count
        {
            get { return _Fields.Count; }
        }

        public string PrimaryField
        {
            get { return _PrimaryField; }
            set { _PrimaryField = value; }
        }

        public bool ShowAlias
        {
            get { return _ShowAlias; }
            set { _ShowAlias = value; }
        }

        #region 方法

        public moField GetItem(Int32 index)
        {
            return _Fields[index];
        }

        public moField GetItem(string name)
        {
            Int32 sIndex = FindField(name);
            if (sIndex >= 0)
            {
                return GetItem(sIndex);
            }
            else { return null; }
        }

        /// <summary>
        /// 查找指定名称的字段，返回其索引号，如果没有返回-1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Int32 FindField(string name)
        {
            Int32 sFieldCount = _Fields.Count;
            for (Int32 i = 0; i <= sFieldCount - 1; i++)
            {
                if (_Fields[i].Name.ToLower() == name.ToLower())
                {
                    return i;
                }
            }
            return -1;
        }

        public void Append(moField field)
        {
            if (FindField(field.Name) >= 0)
            {
                throw new Exception("Fields对象不能存在重名的字段");
            }
            else
            {
                _Fields.Add(field);
                // 触发事件
                if (FieldAppended != null)
                {
                    FieldAppended(this, field);
                }
            }
        }

        public void RemoveAt(Int32 index)
        {
            moField sField = _Fields[index];
            _Fields.RemoveAt(index);
            // 触发事件
            if (FieldRemoved != null)
            {
                FieldRemoved(this, index, sField);
            }
        }

        #endregion

        #region 事件

        internal delegate void FieldAppendHandle(object sender, moField fieldAppended);
        /// <summary>
        /// 有字段被加入
        /// </summary>
        internal event FieldAppendHandle FieldAppended;

        internal delegate void FieldRemoveHandle(object sender, Int32 fieldIndex, moField fieldRemoved);
        /// <summary>
        /// 有字段被删除
        /// </summary>
        internal event FieldRemoveHandle FieldRemoved;

        #endregion


    }
}
