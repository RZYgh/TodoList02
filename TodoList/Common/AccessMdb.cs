using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Configuration;

namespace TodoList.Common
{
    public class AccessMdb
    {
        /// <summary>
        /// SQLコネクション
        /// </summary>
        private OleDbConnection _con = null;

        /// <summary>
        /// トランザクション・オブジェクト
        /// </summary>
        /// <remarks></remarks>
        private OleDbTransaction _trn = null;

        /// <summary>
        /// DB接続
        /// </summary>
        /// <param name="dsn">データソース名</param>
        /// <param name="tot">タイムアウト値</param>
        /// <remarks></remarks>
        public void Connect()
        {

            try
            {
                if (_con == null)
                {
                    _con = new OleDbConnection();
                }


                _con.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];

                _con.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Connect Error", ex);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_con != null)
                {
                    _con.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Disconnect Error", ex);
            }
            }

        /// <summary>
        /// SQLの実行
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="tot">タイムアウト値</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable ExecuteSql(String sql, int tot = -1)
        {
            DataTable dt = new DataTable();

            try
            {
                OleDbCommand sqlCommand = new OleDbCommand(sql, _con, _trn);

                if (tot > -1)
                {
                    sqlCommand.CommandTimeout = tot;
                }

                OleDbDataAdapter adapter = new OleDbDataAdapter(sqlCommand);

                adapter.Fill(dt);
                adapter.Dispose();
                sqlCommand.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteSql Error", ex);
            }

            return dt;
            }

        public void BeginTransaction()
        {
            try
            {
                _trn = _con.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception("BeginTransaction Error", ex);
            }
        }


        public void CommitTransaction()
        {
            try
            {
                if (_trn != null)
                {
                    _trn.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CommitTransaction Error", ex);
            }
            finally
            {
                _trn = null;
            }
        }


        public void RollbackTransaction()
        {
            try
            {
                if (_trn != null)
                {
                    _trn.Rollback();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("RollbackTransaction Error", ex);
            }
            finally
            {
                _trn = null;
            }
        }

        ~AccessMdb()
        {
            Disconnect();
        }
    }
}