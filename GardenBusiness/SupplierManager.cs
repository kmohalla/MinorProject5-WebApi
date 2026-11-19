using GardenData;
using GardenModels;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace GardenBusiness
{
    public class SupplierManager
    {
        private readonly GardenContext _context;
        public SupplierManager(GardenContext context)
        {
            _context = context;
        }
        public async Task<Supplier> GetSupplierById(int supplierId)
        {
            var supplier = await _context.GetSupplierByIdAsync(supplierId);
            if (supplier != null && supplier.Id > 0)
            {
                return supplier;
            }
            else
            {
                //throw new ApplicationException($"Supplier with id {supplierId} not found.");//only business rules should throw exceptions here, the API handles the rest
                return null;
            }
        }
        public async Task<List<Supplier>> GetAllSuppliers()
        {
            var suppliers = await _context.GetAllSuppliersAsync();
            if (suppliers != null && suppliers.Count > 0)
            {
                return suppliers;
            }
            else
            {
                //throw new ApplicationException("No suppliers found.");
                return null;
            }
        }

        public async Task<String> SaveSupplier(Supplier supplier, bool isNewSupplier)
        {

            if (isNewSupplier)
            {
                await _context.CreateSupplierAsync(supplier);
                //add logic for sending a notification on success/failure
            }
            else
            {
                try
                {
                    await _context.UpdateSupplierAsync(supplier);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error updating supplier with id {supplier.Id}");
                }
            }
            return $"Supplier with id {supplier.Id} saved successfully.";
        }
        public async Task<String> DeleteSupplier(int supplierId)
        {
            //checking business rule 2: A Supplier can't be deleted while a plant is linked to it
            var supplier = await GetSupplierById(supplierId);
            bool isValid = ValidateSupplier(supplier);
            if (!isValid)
            {
                throw new ApplicationException($"Supplier with id {supplierId} can't be deleted while a plant is linked to it. Supplier is linked to {supplier.Plants.Count} plants.");
            }
            try
            {
                await _context.DeleteSupplierAsync(supplierId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting supplier with id {supplierId}");
            }

            return $"Supplier with id {supplierId} deleted successfully.";
        }
        private static bool ValidateSupplier(Supplier supplier)
        {
            if (supplier.Plants.IsNullOrEmpty())
            {
                return true;
            }
            else
            {
                return false;
                throw new ApplicationException($"Supplier can't be deleted while a plant is linked to it. Supplier is linked to {supplier.Plants.Count} plants." );
            }
        }

        //Data conversion methods to convert between DTO and entity classes
        public static SupplierDTO ConvertToDTO(Supplier supplier)
        {
            // Convert the list of Plant objects to a list of plant names
            List<string> plantNames = new();
            if (supplier.Plants != null)
            {
                foreach (var plant in supplier.Plants)
                {
                    plantNames.Add(plant.Name);
                }
            }
            return new SupplierDTO
            {
                ID = supplier.Id,
                Supplier = supplier.Name,
                Location = supplier.State,
                Plants = plantNames
            };
        }

        public async Task<Supplier> ConvertToData(SupplierDTO supplierDTO)
        {
            // Convert the list of plant names to a list of Plant objects
            List<Plant> plants = new();
            if (supplierDTO.Plants != null)
            {
                foreach (var plantName in supplierDTO.Plants)
                {
                    List<Plant> matchingPlants = await _context.SearchAsync(name: plantName, family: "", color: "");
                    Plant? existingPlant = matchingPlants.FirstOrDefault();
                    if (existingPlant != null)
                    {
                        plants.Add(existingPlant);
                    }
                }
            }
            return new Supplier
            {
                Id = supplierDTO.ID,
                Name = supplierDTO.Supplier,
                State = supplierDTO.Location,
                Plants = plants
            };
        }
        public async Task UpdateFromDTO(Supplier existingSupplier, SupplierDTO supplierDTO)
        {
            existingSupplier.Name = supplierDTO.Supplier;
            existingSupplier.State = supplierDTO.Location;
            List<Plant> plants = new();
            if (supplierDTO.Plants != null)
            {
                foreach (var plantName in supplierDTO.Plants)
                {
                    List<Plant> matchingPlants = await _context.SearchAsync(name: plantName, family: "", color: "");
                    Plant? existingPlant = matchingPlants.FirstOrDefault();
                    if (existingPlant != null)
                    {
                        plants.Add(existingPlant);
                    }
                }
            }
            existingSupplier.Plants = plants;
        }
    }
}
