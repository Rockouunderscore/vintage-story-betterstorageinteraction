
namespace betterstorageinteractions;

public class Config
{
    public bool PatchPlayerInventoryManager { get; set; } = false;
    
    public bool PatchCrate { get; set; } = true;
    public bool PatchShelf { get; set; } = true;
    public bool PatchItemPile { get; set; } = true;
    public bool PatchGroundStorage { get; set; } = true;
    
    public bool FillHandFirst { get; set; } = true;
    public bool FillHandFirstAllowOverflow { get; set; } = false;
}