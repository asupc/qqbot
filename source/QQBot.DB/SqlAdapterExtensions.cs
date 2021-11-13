using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QQBot.DB
{
    internal static class SqlAdapterExtensions
    {
        public static void AppendColumnNameEqualsValue(this ISqlAdapter adapter, StringBuilder sb, string columnName, string columnValue)
        {
            switch (adapter)
            {
                case SQLiteAdapter sqliteAdpter:
                    sb.AppendFormat("\"{0}\" = @{1}", columnName, columnValue);
                    break;
                case PostgresAdapter pgAdpter:
                    sb.AppendFormat("\"{0}\" = @{1}", columnName, columnValue);
                    break;
                case MySqlAdapter mysqlAdpter:
                    sb.AppendFormat("`{0}` = @{1}", columnName, columnValue);
                    break;
                case SqlCeServerAdapter sqlceAdapter:
                    sb.AppendFormat("[{0}] = @{1}", columnName, columnValue);
                    break;
                case SqlServerAdapter sqlAdapter:
                    sb.AppendFormat("[{0}] = @{1}", columnName, columnValue);
                    break;
            }
        }

        public static void AppendColumnNameWithValue(this ISqlAdapter adapter, StringBuilder sb, PropertyInfo property)
        {
            var columnName = property.Name;
            var columnValue = property.Name;
            var valueType = property.PropertyType;
            adapter.AppendColumnNameWithValue(sb, columnName, columnValue, valueType);
        }

        public static void AppendColumnNameWithValue(this ISqlAdapter adapter, StringBuilder sb, string columnName, string columnValue, Type valueType)
        {
            if (valueType.Equals(typeof(string)) || !typeof(IEnumerable).IsAssignableFrom(valueType))
            {
                adapter.AppendColumnNameEqualsValue(sb, columnName, columnValue);
                return;
            }

            switch (adapter)
            {
                case SQLiteAdapter sqliteAdpter:
                    sb.AppendFormat("\"{0}\" IN @{1}", columnName, columnValue);
                    break;
                case PostgresAdapter pgAdpter:
                    sb.AppendFormat("\"{0}\" IN @{1}", columnName, columnValue);
                    break;
                case MySqlAdapter mysqlAdpter:
                    sb.AppendFormat("`{0}` IN @{1}", columnName, columnValue);
                    break;
                case SqlCeServerAdapter sqlceAdapter:
                    sb.AppendFormat("[{0}] IN @{1}", columnName, columnValue);
                    break;
                case SqlServerAdapter sqlAdapter:
                    sb.AppendFormat("[{0}] IN @{1}", columnName, columnValue);
                    break;
            }
        }
    }
}
