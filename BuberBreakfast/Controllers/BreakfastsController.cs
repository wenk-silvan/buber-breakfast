using BuberBreakfast.Contracts.Breakfast;
using BuberBreakfast.Models;
using BuberBreakfast.Services.Breakfasts;
using Microsoft.AspNetCore.Mvc;

namespace BuberBreakfast.Controllers;

public class BreakfastsController : ApiController
{
    private readonly IBreakfastService _breakfastService;

    public BreakfastsController(IBreakfastService breakfastService)
    {
        _breakfastService = breakfastService;
    }

    [HttpPost]
    public IActionResult CreateBreakfast(CreateBreakfastRequest request)
    {
        var requestToBreakfastResult = Breakfast.From(request);

        if (requestToBreakfastResult.IsError)
        {
            return Problem(requestToBreakfastResult.Errors);
        }

        var breakfast = requestToBreakfastResult.Value;
        var createBreakfastResult = _breakfastService.CreateBreakfast(breakfast);

        return createBreakfastResult.Match(
            onValue: created => CreatedAtGetBreakfast(breakfast),
            onError: Problem
        );
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetBreakfast(Guid id)
    {
        var getBreakfastResult = _breakfastService.GetBreakfast(id);

        return getBreakfastResult.Match(
            onValue: breakfast => Ok(MapBreakfastResponse(breakfast)),
            onError: Problem
        );
    }

    [HttpPut("{id:guid}")]
    public IActionResult UpsertBreakfast(Guid id, UpsertBreakfastRequest request)
    {
        var requestToBreakfastResult = Breakfast.From(id, request);
        
        if (requestToBreakfastResult.IsError)
        {
            return Problem(requestToBreakfastResult.Errors);
        }

        var breakfast = requestToBreakfastResult.Value;
        var upsertedBreakfastResult = _breakfastService.UpsertBreakfast(breakfast);
        
        return upsertedBreakfastResult.Match(
            onValue: upserted => upserted.IsNewlyCreated ? CreatedAtGetBreakfast(breakfast) : NoContent(),
            onError: Problem
        );
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteBreakfast(Guid id)
    {
        var deleteBreakfastResult = _breakfastService.DeleteBreakfast(id);

        return deleteBreakfastResult.Match(
            onValue: deleted => NoContent(),
            onError: Problem
        );
    }

    private static BreakfastResponse MapBreakfastResponse(Breakfast breakfast)
    {
        var response = new BreakfastResponse(
            breakfast.Id,
            breakfast.Name,
            breakfast.Description,
            breakfast.StartDateTime,
            breakfast.EndDateTime,
            breakfast.LastModifiedDateTime,
            breakfast.Savory,
            breakfast.Sweet
        );
        return response;
    }

    private CreatedAtActionResult CreatedAtGetBreakfast(Breakfast breakfast)
    {
        return CreatedAtAction(
            actionName: nameof(GetBreakfast),
            new { id = breakfast.Id },
            value: MapBreakfastResponse(breakfast)
        );
    }
}