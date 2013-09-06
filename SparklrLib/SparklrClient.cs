﻿using Newtonsoft.Json;
using SparklrLib.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SparklrLib
{
    /// <summary>
    /// The main Sparklr Client
    /// </summary>
    public class SparklrClient
    {
        public const string LikesEscape = "☝";

        /// <summary>
        /// Is raised when the login details are invalid.
        /// </summary>
        public event EventHandler CredentialsExpired;

        public static event EventHandler<NotificationEventArgs> NotificationsReceived;

        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        /// <value>
        /// The authentication token.
        /// </value>
        public string AuthToken { get; set; }
        /// <summary>
        /// Gets the user unique identifier.
        /// </summary>
        /// <value>
        /// The user unique identifier.
        /// </value>
        public long UserId { get; private set; }
        /// <summary>
        /// Get is user logged in
        /// </summary>
        /// <value>
        /// User login status
        /// </value>
        public bool IsLoggedIn
        {
            get
            {
                return AuthToken != null && UserId != 0L;
            }
        }
        /// <summary>
        /// Gets or sets the usernames.
        /// </summary>
        /// <value>
        /// The usernames.
        /// </value>
        public Dictionary<int, string> Usernames { get; set; }
        /// <summary>
        /// The base URI
        /// </summary>
        public const string BaseURI = "https://sparklr.me/";

        /// <summary>
        /// Initializes a new instance of the <see cref="SparklrClient"/> class.
        /// </summary>
        public SparklrClient()
        {
            Usernames = new Dictionary<int, string>();
        }

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public HttpWebRequest CreateRequest(string path)
        {
            return CreateRequest(path, "");
        }
        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="xdata">The xdata.</param>
        /// <returns></returns>
        public HttpWebRequest CreateRequest(string path, string xdata)
        {
            if (path[0] == '/') path = path.Substring(1);
            HttpWebRequest newReq = HttpWebRequest.CreateHttp(BaseURI + path);
            if (AuthToken != null)
            {
                newReq.Headers["Cookie"] = "D=" + UserId.ToString() + ',' + AuthToken;
            }
            newReq.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.57 Safari/537.36";
            if (xdata != "")
            {
                newReq.Headers["X-Data"] = xdata;
            }
            if (AuthToken != null)
            {
                newReq.Headers["X-X"] = AuthToken;
            }
            return newReq;
        }

        /// <summary>
        /// Requests the json object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path.</param>
        private Task<JSONRequestEventArgs<T>> requestJsonObjectAsync<T>(string path)
        {
            return requestJsonObjectAsync<T>(path, "", "");
        }

        /// <summary>
        /// Requests the json object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path.</param>
        /// <param name="xdata">The xdata.</param>
        private Task<JSONRequestEventArgs<T>> requestJsonObjectAsync<T>(string path, object xdata)
        {
            return requestJsonObjectAsync<T>(path, JsonConvert.SerializeObject(xdata), "");
        }

        /// <summary>
        /// Requests the json object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path.</param>
        /// <param name="xdata">The xdata.</param>
        private Task<JSONRequestEventArgs<T>> requestJsonObjectAsync<T>(string path, string xdata)
        {
            return requestJsonObjectAsync<T>(path, xdata, "");
        }

        /// <summary>
        /// Requests the json object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path.</param>
        /// <param name="xdata">The xdata.</param>
        /// <param name="postdata">The postdata.</param>
        private Task<JSONRequestEventArgs<T>> requestJsonObjectAsync<T>(string path, object xdata, string postdata, string method = "GET")
        {
            return requestJsonObjectAsync<T>(path, JsonConvert.SerializeObject(xdata), postdata, method);
        }

        /// <summary>
        /// Requests the json object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path.</param>
        /// <param name="xdata">The xdata.</param>
        /// <param name="postdata">The postdata.</param>
        private async Task<JSONRequestEventArgs<T>> requestJsonObjectAsync<T>(string path, string xdata, string postdata, string method = "GET")
        {
            HttpWebRequest streamReq = CreateRequest(path, xdata);
            streamReq.Method = method;

            try
            {
                if (!String.IsNullOrEmpty(postdata))
                {
                    streamReq.Method = "POST";
                    using (Stream postStream = await streamReq.GetRequestStreamAsync())
                    {
                        // Create the post data
                        byte[] byteArray = Encoding.UTF8.GetBytes(postdata);
                        // Add the post data to the web request
                        postStream.Write(byteArray, 0, byteArray.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                return new JSONRequestEventArgs<T>()
                {
                    IsSuccessful = false,
                    Error = ex,
                    Response = null
                };
            }
            bool error = false;
            WebException exc;
            try
            {
                T desiredObject = default(T);
                using (HttpWebResponse streamResp = (HttpWebResponse)await streamReq.GetResponseAsync())
                using (Stream respStr = streamResp.GetResponseStream())
                using (StreamReader strReader = new StreamReader(respStr, Encoding.UTF8))
                {
                    try
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        string response = await strReader.ReadToEndAsync();
                        desiredObject = JsonConvert.DeserializeObject<T>(response);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
#endif
                        return new JSONRequestEventArgs<T>()
                        {
                            IsSuccessful = false,
                            Error = ex,
                            Response = streamResp
                        };
                    }
                }

                return new JSONRequestEventArgs<T>()
                {
                    IsSuccessful = true,
                    Error = null,
                    Object = desiredObject
                };
            }
            catch (WebException ex)
            {
                error = true;
                exc = ex;
            }
            catch (Exception ex)
            {
                return new JSONRequestEventArgs<T>()
                {
                    IsSuccessful = false,
                    Error = ex
                };
            }
            if (error)
            {
                if (((HttpWebResponse)exc.Response).StatusCode == HttpStatusCode.Forbidden)
                {
                    using (StreamReader strReader = new StreamReader(exc.Response.GetResponseStream(), Encoding.UTF8))
                    {
                        string json = await strReader.ReadToEndAsync();

                        if (json == "")
                        {
                            raiseCredentialsExpired();
                        }
                        else
                        {
                            //TODO: Handle other errors here, for example not authorized to see user profile:
                            //{
                            //    "error": true,
                            //    "info": {
                            //        "notFriends": true,
                            //        "following": false
                            //    }
                            //}
                        }
                    }
                }

                return new JSONRequestEventArgs<T>()
                {
                    IsSuccessful = false,
                    Error = exc,
                    Response = (HttpWebResponse)exc.Response
                };
            }
            return new JSONRequestEventArgs<T>()
            {
                IsSuccessful = false,
                Error = null,
                Response = null
            };
        }

        public void ManualLogin(string authToken, long userId)
        {
            //TODO: Could use some checks
            this.AuthToken = authToken;
            this.UserId = userId;
        }

        /// <summary>
        /// Performans an asynchronous login of the user
        /// </summary>
        /// <param name="Username">The username.</param>
        /// <param name="Password">The password.</param>
        public async Task<LoginEventArgs> LoginAsync(string Username, string Password)
        {
            HttpWebRequest loginReq = CreateRequest("work/signin/" + Username + "/" + Password + "/");

            try
            {
                HttpWebResponse response = (HttpWebResponse)await loginReq.GetResponseAsync();

                if (response.Headers["Set-Cookie"] == null)
                {
                    return new LoginEventArgs()
                    {
                        //TODO: Use custom exception types
                        Error = new Exception("Didn't receive Auth token"),
                        IsSuccessful = false,
                        Response = response
                    };
                }
                else
                {
                    string[] cookieParts = response.Headers["Set-Cookie"].Split(';');
                    string cookieD = "";

                    //TODO: Suggestion: use a regex instead?
                    foreach (string sortaCookie in cookieParts)
                    {
                        string sortaTrimmedCookie = sortaCookie.TrimStart();
                        if (sortaTrimmedCookie.StartsWith("D="))
                        {
                            cookieD = sortaTrimmedCookie.Substring(2);
                            break;
                        }
                    }

                    string[] loginBits = cookieD.Split(',');

                    if (cookieD.Length == 0)
                    {
                        return new LoginEventArgs()
                        {
                            Error = new Exception("Auth token not included"),
                            IsSuccessful = false,
                            Response = response
                        };
                    }
                    else if (loginBits.Length < 2)
                    {
                        return new LoginEventArgs()
                        {
                            Error = new Exception("Auth token is corrupted"),
                            IsSuccessful = false,
                            Response = response
                        };
                    }
                    else
                    {
                        try
                        {
                            UserId = long.Parse(loginBits[0]);
                        }
                        catch (Exception)
                        {
                            return new LoginEventArgs()
                            {
                                Error = new Exception("Auth token is corrupted"),
                                IsSuccessful = false,
                                Response = response
                            };
                        }
                        AuthToken = loginBits[1];
                        return new LoginEventArgs()
                        {
                            Error = null,
                            IsSuccessful = true,
                            Response = response,
                            AuthToken = AuthToken,
                            UserId = UserId
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException)
                {
                    return new LoginEventArgs()
                    {
                        IsSuccessful = false,
                        Error = ex,
                        Response = (HttpWebResponse)((WebException)ex).Response
                    };
                }
                else
                {
#if DEBUG
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
#endif
                    return new LoginEventArgs()
                    {
                        IsSuccessful = false,
                        Error = ex,
                        Response = null
                    };
                }
            }
        }

        /// <summary>
        /// Gets the beacon stream.
        /// </summary>
        public Task<JSONRequestEventArgs<Objects.Responses.Beacon.Stream>> GetBeaconStreamAsync()
        {
            return GetBeaconStreamAsync(0, 20, 0);
        }

        /// <summary>
        /// Gets the beacon stream.
        /// </summary>
        /// <param name="lastTime">The last time.</param>
        public Task<JSONRequestEventArgs<Objects.Responses.Beacon.Stream>> GetBeaconStreamAsync(int lastTime)
        {
            return GetBeaconStreamAsync(lastTime, 0, 0);
        }

        /// <summary>
        /// Retreives thes post info from the server
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JSONRequestEventArgs<Objects.Responses.Work.Post>> GetPostInfoAsync(int id)
        {
            JSONRequestEventArgs<Objects.Responses.Work.Post> args = await requestJsonObjectAsync<Objects.Responses.Work.Post>("/work/post/" + id.ToString());
            return args;
        }

        /// <summary>
        /// Returns more items from the stream
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="stream"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        public async Task<JSONRequestEventArgs<Objects.Responses.Beacon.Stream>> GetMoreItems(int startTime, int stream = 0, int network = 0)
        {
#if DEBUG
            stream = 2;
            network = 1;
#endif
            //TODO: implement properly
            JSONRequestEventArgs<Objects.Responses.Beacon.Stream> args = await requestJsonObjectAsync<Objects.Responses.Beacon.Stream>("/beacon/stream/" + stream + "?since=0?starttime=" + startTime.ToString() + (network != 0 ? "&network=" + network.ToString() : ""));
            return args;
        }

        /// <summary>
        /// Gets the beacon stream.
        /// </summary>
        /// <param name="lastTime">The last time.</param>
        /// <param name="lastNotificationTime">The last notification time.</param>
        public Task<JSONRequestEventArgs<Objects.Responses.Beacon.Stream>> GetBeaconStreamAsync(int lastTime, int lastNotificationTime)
        {
            return GetBeaconStreamAsync(lastTime, lastNotificationTime, 0);
        }

        /// <summary>
        /// Gets the beacon stream.
        /// </summary>
        /// <param name="lastTime">The last time.</param>
        /// <param name="lastNotificationTime">The last notification time.</param>
        /// <param name="network">The network.</param>
        public async Task<JSONRequestEventArgs<Objects.Responses.Beacon.Stream>> GetBeaconStreamAsync(int lastTime, int lastNotificationTime, int network)
        {
            int stream = 0;
#if DEBUG
            stream = 2;
            network = 1;
#endif
            JSONRequestEventArgs<Objects.Responses.Beacon.Stream> args = await requestJsonObjectAsync<Objects.Responses.Beacon.Stream>("/beacon/stream/" + stream + "?since=" + lastTime.ToString() + "&n=" + lastNotificationTime.ToString() + (network != 0 ? "&network=" + network.ToString() : ""));
            return args;
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="image">The image.</param>
        public async Task<SparklrEventArgs> PostAsync(string message, Stream image)
        {
            string data64str = "";
            if (image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Seek(0, SeekOrigin.Begin);
#if PORTABLELIB
                    byte[] array = new byte[81920];
                    int count;
                    while ((count = image.Read(array, 0, array.Length)) != 0)
                    {
                        ms.Write(array, 0, count);
                    }
#else
                    image.CopyTo(ms);
#endif
                    data64str = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }
            JSONRequestEventArgs<Objects.Responses.Generic> args = await requestJsonObjectAsync<Objects.Responses.Generic>("/work/post", new Objects.Requests.Work.Post()
            {
                body = message,
#if DEBUG
                network = 2,
#endif
                img = data64str != ""
            }, data64str, "POST");

            return new SparklrEventArgs()
            {
                IsSuccessful = args.IsSuccessful && args.Object.error == null,
                Error = args.IsSuccessful ? args.Object.error == true ? new Exception("Sparklr said noooooo") : null : args.Error
            };
        }

        /// <summary>
        /// Gets the usernames by the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        public async Task<JSONRequestEventArgs<Objects.Responses.Work.Username[]>> GetUsernamesAsync(int[] ids)
        {
            List<int> idsToRequest = new List<int>();
            foreach (int id in ids)
            {
                if (!Usernames.ContainsKey(id))
                {
                    idsToRequest.Add(id);
                }
            }
            if (idsToRequest.Count > 0)
            {
                JSONRequestEventArgs<Objects.Responses.Work.Username[]> args = await requestJsonObjectAsync<Objects.Responses.Work.Username[]>("/work/username/" + String.Join(",", (string[])(from id in ids select id.ToString()).ToArray()));

                if (args.IsSuccessful)
                {
                    foreach (Objects.Responses.Work.Username un in args.Object)
                    {
                        Usernames[un.id] = un.username;
                    }
                    List<Objects.Responses.Work.Username> usrnms = new List<Objects.Responses.Work.Username>();
                    foreach (int id in ids)
                    {
                        if (Usernames.ContainsKey(id))
                        {
                            usrnms.Add(new Objects.Responses.Work.Username() { id = id, username = Usernames[id] });
                        }
                    }

                    return new JSONRequestEventArgs<Objects.Responses.Work.Username[]>()
                    {
                        Error = null,
                        IsSuccessful = true,
                        Object = usrnms.ToArray()
                    };
                }
            }
            else
            {
                List<Objects.Responses.Work.Username> usrnms = new List<Objects.Responses.Work.Username>();
                foreach (int id in ids)
                {
                    if (Usernames.ContainsKey(id))
                    {
                        usrnms.Add(new Objects.Responses.Work.Username() { id = id, username = Usernames[id] });
                    }
                }
                return new JSONRequestEventArgs<Objects.Responses.Work.Username[]>()
                {
                    Error = null,
                    IsSuccessful = true,
                    Object = usrnms.ToArray()
                };
            }
            return new JSONRequestEventArgs<Objects.Responses.Work.Username[]>()
            {
                Error = new Exception("Invalid data was provided"),
                IsSuccessful = false
            };
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.OnlineFriends[]>> GetOnlineFriendsAsync()
        {
            return requestJsonObjectAsync<Objects.Responses.Work.OnlineFriends[]>("/work/onlinefriends");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.Friends>> GetFriendsAsync()
        {
            return requestJsonObjectAsync<Objects.Responses.Work.Friends>("/work/friends");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.User>> GetUserAsync(string username)
        {
            return requestJsonObjectAsync<Objects.Responses.Work.User>("/work/user/" + username);
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.User>> GetUserAsync(int userid)
        {
            return requestJsonObjectAsync<Objects.Responses.Work.User>("/work/user/" + userid);
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.User>> GetUserMentionsAsync(int userid)
        {
            return requestJsonObjectAsync<Objects.Responses.Work.User>("/work/user/" + userid + "/mentions");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.User>> GetUserPhotosAsync(int userid)
        {
            return requestJsonObjectAsync<Objects.Responses.Work.User>("/work/user/" + userid + "/photos");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Generic>> PostCommentAsync(int authorid, int postid, string comment)
        {
            return requestJsonObjectAsync<Objects.Responses.Generic>("/work/comment", new Objects.Requests.Work.Comment()
            {
                to = authorid,
                id = postid,
                comment = comment
            }, null, "POST");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Generic>> LikePostAsync(int authorid, int postid)
        {
            return requestJsonObjectAsync<Objects.Responses.Generic>("/work/like", new Objects.Requests.Work.Like()
            {
                to = authorid,
                id = postid
            }, null, "POST");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Generic>> DeleteCommentAsync(int commentid)
        {
            return requestJsonObjectAsync<Objects.Responses.Generic>("/work/delete/comment/" + commentid.ToString());
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Generic>> FollowAsync(int userid)
        {
            return requestJsonObjectAsync<Objects.Responses.Generic>("/work/follow/" + userid.ToString());
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Generic>> UnfollowAsync(int userid)
        {
            return requestJsonObjectAsync<Objects.Responses.Generic>("/work/unfollow/" + userid.ToString());
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.Search>> SearchAsync(string keyword)
        {
            return requestJsonObjectAsync<Objects.Responses.Work.Search>("/work/search/" + HttpUtility.UrlEncode(keyword));
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Generic>> Repost(int postid)
        {
            return Repost(postid, "");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Generic>> Repost(int postid, string reply)
        {
            return requestJsonObjectAsync<Objects.Responses.Generic>("/work/repost", new Objects.Requests.Work.Repost(postid, reply), null, "POST");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.Inbox>> GetInboxAsync()
        {
            return requestJsonObjectAsync<Objects.Responses.Work.Inbox>("/work/inbox");
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Work.Chat[]>> GetChatAsync(int otherid)
        {
            return requestJsonObjectAsync<Objects.Responses.Work.Chat[]>("/work/chat/" + otherid.ToString());
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Beacon.Chat>> GetBeaconChatAsync(int otherid, int since, int limit = 0)
        {
            return requestJsonObjectAsync<Objects.Responses.Beacon.Chat>("/beacon/chat/" + otherid.ToString() + "?since=" + since.ToString() + "&n=" + limit.ToString());
        }

        public Task<JSONRequestEventArgs<Objects.Responses.Generic>> PostChatMessageAsync(int recipient, string message)
        {
            return requestJsonObjectAsync<Objects.Responses.Generic>("/work/chat", new Objects.Requests.Work.Chat()
                {
                    to = recipient,
                    message = message
                }, "", "POST");
        }

        private void raiseCredentialsExpired()
        {
            //Fire only once
            if (IsLoggedIn)
            {
                AuthToken = null;
                if (CredentialsExpired != null)
                    CredentialsExpired(this, null);
            }
        }

        static internal void RaiseNotificationReceived(object sender, NotificationEventArgs e)
        {
            if (NotificationsReceived != null)
                NotificationsReceived(sender, e);
        }
    }
}
