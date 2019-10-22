namespace Lamp.Dapper
{
    public class DapperConifg
    {
        public string ConnectionString { get; set; }

        public DbType DbType { get; set; }
    }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DbType
    {
        SQLServer,
        MySql
    }
}
