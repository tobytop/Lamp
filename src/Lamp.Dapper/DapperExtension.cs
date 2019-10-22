using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Transactions;

namespace Lamp.Dapper
{
    public static class DapperExtension
    {
        /// <summary>
        /// 新增拓展函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static S Insert<T, S>(this IDbConnection connection, T entity, DbType dbType = DbType.MySql) where T : class
        {
            PropertyInfo[] ps = entity.GetType().GetProperties();
            List<string> @colms = new List<string>();
            List<string> @params = new List<string>();

            string tableName = GetTableName<T>(entity);

            object keyValue = null;

            foreach (PropertyInfo p in ps)
            {
                if (p.GetCustomAttributes(false).Count(o => o.GetType() == typeof(IgnoreAttribute)) > 0)
                {
                    continue;
                }

                KeyAttribute property = (KeyAttribute)p.GetCustomAttributes(false).FirstOrDefault(o => o.GetType() == typeof(KeyAttribute));

                if (property != null || !property.Identity)
                {
                    keyValue = p.GetValue(entity, null);
                }

                switch (Type.GetTypeCode(p.PropertyType))
                {
                    case TypeCode.DateTime:
                        if (Convert.ToDateTime(p.GetValue(entity, null)) > DateTime.MinValue)
                        {
                            @colms.Add(string.Format("{0}", p.Name));
                            @params.Add(string.Format("@{0}", p.Name));
                        };
                        break;
                    case TypeCode.Int32:
                        if (!p.GetValue(entity, null).ToString().Equals("0"))
                        {
                            @colms.Add(string.Format("{0}", p.Name));
                            @params.Add(string.Format("@{0}", p.Name));
                        }
                        break;
                    case TypeCode.Int64:
                        if (!p.GetValue(entity, null).ToString().Equals("0"))
                        {
                            @colms.Add(string.Format("{0}", p.Name));
                            @params.Add(string.Format("@{0}", p.Name));
                        }
                        break;
                    case TypeCode.Decimal:
                        if (!p.GetValue(entity, null).ToString().Equals("0"))
                        {
                            @colms.Add(string.Format("{0}", p.Name));
                            @params.Add(string.Format("@{0}", p.Name));
                        }
                        break;
                    default:
                        if ((property == null && p.GetValue(entity, null) != null) || (property != null && !property.Identity))
                        {
                            @colms.Add(string.Format("{0}", p.Name));
                            @params.Add(string.Format("@{0}", p.Name));
                        }
                        break;
                }
            }
            string sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, string.Join(", ", @colms), string.Join(", ", @params));
            if (keyValue == null)
            {
                switch (dbType)
                {
                    case DbType.MySql:
                        sql += ";select last_insert_id()";
                        break;
                    case DbType.SQLServer:
                        sql += "SELECT CAST(SCOPE_IDENTITY() as int)";
                        break;
                }
                return connection.Query<S>(sql, entity).FirstOrDefault();
            }
            else
            {
                return (S)keyValue;
            }

        }

        /// <summary>
        /// 更新拓展函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static int Update<T>(this IDbConnection connection, T entity)
        {
            PropertyInfo[] ps = entity.GetType().GetProperties();
            List<string> @params = new List<string>();

            string where = string.Empty;

            string tableName = GetTableName<T>(entity);
            foreach (PropertyInfo p in ps)
            {
                if (p.GetCustomAttributes(false).Count(o => o.GetType() == typeof(IgnoreAttribute)) > 0)
                {
                    continue;
                }

                KeyAttribute property = (KeyAttribute)p.GetCustomAttributes(false).FirstOrDefault(o => o.GetType() == typeof(KeyAttribute));
                if (property != null)
                {
                    where = string.Format("{0}=@{0}", p.Name);
                    continue;
                }

                switch (Type.GetTypeCode(p.PropertyType))
                {
                    case TypeCode.DateTime:
                        if (Convert.ToDateTime(p.GetValue(entity, null)) > DateTime.MinValue)
                        {
                            @params.Add(string.Format("{0}=@{0}", p.Name));
                        };
                        break;
                    case TypeCode.Int32:
                        if (!p.GetValue(entity, null).ToString().Equals("0"))
                        {
                            @params.Add(string.Format("{0}=@{0}", p.Name));
                        }
                        break;
                    case TypeCode.Int64:
                        if (!p.GetValue(entity, null).ToString().Equals("0"))
                        {
                            @params.Add(string.Format("{0}=@{0}", p.Name));
                        }
                        break;
                    case TypeCode.Decimal:
                        if (!p.GetValue(entity, null).ToString().Equals("0"))
                        {
                            @params.Add(string.Format("{0}=@{0}", p.Name));
                        }
                        break;
                    default:
                        if (p.GetValue(entity, null) != null)
                        {
                            @params.Add(string.Format("{0}=@{0}", p.Name));
                        }
                        break;
                }
            }
            string sql = string.Format("update {0} set {1} where {2}", tableName, string.Join(", ", @params), where);
            return connection.Execute(sql, entity);
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string GetTableName<T>(T entity)
        {
            string tableName = string.Empty;
            object[] objAttrs = entity.GetType().GetCustomAttributes(typeof(TableAttribute), true);
            if (objAttrs.Length > 0)
            {
                TableAttribute attr = objAttrs[0] as TableAttribute;
                if (attr != null)
                {
                    tableName = attr.Name;
                }
            }
            return tableName;
        }

        /// <summary>
        /// 事务语句统一执行
        /// </summary>
        /// <param name="ac"></param>
        /// <returns></returns>
        public static bool TransactionExecute(this IDbConnection connection, Action<IDbConnection> ac)
        {
            try
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    ac.Invoke(connection);
                    ts.Complete();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 事务语句统一执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fun"></param>
        /// <returns></returns>
        public static T TransactionExecute<T>(this IDbConnection connection, Func<IDbConnection, T> fun)
        {
            T re = default(T);
            try
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    re = fun.Invoke(connection);
                    ts.Complete();
                }
                return re;
            }
            catch
            {
                return re;
            }
        }
    }
}
