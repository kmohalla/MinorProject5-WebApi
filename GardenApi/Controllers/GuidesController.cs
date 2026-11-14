using GardenModels;
using GardenBusiness;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace GardenApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GuidesController : ControllerBase
{
    private readonly GuideManager _guideManager;

    public GuidesController(GuideManager guideManager)
    {
        _guideManager = guideManager;
    }

    // GET: api/Guides
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuideDTO>>> GetGuides()
    {
        var guides = await _guideManager.GetAllGuides();
        List<GuideDTO> guideDTO = new List<GuideDTO>();
        if (guides == null || guides.Count == 0)
        {
            return NotFound();
        }

        foreach (var guide in guides)
        {
            guideDTO.Add(GuideManager.ConvertToDTO(guide));
        }
        return Ok(guideDTO);
    }

    // GET: api/Guides/1
    [HttpGet("{id}")]
    public async Task<ActionResult<GuideDTO>> GetGuide(int id)
    {
        var guide = await _guideManager.GetGuideById(id);
        if (guide is null)
        {
            return NotFound();
        }

        return Ok(GuideManager.ConvertToDTO(guide));
    }

    // POST: api/Guides
    [HttpPost]
    public async Task<ActionResult<GuideDTO>> Create([FromBody] GuideDTO guide)
    {
        try
        {
            String inserted = await _guideManager.SaveGuide( GuideManager.ConvertToData(guide), true);
            return Ok(inserted);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }
    }

    // PUT: api/Guides/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GuideDTO guide)
    {
        if (id != guide.ID)
        {
            return BadRequest();
        }

        var existing = await _guideManager.GetGuideById(id);
        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            //String updated = await _gardenBedManager.SaveGardenBed(await _gardenBedManager.ConvertToData(gardenBed), false);
            GuideManager.UpdateFromDTO(existing, guide);
            String updated = await _guideManager.SaveGuide(existing, false);
            return Ok(updated);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }

    }

    // DELETE: api/Guides/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _guideManager.GetGuideById(id);

        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            var succeeded = await _guideManager.DeleteGuide(id);
            return Ok(succeeded);
        }
        catch (ApplicationException ae)
        {
            return StatusCode(403, ae.Message);
        }

    }

}
