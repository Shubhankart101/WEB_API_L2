using System.ComponentModel.DataAnnotations;

public class AssetInformationUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RetailPrice { get; set; }

    public int UserId { get; set; } // <-- Add this line  
     public int Status { get; set; }  
    public int AssetId { get; set; } // <-- Add this line  
    }