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
            services.AddMemoryCache();//使用本地缓存必须添加（按需）

            services.AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
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

            app.UseSenparcGlobal(env, senparcSetting.Value, globalRegister => { /* 全局注册设置 */})
               //使用 Senparc.Weixin SDK
               .UseSenparcWeixin(senparcWeixinSetting.Value, weixinRegister =>
               {
                   //注册公众号（可注册多个）
                   weixinRegister.RegisterMpAccount(senparcWeixinSetting.Value, "【盛派网络小助手】公众号");
               });

            //使用 公众号 MessageHandler 中间件
            app.UseMessageHandlerForMp("/Weixin", CustomMpMessageHandler.GenerateMessageHandler,
                o => o.AccountSettingFunc = c => senparcWeixinSetting.Value);

            //小程序、企业号使用相同方法注册，参考：
            //https://github.com/JeffreySu/WeiXinMPSDK/blob/master/Samples/netcore3.0-mvc/Senparc.Weixin.Sample.NetCore3/Startup.cs
        }
    }

    /// <summary>
    /// 自定义公众号消息处理
    /// 出于展示方便，写在同一个文件中，实际开发建议分离到独立文件
    /// </summary>
    public class CustomMpMessageHandler : Senparc.Weixin.MP.MessageHandlers.MessageHandler<DefaultMpMessageContext>
    {
        /// <summary>
        /// 为中间件提供生成当前类的委托
        /// </summary>
        public static Func<Stream, PostModel, int, CustomMpMessageHandler> GenerateMessageHandler = (stream, postModel, maxRecordCount)
                        => new CustomMpMessageHandler(stream, postModel, maxRecordCount, false/* 是否只允许处理加密消息，以提高安全性 */);

        public CustomMpMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0, bool onlyAllowEcryptMessage = false, DeveloperInfo developerInfo = null)
            : base(inputStream, postModel, maxRecordCount, onlyAllowEcryptMessage, developerInfo)
        {
        }

        public override async Task<IResponseMessageBase> OnTextRequestAsync(RequestMessageText requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = $"您发送了文字：{requestMessage.Content}";
            return responseMessage;
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageNews>();
            responseMessage.Articles.Add(new Article()
            {
                Title = "欢迎使用 Senparc.Weixin SDK",
                Description = "这是一条默认消息",
                PicUrl = "https://sdk.weixin.senparc.com/images/v2/logo.png",
                Url = "https://weixin.senparc.com"
            });
            return responseMessage;
        }
    }
}
