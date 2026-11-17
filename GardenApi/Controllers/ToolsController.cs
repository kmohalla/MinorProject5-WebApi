using GardenModels;
using GardenBusiness;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace GardenApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ToolsController : ControllerBase
{
    private readonly ToolManager _toolManager;

    public ToolsController(ToolManager toolManager)
    {
        _toolManager = toolManager;
    }

    // GET: api/Tools
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ToolDTO>>> GetTools()
    {
        var tools = await _toolManager.GetAllTools();
        List<ToolDTO> toolDTO = new List<ToolDTO>();
        if (tools == null || tools.Count == 0)
        {
            return NotFound();
        }

        foreach (var tool in tools)
        {
            toolDTO.Add(ToolManager.ConvertToDTO(tool));
        }
        return Ok(toolDTO);
    }

    // GET: api/Tools/1
    [HttpGet("{id}")]
    public async Task<ActionResult<ToolDTO>> GetTool(int id)
    {
        var tool = await _toolManager.GetToolById(id);
        if (tool is null)
        {
            return NotFound();
        }

        return Ok(ToolManager.ConvertToDTO(tool));
    }

    // POST: api/Tools
    [HttpPost]
    public async Task<ActionResult<ToolDTO>> Create([FromBody] ToolDTO tool)
    {
        try
        {
            String inserted = await _toolManager.SaveTool(ToolManager.ConvertToData(tool), true);
            return Ok(inserted);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }
    }

    // PUT: api/Tools/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ToolDTO tool)
    {
        if (id != tool.ID)
        {
            return BadRequest();
        }

        var existing = await _toolManager.GetToolById(id);
        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            //String updated = await _gardenBedManager.SaveGardenBed(await _gardenBedManager.ConvertToData(gardenBed), false);
            ToolManager.UpdateFromDTO(existing, tool);
            String updated = await _toolManager.SaveTool(existing, false);
            return Ok(updated);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }

    }

    // DELETE: api/Tools/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _toolManager.GetToolById(id);

        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            var succeeded = await _toolManager.DeleteTool(id);
            return Ok(succeeded);
        }
        catch (ApplicationException ae)
        {
            return StatusCode(403, ae.Message);
        }

    }

}
