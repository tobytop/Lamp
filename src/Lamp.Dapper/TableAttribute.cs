﻿using System;

namespace Lamp.Dapper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string Name
        {
            get; private set;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute(bool identity = true)
        {
            Identity = identity;
        }

        /// <summary>
        /// 是否为自增长的字段
        /// </summary>
        public bool Identity
        {
            get; private set;
        }
    }
}
