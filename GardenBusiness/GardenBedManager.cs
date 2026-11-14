using GardenData;
using GardenModels;

namespace GardenBusiness
{
    public class GardenBedManager
    {
        private readonly GardenContext _context;
        public GardenBedManager(GardenContext context)
        {
            _context = context;
        }
        public async Task<GardenBed> GetGardenBedById(int gardenBedId)
        {
            var gardenBed = await _context.GetGardenBedByIdAsync(gardenBedId);
            if (gardenBed != null && gardenBed.Id > 0)
            {
                return gardenBed;
            }
            else
            {
                throw new ApplicationException($"GardenBed with id {gardenBedId} not found.");
            }
        }
        public async Task<List<GardenBed>> GetAllGardenBeds()
        {
            var gardenBeds = await _context.GetAllGardenBedsAsync();
            return gardenBeds;
        }

        public async Task<String> SaveGardenBed(GardenBed gardenBed, bool isNewGardenBed)
        {
            //checking business rule 6: Status must be Planned, Growing, or Harvested. If status = growing or harvested, date planted must be today or earlier.
            bool isValid = ValidateGardenBed(gardenBed);
            if (isNewGardenBed && isValid)
            {
                await _context.CreateGardenBedAsync(gardenBed);
                //add logic for sending a notification on success/failure
            }
            else if (!isNewGardenBed && isValid)
            {
                try
                {
                    await _context.UpdateGardenBedAsync(gardenBed);
                }
                catch (Exception ex)
                {
                    //TODO: remove the message from the exception after testing
                    throw new ApplicationException($"Error updating garden bed with id {gardenBed.Id}: {ex.Message}");
                }
            }
            else
            {
                throw new ApplicationException("Garden Bed is not valid. Please check the status (Planned, Growing, Harvested). If the status is growing or harvested the planted date cannot be in the future.");
            }
            return "Garden Bed saved successfully.";
        }
        public async Task<String> DeleteGardenBed(int gardenBedId)
        {
            try
            {
                await _context.DeleteGardenBedAsync(gardenBedId);
            }
            catch (Exception ex)
            {
                //TODO: remove the message from the exception after testing
                throw new ApplicationException($"Error deleting garden bed with id {gardenBedId}: {ex.Message} Try Again later");
            }
            return "Garden Bed deleted successfully.";
        }
        private bool ValidateGardenBed(GardenBed gardenBed)
        {
            List<String> Statuses = new List<String> { "Planned", "Growing", "Harvested" };

            if (!Statuses.Contains(gardenBed.Status))
            {
                return false;
            }
            if ((gardenBed.Status == "Growing" || gardenBed.Status == "Harvested") && gardenBed.PlantedDate > DateTime.Today)
            {
                return false;
            }
            return true;
        }


        //Data conversion methods to convert between DTO and entity classes
        public static GardenBedDTO ConvertToDTO(GardenBed gardenBed)
        {
            return new GardenBedDTO
            {
                ID = gardenBed.Id,
                BedName = gardenBed.Name,
                BedState = gardenBed.Status,
                DatePlanted = gardenBed.PlantedDate,
                GuideTitle = gardenBed.Guide?.Name,
                PlantName = gardenBed.Plant?.Name,
                ToolName = gardenBed.Tool?.Type
            };
        }

        public async Task<GardenBed> ConvertToData(GardenBedDTO gardenBedDTO)
        {
            //find the Guide, Plant and tool based on the DTO
            Guide? existingGuide = gardenBedDTO.GuideTitle ==null? null : await _context.FindGuideByName(gardenBedDTO.GuideTitle);
            if (existingGuide == null)
            {
                throw new ApplicationException($"Guide with name {gardenBedDTO.GuideTitle} not found.");
            }

            List<Plant> matchingPlants =gardenBedDTO.PlantName ==null? new List<Plant>() :  await _context.SearchAsync(name: gardenBedDTO.PlantName, family: "", color: "");
            Plant? existingPlant =matchingPlants.FirstOrDefault();
            if (existingPlant == null)
            {
                throw new ApplicationException($"Plant with name {gardenBedDTO.PlantName} not found.");
            }

            Tool? existingTool =gardenBedDTO.ToolName ==null? null : await _context.FindToolByType(gardenBedDTO.ToolName);
            if (existingTool == null)
            {
                throw new ApplicationException($"Tool with type {gardenBedDTO.ToolName} not found.");
            }




            return new GardenBed
            {
                Id = gardenBedDTO.ID,
                Name = gardenBedDTO.BedName,
                Status = gardenBedDTO.BedState,
                PlantedDate = gardenBedDTO.DatePlanted.Value,
                GuideId = existingGuide.Id,
                Guide = existingGuide,
                PlantId = existingPlant.Id,
                Plant = existingPlant,
                ToolId = existingTool.Id,
                Tool = existingTool 
            };
        }
        public async Task UpdateFromDTO(GardenBed existingBed, GardenBedDTO gardenBedDTO)
        {
            existingBed.Name = gardenBedDTO.BedName;
            existingBed.Status = gardenBedDTO.BedState;
            existingBed.PlantedDate = gardenBedDTO.DatePlanted.Value;

            // Find and update related entities
            if (!string.IsNullOrEmpty(gardenBedDTO.GuideTitle))
            {
                var guide = await _context.FindGuideByName(gardenBedDTO.GuideTitle);
                if (guide != null)
                    existingBed.GuideId = guide.Id;
            }

            if (!string.IsNullOrEmpty(gardenBedDTO.PlantName))
            {
                var plants = await _context.SearchAsync(name: gardenBedDTO.PlantName, family: "", color: "");
                if (plants.Any())
                    existingBed.PlantId = plants.First().Id;
            }

            if (!string.IsNullOrEmpty(gardenBedDTO.ToolName))
            {
                var tool = await _context.FindToolByType(gardenBedDTO.ToolName);
                if (tool != null)
                    existingBed.ToolId = tool.Id;
            }
        }
    }
}