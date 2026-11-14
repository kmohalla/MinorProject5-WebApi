using GardenModels;
using GardenBusiness;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace GardenApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlantsController : ControllerBase
{
    private readonly PlantManager _plantManager;

    public PlantsController(PlantManager plantManager)
    {
        _plantManager = plantManager;
    }

    // GET: api/Plants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantDTO>>> GetPlants()
    {
        var plants = await _plantManager.GetAllPlants();
        List<PlantDTO> plantDTO = new List<PlantDTO>();
        if (plants == null || plants.Count == 0)
        {
            return NotFound();
        }

        foreach (var plant in plants)
        {
            plantDTO.Add(PlantManager.ConvertToDTO(plant));
        }
        return Ok(plantDTO);
    }

    // GET: api/Plants/1
    [HttpGet("{id}")]
    public async Task<ActionResult<PlantDTO>> GetPlant(int id)
    {
        var plant = await _plantManager.GetPlantById(id);
        if (plant is null)
        {
            return NotFound();
        }

        return Ok(PlantManager.ConvertToDTO(plant));
    }

    // POST: api/Plants
    [HttpPost]
    public async Task<ActionResult<PlantDTO>> Create([FromBody] PlantDTO plant)
    {
        try
        {
            String inserted = await _plantManager.SavePlant( await _plantManager.ConvertToData(plant), true);
            return Ok(inserted);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }
    }

    // PUT: api/Plants/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PlantDTO plant)
    {
        if (id != plant.ID)
        {
            return BadRequest();
        }

        var existing = await _plantManager.GetPlantById(id);
        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            //String updated = await _gardenBedManager.SaveGardenBed(await _gardenBedManager.ConvertToData(gardenBed), false);
            await _plantManager.UpdateFromDTO(existing, plant);
            String updated = await _plantManager.SavePlant(existing, false);
            return Ok(updated);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }

    }

    // DELETE: api/Plants/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _plantManager.GetPlantById(id);

        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            var succeeded = await _plantManager.DeletePlant(id);
            return Ok(succeeded);
        }
        catch (ApplicationException ae)
        {
            return StatusCode(403, ae.Message);
        }

    }

}