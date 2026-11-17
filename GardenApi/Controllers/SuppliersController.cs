using GardenModels;
using GardenBusiness;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace GardenApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SuppliersController : ControllerBase
{
    private readonly SupplierManager _supplierManager;

    public SuppliersController(SupplierManager supplierManager)
    {
        _supplierManager = supplierManager;
    }

    // GET: api/Suppliers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDTO>>> GetSuppliers()
    {
        var suppliers = await _supplierManager.GetAllSuppliers();
        List<SupplierDTO> supplierDTO = new List<SupplierDTO>();
        if (suppliers == null || suppliers.Count == 0)
        {
            return NotFound();
        }

        foreach (var supplier in suppliers)
        {
            supplierDTO.Add(SupplierManager.ConvertToDTO(supplier));
        }
        return Ok(supplierDTO);
    }

    // GET: api/Suppliers/1
    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDTO>> GetSupplier(int id)
    {
        var supplier = await _supplierManager.GetSupplierById(id);
        if (supplier is null)
        {
            return NotFound();
        }

        return Ok(SupplierManager.ConvertToDTO(supplier));
    }

    // POST: api/Suppliers
    [HttpPost]
    public async Task<ActionResult<SupplierDTO>> Create([FromBody] SupplierDTO supplier)
    {
        try
        {
            String inserted = await _supplierManager.SaveSupplier(await _supplierManager.ConvertToData(supplier), true);
            return Ok(inserted);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }
    }

    // PUT: api/Suppliers/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupplierDTO supplier)
    {
        if (id != supplier.ID)
        {
            return BadRequest();
        }

        var existing = await _supplierManager.GetSupplierById(id);
        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            //String updated = await _gardenBedManager.SaveGardenBed(await _gardenBedManager.ConvertToData(gardenBed), false);
            await _supplierManager.UpdateFromDTO(existing, supplier);
            String updated = await _supplierManager.SaveSupplier(existing, false);
            return Ok(updated);
        }
        catch (ApplicationException ae)
        {
            //return Forbid();
            return BadRequest(ae.Message);
        }

    }

    // DELETE: api/Suppliers/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _supplierManager.GetSupplierById(id);

        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            var succeeded = await _supplierManager.DeleteSupplier(id);
            return Ok(succeeded);
        }
        catch (ApplicationException ae)
        {
            return StatusCode(403, ae.Message);
        }

    }

}