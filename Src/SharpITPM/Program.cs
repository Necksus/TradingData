
using System.Diagnostics.Metrics;
using IPTMGrabber.InvestorWebsite;
using IPTMGrabber.Utils;
using SharpITPM.Components;

namespace SharpITPM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Blazor stuff
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            // Add services to the container.
            builder.Services.AddITPM();

            //builder.Services.AddRazorPages();
            //builder.Services.AddServerSideBlazor();
            builder.Services.AddBlazorBootstrap();

            // Add API stuff
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            //app.UseRouting();

            //app.MapBlazorHub();
            //app.MapFallbackToPage("/_Host");

            //app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Counter<>).Assembly);       // TODO: remove this

            app.Run();
        }
    }
}