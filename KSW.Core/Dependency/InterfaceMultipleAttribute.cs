using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSW.Dependency
{
    /// <summary>
    /// 标记接口为多个实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class InterfaceMultipleAttribute : Attribute
    {
        public InterfaceMultipleAttribute(bool isMultiple = true)
        {
            IsMultiple = isMultiple;
        }

        /// <summary>
        /// 是否是多个实例
        /// </summary>
        public bool IsMultiple { get; set; }
    }
}
