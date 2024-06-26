using CBShare.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CTPServer.MongoDB;
using WebServices.Hubs;
using LitJson;
using CBShare.Data;
using CBShare.Common;

namespace WebServices
{
    public class Startup
    {
        public static Startup Instance { get; set; }
        public IConfiguration Configuration { get; }
        private System.Timers.Timer _timer = new System.Timers.Timer();

        public Startup(IConfiguration configuration)
        {
            Instance = this;
            Configuration = configuration;
            Utility.Init();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        /*public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSignalR();
        }*/

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR(hupoptions =>
            {
                hupoptions.EnableDetailedErrors = false;
            }).AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                options.PayloadSerializerSettings.Culture = System.Globalization.CultureInfo.InvariantCulture;
                options.PayloadSerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
            });

            services.AddSingleton<GameMemCache>();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed((host) => true)
                .AllowCredentials()
                );
            });

            // services.AddResponseCaching();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    string welcome_str = "Welcome to Ludo";
                    await context.Response.WriteAsync(welcome_str);
                });

                //endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapHub<BattleHub>("/battleHub");
                endpoints.MapPost("/game/request", HTTPRequest);
                endpoints.MapPost("/game/checkConnect", HTTPCheckConnect);
                endpoints.MapPost("/game/request/RequestLogin", HTTPRequestWithMethod);

                endpoints.MapPost("gmtool/userManager", GMToolUserManager);
                endpoints.MapPost("gmtool/getUsersList", GMToolGetUsersList);
                endpoints.MapPost("gmtool/lockUser", GMToolLockUser);
                endpoints.MapPost("gmtool/sendRewardToUser", GMToolSendRewardToUser);

                endpoints.MapPost("gmtool/dashboardBattleLogs", GMToolDashboardBattleLogs);
                endpoints.MapPost("gmtool/gamerBattleLogs", GMToolGamerBattleLogs);
                endpoints.MapPost("gmtool/thongkeNapGold", GMToolThongKeNapGold);
                endpoints.MapPost("gmtool/thongkeTieuGold", GMToolThongKeTieuGold);
                endpoints.MapPost("gmtool/thongkeNapDiamond", GMToolThongKeNapDiamond);
                endpoints.MapPost("gmtool/thongkeTieuDiamond", GMToolThongKeTieuDiamond);
                endpoints.MapPost("gmtool/thongkeGiaiDau", GMToolThongKeGiaiDau);
            });

            this.InitConfigs(env.ContentRootPath);
            this.InitDatabases();
            //this.InitTimer();
        }

        private void InitTimer()
        {
            this._timer.Interval = 6000;
            this._timer.Elapsed += TimerElapsed;
            this._timer.Start();
        }

        void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /*AutoTimerTongKim();
            AutoTimerClanWar();
            TimeEventMutexMongoDB.CheckResetBXHHuyetChien();
            TimeEventMutexMongoDB.CheckResetArenaSeason();
            TimeEventMutexMongoDB.CheckResetArena3DoiHinh();
            //AutoResetEventData();
            AutoTimerCongThanhChien();*/
        }

        private void InitConfigs(string path)
        {
            try
            {
                // Localization Language.
                Localization.LoadData(path);
                Localization.ServerLanguage = "Vietnamese";

                // Code that runs on application startup
                //var path = "CTP_Server/ServerConfigs/";
                Localization.LoadData(path);
                string othersTxt = File.ReadAllText(path + "/SrvConfigs/Other.txt");
                string charactersTxt = File.ReadAllText(path + "/SrvConfigs/Characters.txt");
                string dicesTxt = File.ReadAllText(path + "/SrvConfigs/Dices.txt");
                string starCardsTxt = File.ReadAllText(path + "/SrvConfigs/StarCards.txt");
                string roomsTxt = File.ReadAllText(path + "/SrvConfigs/Rooms.txt");
                string shopsTxt = File.ReadAllText(path + "/SrvConfigs/Shops.txt");
                string actionCardsTxt = File.ReadAllText(path + "/SrvConfigs/ActionCards.txt");
                string battleTxt = File.ReadAllText(path + "/SrvConfigs/Battle.txt");
                ConfigManager.instance.ReadAllConfigs(othersTxt, charactersTxt, dicesTxt, starCardsTxt, shopsTxt, roomsTxt, actionCardsTxt, battleTxt);
            }
            catch (System.Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                throw new ApplicationException(ex.ToString());
            }
        }

        private void InitDatabases()
        {
            try
            {
                BsonSerializer.RegisterSerializer(DateTimeSerializer.LocalInstance);
                CounterMongoDB.RegisterClass();
                UserLoginMongoDB.RegisterClass();
                GamerMongoDB.RegisterClass();
                CharacterGamerMongoDB.RegisterClass();
                DiceGamerMongoDB.RegisterClass();
                BattleMongoDB.RegisterClass();
                BattleReplayMongoDB.RegisterClass();
            }
            catch (System.Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                throw new ApplicationException(ex.ToString());
            }
        }

        async Task HTTPRequest(HttpContext context)
        {
            try
            {
                string methodName = null;
                string body = null;
                byte[] binaryData = null;
                string iv = null;
                string data = null;

                foreach (string key in context.Request.Form.Keys)
                {
                    if (key == "binary")
                    {
                        string binaryStr = context.Request.Form[key].ToString();
                        //binaryStr.Trim();
                        binaryData = Convert.FromBase64String(binaryStr);
                        //binaryData = ReadText.DecompressByteArray(binaryData, false);
                    }
                    else if (key == "iv")
                    {
                        iv = context.Request.Form[key];
                    }
                    else
                    {
                        methodName = key;
                        body = context.Request.Form[key];
                        data = body.Trim();
                    }
                }
                //string responseData = ProcessRequest(methodName, data, iv, binaryData);
                bool encrypted = true;
                data = ReadText.DecompressString(data, iv, encrypted);
                MethodInfo mi = (typeof(BaseWebService)).GetMethod(methodName);
                string responseData = null;

                if (binaryData != null)
                {
                    responseData = (string)mi.Invoke(null, new object[] { data, binaryData });
                }
                else
                {
                    responseData = (string)mi.Invoke(null, new object[] { data });
                }

                // TODO: add response cache on gid request to prevent brute-force send same request
                Console.Out.WriteLine("response :" + responseData);
                string new_iv = encrypted ? HikerAes.GenerateIV() : string.Empty;
                responseData = ReadText.CompressString(responseData, new_iv, encrypted);
                var response = new Dictionary<string, string>();
                response.Add("data", responseData);
                response.Add("iv", new_iv);
                await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(response));
            }
            catch (Exception e)
            {
                var encrypted = false;
                string new_iv = encrypted ? HikerAes.GenerateIV() : string.Empty;
                string responseData = "Invalid Request";
                var response = new Dictionary<string, string>();
                response.Add("data", responseData);
                response.Add("iv", new_iv);
                await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(response));
            }
        }

        async Task HTTPRequestWithMethod(HttpContext context)
        {
            try
            {
                var paths = context.Request.Path.ToString().Split('/');
                string methodName = paths.Last();
                MethodInfo mi = (typeof(BaseWebService)).GetMethod(methodName);

                bool encrypted = true;
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);

                var responseData = (string)mi.Invoke(null, new object[] { body });
                // TODO: add response cache on gid request to prevent brute-force send same request
                Console.Out.WriteLine("response :" + responseData);
                //string new_iv = encrypted ? HikerAes.GenerateIV() : string.Empty;
                //responseData = ReadText.CompressString(responseData, new_iv, encrypted);
                //var response = new Dictionary<string, string>();
                //response.Add("data", responseData);
                //response.Add("iv", new_iv);
                await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(responseData));
            }
            catch (Exception e)
            {
                var encrypted = false;
                string new_iv = encrypted ? HikerAes.GenerateIV() : string.Empty;
                string responseData = "Invalid Request";
                var response = new Dictionary<string, string>();
                response.Add("data", responseData);
                response.Add("iv", new_iv);
                await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(response));
            }
        }

        async Task HTTPCheckConnect(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                await context.Response.WriteAsync(body);
            }
            catch (Exception e)
            {
                var encrypted = false;
                string new_iv = encrypted ? HikerAes.GenerateIV() : string.Empty;
                string responseData = "Invalid Request";
                var response = new Dictionary<string, string>();
                response.Add("data", responseData);
                response.Add("iv", new_iv);
                await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(response));
            }
        }

        /*async Task HandleGMToolRequest(HttpContext context)
        {
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                context.Request.EnableBuffering();
                string methodName = null;
                string data = null;

                foreach (string key in context.Request.Query.Keys)
                {
                    string query = context.Request.Query[key];
                    query = query.Trim();
                    if (key == "method")
                    {
                        methodName = query;
                    }
                    else if (key == "data")
                    {
                        data = query;
                    }
                }

                if (context.Request.Method == HttpMethods.Post)
                {
                    foreach (string key in context.Request.Form.Keys)
                    {
                        string value = context.Request.Form[key];
                        value = value.Trim();
                        if (key == "method")
                        {
                            methodName = value;
                        }
                        else if (key == "data")
                        {
                            data = value;
                        }
                    }
                    using var reader = new StreamReader(context.Request.Body);
                    data = await reader.ReadToEndAsync();
                }
                //string responseData = ProcessRequest(methodName, data, iv, binaryData);
                bool encrypted = true;
                MethodInfo mi = (typeof(BaseWebService)).GetMethod(methodName);
                string responseData = null;

                if (data != null)
                {
                    responseData = (string)mi.Invoke(null, new object[] { data });
                }
                else
                {
                    responseData = (string)mi.Invoke(null, new object[] { });
                }

                if (context.Request.Method == HttpMethods.Get)
                {
                    await context.Response.WriteAsync(responseData);
                }
                else
                {
                    // TODO: add response cache on gid request to prevent brute-force send same request
                    Console.Out.WriteLine("response :" + responseData);
                    string new_iv = encrypted ? HikerAes.GenerateIV() : string.Empty;
                    responseData = ReadText.CompressString(responseData, new_iv);
                    var response = new Dictionary<string, string>();
                    response.Add("data", responseData);
                    response.Add("iv", new_iv);
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                }
            }
            catch (Exception e)
            {
                var encrypted = false;
                string new_iv = encrypted ? HikerAes.GenerateIV() : string.Empty;
                string responseData = "Invalid Request";
                var response = new Dictionary<string, string>();
                response.Add("data", responseData);
                response.Add("iv", new_iv);
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }*/

        async Task GMToolUserManager(HttpContext context)
        {
            var response = new GMToolUserManagerResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                var bodyJson = JsonMapper.ToObject(body);

                response.ErrorCode = ErrorCode.OK;
                response.totalUsersCount = GamerMongoDB.GetTotalGamersCount();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestUserManager " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolGetUsersList(HttpContext context)
        {
            var response = new GMToolGetUsersListResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                //var bodyJson = JsonMapper.ToObject(body);

                var request = JsonMapper.ToObject<GMToolGetUsersListRequest>(body);
                var gamersList = GamerMongoDB.GetGamersList(request.fromIdx, request.toIdx);
                response.usersList = new List<GMToolUserData>();
                var i = 0;
                foreach (var gamerData in gamersList)
                {
                    i++;
                    var userData = new GMToolUserData()
                    {
                        index = i,
                        GID = gamerData.ID,
                        userName = gamerData.displayName,
                    };
                    response.usersList.Add(userData);
                }
                response.ErrorCode = ErrorCode.OK;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestGetUsersList " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolLockUser(HttpContext context)
        {
            var response = new GMToolLockUserResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                //var bodyJson = JsonMapper.ToObject(body);

                var request = JsonMapper.ToObject<GMToolLockUserRequest>(body);
                response.ErrorCode = ErrorCode.OK;
                response.Message = "LOCK_USER_SUCCESS!";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestGetUsersList " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolSendRewardToUser(HttpContext context)
        {
            var response = new GMToolSendRewardToUserResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                //var bodyJson = JsonMapper.ToObject(body);

                var request = JsonMapper.ToObject<GMToolSendRewardToUserRequest>(body);
                response.ErrorCode = ErrorCode.OK;
                response.Message = "LOCK_USER_SUCCESS!";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestGetUsersList " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolDashboardBattleLogs(HttpContext context)
        {
            var response = new GMToolGetDashboardBattleLogsResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                var request = JsonMapper.ToObject<GMToolGetDashboardBattleLogsRequest>(body);

                response.ErrorCode = ErrorCode.OK;
                if (request.logType == BattleLogType.All)
                {

                }
                else if (request.logType == BattleLogType.Playing)
                {
                    var battlePropertiesList = BattleMongoDB.GetAllsPlayingList();

                }
                
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestUserManager " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolGamerBattleLogs(HttpContext context)
        {
            var response = new GMToolGetGamerBattleLogsResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                var request = JsonMapper.ToObject<GMToolGetGamerBattleLogsRequest>(body);

                response.ErrorCode = ErrorCode.OK;
                var battlePropertiesList = BattleMongoDB.GetAllsPlayingList();

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestUserManager " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolThongKeNapGold(HttpContext context)
        {
            var response = new GMToolThongKeNapGoldResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                var request = JsonMapper.ToObject<GMToolThongKeNapGoldRequest>(body);

                response.ErrorCode = ErrorCode.OK;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestUserManager " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolThongKeTieuGold(HttpContext context)
        {
            var response = new GMToolThongKeTieuGoldResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                var request = JsonMapper.ToObject<GMToolThongKeTieuGoldRequest>(body);

                response.ErrorCode = ErrorCode.OK;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestUserManager " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolThongKeNapDiamond(HttpContext context)
        {
            var response = new GMToolThongKeNapDiamondResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                var request = JsonMapper.ToObject<GMToolThongKeNapDiamondRequest>(body);

                response.ErrorCode = ErrorCode.OK;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestUserManager " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolThongKeTieuDiamond(HttpContext context)
        {
            var response = new GMToolThongKeTieuDiamondResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                var request = JsonMapper.ToObject<GMToolThongKeTieuDiamondRequest>(body);

                response.ErrorCode = ErrorCode.OK;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestUserManager " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }

        async Task GMToolThongKeGiaiDau(HttpContext context)
        {
            var response = new GMToolThongKeGiaiDauResponse();
            try
            {
                context.Request.Headers.TryGetValue("X-Client-Id", out var xClientID);
                context.Request.Headers.TryGetValue("X-Client-Secret", out var xClientSecret);
                if (!xClientID.Equals(GameManager.API_CLIENT_ID) || !xClientSecret.Equals(GameManager.API_CLIENT_SECRET))
                {
                    response.ErrorCode = ErrorCode.DISPLAY_MESSAGE;
                    response.Message = "Missing or invalid CLIENT-ID/ CLIENT-SECRET";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonMapper.ToJson(response));
                    return;
                }

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                // do something
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                var request = JsonMapper.ToObject<GMToolThongKeGiaiDauRequest>(body);

                response.ErrorCode = ErrorCode.OK;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add("GMToolRequestUserManager " + ex.ToString());
                response.ErrorCode = ErrorCode.UNKNOW_ERROR;
                response.Message = "Unknown error if any.";
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonMapper.ToJson(response));
            }
        }
    }
}
