using GardenData;
using GardenModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Text;


namespace GardenBusiness
{
    public  class ToolManager
    {
        private readonly GardenContext _context;
        public ToolManager(GardenContext context)
        {
            _context = context;
        }
        public async Task<Tool> GetToolById(int toolId)
        {
            var tool =  await _context.GetToolByIdAsync(toolId);
            if (tool != null && tool.Id > 0)
            {
                return tool;
            }
            else
            {
                // throw new ApplicationException($"Tool with id {toolId} not found."); //exception is thrown by the API now, so null check here is not needed
                return null;
            }
        }
        public async Task<List<Tool>> GetAllTools()
        {
            var tools = await _context.GetAllToolsAsync();
            if (tools != null && tools.Count > 0)
            {
                return tools;
            }
            else
            {
                //throw new ApplicationException("No tools found.");
                return null;
            }
        }

        public async Task<String> SaveTool(Tool tool, bool isNewTool)
        {
            //checking business rule 1: weight must be between 0.05kg and 25kg, length must be between 5cm and 300cm
            bool isValid = ValidateTool(tool);
            if (isNewTool && isValid)
            {
                await _context.CreateToolAsync(tool);
            //add logic for sending a notification on success/failure
            }
            else if (!isNewTool && isValid)
            {
                try
                {
                   await  _context.UpdateToolAsync(tool);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error updating tool with id {tool.Id}");
                }
            }
            else
            {
                throw new ApplicationException("Tool is not valid. Weight must be entered in either lbs (0.1102311-55.1156) or kg (0.05-25) the other may be ommitted or set to 0. Length must be entered in either inches (1.9685-118.11) or cm (5-300)the other may be ommitted or set to 0. If both metric and imperial measurements are given they must be equal");
            }
            return $"Tool with id {tool.Id} saved successfully.";
        }
        public async Task<String> DeleteTool(int toolId)
        {
            try
            {
                await _context.DeleteToolAsync(toolId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting tool with id {toolId}");

            }
            return $"Tool with id {toolId} deleted successfully.";
        }
        private bool ValidateTool(Tool tool)
        {
            if (tool.WeightKg < 0.05m || tool.WeightKg > 25)
            {
                return false;
            }
            if (tool.LengthCm < 5 || tool.LengthCm > 300)
            {
                return false;
            }
            return true;
        }

        //Data conversion methods to convert between DTO and entity classes
        public static ToolDTO ConvertToDTO(Tool tool)
        {
            return new ToolDTO
            {
                ID = tool.Id,
                ToolWeightMetricKg = tool.WeightKg,
                ToolWeightImperialLbs = tool.WeightKg * 2.20462m,
                ToolName = tool.Type,
                Material = tool.Material,
                ToolLengthMetricCm = tool.LengthCm,
                ToolLengthImperialIn = ((int)(tool.LengthCm * 0.393701m))


            };
        }
        public static Tool ConvertToData(ToolDTO toolDTO)
        {
           //check imperial length
            if (toolDTO.ToolLengthMetricCm == 0 && toolDTO.ToolLengthImperialIn != 0)
            {
                //user provided the imperial length but not metric length
                toolDTO.ToolLengthMetricCm = (int)(toolDTO.ToolLengthImperialIn / 0.393701m);
            }
            else if (toolDTO.ToolLengthImperialIn != 0 && toolDTO.ToolLengthImperialIn != ((int)toolDTO.ToolLengthMetricCm * 0.393701m))
            {
                //user provided the imperial length, but it isn't equal to the metric length
                throw new ApplicationException("Tool length in inches does not match the tool length in cm. Please ommit one of the values, set one of the values to 0, or ensure they are equal.");
            }


            //check imperial weight
            if (toolDTO.ToolWeightMetricKg == 0 && toolDTO.ToolWeightImperialLbs != 0)
            {
                //user provided the imperial weight but not metric weight
                toolDTO.ToolWeightMetricKg = (toolDTO.ToolWeightImperialLbs / 2.20462m);
            }
            else if (toolDTO.ToolWeightImperialLbs != 0 && toolDTO.ToolWeightImperialLbs != (toolDTO.ToolWeightMetricKg * 2.20462m))
            {
                //user provided the imperial weight, but it isn't equal to the metric weight
                throw new ApplicationException("Tool weight in pounds does not match the tool weight in kg. Please ommit one of the values, set one of the values to 0, or ensure they are equal.");
            }
            return new Tool
            {
                Id = toolDTO.ID,
                WeightKg = toolDTO.ToolWeightMetricKg,
                Type = toolDTO.ToolName,
                Material = toolDTO.Material,
                LengthCm = toolDTO.ToolLengthMetricCm
            };
        }

        public static void UpdateFromDTO(Tool existingTool, ToolDTO toolDTO)
        {
            // metric imperial alignment
            //1: check if the weight in kg is different from the weight in lbs converted to kg
            if (existingTool.WeightKg!= toolDTO.ToolWeightMetricKg)
            {
                //user has updated the metric weight, so update the imperial to match (note this will not be saved in the database because the database only stores the metric weight)
                toolDTO.ToolWeightImperialLbs = (toolDTO.ToolWeightMetricKg * 2.20462m);
            }
            else if (toolDTO.ToolWeightImperialLbs != (toolDTO.ToolWeightMetricKg* 2.20462m))
            {
                //user has updated the imperial weight, so update the metric weight to match
                toolDTO.ToolWeightMetricKg = (toolDTO.ToolWeightImperialLbs / 2.20462m);
            }
            // update the existing tool accordingly
            existingTool.WeightKg = toolDTO.ToolWeightMetricKg;


            //2: check if the length in cm is different from the length in inches converted to cm
            if (existingTool.LengthCm != toolDTO.ToolLengthMetricCm)
            {
                //user has updated the metric length, so update the imperial to match (note this will not be saved in the database because the database only stores the metric length)
                toolDTO.ToolLengthImperialIn = ((int)(toolDTO.ToolLengthMetricCm * 0.393701m));
            }
            else if (toolDTO.ToolLengthImperialIn != ((int)(toolDTO.ToolLengthMetricCm * 0.393701m)))
            {
                //user has updated the imperial length, so update the metric length to match
                toolDTO.ToolLengthMetricCm = (int)(toolDTO.ToolLengthImperialIn / 0.393701m);
            }
            //update the existing tool accordingly
            existingTool.LengthCm = toolDTO.ToolLengthMetricCm;


            existingTool.Type = toolDTO.ToolName;
            existingTool.Material = toolDTO.Material;
        }
    }
}
