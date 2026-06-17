using Autofac;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Business.Services;

namespace JokerNutrition.Business.Autofac;

public class ServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<JwtTokenHelper>().As<IJwtTokenHelper>().InstancePerLifetimeScope();
        builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();
        builder.RegisterType<AuthService>().As<IAuthService>().InstancePerLifetimeScope();
        builder.RegisterType<InvitationService>().As<IInvitationService>().InstancePerLifetimeScope();
    }
}
