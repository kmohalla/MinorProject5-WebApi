using GardenModels;
using GardenBusiness;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace GardenApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GardenBedsController : ControllerBase
{
    private readonly GardenBedManager _gardenBedManager;

    public GardenBedsController(GardenBedManager gardenBedManager)
    {
        _gardenBedManager = gardenBedManager;
    }
    
    // GET: api/GardenBeds
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GardenBedDTO>>> GetGardenBeds()
    {
        var gardenbeds = await _gardenBedManager.GetAllGardenBeds();
        List<GardenBedDTO> bedDTO = new List<GardenBedDTO>();
        if (gardenbeds == null || gardenbeds.Count == 0)
        {
            return NotFound();
        }

        foreach (var bed in gardenbeds)
        {
            bedDTO.Add(GardenBedManager.ConvertToDTO(bed));
        }
        return Ok(bedDTO);
    }

    // GET: api/GardenBeds/1
    [HttpGet("{id}")]
    public async Task<ActionResult<GardenBedDTO>> GetGardenBed(int id)
    {
        var gardenBed = await _gardenBedManager.GetGardenBedById(id);
        if (gardenBed is null)
        {
            return NotFound();
        }

        return Ok(GardenBedManager.ConvertToDTO(gardenBed));
    }
   
    // POST: api/GardenBeds
    [HttpPost]
    public async Task<ActionResult<GardenBedDTO>> Create([FromBody]GardenBedDTO gardenBed)
    {
        try
        {
            String inserted = await _gardenBedManager.SaveGardenBed(await _gardenBedManager.ConvertToData(gardenBed), true);
            return Ok(inserted);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }
    }

    // PUT: api/GardenBeds/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id,[FromBody] GardenBedDTO gardenBed)
    {
        if (id != gardenBed.ID)
        {
            return BadRequest();
        }

        var existing=await _gardenBedManager.GetGardenBedById(id);
        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            //String updated = await _gardenBedManager.SaveGardenBed(await _gardenBedManager.ConvertToData(gardenBed), false);
            await _gardenBedManager.UpdateFromDTO(existing, gardenBed);
            String updated = await _gardenBedManager.SaveGardenBed(existing, false);
            return Ok(updated);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }

    }

    // DELETE: api/GardenBeds/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _gardenBedManager.GetGardenBedById(id);

        if(existing == null)
        {
            return NotFound();
        }

        try {
            var succeeded= await _gardenBedManager.DeleteGardenBed(id); 
            return Ok(succeeded);
        }
        catch(ApplicationException ae)
        {
            return StatusCode(403, ae.Message);
        }

    }

}