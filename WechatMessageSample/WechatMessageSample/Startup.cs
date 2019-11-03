using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.WebSocket;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;

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
            services.AddSenparcWeixinServices(Configuration)//Senparc.Weixin 注册	
                    .AddSenparcWebSocket<CustomWebSocketMessageHandler>();//注册 CustomWebSocketMessageHandler
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<SenparcHub>("/SenparcHub");
            });

            app.UseSenparcGlobal(env, senparcSetting.Value, globalRegister => { /* 全局注册设置 */});


        }
    }
}