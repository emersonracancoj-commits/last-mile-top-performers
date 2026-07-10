using FluentValidation;
using MediatR;
using System.Text.Json.Serialization; 

namespace Forza.LastMile.TopPerformers.Application.Features.Repartidores.Commands.AjustarMeta;

public sealed class AjustarMetaCommand : IRequest
{
    public Guid RepartidorId { get; init; }

    [JsonPropertyName("nuevaMeta")] // Obliga a que coincida con Postman
    public int NuevaMeta { get; init; }
    
    [JsonPropertyName("descripcion")] // Obliga a que coincida con Postman
    public string Descripcion { get; init; } = string.Empty;
}

public sealed class AjustarMetaCommandValidator : AbstractValidator<AjustarMetaCommand>
{
    public AjustarMetaCommandValidator()
    {
        RuleFor(x => x.NuevaMeta)
            .GreaterThan(0)
            .WithMessage("La nueva meta debe ser mayor a 0.");
            
        RuleFor(x => x.Descripcion) // Agrega esta validación
            .NotEmpty()
            .WithMessage("La descripción es requerida.")
            .MinimumLength(5)
            .WithMessage("La descripción debe tener al menos 5 caracteres.");
    }
}
