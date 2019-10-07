using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.NeuChar.App.AppStore;
using Senparc.NeuChar.Entities;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageContexts;
using Senparc.Weixin.MP.MessageHandlers.Middleware;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.Work;
using Senparc.Weixin.WxOpen;

namespace WechatMessageSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();//使用本地缓存必须添加

            services.AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            //引入EnableRequestRewind中间件
            app.UseEnableRequestRewind();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.UseSenparcGlobal(env, senparcSetting.Value, globalRegister => { })
               //使用 Senparc.Weixin SDK
               .UseSenparcWeixin(senparcWeixinSetting.Value, weixinRegister =>
               {
                   #region 注册公众号或小程序（按需）

                   //注册公众号（可注册多个）
                   weixinRegister
                          .RegisterMpAccount(senparcWeixinSetting.Value, "【盛派网络小助手】公众号")
                          //注册多个公众号或小程序（可注册多个）
                          .RegisterWxOpenAccount(senparcWeixinSetting.Value, "【盛派网络小助手】小程序")
                   #endregion

                   #region 注册企业号（按需）

                          //注册企业微信（可注册多个）
                          .RegisterWorkAccount(senparcWeixinSetting.Value, "【盛派网络】企业微信");
                   #endregion
               });

            //使用 公众号 MessageHandler 中间件
            app.UseMessageHandlerForMp("/Weixin", CustomMessageHandler.GenerateMessageHandler,
                o => o.AccountSettingFunc = c => senparcWeixinSetting.Value);
        }
    }

    /// <summary>
    /// 出于展示方便，写在同一个文件中，实际开发建议分离到独立文件
    /// </summary>
    public class CustomMessageHandler : Senparc.Weixin.MP.MessageHandlers.MessageHandler<DefaultMpMessageContext>
    {
        /// <summary>
        /// 为中间件提供生成当前类的委托
        /// </summary>
        public static Func<Stream, PostModel, int, CustomMessageHandler> GenerateMessageHandler = (stream, postModel, maxRecordCount)
                        => new CustomMessageHandler(stream, postModel, maxRecordCount, false/* 是否只允许处理加密消息，以提高安全性 */);


        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0, bool onlyAllowEcryptMessage = false, DeveloperInfo developerInfo = null)
            : base(inputStream, postModel, maxRecordCount, onlyAllowEcryptMessage, developerInfo)
        {
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "这是一条默认消息";
            return responseMessage;
        }
    }
}
