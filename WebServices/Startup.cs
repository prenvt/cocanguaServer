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
                    string welcome_str = "Welcome to Monopoly - LOCAL";
#if SOHA
                    welcome_str = "Welcome to Monopoly : " + Configuration.GetValue<string>("SERVER_TYPE");
#endif

                    await context.Response.WriteAsync(welcome_str);
                });
                //endpoints.MapRazorPages();
                //endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapHub<BattleHub>("/battleHub");
                endpoints.MapPost("/game/request", HTTPRequest);
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
                StarCardGamerMongoDB.RegisterClass();
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
    }
}
