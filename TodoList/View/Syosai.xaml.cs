using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace TodoList.View
{
    /// <summary>
    /// Syosai.xaml 的交互逻辑
    /// </summary>
    public partial class Syosai : Window
    {
        public List<ReceivePeople> receivePeopleList;
        public List<UpLoadFile> upLoadFiles;
        private Dictionary<string, string> ViewData = new Dictionary<string, string>();
      
        public Syosai(Message message,string userID)
        {
            InitializeComponent();
            string[] MsgSplit = message.MessageID.Split(',');
            ViewData["MessageID"] = MsgSplit[0];
            ViewData["MsgSeqNo"] = MsgSplit[1];
            ViewData["UserID"] = userID;
            Initialize(message);
        }
        public void Initialize(Message message)
        {

            grid_MsgTable.DataContext = message;
            dg_UserList.ItemsSource = ReceivePeopleList(message);
            sp_Over.DataContext = message;

        }
        public List<ReceivePeople>  ReceivePeopleList(Message message)
        {
            try
            {
 　     　　 AccessMdb mdb = new AccessMdb();
            mdb.Connect();
            string sql = "";
            sql+= "Select";
            sql+= " s.OpenedDate";
            sql+= ",s.LastRefData";
            sql+= ",s.StatsFlag";
            sql+= ",s.CompletionDate";
            sql+= ",u.UserName";
            sql+= ",c.Cmt";
            sql+= ",c.CreatDateTime";
            sql+= " From";
            sql+= "(";
            sql+= "T_SendDetails As s";
            sql+= " Left Join";
            sql+= " T_USER As u";
            sql+= " On s.UserID = u.UserID";
            sql+= ")";
            sql+= " Left Join";
            sql+= " T_MessageCmt As c";
            sql+= " On s.MessageID = c.MessageID";
            sql+= " And s.UserID = c.UserID";
            sql+= " Where s.MessageID='"+ ViewData["MessageID"] + "'";
            DataTable Datatb_ReceivePeople = mdb.ExecuteSql(sql);
            mdb.Disconnect();
           if(Datatb_ReceivePeople.Rows.Count == 0)
            {
                dg_UserList.Visibility = Visibility.Hidden;
                tb_warnning.Visibility = Visibility.Visible;
            }

            receivePeopleList = new List<ReceivePeople>();
            for (int i = 0; i < Datatb_ReceivePeople.Rows.Count; i++)
            {

             receivePeopleList.Add(
                 new ReceivePeople()
                 {
                     UserName= Datatb_ReceivePeople.Rows[i]["UserName"].NullToString()
                     ,
                     OpenedDateTime=Datatb_ReceivePeople.Rows[i]["OpenedDate"].NullToString()
                     ,
                     LastReDateTime=Datatb_ReceivePeople.Rows[i]["LastRefData"].NullToString()
                     ,
                     State=Datatb_ReceivePeople.Rows[i]["StatsFlag"].NullToString()
                     ,
                     CompletionDate=Datatb_ReceivePeople.Rows[i]["CompletionDate"].NullToString()
                     ,
                     CMTDateTime = Datatb_ReceivePeople.Rows[i]["CreatDateTime"].NullToString()

                 });
            }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }

            return receivePeopleList;
        }

        private void btn_addFile_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            OpenFileDialog openFile = new OpenFileDialog();
            upLoadFiles = new List<UpLoadFile>();
            try
            {

            if((bool)openFile.ShowDialog())
            {

                if (btn.Name == btn_addFile1.Name)
                {
                    if (openFile.OpenFile() != null)
                    {
                            upLoadFiles.Add ( new UpLoadFile() { FileName = openFile.FileName });
                            tb_file1.DataContext = upLoadFiles;
                            ViewData["File1"] = openFile.SafeFileName; 

                    }

                }
                else if (btn.Name == btn_addFile2.Name)
                {
                    if (openFile.OpenFile() != null)
                    {
                            upLoadFiles.Add(new UpLoadFile() { FileName = openFile.FileName });
                            tb_file2.DataContext = upLoadFiles;
                            ViewData["File2"] = openFile.SafeFileName;

                        }

                    }
                }
               
            }
            catch (Exception)
            {

                MessageBox.Show("エクスプローラーを起動できません！");
            }
        }

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            bool canUpdate = false;
            string[] files = new string[] { tb_file1.Text,tb_file2.Text };
            bool msgStatus = (bool)cb_CompletionStatus.IsChecked;
            string coment = txtArea_Conment.Text;
             List<string> msgBox = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {    
                 if (files[i] != string.Empty)
                 {
                    files[i] = ViewData["File" + (i + 1).ToString()].NullToString();
                 }
             }
            AccessMdb mdb = new AccessMdb();
            mdb.Connect();
            //更新要かどうかを判断する
            if (coment != string.Empty
                || files[0]  != string.Empty
                || files[1]  != string.Empty
                || msgStatus != cb_HiddenStatus.IsChecked)
            {
                canUpdate = true;
                if(msgStatus != cb_HiddenStatus.IsChecked)
                {
                    msgBox.Add("CompletionStatus");

                }
            }
            else
            {
                MessageBox.Show("送信できる内容を見つかれなかった！");
            }

            //コメント、完了状況、添付ファイルを更新します
            if (canUpdate)
            {
                DataTable dtb_Cmt = mdb.ExecuteSql("Select MessageID From T_MessageCmt Where MessageID='"+ViewData["MessageID"]+ "'" +
                    " And  UserID='"+ViewData["UserID"]+"'");
               if(dtb_Cmt.Rows.Count == 0)
                {
                //完了状況を更新します
                int statsFlag;
                if (cb_CompletionStatus.IsChecked == true)
                {
                    statsFlag = 1;
                }
                else
                {
                    statsFlag = 0;
                }
                mdb.ExecuteSql("Update T_SendDetails Set StatsFlag=" + statsFlag +
                    " Where MessageID='" + ViewData["MessageID"] + "'" +
                    " And  SeqNo=" +  ViewData["MsgSeqNo"] );
                //コメントを追加します
                     mdb.ExecuteSql("Insert Into T_MessageCmt (" +
                        "[MessageID]" +
                        ",[UserID]" +
                        ",[Cmt]" +
                        ",[AttachmentsFile1]" +
                        ",[AttachmentsFile2]" +
                        ",[CreatDateTime]" +
                        ",[UpdateDateTime]" +
                        ")"+
                        " Values" +
                        " (" +
                        "'" + ViewData["MessageID"].NullToString() + "'" +
                        ",'" + ViewData["UserID"].NullToString() + "'" +
                        ",'" + coment + "'" +
                        ",'" + files[0] + "'" +
                        ",'" + files[1] + "'" +
                        ",Now()" +
                        ",Now()" +
                        ")");
           　
                }
                else //コメントを更新します
                {
                    string sql = "";
                    sql += "Update T_MessageCmt Set";
                    sql += " UpdateDateTime=Now()";
                    if(coment != string.Empty)
                    {
                    sql += " And Cmt='"+coment+"'";
                        msgBox.Add("Cmt");
                    }
                    if (files[0] != string.Empty)
                    {
                    sql+=" And AttachmentsFile1='"+files[0]+"'";
                        msgBox.Add("File1");
                    }
                    if (files[1] != string.Empty)
                    {
                    sql+=" And AttachmentsFile2='"+files[1]+"'";
                        msgBox.Add("File2");
                    }
                    sql += " Where MessageID='"+ViewData["MessageID"]+"'";
                    sql += " And UserID='"+ViewData["UserID"]+"'";
                   
                    string showMsg = "";
                    for (int i = 0; i < msgBox.Count; i++)
                    {   
                         if(msgBox[i] == "CompletionStatus")
                        {
                            showMsg += "[完了状況]";
                        }
                         else if(msgBox[i] == "Cmt")
                        {
                            showMsg += "[コメント]";
                        }
                        else if(msgBox[i] =="File1")
                        {
                            showMsg += "[添付ファイル:1]";
                        }
                        else if(msgBox[i] == "File2")
                        {
                            showMsg += "[添付ファイル:2]";
                        }
                     
                    }
                     showMsg += " を更新しますか？";
                    if (MessageBoxResult.Yes ==
                        MessageBox.Show(showMsg, "",
                        MessageBoxButton.YesNo, MessageBoxImage.Information))
                    {
                        bool saveFlag;
                        try
                        {
                            mdb.ExecuteSql(sql);
                            saveFlag = true;
                        }
                        catch 
                        {

                            MessageBox.Show("保存エラーが発生しました！");
                            saveFlag = false;
                        }
                        if (saveFlag)
                        {
                            MessageBox.Show("送信が完了しました");
                        }
                         
                    }
                    else
                    {
                       mdb.Disconnect();

                    }

                }

            }

        mdb.Disconnect();


        }
        

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtArea_Conment.Text != string.Empty 
                    || tb_file1.Text != string.Empty 
                    || tb_file2.Text != string.Empty)
                {
                  if(MessageBoxResult.Yes==
                        MessageBox.Show("コメント内容が発見しました、このまま閉じましたらコメントを保存できない、また本ページを閉じますか？",
                        "",
                        MessageBoxButton.YesNo,MessageBoxImage.Information))
                    {
                        this.Close();
                    }
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception )
            {

            }
        }

        private void btn_Re_Click(object sender, RoutedEventArgs e)
        {
            Button btn_Re = (Button)sender;
            if(btn_Re.Name == btn_ReALL.Name)
            {

            }
            else if(btn_Re.Name == btn_ReSender.Name)
            {

            }
        }
    }
    public class ReceivePeople{
        public string  UserName { get; set; }
        public string  OpenedDateTime { get; set; }
        public string  LastReDateTime { get; set; }
        public string  State { get; set; }
        public string CompletionDate { get; set; }
        public string  CMTDateTime { get; set; }
    }
    public class UpLoadFile
    {
        public string FileName { get; set; }
     
    }
}