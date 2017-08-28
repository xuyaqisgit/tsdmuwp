using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace LaCODESoftware.Tsdm
{
    public interface ITsdmHelper
    {
        CookieContainer Cookie { get; set; }

        Task<string> CheckAsync();
        Task<Json> GetForumAsync(string gid);
        Task<Json> GetForumAsync(string fid, string page);
        Task<string> GetThreadAsync(string tid, string page);
        Task<bool> LogInAsync(string username, string password, string verifycode, string loginfield);
        void PayAsync(string tid);
        Task<bool> ReplyAsync(string tid, string body);
        Task<string> SreachAsync(string body);
        Task<Json> UserInfoAsync();
        Task<Stream> VerifyCodeAsync();
    }
}