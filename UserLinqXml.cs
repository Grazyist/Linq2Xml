using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
   /// <summary>
   /// 用来对xml用户配置文件进行操作
   /// </summary>
    class UserLinqXml
    {
       
        /// <summary>
        /// 创建一个xml文档,默认1项，用户名为Admin 密码为123456, 年龄为999
        /// </summary>
        public void CreateUserXml(string xmlPath)
        {
            XDocument xDocu = new XDocument(
                new XDeclaration("2.0","UTF-8","yes"),
                new XComment("存储用户登录信息"),
                new XElement("Users", new XElement("User",new XElement("UserName","Admin"),new XElement("Password","123456"),new XElement("Age","999")))
                );
            xDocu.Save(xmlPath);
        }

        /// <summary>
        /// 向xml文档中增添单条数据
        /// </summary>
        /// <param name="user"></param>
        public void AddUser(UserInfo user,string xmlPath)
        {
            try
            {
                if (user == null) throw new Exception("User 数据为空");
                if (user.UserName.Length == 0 || user.Password.Length == 0 || user.age < 0) throw new Exception("用户信息数据异常");
                if (!File.Exists(xmlPath)) throw new Exception("未发现配置文件，可能文件已丢失");

                XDocument xdoc = XDocument.Load(xmlPath);
                //用户名存在？不追加：追加
                if(!UserExist(user.UserName,xmlPath))
                { 
                    xdoc.Root.Add(
                    new XElement("User",new XElement("UserName",user.UserName), new XElement("Password",user.Password), new XElement("Age",user.age)));
                    xdoc.Save(xmlPath);
                }
               
            }catch(Exception ex)
            {
                Console.WriteLine($"发生异常:{ex.Message}");
            }
        }

        /// <summary>
        /// 向xml文档中添加用户信息集合
        /// </summary>
        /// <param name="user"></param>
        /// <param name="xmlPath"></param>
        public void AddUserRange(List<UserInfo> users,string xmlPath)
        {
            try
            {
                if (users.Count == 0) throw new Exception("数据集合为空异常");
                if (!File.Exists(xmlPath)) throw new Exception("xml文档路径无效异常");
                foreach(var user in users)
                {
                    AddUser(user,xmlPath);
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"异常:{ex.Message}");
            }
        }

        /// <summary>
        /// 查询用户信息是否存在,存在返回 true ，反之 false
        /// </summary>
        /// <param name="user"></param>
        public bool UserExist(string username,string xmlPath)
        {
            bool flag = false;
            try
            {
                if (username.Trim().Length == 0) throw new Exception("用户信息数据异常");
                if (!File.Exists(xmlPath)) throw new Exception("未发现配置文件，可能文件已丢失");

                var result = from u in XDocument.Load(xmlPath).Element("Users").Elements("User")
                             where u.Element("UserName").Value == username
                             select u;
                if(result.ToList().Count!=0)
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常:{ex.Message}");
            }
            return flag;
        }

        /// <summary>
        /// 通过输入用户名获取单个用户信息,如果用户存在，返回用户信息，否则返回 Null
        /// </summary>
        /// <param name="username"></param>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public UserInfo GetSingleUser(string username,string xmlPath)
        {
            UserInfo user = null;
            var result = from u in XDocument.Load(xmlPath).Root.Elements("User")
                         where u.Element("UserName").Value == username
                         select u;
            if(result.ToList().Count==1)
            {
                user = new UserInfo();
                user.UserName=result.ToList().ElementAtOrDefault(0).Element("UserName").Value;
                user.Password = result.ToList().ElementAtOrDefault(0).Element("Password").Value;
                user.age = int.Parse(result.ToList().ElementAtOrDefault(0).Element("Age").Value);
            }
            return user;
        }

        /// <summary>
        /// 获取所有用户信息
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public List<UserInfo> GetAllUsers(string xmlPath)
        {
            List<UserInfo> users = null;
            var res = from u in XDocument.Load(xmlPath).Root.Elements()
                      select u;
            if(res.ToList().Count>0)
            {
                users = new List<UserInfo>();
                foreach(var item in res)
                {
                    users.Add(new UserInfo() {UserName= item.Element("UserName").Value,
                    Password=item.Element("Password").Value,
                    age=int.Parse(item.Element("Age").Value)});
                }
            }
            return users;
        }

        /// <summary>
        /// 更改单个用户信息
        /// </summary>
        /// <param name="username">username 是查询条件</param>
        /// <param name="user">更改后的用户信息</param>
        /// <param name="xmlpath"></param>
        public void UpdateUserInfo(string username,UserInfo user,string xmlpath)
        {
            try
            {
                if (!UserExist(username, xmlpath)) throw new Exception("用户的信息异常");
                XDocument xdoc = XDocument.Load(xmlpath);
                    
                    xdoc.Element("Users").Elements("User")
                    .Where(x => x.Element("UserName").Value == username)
                    .Select(x => x.Element("Password")).FirstOrDefault().SetValue(user.Password);

                    xdoc.Element("Users").Elements("User")
                    .Where(x => x.Element("UserName").Value == username)
                    .Select(x => x.Element("Age")).FirstOrDefault().SetValue(user.age);

                    xdoc.Element("Users").Elements("User")
                    .Where(x => x.Element("UserName").Value == username)
                    .Select(x => x.Element("UserName")).FirstOrDefault().SetValue(user.UserName);

                xdoc.Save(xmlpath);
            }catch(Exception ex)
            {
                Console.WriteLine($"异常:{ex.Message}");
            }
        }

        /// <summary>
        /// 删除单个用户信息
        /// </summary>
        /// <param name="username"></param>
        public void RemoveUserInfo(string username,string xmlpath)
        {
            try
            {
                if(!UserExist(username,xmlpath)) throw new Exception("用户的信息异常");
                XDocument xdoc = XDocument.Load(xmlpath);
                xdoc.Element("Users").Elements("User").Where(x => x.Element("UserName").Value == username).Remove();
                xdoc.Save(xmlpath);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"异常：{ex.Message}");
            }
        }

        ///// <summary>
        ///// 删除所有用户信息(暂时来说存在一些问题,如果必须使用的话，务必保证下次使用 CreateUserXml（）创建一个xml文件)
        ///// </summary>
        ///// <param name="xmlpath"></param>
        //public void RemoveAllUsers(string xmlpath)
        //{
        //    XDocument xdoc = XDocument.Load(xmlpath);
        //    xdoc.Root.RemoveAll();
        //    xdoc.Save(xmlpath);
        //}
    }
}
