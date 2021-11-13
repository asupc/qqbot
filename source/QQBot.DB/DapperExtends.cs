using Dapper;
using Dapper.Contrib.Extensions;
using QQBot.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QQBot.DB
{

    public class BaseRepository<T> where T : class
    {
        public static BaseRepository<T> Init(IDbConnection dbConnection)
        {
            return new BaseRepository<T>(dbConnection);
        }

        public static BaseRepository<T> Instance
        {
            get
            {
                return new BaseRepository<T>(InstallConfigHelper.GetDbConnection);
            }
        }

        public BaseRepository(IDbConnection connection)
        {
            this._connection = connection;
            _sqlAdapter = GetFormatter(connection);
            var sb = new StringBuilder();
            _sqlAdapter.AppendColumnName(sb, GetTableName(typeof(T)));
            _tableName = sb.ToString();
        }

        protected ISqlAdapter _sqlAdapter;
        protected IDbConnection _connection;
        protected IDbTransaction _transaction;
        public string _tableName { get; set; }
        protected static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        public static SqlMapperExtensions.GetDatabaseTypeDelegate GetDatabaseType;
        private static readonly ISqlAdapter DefaultAdapter = new SqlServerAdapter();
        private static readonly Dictionary<string, ISqlAdapter> AdapterDictionary
            = new Dictionary<string, ISqlAdapter>
            {
                {"sqlconnection", new SqlServerAdapter()},
                {"sqlceconnection", new SqlCeServerAdapter()},
                {"npgsqlconnection", new PostgresAdapter()},
                {"sqliteconnection", new SQLiteAdapter()},
                {"mysqlconnection", new MySqlAdapter()},
            };

        private static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            var name = GetDatabaseType?.Invoke(connection).ToLower()
                       ?? connection.GetType().Name.ToLower();

            return !AdapterDictionary.ContainsKey(name)
                ? DefaultAdapter
                : AdapterDictionary[name];
        }

        /// <summary>
        /// 获取实体对象对应的数据库表名称；
        /// </summary>
        /// <param name="type">数据库表映射的实体类型</param>
        /// <returns>数据库表名</returns>
        protected static string GetTableName(Type type)
        {
            var tableattr = type.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TableAttribute") as dynamic;
            if (tableattr != null)
                return tableattr.Name;

            var name = type.Name;
            if (type.IsInterface && name.StartsWith("I"))
                name = name.Substring(1);
            return name;
        }

        public static List<PropertyInfo> TypePropertiesCache(Type type)
        {
            if (TypeProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pis))
            {
                return pis.ToList();
            }

            var properties = type.GetProperties().Where(IsWriteable).ToArray();
            TypeProperties[type.TypeHandle] = properties;
            return properties.ToList();
        }

        /// <summary>
        /// 判断属性是否可写
        /// </summary>
        /// <param name="pi">反射出来的属性信息</param>
        /// <returns>返回true表示可以写，false表示不可以写</returns>
        protected static bool IsWriteable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(WriteAttribute), false).AsList();
            if (attributes.Count != 1) return true;

            var writeAttribute = (WriteAttribute)attributes[0];
            return writeAttribute.Write;
        }

        /// <summary>
        /// 根据查询条件对象，生成and关系查询条件(不包含where关键字)
        /// </summary>
        /// <param name="queryParam">查询条件对象</param>
        /// <returns>返回and关系查询条件(不包含where关键字)</returns>
        protected string GetWhereCondition(object queryParam)
        {
            if (queryParam == null)
                return null;

            Type type = queryParam.GetType();
            var allProperties = TypePropertiesCache(type);
            var sbParameterList = new StringBuilder();
            for (var i = 0; i < allProperties.Count; i++)
            {
                var property = allProperties.ElementAt(i);
                if (i < allProperties.Count - 1)
                {
                    _sqlAdapter.AppendColumnNameWithValue(sbParameterList, property);
                    sbParameterList.Append(" and ");
                    continue;
                }
                _sqlAdapter.AppendColumnNameWithValue(sbParameterList, property);
            }
            return sbParameterList.ToString();
        }

        /// <summary>
        /// 根据查询条件对象，生成and关系where查询语句(包含 where关键字)
        /// </summary>
        /// <param name="queryParam">查询条件对象</param>
        /// <returns>返回where查询语句</returns>
        protected string GetWhereSql(object queryParam)
        {
            var whereCondition = GetWhereCondition(queryParam);
            return GetWhereSql(whereCondition);
        }

        /// <summary>
        /// 根据查询条件语句(不包含Where关键字)，生成Where查询语句
        /// </summary>
        /// <param name="whereCondition">查询条件语句(不包含Where关键字)</param>
        /// <returns>where条件语句</returns>
        protected static string GetWhereSql(string whereCondition)
        {
            if (string.IsNullOrWhiteSpace(whereCondition))
                return null;
            return "where " + whereCondition;
        }

        /// <summary>
        /// 根据排序条件语句(不包含order by关键字)，生成Order By语句
        /// </summary>
        /// <param name="orderByCondition">排序条件语句(不包含order by关键字)</param>
        /// <returns>Order By排序语句</returns>
        protected static string GetOrderBySql(string orderByCondition)
        {
            if (string.IsNullOrWhiteSpace(orderByCondition))
                return null;
            return "order by " + orderByCondition;
        }

        /// <summary>
        /// 根据查询生成的对象类型，生成查询需要列字段
        /// </summary>
        /// <param name="type">查询生成的对象类型</param>
        /// <param name="tableAlias">数据库表别名</param>
        /// <returns>查询列字段字符串</returns>
        protected string GetSelectFields(Type type, string tableAlias = null)
        {
            var prefix = (tableAlias == null) ? null : $"{tableAlias}.";
            if (type == typeof(T))
                return $"{prefix}*";

            var allProperties = TypePropertiesCache(type);
            var sbParameterList = new StringBuilder(null);
            for (var i = 0; i < allProperties.Count; i++)
            {
                var property = allProperties.ElementAt(i);
                if (i < allProperties.Count - 1)
                {
                    sbParameterList.Append(prefix);
                    _sqlAdapter.AppendColumnName(sbParameterList, property.Name);
                    sbParameterList.Append(",");
                    continue;
                }

                sbParameterList.Append(prefix);
                _sqlAdapter.AppendColumnName(sbParameterList, property.Name);
            }
            return sbParameterList.ToString();
        }

        /// <summary>
        /// 根据查询当前页数和每页结果数生成分页SQL语句
        /// </summary>
        /// <param name="pageIndex">当前页数，为0为不分页</param>
        /// <param name="pageSize">每页返回的实体对象个数</param>
        /// <returns>分页SQL语句</returns>
        protected static string GetPageSql(int pageIndex, int pageSize)
        {
            string limitString = null;
            if (pageIndex > 0)
            {
                var offset = (pageIndex - 1) * pageSize;
                limitString = $"limit {offset},{pageSize}";
            }
            return limitString;
        }

        protected string BuildSelectSql<TQuery>(string whereCondition, string orderByCondition)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("select {0} from " + _tableName);
            sqlBuilder.Append($" {GetWhereSql(whereCondition)}");
            sqlBuilder.Append($" {GetOrderBySql(orderByCondition)}");

            string sql = sqlBuilder.ToString();
            var selectFields = GetSelectFields(typeof(TQuery));
            sql = string.Format(sql, selectFields);
            return sql;
        }

        /// <summary>
        /// 插入单个实体
        /// </summary>
        /// <typeparam name="T">数据库表映射的实体类型，默认实体名称作为表名，如果需要特殊指定表名，需要在实体类似加上TableAttribute特性</typeparam>
        /// <param name="entity">实体对象的值</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>返回受插入影响的列数</returns>
        public long Add(T entity, IDbTransaction transaction = null)
        {
            return _connection.Insert<T>(entity, transaction);
        }

        /// <summary>
        /// 批量插入实体到数据库
        /// </summary>
        /// <param name="entities">实体对象组</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>返回受插入影响的列数</returns>
        public long AddRange(IEnumerable<T> entities, IDbTransaction transaction = null)
        {
            return _connection.Insert(entities, transaction);
        }

        /// <summary>
        /// 查询数据表中所有的行，生成指定对象的列表
        /// </summary>
        /// <typeparam name="TQuery">查询生成的对象类型，对象的属性名称和要查询的列名一一对应</typeparam>
        /// <returns>指定对象的列表</returns>
        //public IEnumerable<T> GetAll()
        //{
        //    var selectedFields = GetSelectFields(typeof(T));
        //    string querySqlFormat = "select {0} from " + _tableName;
        //    var sql = string.Format(querySqlFormat, selectedFields);
        //    IEnumerable<T> data = CacheHelper.Get<IEnumerable<T>>(_tableName);
        //    if (data != null && data.Any())
        //    {
        //        Console.WriteLine(_tableName + ",Cache Data" + DateTime.Now);
        //        return data;
        //    }
        //    data = _connection.Query<T>(sql);
        //    Console.WriteLine(_tableName + ",No Cache Data" + DateTime.Now);
        //    CacheHelper.Add(_tableName, data, DateTimeOffset.Now.AddSeconds(10));
        //    return data;
        //}


        /// <summary>
        /// 根据主键Id获取实体对象
        /// </summary>
        /// <typeparam name="T">数据库表映射的实体类型，默认实体名称作为表名，如果需要特殊指定表名，需要在实体类似加上TableAttribute特性</typeparam>
        /// <param name="id">查询对象参数对象</param>
        /// <returns>返回指定Id的尸体对象</returns>
        public T GetById(object id)
        {
            return _connection.Get<T>(id);
        }

        /// <summary>
        /// 根据查询条件获取实体对象列表
        /// </summary>
        /// <param name="queryParam">对象查询条件对象，查询对象的属性为数据库表的列，属性的值为查询where限定条件,各个属性值的限定关系为and关系</param>
        /// <returns>返回满足查询条件的实体对象列表</returns>
        public IEnumerable<T> Get(object queryParam)
        {
            string whereSql = GetWhereSql(queryParam);
            string sql = $"select * from {_tableName} {whereSql}";
            return _connection.Query<T>(sql, queryParam);
        }


        /// <summary>
        /// 根据查询条件获取实体对象列表
        /// </summary>
        /// <param name="queryParam">对象查询条件对象，查询对象的属性为数据库表的列，属性的值为查询where限定条件,各个属性值的限定关系为and关系</param>
        /// <returns>返回满足查询条件的实体对象列表</returns>
        public IEnumerable<T> GetAll()
        {
            string sql = $"select * from {_tableName}";
            return _connection.Query<T>(sql, null);
        }

        /// <summary>
        /// 根据查询条件获取数据库表指定字段生成的对象列表
        /// </summary>
        /// <typeparam name="TQuery">查询生成的对象类型，对象的属性名称和要查询的列名一一对应</typeparam>
        /// <param name="queryParam">对象查询条件对象，查询对象的属性为数据库表的列，属性的值为查询where限定条件,各个属性值的限定关系为and关系</param>
        /// <param name="orderByCondition">查询限定的排序条件，不包含oder by关键字</param>
        /// <returns>返回满足查询条件的指定对象列表</returns>
        public IEnumerable<TQuery> Get<TQuery>(object queryParam, string orderByCondition = null)
        {
            string whereCondition = GetWhereCondition(queryParam);
            var sql = BuildSelectSql<TQuery>(whereCondition, orderByCondition);
            return _connection.Query<TQuery>(sql, queryParam);
        }

        /// <summary>
        /// 根据查询条件获取数据库表指定字段生成的对象列表
        /// </summary>
        /// <typeparam name="TQuery">查询生成的对象类型，对象的属性名称和要查询的列名一一对应</typeparam>
        /// <param name="whereCondition">查询的where限定条件，不包含where关键字</param>
        /// <param name="orderByCondition">查询限定的排序条件，不包含oder by关键字</param>
        /// <param name="param">参数化查询的参数值对象</param>
        /// <returns>返回满足查询条件的指定对象列表</returns>
        public IEnumerable<TQuery> Get<TQuery>(string whereCondition = null, string orderByCondition = null, object param = null)
        {
            var sql = BuildSelectSql<TQuery>(whereCondition, orderByCondition);
            return _connection.Query<TQuery>(sql, param);
        }

        /// <summary>
        /// 判断指定查询条件的实体对象是否存在
        /// </summary>
        /// <param name="queryParam">对象查询条件对象，查询对象的属性为数据库表的列，属性的值为查询where限定条件,各个属性值的限定关系为and关系</param>
        /// <returns>存在返回true，不存在返回false</returns>
        public bool IsExist(object queryParam)
        {
            return Count(queryParam) > 0;
        }

        /// <summary>
        /// 获取满足指定条件的行数
        /// </summary>
        /// <param name="queryParam">对象查询条件对象，查询对象的属性为数据库表的列，属性的值为查询where限定条件,各个属性值的限定关系为and关系</param>
        /// <returns>满足查询条件的行数</returns>
        public int Count(object queryParam)
        {
            var whereSql = GetWhereSql(queryParam);
            string sql = $"select count(1) from {_tableName} {whereSql}";
            return _connection.ExecuteScalar<int>(sql, queryParam);
        }

        /// <summary>
        /// 获取满足指定条件的行数
        /// </summary>
        /// <param name="whereCondition">查询的where限定条件，不包含where关键字</param>
        /// <param name="param">参数化查询的参数值对象</param>
        /// <returns>满足查询条件的行数</returns>
        public int Count(string whereCondition = null, object param = null)
        {
            var whereSql = GetWhereSql(whereCondition);
            string sql = $"select count(1) from {_tableName} {whereSql}";
            return _connection.ExecuteScalar<int>(sql, param);
        }


        /// <summary>
        /// 根据主键Id删除指定对象
        /// </summary>
        /// <param name="id">主键Id的值</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示删除成功，false表示删除失败</returns>
        public bool DeleteById(object id, IDbTransaction transaction = null)
        {
            string sql = $"delete from {_tableName} where Id=@Id";
            var deleted = _connection.Execute(sql, new { Id = id }, transaction);
            return deleted > 0;
        }
        /// <summary>
        /// 根据主键Id删除指定对象
        /// </summary>
        /// <param name="id">主键Id的值</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示删除成功，false表示删除失败</returns>
        public bool DeleteAll()
        {
            string sql = $"delete from {_tableName}";
            var deleted = _connection.Execute(sql);
            return deleted > 0;
        }


        /// <summary>
        /// 根据主键Id删除指定对象
        /// </summary>
        /// <param name="id">主键Id的值</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示删除成功，false表示删除失败</returns>
        public bool DeleteByIds(IEnumerable<string> ids, IDbTransaction transaction = null)
        {
            if (ids == null || !ids.Any())
            {
                return false;
            }
            string sql = $"delete from {_tableName} where Id in @Ids";
            var deleted = _connection.Execute(sql, new { Ids = ids }, transaction);
            return deleted > 0;
        }

        /// <summary>
        /// 从数据库表中删除指定的实体对象
        /// </summary>
        /// <param name="enity">实体对象的值</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示删除成功，false表示删除失败</returns>
        public bool Delete(T enity, IDbTransaction transaction = null)
        {
            return _connection.Delete<T>(enity, transaction);
        }

        /// <summary>
        /// 批量删除数据库表中的实体对象
        /// </summary>
        /// <param name="entities">新的实体对象值列表</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示删除成功，false表示删除失败</returns>
        public bool DeleteRange(IEnumerable<T> entities, IDbTransaction transaction = null)
        {
            return _connection.Delete<IEnumerable<T>>(entities, transaction);
        }

        /// <summary>
        /// 从数据库表中删除满足指定条件的行
        /// </summary>
        /// <param name="queryParam">对象查询条件对象，查询对象的属性为数据库表的列，属性的值为查询where限定条件,各个属性值的限定关系为and关系</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示删除成功，false表示删除失败</returns>
        public long Delete(object queryParam, IDbTransaction transaction = null)
        {
            string whereSql = GetWhereSql(queryParam);
            string sql = $"delete from {_tableName} {whereSql}";
            return _connection.Execute(sql, queryParam, transaction);
        }

        /// <summary>
        /// 从数据库表中删除满足指定条件的行
        /// </summary>
        /// <param name="whereCondition">对象查询where限定条件，不包含where关键字</param>
        /// <param name="param">参数化查询的参数值对象</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示删除成功，false表示删除失败</returns>
        public bool Delete(string whereCondition, object param, IDbTransaction transaction = null)
        {
            var whereSql = GetWhereSql(whereCondition);
            string sql = $"delete from {_tableName} {whereSql}";
            return _connection.Execute(sql, param, transaction) > 0;
        }

        /// <summary>
        /// 更新数据库表中的实体对象
        /// </summary>
        /// <param name="entity">新的实体对象的值</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示更新成功，false表示更新失败</returns>
        public bool Update(T entity, IDbTransaction transaction = null)
        {
            return _connection.Update<T>(entity, transaction);
        }

        public long Execute(string text, object param)
        {
            return _connection.Execute(text, param);
        }


        /// <summary>
        /// 批量更新数据库表中的实体对象
        /// </summary>
        /// <param name="entities">新的实体对象值列表</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示更新成功，false表示更新失败</returns>
        public bool UpdateRange(IEnumerable<T> entities, IDbTransaction transaction = null)
        {
            return _connection.Update(entities, transaction);
        }

        /// <summary>
        /// 用指定的值更新数据库表中满足条件的行
        /// </summary>
        /// <param name="queryParam">对象查询条件对象，查询对象的属性为数据库表的列，属性的值为查询where限定条件,各个属性值的限定关系为and关系</param>
        /// <param name="setParam">更新列的值对象，对象的属性为数据库表的列，属性的值为要更新为的新值</param>
        /// <param name="transaction">数据库事务对象</param>
        /// <returns>true表示更新成功，false表示更新失败</returns>
        public bool Update(object queryParam, object setParam, IDbTransaction transaction = null)
        {
            var sbSetParam = new StringBuilder(null);
            int paramIndex = 0;
            IDictionary<string, object> param = new ExpandoObject();
            IDictionary<string, object> setParamDic = paramsFormat(setParam);
            foreach (KeyValuePair<string, object> keyValue in setParamDic)
            {
                var sqlParam = "P" + paramIndex;
                paramIndex++;
                param.Add(sqlParam, keyValue.Value);
                _sqlAdapter.AppendColumnNameEqualsValue(sbSetParam, keyValue.Key, sqlParam);
                sbSetParam.Append(",");
            }
            sbSetParam.Remove(sbSetParam.Length - 1, 1);
            IDictionary<string, object> whereParamDic = paramsFormat(queryParam);
            var sbWhereParam = new StringBuilder(null);
            foreach (KeyValuePair<string, object> keyValue in whereParamDic)
            {
                var sqlParam = "P" + paramIndex;
                paramIndex++;
                param.Add(sqlParam, keyValue.Value);
                _sqlAdapter.AppendColumnNameWithValue(sbWhereParam, keyValue.Key, sqlParam, keyValue.Value.GetType());
                sbWhereParam.Append(" and ");
            }
            sbWhereParam.Remove(sbWhereParam.Length - 4, 4);
            string sql = $"update {_tableName} set {sbSetParam} where {sbWhereParam}";
            var count = _connection.Execute(sql, param);
            return count > 0;
        }


        private IDictionary<string, object> paramsFormat(object obj)
        {
            Type type = obj.GetType();
            if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
            {

                var dics = obj as IDictionary<string, object>;
                return dics;
            }
            else
            {
                var setProperties = TypePropertiesCache(type);
                Dictionary<string, object> dics = new Dictionary<string, object>();
                for (var i = 0; i < setProperties.Count; i++)
                {
                    var property = setProperties[i];
                    dics.Add(property.Name, property.GetValue(obj));
                }
                return dics;
            }
        }


        /// <summary>
        /// 查询集合
        /// </summary>
        /// <param name="sql">执行sql</param>
        /// <param name="param">参数</param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public List<TQuery> Get<TQuery>(string sql, object param, IDbTransaction transaction = null)
        {
            return _connection.Query<TQuery>(sql, param, transaction).ToList();
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">执行sql</param>,
        /// <param name="param">参数</param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public TQuery GetOne<TQuery>(string sql, object param, IDbTransaction transaction = null)
        {
            return _connection.QuerySingle<TQuery>(sql, param, transaction);
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public IDbTransaction BeginTransaction()
        {
            var wasClosed = this._connection.State == ConnectionState.Closed;
            if (wasClosed) this._connection.Open();
            this._transaction = this._connection.BeginTransaction();
            return this._transaction;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            this._transaction.Commit();
            this._transaction.Dispose();
            this._connection.Close();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        /// <param name="tran"></param>
        public void Rollback()
        {
            this._transaction.Rollback();
            this._transaction.Dispose();
            this._connection.Close();
        }
    }
}
