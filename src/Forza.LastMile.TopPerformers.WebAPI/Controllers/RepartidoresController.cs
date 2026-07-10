using Forza.LastMile.TopPerformers.Application.Features.Repartidores.Commands.AjustarMeta;
using Forza.LastMile.TopPerformers.Application.Performers.Commands;
using Forza.LastMile.TopPerformers.Application.Performers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Forza.LastMile.TopPerformers.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepartidoresController : ControllerBase
{
private readonly IMediator _mediator;

public RepartidoresController(IMediator mediator)
{
_mediator = mediator;
}

/// <summary>
/// Obtiene el ranking de repartidores del per�odo actual ordenados por entregas completadas (descendente).
/// </summary>
[HttpGet]
public async Task<ActionResult<List<RepartidorRendimientoDto>>> GetTopPerformers(CancellationToken cancellationToken = default)
{
var query = new GetTopPerformersQuery();
var result = await _mediator.Send(query, cancellationToken);
return Ok(result);
}

/// <summary>
/// Ajusta la meta diaria de un repartidor por incidencia.
/// </summary>
[HttpPatch("{id:guid}/meta")]
public async Task<ActionResult> AjustarMeta(Guid id, [FromBody] AjustarMetaRequest request, CancellationToken cancellationToken = default)
{
Console.WriteLine($"DEBUG: Recibí NuevaMeta={request.NuevaMetaSugerida} y Descripcion={request.IncidenciaDescripcion}");

if (request == null)
{
return BadRequest("El cuerpo de la solicitud no puede estar vac�o.");
}

if (string.IsNullOrWhiteSpace(request.IncidenciaDescripcion))
{
return BadRequest("La descripci�n de la incidencia es requerida.");
}

if (request.NuevaMetaSugerida <= 0)
{
return BadRequest("La nueva meta sugerida debe ser mayor que cero.");
}

var command = new AjustarMetaPorIncidenciaCommand(
id,
request.IncidenciaDescripcion,
request.NuevaMetaSugerida);

var success = await _mediator.Send(command, cancellationToken);

if (!success)
{
return BadRequest($"No se encontr� el repartidor con ID {id}.");
}

return Ok();
}

[HttpPatch("{id:guid}/ajustar-meta")]
public async Task<ActionResult> AjustarMetaPorComando(Guid id, [FromBody] AjustarMetaCommandRequest request, CancellationToken cancellationToken = default)
{
if (request == null)
{
return BadRequest("El cuerpo de la solicitud no puede estar vac�o.");
}

if (request.NuevaMeta <= 0)
{
return BadRequest("El campo nueva_meta debe ser mayor que cero.");
}

if (string.IsNullOrWhiteSpace(request.Descripcion))
{
return BadRequest("El campo descripcion es requerido.");
}

var command = new AjustarMetaCommand
    {
        RepartidorId = id,
        NuevaMeta = request.NuevaMeta,
        Descripcion = request.Descripcion
    };

    try
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
    catch (KeyNotFoundException)
    {
        return NotFound();
    }
}
}

public sealed record AjustarMetaRequest(string IncidenciaDescripcion, int NuevaMetaSugerida);

public sealed record AjustarMetaCommandRequest(
[property: JsonPropertyName("nueva_meta")] int NuevaMeta,
[property: JsonPropertyName("descripcion")] string Descripcion);
