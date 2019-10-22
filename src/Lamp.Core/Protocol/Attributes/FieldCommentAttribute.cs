using System;

namespace Lamp.Core.Protocol.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class FieldCommentAttribute : Attribute
    {
        public FieldCommentAttribute() { }
        public FieldCommentAttribute(string comment)
        {
            Comment = comment;
        }
        public FieldCommentAttribute(string field, string comment)
        {
            FieldName = field;
            Comment = comment;
        }
        /// <summary>
        /// <summary>
        /// 为swagger服务
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        ///  字段名
        /// </summary>
        public string FieldName { get; set; }
    }
}
