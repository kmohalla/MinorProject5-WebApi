using GardenData;
using GardenModels;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GardenBusiness
{
    public class PlantManager
    {
        private readonly GardenContext _context;
        public PlantManager(GardenContext context)
        {
            _context = context;
        }
        public async Task<Plant> GetPlantById(int plantId)
        {
            var plant = await _context.GetPlantByIdAsync(plantId);
            if (plant != null && plant.Id > 0)
            {
                return plant;
            }
            else
            {
                // throw new ApplicationException($"Plant with id {plantId} not found."); //only business logic errors should be thrown here. the rest are handeled by the API
                return null;
            }
        }
        public async Task<List<Plant>> GetAllPlants()
        {
            var plants = await _context.GetAllPlantsAsync();
            if (plants != null && plants.Count > 0)
            {
                return plants;
            }
            else
            {
                //throw new ApplicationException("No plants found.");
                return null;
            }
        }

        public async Task<String> SavePlant(Plant plant, bool isNewPlant)
        {
            //checking business rule 3: A Plant name is required, must be trimmed, and two plants can't have the same name
            bool isValid = await ValidatePlantSave(plant);



            if (isNewPlant && isValid)
            {
                await _context.CreatePlantAsync(plant);
                //add logic for sending a notification on success/failure
            }
            else if (!isNewPlant && isValid)
            {
                try
                {
                    await _context.UpdatePlantAsync(plant);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error updating plant with id {plant.Id}");
                }
            }
            else
            {
                throw new ApplicationException("Plant validation failed. Please check that the plant has a unique name");
            }
           
            return $"Plant with id {plant.Id} saved successfully.";
        }
        public async Task<String> DeletePlant(int plantId)
        {
            //checking business rule 4: A plant can't be deleted while it's linked to a bed
            var plant = await GetPlantById(plantId);
            bool isValid = await ValidatePlantDelete(plant);
            if (!isValid)
            {
                throw new ApplicationException("Plant can't be deleted while it's linked to a bed.");
            }
            try
            {
                await _context.DeletePlantAsync(plantId);
            }
            catch (Exception ex)
            {
                
                throw new ApplicationException($"Error deleting plant with id {plantId}");
            }

            return $"Plant with id {plantId} deleted successfully.";
        }
        private async Task<bool> ValidatePlantSave(Plant plant)
        {
            //plant has a name
            if (string.IsNullOrWhiteSpace(plant.Name))
            {
                return false;
                //throw new ApplicationException("Plant name is required.");
            }

            //plant name is trimmed
            plant.Name = plant.Name.Trim();

            //plant does not already exist
            List<Plant> sameName = await _context.SearchAsync(name: plant.Name, family: "", color: "");
           
            if (sameName != null && sameName.Count > 0)
            {
                foreach (var foundName in sameName)
                {
                    if(foundName.Id != plant.Id) { 
                        return false;
                        //throw new ApplicationException($"Plant with name {plant.Name} already exists.");
                    }
                }
            }

            return true;
        }
        private async Task<bool> ValidatePlantDelete(Plant plant)
        {
            //plant isnt in any bed
            List<GardenBed> beds = await _context.GetAllGardenBedsAsync();
            foreach (GardenBed bed in beds)
            {
                if (bed.Plant.Name == plant.Name)
                {
                    return false;
                }
            }
            return true;
        }

        //Data conversion methods to convert between DTO and entity classes
        public static PlantDTO ConvertToDTO(Plant plant)
        {
            return new PlantDTO
            {
                ID = plant.Id,
                CommonName = plant.Name,
                Family = plant.Family,
                Type = plant.Type,
                Color = plant.Color,
                SourceName = plant.Supplier?.Name,
            };
        }
        public async Task<Plant> ConvertToData(PlantDTO plantDTO)
        {
            //find the supplier based on the DTO
            Supplier? existingSupplier = plantDTO.SourceName == null ? null : await _context.FindSupplierByName(plantDTO.SourceName);
            if (existingSupplier == null)
            {
                throw new ApplicationException($"SourceName {plantDTO.SourceName} not found.");
            }
            return new Plant
            {
                Id = plantDTO.ID,
                Name = plantDTO.CommonName,
                Family = plantDTO.Family,
                Type = plantDTO.Type,
                Color = plantDTO.Color,
                SupplierId = existingSupplier.Id,
                Supplier = existingSupplier
            };
        }
        public async Task UpdateFromDTO(Plant existingPlant, PlantDTO plantDTO)
        {
            //find the supplier based on the DTO
            Supplier? existingSupplier = plantDTO.SourceName == null ? null : await _context.FindSupplierByName(plantDTO.SourceName);
            if (existingSupplier == null)
            {
                throw new ApplicationException($"SourceName {plantDTO.SourceName} not found.");
            }
            existingPlant.Name = plantDTO.CommonName;
            existingPlant.Family = plantDTO.Family;
            existingPlant.Type = plantDTO.Type;
            existingPlant.Color = plantDTO.Color;
            existingPlant.SupplierId = existingSupplier.Id;
            existingPlant.Supplier = existingSupplier;
        }
    }
}