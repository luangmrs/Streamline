using MediatR;
using AutoMapper;
using Streamline.API.Dtos;
using Streamline.Application.Commands;
using Streamline.Application.Queries;
using Streamline.Domain.Enums;
using Streamline.API.Monitoring;

namespace Streamline.API.Routes
{
    public static class OrderRoutes
    {
        public static void MapOrderRoutes(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup("/order")
                .WithTags("Order");

            group.MapGet("/", async (
                IMediator mediator,
                int page = 1,
                int limit = 10,
                EStatusOrder? status = null,
                int? customerId = null,
                DateTime? createdFrom = null,
                DateTime? createdTo = null) =>
            {
                var query = new ListOrderQuery
                {
                    Page = Math.Max(page, 1),
                    Limit = Math.Max(limit, 10),
                    Status = status,
                    CustomerId = customerId,
                    CreatedFrom = createdFrom,
                    CreatedTo = createdTo
                };

                var result = await mediator.Send(query);
                StreamlineMetrics.OrderListRequests.Inc();
                return Results.Ok(result);
            })
            .WithMetadata(new Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute(
                summary: "List orders",
                description: "Returns orders optionally filtered by status, customer, and creation date."
            ));

            group.MapGet("/{id}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetOrderByIdQuery { Id = id });
                return Results.Ok(result);
            })
            .WithMetadata(new Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute(
                summary: "Get order by ID",
                description: "Retrieves a single order along with its customer and product details based on the specified order ID."
            ));

            group.MapPost("/", async (
                CreateOrderDto dto,
                IMediator mediator,
                IMapper mapper) =>
            {
                var command = mapper.Map<CreateOrderCommand>(dto);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .WithMetadata(new Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute(
                summary: "Create a new order",
                description: "Creates a new order for a customer with one or more products. Validates the input data and returns the created order's details."
            ));


            group.MapPatch("/{id}/pay", async (int id, IMediator mediator) =>
            {   
                // TODO: implementar DTO com dados de pagamento.
                var result = await mediator.Send(new PayOrderByIdCommand { Id = id });
                return Results.Ok(result);
            })
            .WithMetadata(new Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute(
                summary: "Pay an order by ID",
                description: "Processes the payment for a specific order."
            ));

            group.MapPatch("/{id}/cancel", async (int id, IMediator mediator) =>
            {   
                // TODO: implementar DTO com dados de cancelamento: motivo, etc..
                var result = await mediator.Send(new CancelOrderByIdCommand { Id = id });
                return Results.Ok(result);
            })
            .WithMetadata(new Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute(
                summary: "Cancel an order by ID",
                description: "Cancels a specific order by its ID. Once canceled, the order's status will be updated and it cannot be processed further."
            ));


            group.MapPut("{id}/update-order", async (
                int id,
                UpdateOrderDto dto,
                IMediator mediator,
                IMapper mapper) =>
            {
                var command = mapper.Map<UpdateOrderByIdCommand>(dto);
                command.OrderId = id;
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .WithMetadata(new Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute(
                summary: "Update an existing order",
                description: "Updates the details of an existing order, including products and quantities based on the provided order ID."
            ));

            group.MapDelete("/{id}", async (int id, IMediator mediator) =>
            {
                await mediator.Send(new DeleteOrderByIdCommand { Id = id });
                return Results.NoContent();
            })
            .WithMetadata(new Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute(
                summary: "Delete order by ID",
                description: "Deletes a specific order based on the provided order ID. Once deleted, the order cannot be recovered."
            ));
        }
    }
}
