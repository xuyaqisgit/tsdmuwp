﻿using LaCODESoftware.BasicHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace LaCODESoftware.Tsdm
{
    /// <summary>
    /// 天使动漫api
    /// </summary>
    public class TsdmHelper : ITsdmHelper
    {
        /// <summary>
        /// 运行时储存Cookie
        /// </summary>
        public CookieContainer Cookie { get; set; }
        /// <summary>
        /// 在网站搜索指定内容
        /// </summary>
        /// <param name="body">内容</param>
        /// <returns>返回搜索所得到的字符串形式html</returns>
        public async Task<string> SreachAsync(string body)
        {
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, String.Format("http://www.tsdm.me/plugin.php?id=Kahrpba:search&query={0}&mobile=yes", body));
            return StreamHelper.StreamToString(tuple.Item1);
        }
        /// <summary>
        /// 主题购买
        /// </summary>
        /// <param name="tid">帖子代码</param>
        public async void PayAsync(string tid)
        {
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, "http://www.tsdm.me/forum.php?mobile=yes&tsdmapp=1&mod=post&action=reply&tid=628244");
            Json json = JsonHelper.DataContractJsonDeserialize<Json>(tuple.Item1);
            string formhash = json.formhash;
            await WebHelper.GetStreamAsync(Cookie, "http://www.tsdm.me/forum.php?mod=misc&action=pay&mobile=yes&paysubmit=yes&infloat=yes", String.Format("formhash={0}&referer=http://www.tsdm.me/./&tid={1}&paysubmit=true", formhash, tid));
            await WebHelper.GetStreamAsync(Cookie, String.Format("http://www.tsdm.me/forum.php?mod=viewthread&tid={0}&mobile=yes", tid));
        }
        /// <summary>
        /// 获取帖子内容
        /// </summary>
        /// <param name="tid">帖子代码</param>
        /// <param name="page">页码</param>
        /// <returns>返回字符串形式Html网页</returns>
        public async Task<string> GetThreadAsync(string tid, string page)
        {
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, String.Format("http://www.tsdm.me/forum.php?mod=viewthread&mobile=yes&tsdmapp=1&tid={0}&page={1} ", tid, page));
            Json json = JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1).Replace("\n", "").Replace("\r", ""));
            Cookie = tuple.Item2;
            string head = "<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" /><base href=\"http://www.tsdm.me/\" /><style type=\"text/css\">img{ width:auto; height:auto; max-width:100%; max-height:100%;}</style></head>";
            string post = "";
            if (json.thread_price != null && json.thread_paid == "0")
            {
                post += String.Format("<div><a href=\"http://www.tsdm.me/forum.php?mod=misc&action=pay&mobile=yes&paysubmit=yes&infloat=yes\">购买主题 价格:{0}</a><div>", json.thread_price);
            }
            foreach (var item in json.postlist)
            {
                post += String.Format("<table><tbody><tr><td width=\"60\"><div width=\"60\"><div width=\"60\"><img src=\"{0}\" onerror=\"this.onerror=null;this.src=\'http://www.tsdm.me/uc_server/images/noavatar_middle.gif\'\" /></div><div>{1}</div><div>{2}</div><div>{3}</div></div></td><td width=\"400\"><div width=\"400\"><div width=\"400\"><table width=\"400\" cellspacing=\"0\" cellpadding=\"0\"><tbody><tr><td width=\"400\">{4}</td></tr></tbody></table></td></tr></tbody></table><hr />", item.avatar.ToString(), item.author.ToString(), item.author_nickname.ToString(), item.authortitle.ToString(), item.message.ToString());
            }
            return (head + "<body>" + post.Replace(" target=\"_blank\"", "") + "</body></html>");
        }
        /// <summary>
        /// 取得板块列表
        /// </summary>
        /// <param name="gid">板块代码，当取""时为主页板块</param>
        /// <returns>返回包含所需信息的实体类</returns>
        public async Task<Json> GetForumAsync(string gid)
        {
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, String.Format("http://www.tsdm.me/forum.php?mobile=yes&tsdmapp=1&gid={0}", gid));
            Cookie = tuple.Item2;
            return JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1));
        }
        /// <summary>
        /// 取得帖子列表
        /// </summary>
        /// <param name="fid">板块代码</param>
        /// <param name="page">页码</param>
        /// <returns>返回包含所需信息的实体类</returns>
        public async Task<Json> GetForumAsync(string fid, string page)
        {
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, String.Format("http://www.tsdm.me/forum.php?mobile=yes&tsdmapp=1&mod=forumdisplay&fid={0}&page={1}", fid, page));
            return JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1));
        }
        /// <summary>
        /// 签到
        /// </summary>
        /// <returns>返回签到回执信息</returns>
        public async Task<string> CheckAsync()
        {
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, "http://www.tsdm.me/forum.php?mobile=yes&tsdmapp=1&mod=post&action=reply&tid=628244");
            Json json = JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1));
            tuple = await WebHelper.GetStreamAsync(Cookie, "http://www.tsdm.me/plugin.php?mobile=yes&tsdmapp=1&id=dsu_paulsign:sign&operation=qiandao&infloat=1&inajax=1", String.Format("qdmode=1&formhash={0}&fastreply=1&qdxq=kx&todaysay=winform客户端签到", json.formhash));
            json = JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1));
            return json.message;
        }
        /// <summary>
        /// 回复帖子
        /// </summary>
        /// <param name="tid">帖子代码</param>
        /// <param name="body">回复内容</param>
        /// <returns>返回是否成功的布尔值</returns>
        public async Task<bool> ReplyAsync(string tid, string body)
        {
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, String.Format("http://www.tsdm.me/forum.php?mobile=yes&tsdmapp=1&mod=post&action=reply&tid={0}", tid));
            Json json = JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1));
            tuple = await WebHelper.GetStreamAsync(Cookie, "http://www.tsdm.me/forum.php?mobile=yes&tsdmapp=2&mod=post&action=reply&replysubmit=yes", String.Format("message={0}&formhash={1}&clienthash=DC0EEB7B38AA1F07AF76895A8E14747B&tid={2}&", body, json.formhash, tid));
            json = JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1));
            if (json.message == "post_reply_succeed")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 取得用户信息
        /// </summary>
        /// <returns>返回包含所需信息的实体类</returns>
        public async Task<Json> UserInfoAsync()
        {
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, "http://www.tsdm.me/home.php?mobile=yes&tsdmapp=1&mod=space&do=profile ");
            Cookie = tuple.Item2;
            return JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1));
        }
        /// <summary>
        /// 取得验证码流
        /// </summary>
        /// <returns>返回流</returns>
        public async Task<Stream> VerifyCodeAsync() => (await WebHelper.GetStreamAsync(Cookie, "http://www.tsdm.me/plugin.php?id=oracle:verify")).Item1;
        /// <summary>
        /// 登录论坛
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="verifycode">验证码</param>
        /// <param name="loginfield">登录方法(在email,username,uid中选择)</param>
        /// <returns>返回是否登录成功的布尔值</returns>
        public async Task<bool> LogInAsync(string username, string password, string verifycode, string loginfield)
        {
            string body = String.Format("password={0}&tsdm_verify={1}&fastloginfield={2}&answer=&username={3}&questionid=0&", password, verifycode, loginfield, username);
            Tuple<Stream, CookieContainer> tuple = await WebHelper.GetStreamAsync(Cookie, "http://www.tsdm.me/member.php?mobile=yes&tsdmapp=1&mod=logging&action=login&loginsubmit=yes", body);
            Json LogResult = JsonHelper.DataContractJsonDeserialize<Json>(StreamHelper.StreamToString(tuple.Item1));
            Cookie = tuple.Item2;
            if (LogResult.message == "location_login_succeed_mobile")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    #region Json类及其所有子类都由计算机生成请勿修改
    public class Group
    {
        public string title { get; set; }
        public string gid { get; set; }
    }
    public class Forum
    {
        public string fid { get; set; }
        public string title { get; set; }
        public string todaypost { get; set; }
    }
    public class Thread
    {
        public string title { get; set; }
        public string tid { get; set; }
        public string authorid { get; set; }
        public string author { get; set; }
        public string displayorder { get; set; }
        public string showclass { get; set; }
        public string showstyle { get; set; }
        public string views { get; set; }
        public string replies { get; set; }
        public string lastpost { get; set; }
        public string lastposter { get; set; }
        public string typeid { get; set; }
        public string closed { get; set; }
        public string typehtml { get; set; }
        public string readperm { get; set; }
    }
    public class Subforum
    {
        public string fid { get; set; }
        public string name { get; set; }
    }
    public class Moderator
    {
        public string username { get; set; }
        public string uid { get; set; }
    }
    public class Recommend
    {
        public string tid { get; set; }
        public string title { get; set; }
    }
    public class Threadtype
    {
        public string typeid { get; set; }
        public string name { get; set; }
    }
    public class Ratelog
    {
        public string uid { get; set; }
        public string username { get; set; }
        public string status { get; set; }
        public IList<string> score { get; set; }
        public string reason { get; set; }
    }
    public class Postlist
    {
        public string pid { get; set; }
        public string author { get; set; }
        public string authorid { get; set; }
        public string avatar { get; set; }
        public string timestamp { get; set; }
        public string subject { get; set; }
        public string message { get; set; }
        public string first { get; set; }
        public string floor { get; set; }
        public object platform { get; set; }
        public string authortitle { get; set; }
        public string authorgid { get; set; }
        public IList<Ratelog> ratelog { get; set; }
        public IList<string> ratelogextcredits { get; set; }
        public string author_nickname { get; set; }
    }
    public class Extcreditsname
    {
    }
    /// <summary>
    /// Json类及其所有子类都由计算机生成请勿修改
    /// </summary>
    public class Json
    {
        public string status { get; set; }
        public IList<Group> group { get; set; }
        public string message { get; set; }
        public string uid { get; set; }
        public string username { get; set; }
        public string nickname { get; set; }
        public string gid { get; set; }
        public string aid { get; set; }
        public string credits { get; set; }
        public string miku { get; set; }
        public string threads { get; set; }
        public string posts { get; set; }
        public string readaccess { get; set; }
        public string regdate { get; set; }
        public string cpuid { get; set; }
        public string cpusername { get; set; }
        public string customstatus { get; set; }
        public string avatar { get; set; }
        public string extcredits1 { get; set; }
        public string extcredits2 { get; set; }
        public string extcredits3 { get; set; }
        public string extcredits4 { get; set; }
        public string extcredits5 { get; set; }
        public string extcredits6 { get; set; }
        public string extcredits7 { get; set; }
        public IList<Forum> forum { get; set; }
        public string groupname { get; set; }
        public IList<Thread> thread { get; set; }
        public string forum_cover { get; set; }
        public IList<Subforum> subforum { get; set; }
        public string threadcnt { get; set; }
        public string total { get; set; }
        public string forumname { get; set; }
        public string ismoderator { get; set; }
        public IList<Moderator> moderator { get; set; }
        public IList<Recommend> recommend { get; set; }
        public IList<Threadtype> threadtype { get; set; }
        public IList<Postlist> postlist { get; set; }
        public string totalpost { get; set; }
        public string tpp { get; set; }
        public string subject { get; set; }
        public string fid { get; set; }
        public string thread_author { get; set; }
        public string thread_authorid { get; set; }
        public Extcreditsname extcreditsname { get; set; }
        public string thread_price { get; set; }
        public string thread_paid { get; set; }
        public string formhash { get; set; }
        public string quotemsg { get; set; }
        public string sechash { get; set; }
        public string answerkey { get; set; }
        public string noticeauthor { get; set; }
        public string noticetrimstr { get; set; }
        public string noticeauthormsg { get; set; }
        public string tid { get; set; }
    }
    #endregion
}
