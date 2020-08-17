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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using TodoList.Common;
using TodoList.View;

namespace TodoList
{
    /// <summary>
    /// List.xaml の相互作用ロジック
    /// </summary>
    public partial class List : Window
    {
        public List<Message> messageList;
        public List<User> userList;
        public Dictionary<string, string> ViewData = new Dictionary<string, string>() { };
        public List(string userID)
        {
            ViewData["TableStatus"] = "";
            InitializeComponent();  
            Initialize(userID);
      

        } 
        public  void Initialize(string userID)
        {
            _UserID.Text = userID;
            dg_List.ItemsSource = MessageList("");
            tvItem_All.IsSelected = true;
            datagrid_UserList.ItemsSource = UserList();
            if(_UserID.Text  != string.Empty)
            {
                AccessMdb mdb = new AccessMdb();
                mdb.Connect();
                DataTable userTable = mdb.ExecuteSql("Select * From T_USER Where UserID='" + _UserID.Text + "'");
                mdb.Disconnect();
                if (userTable.Rows.Count != 0)
                {
                    if (userTable.Rows[0]["AuthorityFlag"].ToString() == "0")
                    {
                        tvItem_user.IsEnabled = false;
                        Color color = (Color)ColorConverter.ConvertFromString("Gray");
                        tvItem_user.Foreground = new SolidColorBrush(color);
                    }
                    else if (userTable.Rows[0]["AuthorityFlag"].ToString() == "9")
                    {
                        tvItem_user.IsEnabled = true;
                        Color color = (Color)ColorConverter.ConvertFromString("#4db6e9");
                        tvItem_user.Foreground = new SolidColorBrush(color);
                    }
                }
                else
                {
                    MessageBox.Show("エラーが発生しました！もう一度ログインしてください！");
            
                    this.Close();

                }
            }
            else
            {
                MessageBox.Show("エラーが発生しました！もう一度ログインしてください！");
                this.Close();
            }
           

        }
        public List<Message> MessageList(string addsql)
        {
            float openRate;
            bool personalStats;
            AccessMdb mdb = new AccessMdb();
            mdb.Connect();

            string sql = "";
            sql += " Select ";
            sql += "    m.ID";
            sql += "  , m.SeqNo";
            sql += "  , m.ActiveFlag  , m.MsgTypeCode";
            sql += "  , m.DisplayFlag";
            sql += "  , m.AttributeCode";
            sql += "  , m.SendUserID";
            sql += "  , m.Title";
            sql += "  , m.Context";
            sql += "  , m.LimitFlag";
            sql += "  , m.LimitData";
            sql += "  , m.AttachmentsFile1";
            sql += "  , m.AttachmentsFile2";
            sql += "  , m.AttachmentsFile3";
            sql += "  , m.AttachmentsFile4";
            sql += "  , m.AttachmentsFile5";
            sql += "  , m.PostStartDate";
            sql += "  , m.SendPeople";
            sql += "  , m.OpenedPeople";
            sql += "  , m.CreatDateTime";
            sql += "  , m.UpdateDateTime";
            sql += "  , m.DisplayFlag";
            sql += "  , u.UserName as SendUserName";
            sql += "  , s.UserID , s.OpenedDate , s.LastRefData , s.StatsFlag , s.CompletionDate  ";
            sql += " FROM";
            sql += "  ( ";
            sql += "      T_MessageList m  LEFT JOIN T_USER u   ON m.SendUserID = u.UserID   ";
            sql += "  ) ";
            sql += "  Left Join T_SendDetails s   ON m.ID = s.MessageID    AND m.SeqNo = s.SeqNo ";
            sql += " WHERE  m.ActiveFlag = 1  ";
            sql += " And  s.UserID='"+_UserID.Text+"'";
            //messagelistのテーブル
            DataTable msgTable = mdb.ExecuteSql(sql + addsql);
            //部署のテーブル
            DataTable msgType = mdb.ExecuteSql("Select * from  M_MessageType  Order By   DisplayNo ");
            Dictionary<string, string> codeName = new Dictionary<string, string>() { };
            for (int i = 0; i < msgType.Rows.Count; i++)
            {
                codeName.Add(msgType.Rows[i]["Code"].ToString(), msgType.Rows[i]["Name2"].ToString());
            }


            mdb.Disconnect();
            messageList = new List<Message>();
            for (int i = 0; i < msgTable.Rows.Count; i++)
            {
                string typeCode = msgTable.Rows[i]["MsgTypeCode"].ToString();
                Label lb1 = new Label(); lb1.Content = $"[{codeName[typeCode]}]";
                lb1.HorizontalContentAlignment = HorizontalAlignment.Center;
                 string attributeCode = msgTable.Rows[i]["AttributeCode"].NullToString();
                List<Dictionary<string, string>> attrNames = new List<Dictionary<string, string>>();
                if (typeCode != "" || attributeCode != "")
                {
                    List<string> attributeCodes = attributeCode.Split(',').ToList();
                    if (msgType != null && msgType.Rows.Count > 0)
                    {
                        for (int j = 0; j < msgType.Rows.Count; j++)
                        {

                            foreach (string attr in attributeCodes)
                            {
                                if (attr == msgType.Rows[j]["Code"].NullToString())
                                {
                                    Dictionary<string, string> item = new Dictionary<string, string>();
                                    item["Name"] = msgType.Rows[j]["Name2"].NullToString();
                                    item["Color"] = msgType.Rows[j]["Color"].NullToString();
                                    attrNames.Add(item);
                                }
                            }
                        }
                    }
                }

                List<Label> labels = new List<Label>();
                labels.Add(lb1);
                foreach (Dictionary<string, string> attr in attrNames)
                {
                    Label lb = new Label();
                    if (attr["Color"] != "")
                    {
                        lb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(attr["Color"]));
                    }
                    lb.Content = $"[{attr["Name"].NullToString()}]";
                    labels.Add(lb);
                }
                Label lb_limitDate = new Label();

                if (msgTable.Rows[i]["LimitData"].NullToString() == string.Empty)
                {
                    lb_limitDate.Content = "未設定";
                }
                else
                {
                    lb_limitDate.Content = msgTable.Rows[i]["LimitData"].NullToString();
                }
                if (msgTable.Rows[i]["LimitData"].NullToString() != string.Empty &&
                   DateTime.Compare(DateTime.Now, DateTime.Parse(lb_limitDate.Content.NullToString())) > 0)
                {
                    lb_limitDate.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC143C"));
                }
                Label lb_refDt = new Label();
                List<Label> lbl_refDt = new List<Label>();
                for (int j = 1; j <= 5; j++)
                {

                    if (msgTable.Rows[i]["AttachmentsFile" + j].ToString() != string.Empty)
                    {
                        lb_refDt = new Label();
                        lb_refDt.Content = msgTable.Rows[i]["AttachmentsFile" + j].NullToString();
                        lbl_refDt.Add(lb_refDt);
                    }
                }
                //開封率
                if ((int)msgTable.Rows[i]["SendPeople"] > 0 && (int)msgTable.Rows[i]["OpenedPeople"] > 0)
                {
                    float sendPeople_num = int.Parse(msgTable.Rows[i]["SendPeople"].ToString());
                    float openedPeople_num = int.Parse(msgTable.Rows[i]["OpenedPeople"].ToString());
                     openRate = openedPeople_num / sendPeople_num * 100;
                }
                else
                {
                    openRate =   0 ;
                }
                if(msgTable.Rows[i]["StatsFlag"].ToString() == "1")
                {
                    personalStats = true;
                }
                else
                {
                    personalStats = false;
                }


                messageList.Add(new Message()
                {
                    MessageID = msgTable.Rows[i]["ID"].NullToString() + "," + msgTable.Rows[i]["SeqNo"].NullToString()
                    ,
                    Type = labels
                    ,
                    State = msgTable.Rows[i]["OpenedPeople"].NullToString()+"/"+msgTable.Rows[i]["SendPeople"].NullToString() +"\n"+"("+ openRate + "%)"
                    ,
                    SendUserName = msgTable.Rows[i]["SendUserName"].NullToString()
                    ,
                    Title = msgTable.Rows[i]["Title"].NullToString()
                    ,
                    CreatDateTime = msgTable.Rows[i]["CreatDateTime"].NullToString()
                    ,
                    OpenedDateTime = msgTable.Rows[i]["OpenedDate"].NullToString()
                     ,
                    Cmt = msgTable.Rows[i]["CreatDateTime"].NullToString()
                    ,
                    LimitDateTime = new List<Label>() { lb_limitDate }
                    ,
                    LastRefDateTime = msgTable.Rows[i]["LastRefData"].NullToString()
                    ,
                    AttachmentsFiles = lbl_refDt
                    ,
                    Context = msgTable.Rows[i]["Context"].NullToString()
                    ,
                    PersonalStats = personalStats
                });

            }
            if (messageList.Count == 0)
            {
                dg_List.Visibility = Visibility.Hidden;
                lb_warning.Visibility = Visibility.Visible;
            }
            else
            {
                dg_List.Visibility = Visibility.Visible;
                lb_warning.Visibility = Visibility.Hidden;
            }
            return messageList;
        }
        public List<User> UserList ()
        {
            AccessMdb mdb = new AccessMdb();
            mdb.Connect();
            DataTable userTable = mdb.ExecuteSql("Select * From T_USER");
            DataTable department = mdb.ExecuteSql("Select * From M_Department");
            mdb.Disconnect();
            Dictionary<string, string> departmentList = new Dictionary<string, string>();
            for (int i = 0; i < department.Rows.Count; i++)
            {   
               departmentList.Add( department.Rows[i]["DeptCode"].ToString(),department.Rows[i]["DeptName"].ToString());
            }



            userList = new List<User>();

             
            for (int i = 0; i < userTable.Rows.Count; i++)
            {
                List<Label> lb_DeptNameList = new List<Label>() { };
                string[] splitStrDe = userTable.Rows[i]["DepartmentCode"].NullToString().Split(',');
                if(departmentList.Count != 0)
                {
                   for (int j = 0; j < splitStrDe.Length; j++)
                   {
                        Label lb_departMentName = new Label();
                        lb_departMentName.HorizontalAlignment = HorizontalAlignment.Center;
                        lb_departMentName.Content = departmentList[splitStrDe[j]];
                        lb_DeptNameList.Add(lb_departMentName);
                     }
                }
                string authority = userTable.Rows[i]["AuthorityFlag"].ToString(); 
                if(authority == "9")
                {
                    authority = "管理者";
                }
                 else if(authority == "0")
                {
                    authority = "_";
                }
                userList.Add(new User() { 
                      UserID = userTable.Rows[i]["UserID"].NullToString()
                      ,
                      UserName = userTable.Rows[i]["UserName"].NullToString()
                      ,
                      DepartmentName = lb_DeptNameList
                      ,
                      Authority= authority 
                      ,
                      CreatDateTime = userTable.Rows[i]["CreatDateTime"].NullToString()
                      ,
                      UpdateDateTime = userTable.Rows[i]["UpdateDateTime"].NullToString()
            });

            }
            return userList;


        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string btnName = btn.Name;
            string addsql = "";
             if(btnName == btn_Search.Name)
            {
                if(tb_title.Text != string.Empty)
                {
                 addsql += " And Title Like'%" + tb_title.Text + "%'";
                }
                if(tb_context.Text!= string.Empty)
                {
                    addsql += " And Context Like'%" + tb_context.Text + "%'";

                }
                 if(tb_sender.Text != string.Empty)
                {
                    addsql += " And SendUserID Like'%" + tb_sender.Text + "%'";

                }
             dg_List.ItemsSource = MessageList(addsql);
            }
             if(btnName == btnLogout.Name)
             {   
                if(MessageBoxResult.Yes ==
                 MessageBox.Show("ログアウトしますか？", "", 
                 MessageBoxButton.YesNo, MessageBoxImage.Information ))
                {
                    Login lgWindow = new Login();
                    lgWindow.Show();
                    this.Close();
                }
                else
                {   
                     return;
                }
            }
        }
        private void dg_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            if(dg.Name == dg_List.Name)
            {
               Message  selectedMessage = (Message)dg.SelectedItem;
               if((Message)dg.SelectedItem != null)
               {   
                  if(ViewData["TableStatus"] == "Send")
                    {
                        Edit editWindow = new Edit("Edit", selectedMessage);
                        editWindow.ShowDialog();
                        editWindow.Owner = this; 
                    }
                    else if(ViewData["TableStatus"] == "Recipient" )
                    {

                    Syosai sywindow = new Syosai(selectedMessage ,_UserID.Text);
                    sywindow.ShowDialog();
                    sywindow.Owner = this;
                    }

                }
            }
           if(dg.Name == datagrid_UserList.Name)
           {
               User selectedUser= (User)datagrid_UserList.SelectedItem;
               if((User)datagrid_UserList.SelectedItem != null)
               {
                   MessageBox.Show(selectedUser.UserID.ToString());
               }
           }
        }
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            //DataGridの切り替え
            grid_MsgList.Visibility = Visibility.Visible;
            datagrid_UserList.Visibility = Visibility.Hidden; 
            TreeViewItem item = (TreeViewItem)sender;
            //DataGrid dguser = (DataGrid)grid_Container.FindName("datagrid_UserList");
            //if (dguser != null)
            //{
            // grid_Container.Children.Remove(dguser);
            // grid_Container.UnregisterName("datagrid_UserList");
            //}
            //foreach ( TreeViewItem   i in  tvItem_js.Items)
            //{
            //       if(i.IsSelected)
            //    {
            //        dgtc_SendUserName.Visibility = Visibility.Visible;
            //        dgtc_OpenedDateTime.Visibility = Visibility.Visible;
            //    }
            //}
            //foreach (TreeViewItem i in tvItem_ss.Items)
            //{
            //    if(i.IsSelected)
            //    {
            //        dgtc_SendUserName.Visibility = Visibility.Hidden;
            //        dgtc_OpenedDateTime.Visibility = Visibility.Hidden;

            //    }
            //}
            if(tvItem_js == item.Parent)
            {
                ViewData["TableStatus"] = "Send";
            }
            if(tvItem_ss == item.Parent)
            {
                ViewData["TableStatus"] = "Recipient";

            }
            if (tvItem_New.Header == item.Header)
            {
                Edit editWindow = new Edit("New" , new Message());
                editWindow.Show();
            }
              else  if (tvItem_notFinished.Header == item.Header)
            {
                dg_List.ItemsSource = MessageList("And s.UserID='"+ _UserID.Text+ "' And StatsFlag=0");
            }
            else if(tvItem_overdue.Header == item.Header)
            {
                dg_List.ItemsSource = MessageList("And s.UserID='" + _UserID.Text + "' And m.LimitData < Now() ");
            }
            else if(tvItem_All.Header == item.Header)
            {
                dg_List.ItemsSource = MessageList("And s.UserID='" + _UserID.Text + "'");
            }
            else if(tvItem_keep.Header ==item.Header)
            {
                dg_List.ItemsSource = MessageList("And SendUserID='" + _UserID.Text + "' And DisplayFlag=0");
                
            }
            else if(tvItem_sent.Header == item.Header)
            {
                dg_List.ItemsSource = MessageList("And SendUserID='" + _UserID.Text + "' And DisplayFlag=1");
               
            }
            else if(tvItem_user.Header == item.Header)
            {  
                 grid_MsgList.Visibility = Visibility.Hidden;
                datagrid_UserList.Visibility = Visibility.Visible;

                //if(grid_Container.FindName("datagrid_UserList") as DataGrid ==null)
                //{
                //    DataGrid dataGrid = new DataGrid();
                //    DataGridTemplateColumn dgTempC = new DataGridTemplateColumn();
                //    dgTempC.Header = "所属";
                //    DataTemplate dataTemp = new DataTemplate();
                //    var Item = new FrameworkElementFactory(typeof(ItemsControl));
                //    Item.SetBinding(ItemsControl.ItemsSourceProperty,new Binding("DepartmentName"));
                //    Item.SetValue(ItemsControl.HorizontalAlignmentProperty,HorizontalAlignment.Center); 
                //    dataTemp.VisualTree = Item;
                //    dgTempC.CellTemplate = dataTemp;
                //    dataGrid.Columns.Add(new DataGridTextColumn() {  Binding=new Binding("UserID"),Header="利用者ID",Width=100});
                //    dataGrid.Columns.Add(new DataGridTextColumn() {  Binding=new Binding("UserName"),Header="利用者名", Width = 100 });
                //    dataGrid.Columns.Add(dgTempC);                   
                //    dataGrid.Columns.Add(new DataGridTextColumn() {  Binding=new Binding("Authority"),Header="権限", Width = 100 });
                //    dataGrid.Columns.Add(new DataGridTextColumn() {  Binding=new Binding("CreatDateTime"),Header="作成日", Width = 100 });
                //    dataGrid.Columns.Add(new DataGridTextColumn() {  Binding=new Binding("UpdateDateTime"),Header="更新日", Width = 100 });
                //    dataGrid.SetValue(Grid.ColumnProperty, 1);
                //    dataGrid.SetValue(Grid.RowProperty, 0);
                //    //dataGrid.ItemsSource = UserList();
                //    dataGrid.HorizontalAlignment = HorizontalAlignment.Center;
                //    grid_Container.RegisterName("datagrid_UserList", dataGrid);
                //    grid_Container.Children.Add(dataGrid);
                //}
            }

        }
    }
    public class Message
    {
        public string MessageID { get; set; }
        public List<Label> Type { get; set; }
        public string State { get; set; }
        public bool PersonalStats { get; set; }
        public string Title { get; set; }
        public string SendUserName { get; set; }
        public string CreatDateTime { get; set; }
        public string OpenedDateTime { get; set; }
        public string Cmt { get; set; }
        public List<Label> LimitDateTime { get; set; }
        public List<Label> AttachmentsFiles { get; set; }
        public string LastRefDateTime { get; set; }
        public string Context { get; set; }
    }
    public class User {
        public string  UserID { get; set; }
        public string  UserName { get; set; }
        public List<Label> DepartmentName { get; set; }
        public string Authority { get; set; }
        public string CreatDateTime { get; set; }
        public string UpdateDateTime { get; set; }
    }

}


