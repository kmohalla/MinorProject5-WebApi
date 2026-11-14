using GardenData;
using GardenModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
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
                throw new ApplicationException($"Tool with id {toolId} not found.");
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
                throw new ApplicationException("No tools found.");
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
                    //TODO: remove the message from the exception after testing
                    throw new ApplicationException($"Error updating tool with id {tool.Id}: {ex.Message}");
                }
            }
            else
            {
                throw new ApplicationException("Tool is not valid. Please check the weight(0.05kg-25kg) and length (5-300cm) input values.");
            }
            return "Tool saved successfully.";
        }
        public async Task<String> DeleteTool(int toolId)
        {
            try
            {
                await _context.DeleteToolAsync(toolId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting tool with id {toolId}: {ex.Message} Try Again later");
            }
            return "Tool deleted successfully.";
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
    }
}
