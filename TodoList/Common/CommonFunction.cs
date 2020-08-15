using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoList.Common
{
     public static class  CommonFunction
    {
        public static Dictionary<string, string> GetUserInfo(string id)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            AccessMdb mdb = new AccessMdb();
            mdb.Connect();
            DataTable user = mdb.ExecuteSql("SELECT * FROM T_USER Where ID = " + id);
            DataTable dept = mdb.ExecuteSql("SELECT * FROM M_Department ");

            mdb.Disconnect();

            if (user != null && user.Rows.Count > 0)
            {
                ret["UserID"] = user.Rows[0]["UserID"].ToString();
                ret["Mail"] = user.Rows[0]["Mail"].ToString();
                ret["UserName"] = user.Rows[0]["UserName"].ToString();
                ret["DepartmentCode"] = user.Rows[0]["DepartmentCode"].ToString();
                List<string> deptCodes = user.Rows[0]["DepartmentCode"].NullToString().Split(',').ToList();
                if (dept != null && dept.Rows.Count > 0 && deptCodes != null && deptCodes[0] != "")
                {
                    for (int i = 0; i < dept.Rows.Count; i++)
                    {
                        if (dept.Rows[i]["DeptCode"].NullToString() == deptCodes[0])
                        {
                            ret["DeptName"] = dept.Rows[i]["DeptName"].NullToString();
                            ret["DeptName2"] = dept.Rows[i]["DeptName2"].NullToString();
                            break;
                        }
                    }
                }
            }
            return ret;
        }
        public static string NullToString<T>(this T obj)
        {
            if (obj == null)
            {
                return "";
            }
            else
            {
                return obj.ToString();
            }
        }
    }
}
