using Autofac;
using QQBot.Application;
using QQBot.Utils;

namespace QQBot.Web
{
    public class DependencyRegistrationModule : Module
    {
        public DependencyRegistrationModule()
        {
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType(typeof(WSocketClientHelp)).SingleInstance().PropertiesAutowired();
            builder.RegisterType(typeof(QLPanelService)).InstancePerRequest().PropertiesAutowired();
            builder.RegisterType(typeof(GoCQHttpHelper)).InstancePerRequest().PropertiesAutowired();
            builder.RegisterType<MessageProcess>().InstancePerRequest().PropertiesAutowired();
            //builder.RegisterType<JDScriptsTask>().SingleInstance().PropertiesAutowired();
            builder.RegisterType<QLHttpService>().InstancePerRequest().PropertiesAutowired();
        }
    }
}