using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TodoList.Common;

namespace TodoList
{
    /// <summary>
    /// Login.xaml の相互作用ロジック
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            this.txtID.Focus();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            #region 変数
            #endregion

            #region 主処理
            try
            {
                UserLogin();
            }
            #endregion

            #region　エラー
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion

            #region　後処理
            finally
            {
            }
            #endregion
        }

        private void txtPW_KeyDown(object sender, KeyEventArgs e)
        {
            #region 変数

            #endregion

            #region 主処理
            try
            {
                if (e.Key == Key.Return)
                {
                    UserLogin();
                }
            }
            #endregion

            #region　エラー
            catch (Exception ex)
            {

            }
            #endregion

            #region　後処理
            finally
            {
            }
            #endregion
        }


        private void UserLogin()
        {
            #region 変数
            AccessMdb mdb = new AccessMdb();
            DataTable userTable = new DataTable();
            #endregion

            #region 主処理
            try
            {
                lblErrorMsg.Visibility = Visibility.Collapsed;
                //入力チェック
                if (this.txtID.Text.NullToString().TrimEnd() == "" || this.txtPW.Password.NullToString().TrimEnd() == "")
                {
                    lblErrorMsg.Visibility = Visibility.Visible;
                    lblErrorMsg.Content = "ログインIDあるいはパスワードを入力してください。";
                    return ;
                }

                mdb.Connect();

                userTable = mdb.ExecuteSql("Select * From T_USER Where UserID='" + txtID.Text + "' And PassWord='" + AESCryption.Encrypt(txtPW.Password.NullToString().TrimEnd()) + "'");
                mdb.Disconnect();
                if (userTable.Rows.Count == 0)
                {
                    lblErrorMsg.Visibility = Visibility.Visible;
                    lblErrorMsg.Content = "ログインIDまたはパスワードが不正です。 ";
                }
                else
                {
                    lblErrorMsg.Visibility = Visibility.Hidden;
                    List lwindow = new List(userTable.Rows[0]["UserID"].NullToString());
                    lwindow.lb_UserID.Content = userTable.Rows[0]["UserName"].NullToString() + " さん";
                    lwindow.Show();
                    this.Close();
                }
            }
            #endregion

            #region　エラー
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion

            #region　後処理
            finally
            {
                mdb.Disconnect();
                mdb = null;
                if (userTable != null)
                {
                    userTable.Dispose();
                    userTable = null;
                }
            }
            #endregion
        }
    }
}
