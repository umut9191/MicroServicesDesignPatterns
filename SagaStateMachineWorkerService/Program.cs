using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachineWorkerService;
using SagaStateMachineWorkerService.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Shared;

IHost host = Host.CreateDefaultBuilder(args)

    .ConfigureServices((ctx, services) =>
    {
    services.AddHostedService<Worker>();

    services.AddMassTransit(cfg => {

    cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>
        ().EntityFrameworkRepository(opt =>
        {
            opt.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
            {
                builder.UseSqlServer(ctx.Configuration.GetConnectionString("SqlCon"), m =>
                {
                    m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                });
            });
        });

    cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configure =>{
        configure.Host(ctx.Configuration.GetConnectionString("RabbitMQAMQPURL"));
        configure.ReceiveEndpoint(RabbitMqSettingsConsts.OrderSaga, e => {
            e.ConfigureSaga<OrderStateInstance>(provider);
        
        });
    
    }));

        });
    })
    .Build();

await host.RunAsync();
