using GardenData;
using GardenModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace GardenBusiness
{
    public class GuideManager
    {
        private readonly GardenContext _context;
        public GuideManager(GardenContext context)
        {
            _context = context;
        }
        public async Task<Guide> GetGuideById(int guideId)
        {
            var guide = await _context.GetGuideByIdAsync(guideId);
            if (guide != null && guide.Id > 0)
            {
                return guide;
            }
            else
            {
                throw new ApplicationException($"Tool with id {guideId} not found.");
            }
        }
        public async Task<List<Guide>> GetAllGuides()
        {
            var guides = await _context.GetAllGuidesAsync();
            if (guides != null && guides.Count > 0)
            {
                return guides;
            }
            else
            {
                throw new ApplicationException("No tools found.");
            }
        }

        public async Task<String> SaveGuide(Guide guide, bool isNewGuide)
        {
            //checking business rule 5: Season must be Spring, Summer, Fall, or Winter. Difficulty must be Beginner, Intermediate, or Advanced.
            bool isValid = ValidateGuide(guide);
            if (isNewGuide && isValid)
            {
                await _context.CreateGuideAsync(guide);
                //add logic for sending a notification on success/failure
            }
            else if (!isNewGuide && isValid)
            {
                try
                {
                    await _context.UpdateGuideAsync(guide);
                }
                catch (Exception ex)
                {
                    //TODO: remove the message from the exception after testing
                    throw new ApplicationException($"Error updating guide with id {guide.Id}: {ex.Message}");
                }
            }
            else
            {
                throw new ApplicationException("Guide is not valid. Please check the season (Spring, Summer, Winter, Fall) and difficulty (Beginner, Intermediate, Advanced) input values.");
            }
            return "Guide saved successfully.";
        }
        public async Task<String> DeleteGuide(int guideId)
        {
            try
            {
                await _context.DeleteGuideAsync(guideId);
            }
            catch (Exception ex)
            {
                //TODO: remove the message from the exception after testing
                throw new ApplicationException($"Error deleting guide with id {guideId}: {ex.Message} Try Again later");
            }
            return "Guide deleted successfully.";
        }
        private bool ValidateGuide(Guide guide)
        {
            List<String> Seasons = new List<String> { "Spring", "Summer", "Fall", "Winter" };  
            List<String> Difficulties = new List<String> { "Beginner", "Intermediate", "Advanced" };

            if (!Seasons.Contains(guide.Season))
            {
                return false;
            }
            if (!Difficulties.Contains(guide.Difficulty))
            {
                return false;
            }
            return true;
        }

        //Data conversion methods to convert getween DTO and entity classes
        public static GuideDTO ConvertToDTO(Guide guide)
        {
            return new GuideDTO
            {
                ID = guide.Id,
                Title = guide.Name,
                Author = guide.Author,
                PlantingSeason = guide.Season,
                Difficulty = guide.Difficulty,
            };
        }

        public static Guide ConvertToData(GuideDTO guideDTO)
        {
            return new Guide
            {
                Id = guideDTO.ID,
                Name = guideDTO.Title,
                Author = guideDTO.Author,
                Season = guideDTO.PlantingSeason,
                Difficulty = guideDTO.Difficulty,
            };
        }
    }
}