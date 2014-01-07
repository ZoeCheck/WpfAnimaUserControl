using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using System.Collections;
using System.Xml;

namespace DataAccess
{
    public class DbHelperSQL
    {
        public static string connectionString = ConfigSet.GetConfigValue("sqlconn");
        //string conn = ConfigurationSettings.AppSettings["sqconn"];

        #region 返回 DataTable

        public DataTable ReturnDataTable(string storedProcName, IDataParameter[] parameters)
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter sda = null;

                try
                {
                    conn.Open();
                    sda = new SqlDataAdapter();
                    sda.SelectCommand = BuildQueryCommand(conn, storedProcName, parameters);
                    sda.Fill(ds, "ds");
                    conn.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (sda != null)
                    {
                        sda.SelectCommand.Dispose();
                        sda.Dispose();
                    }
                    if (conn.State.ToString() != "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            if (ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region 返回 DataSet

        public DataSet ReturnDataSet(string storedProcName, IDataParameter[] parameters)
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter sda = null;

                try
                {
                    conn.Open();
                    sda = new SqlDataAdapter();
                    sda.SelectCommand = BuildQueryCommand(conn, storedProcName, parameters);
                    sda.Fill(ds, "ds");
                    conn.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (sda != null)
                    {
                        sda.SelectCommand.Dispose();
                        sda.Dispose();
                    }
                    if (conn.State.ToString() != "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            if (ds.Tables.Count > 0)
            {
                return ds;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Tmp

        #region 执行存储过程

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader RunProcedureByReader(string storedProcName, IDataParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataReader returnReader;
                try
                {
                    conn.Open();
                    SqlCommand command = BuildQueryCommand(conn, storedProcName, parameters);
                    command.CommandType = CommandType.StoredProcedure;
                    returnReader = command.ExecuteReader();
                    return returnReader;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn != null)
                    {
                        if (conn.State.ToString() == "Open")
                        {
                            conn.Close();
                        }
                        conn.Dispose();
                    }
                }
            }
        }

        #endregion

        #region 执行存储过程，返回影响的行数 (OK)

        /// <summary>
        /// 执行存储过程，返回影响的行数
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public Int64 RunProcedureByInt64(string storedProcName, IDataParameter[] parameters, out string ErrMsgString)
        {
            int rowsAffected = 0;

            ErrMsgString = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                Int64 result = 0;
                SqlCommand cmd = null;

                try
                {
                    conn.Open();
                    cmd = BuildIntCommand(conn, storedProcName, parameters);
                    rowsAffected = cmd.ExecuteNonQuery();
                    result = (Int64)cmd.Parameters["ReturnValue"].Value;

                    // 执行成功
                    ErrMsgString = "Succeeds";
                }
                catch (Exception ex)
                {
                    ErrMsgString = ex.Message;
                    rowsAffected = -1;
                }
                finally
                {
                    parameters = null;
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                    if (conn.State.ToString() == "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }

                return result;
            }
        }

        #endregion

        #region 执行存储过程，返回影响的行数 (OK)

        /// <summary>
        /// 执行存储过程，返回影响的行数
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedureByInt(string storedProcName, IDataParameter[] parameters, out string ErrMsgString)
        {
            int rowsAffected = 0;

            ErrMsgString = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                int result = 0;
                SqlCommand cmd = null;

                try
                {
                    conn.Open();
                    cmd = BuildIntCommand(conn, storedProcName, parameters);
                    rowsAffected = cmd.ExecuteNonQuery();
                    result = (int)cmd.Parameters["ReturnValue"].Value;

                    // 执行成功
                    ErrMsgString = "Succeeds";
                }
                catch (Exception ex)
                {
                    ErrMsgString = ex.Message;
                    rowsAffected = -1;
                }
                finally
                {
                    parameters = null;
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                    if (conn.State.ToString() == "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }

                return result;
            }
        }

        #endregion

        #region 执行存储过程，并获取返回值 (OK)

        /// <summary>
        /// 执行存储过程，并获取返回值
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedureByInt(string storedProcName, out string ErrMsgString)
        {
            int result = 0;

            ErrMsgString = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = null;
                SqlParameter Param = null;

                try
                {
                    conn.Open();

                    cmd = new SqlCommand(storedProcName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    Param = new SqlParameter("@RC", SqlDbType.Int);
                    Param.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(Param);

                    cmd.ExecuteNonQuery();

                    ErrMsgString = "Succeeds";

                    result = (int)Param.Value;
                }
                catch (Exception ex)
                {
                    ErrMsgString = ex.Message;
                    ex = null;
                }
                finally
                {
                    Param = null;
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                    if (conn.State.ToString() != "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            return result;
        }

        #endregion

        #region 执行存储过程，并返回DataSet (OK)

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedureByDataSet(string storedProcName, string tableName, out string ErrMsgString)
        {
            ErrMsgString = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter sda = null;
                try
                {
                    DataSet dataSet = new DataSet();

                    conn.Open();

                    sda = new SqlDataAdapter();
                    sda.SelectCommand = BuildQueryCommand(conn, storedProcName);
                    sda.Fill(dataSet, tableName);

                    conn.Close();
                    ErrMsgString = "Succeeds";

                    return dataSet;
                }
                catch (Exception ex)
                {
                    ErrMsgString = ex.Message;
                    //	em = ErrInfo.BuildErrMsg(ex);
                    ex = null;
                }
                finally
                {
                    if (sda != null)
                    {
                        sda.SelectCommand.Dispose();
                        sda.Dispose();
                    }
                    if (conn.State.ToString() != "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            return null;
        }

        #endregion

        #region 执行存储过程，并返回DataSet (OK)

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedureByDataSet(string storedProcName, string tableName, IDataParameter[] parameters,
                                                    out string ErrMsgString)
        {
            ErrMsgString = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                SqlDataAdapter sda = null;

                try
                {
                    conn.Open();
                    sda = new SqlDataAdapter();
                    sda.SelectCommand = BuildQueryCommand(conn, storedProcName, parameters);
                    sda.Fill(ds, tableName);
                    conn.Close();

                    ErrMsgString = "Succeeds";


                    return ds;
                }
                catch (Exception ex)
                {
                    //	em = ErrInfo.BuildErrMsg(ex);
                    ErrMsgString = ex.Message;
                    ex = null;
                }
                finally
                {
                    if (sda != null)
                    {
                        sda.SelectCommand.Dispose();
                        sda.Dispose();
                    }
                    if (conn.State.ToString() != "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            return null;
        }

        #endregion

        #region 执行存储过程，并获取返回值 (OK)

        /// <summary>
        /// 执行存储过程，并获取返回值
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public string RunProcedureReturnString(string storedProcName, IDataParameter[] parameters,
                                                      out string ErrMsgString)
        {
            string result = string.Empty;

            ErrMsgString = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = null;
                SqlParameter Param = null;

                try
                {
                    conn.Open();

                    cmd = BuildQueryCommand(conn, storedProcName, parameters);

                    Param = new SqlParameter("@RC", SqlDbType.Int);
                    Param.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(Param);

                    result = cmd.ExecuteScalar().ToString();

                    ErrMsgString = "Succeeds";

                    return result;
                }
                catch (Exception ex)
                {
                    ErrMsgString = ex.Message;
                    ex = null;
                }
                finally
                {
                    Param = null;
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                    if (conn.State.ToString() != "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// 执行存储过程，并获取返回值
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedureReturnInt(string storedProcName, IDataParameter[] parameters, out string ErrMsgString)
        {
            int result = -1;

            ErrMsgString = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = null;
                SqlParameter Param = null;

                try
                {
                    conn.Open();

                    cmd = BuildQueryCommand(conn, storedProcName, parameters);

                    Param = new SqlParameter("@RC", SqlDbType.Int);
                    Param.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(Param);

                    result = int.Parse(cmd.ExecuteScalar().ToString());

                    ErrMsgString = "Succeeds";

                    return result;
                }
                catch (Exception ex)
                {
                    ErrMsgString = ex.Message;
                    ex = null;
                }
                finally
                {
                    Param = null;
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                    if (conn.State.ToString() != "Open")
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            return result;
        }

        #region 公用方法

        public int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }

        public bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool Exists(string strSql, params SqlParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        cmd.CommandTimeout = 0;
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        public int ExecuteSqlByTime(string SQLString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public void ExecuteSqlTran(ArrayList SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (SqlException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
            }
        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString, string content)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(SQLString, connection);
                SqlParameter myParameter = new SqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public object ExecuteSqlGet(string SQLString, string content)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(SQLString, connection);
                cmd.CommandTimeout = 0;
                SqlParameter myParameter = new SqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    object obj = cmd.ExecuteScalar();
                    if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(strSQL, connection);
                cmd.CommandTimeout = 0;
                SqlParameter myParameter = new SqlParameter("@fs", SqlDbType.Image);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        cmd.CommandTimeout = 0;
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回SqlDataReader
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string strSQL)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(strSQL, connection);
            try
            {
                cmd.CommandTimeout = 0;
                connection.Open();
                SqlDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
            //			finally
            //			{
            //				cmd.Dispose();
            //				connection.Close();
            //			}	
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                    command.SelectCommand.CommandTimeout = 0;
                    command.Fill(ds, "ds");
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }

        public DataSet Query(string SQLString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                    command.SelectCommand.CommandTimeout = Times;
                    command.Fill(ds, "ds");
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }

        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (SqlException E)
                    {
                        throw new Exception(E.Message);
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();

                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回SqlDataReader
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (SqlException e)
            {
                throw new Exception(e.Message);
            }
            //			finally
            //			{
            //				cmd.Dispose();
            //				connection.Close();
            //			}	
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }


        private void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText,
                                           SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text; //cmdType;
            if (cmdParms != null)
            {
                foreach (SqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataReader returnReader;
                connection.Open();
                SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
                command.CommandType = CommandType.StoredProcedure;
                returnReader = command.ExecuteReader();
                return returnReader;
            }
        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }

        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.SelectCommand.CommandTimeout = Times;
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }


        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName,
                                                    IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandTimeout = 0;
            command.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                foreach (SqlParameter parameter in parameters)
                {
                    if (parameter != null)
                    {
                        // 检查未分配值的输出参数,将其分配以DBNull.Value.
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }

            return command;
        }

        /// <summary>
        /// 执行存储过程，返回影响的行数		
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                int result = 0;
                SqlCommand cmd = null;

                try
                {
                    conn.Open();
                    cmd = BuildIntCommand(conn, storedProcName, parameters);
                    rowsAffected = cmd.ExecuteNonQuery();
                    result = (int)cmd.Parameters["ReturnValue"].Value;
                }
                catch (Exception ex)
                {
                    ex = null;
                    rowsAffected = -1;
                }
                finally
                {
                    parameters = null;
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();
                }

                return result;
            }
        }

        #region 执行存储过程，并获取返回整型值

        /// <summary>
        /// 执行存储过程，并获取返回值
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedure(string storedProcName)
        {
            int result = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = null;
                SqlParameter Param = null;

                try
                {
                    conn.Open();

                    cmd = new SqlCommand(storedProcName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    Param = new SqlParameter("@RC", SqlDbType.Int);
                    Param.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(Param);

                    cmd.ExecuteNonQuery();

                    result = (int)Param.Value;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Param = null;
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <returns>SqlCommand</returns>
        private SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;


            return command;
        }

        /// <summary>
        /// 创建 SqlCommand 对象实例(用来返回一个整数值)	
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand 对象实例</returns>
        private SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue",
                                                    SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                                                    false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        /// <summary>
        /// 执行一个存储过程，返回一个DataSet记录集
        /// </summary>
        /// <param name="procName">存储过程的名字</param>
        /// <param name="sqlParmeters">存储过程的参数</param>
        /// <returns>DataSet表的记录集</returns>
        public DataSet GetDataSet(string procName, SqlParameter[] sqlParmeters)
        {
            //打开一个连接
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    sqlConn.Open();
                    DataSet ds = new DataSet();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
                    sqlDataAdapter.SelectCommand = BuildSqlCommand(sqlConn, procName, sqlParmeters);
                    sqlDataAdapter.Fill(ds);
                    sqlDataAdapter = null;
                    sqlConn.Close();
                    return ds;
                }
            }
            catch (System.Data.SqlClient.SqlException sqlex)
            {
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 构造一个SqlCommand对象
        /// </summary>
        /// <param name="procName">过程名称</param>
        /// <param name="sqlParmeters">参数内容</param>
        /// <returns>SqlCommand</returns>
        private SqlCommand BuildSqlCommand(SqlConnection sqlConn, string procName, SqlParameter[] sqlParmeters)
        {
            SqlCommand sqlComm = new SqlCommand(procName, sqlConn);
            sqlComm.CommandType = CommandType.StoredProcedure;
            if (sqlParmeters != null)
            {
                foreach (SqlParameter sqlParameter in sqlParmeters)
                {
                    sqlComm.Parameters.Add(sqlParameter);
                }
            }
            return sqlComm;
        }
        #endregion

        #endregion
    }
}
