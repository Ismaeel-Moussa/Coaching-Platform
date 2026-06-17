using Autofac;
using JokerNutrition.Api.Filters;

namespace JokerNutrition.Api.Autofac;

public class ControllersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ApiExceptionFilter>().InstancePerLifetimeScope();
    }
}
