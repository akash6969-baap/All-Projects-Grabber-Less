using System;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Diagnostics;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace FearAuth
{

    public class api
    {

        public string name, ownerid, secret, version;
        public static long responseTime;
        private const string API_URL = "https://api.fearauth.online/api/1.2";

        public api(string name, string ownerid, string secret, string version)
        {
            this.name = name;
            this.ownerid = ownerid;
            this.secret = secret;
            this.version = version;
        }

        [DataContract]
        private class response_structure
        {
            [DataMember] public bool success { get; set; }
            [DataMember] public bool newSession { get; set; }
            [DataMember] public string sessionid { get; set; }
            [DataMember] public string contents { get; set; }
            [DataMember] public string response { get; set; }
            [DataMember] public string message { get; set; }
            [DataMember] public string download { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public user_data_structure info { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)]
            public app_data_structure appinfo { get; set; }
            [DataMember] public List<msg> messages { get; set; }
            [DataMember] public List<users> users { get; set; }
        }

        [DataContract]
        private class user_data_structure
        {
            [DataMember] public string username { get; set; }
            [DataMember] public string ip { get; set; }
            [DataMember] public string hwid { get; set; }
            [DataMember] public string createdate { get; set; }
            [DataMember] public string lastlogin { get; set; }
            [DataMember] public List<data_sub> subscriptions { get; set; }
        }

        [DataContract]
        private class data_sub
        {
            [DataMember] public string subscription { get; set; }
            [DataMember] public string expiry { get; set; }
            [DataMember] public string timeleft { get; set; }
            [DataMember] public string level { get; set; }
        }

        [DataContract]
        private class app_data_structure
        {
            [DataMember] public string numUsers { get; set; }
            [DataMember] public string numOnlineUsers { get; set; }
            [DataMember] public string numKeys { get; set; }
            [DataMember] public string version { get; set; }
            [DataMember] public string customerPanelLink { get; set; }
            [DataMember] public string downloadLink { get; set; }
        }

        public class msg
        {
            public string message { get; set; }
            public string author { get; set; }
            public string timestamp { get; set; }
        }

        public class users
        {
            public string credential { get; set; }
        }

        private static string sessionid, enckey;
        bool initialized;

        public void init()
        {
            if (initialized) return;
            string sentKey = encryption.iv_key();
            enckey = sentKey + "-" + secret;
            var payload = new NameValueCollection
            {
                ["type"] = "init",
                ["ver"] = version,
                ["hash"] = checksum(Process.GetCurrentProcess().MainModule.FileName),
                ["enckey"] = sentKey,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            var response = req(payload);
            if (response == "KeyAuth_Invalid")
            {
                error("Application not found on FearAuth server");
                Environment.Exit(0);
            }
            var match = Regex.Match(response, "\"sessionid\":\"(.*?)\"");
            sessionid = match.Groups[1].Value;
            initialized = true;
        }

        public void login(string username, string pass)
        {
            RequireInit();
            string hwid = WindowsIdentity.GetCurrent().User.Value;
            var payload = new NameValueCollection
            {
                ["type"] = "login",
                ["username"] = username,
                ["pass"] = pass,
                ["hwid"] = hwid,
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            var raw = req(payload);
            var json = response_decoder.string_to_generic<response_structure>(raw);
            load_response_struct(json);
            if (json.success) load_user_data(json.info);
        }

        public void register(string username, string pass, string key, string email = "")
        {
            RequireInit();
            string hwid = WindowsIdentity.GetCurrent().User.Value;
            var payload = new NameValueCollection
            {
                ["type"] = "register",
                ["username"] = username,
                ["pass"] = pass,
                ["key"] = key,
                ["email"] = email,
                ["hwid"] = hwid,
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            var raw = req(payload);
            var json = response_decoder.string_to_generic<response_structure>(raw);
            load_response_struct(json);
        }

        public void license(string key)
        {
            RequireInit();
            string hwid = WindowsIdentity.GetCurrent().User.Value;
            var payload = new NameValueCollection
            {
                ["type"] = "license",
                ["key"] = key,
                ["hwid"] = hwid,
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            var raw = req(payload);

            // Use reliable string check — same as original KeyAuth approach
            response.success = raw.Contains("\"success\":true");
            response.message = response.success
                ? ExtractJsonField(raw, "message")
                : (ExtractJsonField(raw, "message") ?? "Invalid License Key.");

            if (response.success)
            {
                user_data.username = ExtractJsonField(raw, "username") ?? "User";
                user_data.ip = ExtractJsonField(raw, "ip") ?? "";
                user_data.hwid = ExtractJsonField(raw, "hwid") ?? hwid;
                user_data.createdate = ExtractJsonField(raw, "createdate") ?? "";
                user_data.lastlogin = ExtractJsonField(raw, "lastlogin") ?? "";
                user_data.subscriptions = ParseSubscriptions(raw);
            }
        }

        private static string ExtractJsonField(string json, string field)
        {
            var match = Regex.Match(json, $"\"{field}\":\"(.*?)\"");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static List<Data> ParseSubscriptions(string json)
        {
            var list = new List<Data>();
            try
            {
                var subsSection = Regex.Match(json, "\"subscriptions\":\\[(.*?)\\]", RegexOptions.Singleline);
                if (!subsSection.Success) return list;
                var entries = Regex.Matches(subsSection.Groups[1].Value, "\\{(.*?)\\}", RegexOptions.Singleline);
                foreach (Match entry in entries)
                {
                    var sub = entry.Groups[1].Value;
                    list.Add(new Data
                    {
                        subscription = ExtractJsonField("{" + sub + "}", "subscription") ?? "default",
                        expiry = ExtractJsonField("{" + sub + "}", "expiry") ?? "0",
                        timeleft = ExtractJsonField("{" + sub + "}", "timeleft") ?? "0",
                        level = ExtractJsonField("{" + sub + "}", "level") ?? "1",
                    });
                }
            }
            catch { }
            return list;
        }

        public void logout()
        {
            RequireInit();
            var payload = new NameValueCollection
            {
                ["type"] = "logout",
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            var raw = req(payload);
            var json = response_decoder.string_to_generic<response_structure>(raw);
            load_response_struct(json);
            initialized = false;
        }

        public void upgrade(string username, string key)
        {
            RequireInit();
            var payload = new NameValueCollection
            {
                ["type"] = "upgrade",
                ["username"] = username,
                ["key"] = key,
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            var raw = req(payload);
            var json = response_decoder.string_to_generic<response_structure>(raw);
            load_response_struct(json);
        }

        public void check()
        {
            RequireInit();
            var payload = new NameValueCollection
            {
                ["type"] = "check",
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            var raw = req(payload);
            var json = response_decoder.string_to_generic<response_structure>(raw);
            load_response_struct(json);
        }

        public void ban(string reason = null)
        {
            RequireInit();
            var payload = new NameValueCollection
            {
                ["type"] = "ban",
                ["reason"] = reason ?? "",
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            var raw = req(payload);
            var json = response_decoder.string_to_generic<response_structure>(raw);
            load_response_struct(json);
        }

        public string expirydaysleft(string Type, int subscription = 0)
        {
            RequireInit();
            if (user_data.subscriptions == null || user_data.subscriptions.Count <= subscription)
                return "0";
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            dt = dt.AddSeconds(long.Parse(user_data.subscriptions[subscription].expiry)).ToLocalTime();
            TimeSpan diff = dt - DateTime.Now;
            switch (Type.ToLower())
            {
                case "months": return Convert.ToString(diff.Days / 30);
                case "days": return Convert.ToString(diff.Days);
                case "hours": return Convert.ToString(diff.Hours);
            }
            return "0";
        }

        public void log(string message)
        {
            RequireInit();
            var payload = new NameValueCollection
            {
                ["type"] = "log",
                ["pcuser"] = Environment.UserName,
                ["message"] = message,
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };
            req(payload);
        }

        public static string checksum(string filename)
        {
            using (MD5 md = MD5.Create())
            using (FileStream fs = File.OpenRead(filename))
            {
                byte[] value = md.ComputeHash(fs);
                return BitConverter.ToString(value).Replace("-", "").ToLowerInvariant();
            }
        }

        private void RequireInit()
        {
            if (!initialized)
            {
                error("Call FearAuthApp.init() first.");
                Environment.Exit(0);
            }
        }

        public static void error(string message)
        {
            string folder = @"Logs", file = Path.Combine(folder, "FearAuthErrors.txt");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            if (!File.Exists(file)) File.AppendAllText(file, DateTime.Now + " > FearAuth Error Log\n");
            File.AppendAllText(file, DateTime.Now + $" > {message}{Environment.NewLine}");
            Process.Start(new ProcessStartInfo("cmd.exe",
                $"/c start cmd /C \"color b && title FearAuth Error && echo {message} && timeout /t 5\"")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
        }

        private static string req(NameValueCollection post_data)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var raw = client.UploadValues(API_URL, post_data);
                    sw.Stop();
                    responseTime = sw.ElapsedMilliseconds;
                    return Encoding.Default.GetString(raw);
                }
            }
            catch (WebException webex)
            {
                var resp = (HttpWebResponse)webex.Response;
                if (resp == null)
                {
                    error($"Server offline or unreachable: {webex.Message}");
                    Environment.Exit(0);
                    return "";
                }
                switch (resp.StatusCode)
                {
                    case (HttpStatusCode)429:
                        error("Too many requests. Slow down.");
                        break;
                    default:
                        error("Connection failure. Try again.");
                        break;
                }
                Environment.Exit(0);
                return "";
            }
        }

        #region app_data
        public app_data_class app_data = new app_data_class();
        public class app_data_class
        {
            public string numUsers { get; set; }
            public string numOnlineUsers { get; set; }
            public string numKeys { get; set; }
            public string version { get; set; }
            public string customerPanelLink { get; set; }
            public string downloadLink { get; set; }
        }
        private void load_app_data(app_data_structure data)
        {
            app_data.numUsers = data.numUsers;
            app_data.numOnlineUsers = data.numOnlineUsers;
            app_data.numKeys = data.numKeys;
            app_data.version = data.version;
            app_data.customerPanelLink = data.customerPanelLink;
            app_data.downloadLink = data.downloadLink;
        }
        #endregion

        #region user_data
        public user_data_class user_data = new user_data_class();
        public class user_data_class
        {
            public string username { get; set; }
            public string ip { get; set; }
            public string hwid { get; set; }
            public string createdate { get; set; }
            public string lastlogin { get; set; }
            public List<Data> subscriptions { get; set; }
        }
        public class Data
        {
            public string subscription { get; set; }
            public string expiry { get; set; }
            public string timeleft { get; set; }
            public string level { get; set; }
        }
        private void load_user_data(user_data_structure data)
        {
            user_data.username = data.username;
            user_data.ip = data.ip;
            user_data.hwid = data.hwid;
            user_data.createdate = data.createdate;
            user_data.lastlogin = data.lastlogin;
            user_data.subscriptions = data.subscriptions?.ConvertAll(s => new Data
            {
                subscription = s.subscription,
                expiry = s.expiry,
                timeleft = s.timeleft,
                level = s.level,
            });
        }
        #endregion

        #region response
        public response_class response = new response_class();
        public class response_class
        {
            public bool success { get; set; }
            public string message { get; set; }
        }
        private void load_response_struct(response_structure data)
        {
            response.success = data.success;
            response.message = data.message;
        }
        #endregion

        private readonly json_wrapper response_decoder = new json_wrapper(new response_structure());
    }

    public static class encryption
    {

        public static string HashHMAC(string key, string message)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] msgBytes = Encoding.ASCII.GetBytes(message);
            var hmac = new HMACSHA256(keyBytes);
            return byte_arr_to_str(hmac.ComputeHash(msgBytes));
        }

        public static string byte_arr_to_str(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba) hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] str_to_byte_arr(string hex)
        {
            try
            {
                int len = hex.Length;
                byte[] bytes = new byte[len / 2];
                for (int i = 0; i < len; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            catch
            {
                api.error("Session ended. Reopen the application.");
                Environment.Exit(0);
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool CheckStringsFixedTime(string a, string b)
        {
            if (a.Length != b.Length) return false;
            int result = 0;
            for (int i = 0; i < a.Length; i++) result |= a[i] ^ b[i];
            return result == 0;
        }

        public static string iv_key() => Guid.NewGuid().ToString().Substring(0, 16);
    }

    public class json_wrapper
    {
        private readonly DataContractJsonSerializer serializer;
        private readonly object current_object;

        public static bool is_serializable(Type t) =>
            t.IsSerializable || t.IsDefined(typeof(DataContractAttribute), true);

        public json_wrapper(object obj)
        {
            current_object = obj;
            var type = current_object.GetType();
            serializer = new DataContractJsonSerializer(type);
            if (!is_serializable(type))
                throw new Exception($"FearAuth: {current_object} is not serializable");
        }

        public object string_to_object(string json)
        {
            var buffer = Encoding.Default.GetBytes(json);
            using (var ms = new MemoryStream(buffer))
                return serializer.ReadObject(ms);
        }

        public T string_to_generic<T>(string json) => (T)string_to_object(json);
    }
}
