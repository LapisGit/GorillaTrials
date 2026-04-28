using GorillaLibrary.Extensions;
using GorillaNetworking;
using HarmonyLib;
using System.Linq;
using GorillaLibrary.Behaviours;
using static CosmeticWardrobe;
using static GorillaNetworking.CosmeticsController;


namespace GorillaTrials.Behaviours;

public class BadgeWardrobe : WardrobeSection
{
    public override string Title => "Badges";
    
    public void Awake()
    {
        RigBadgeManager.Instance.onCosmeticUpdate.Subscribe(UpdateCosmetics);
    }

    public override void ApplyCosmetic(CosmeticWardrobeSelection selection, int index)
    {
        var outfits = instance.GetField<CosmeticSet[]>("savedOutfits");
        var outfit = index != SelectedOutfit ? outfits[index] : instance.currentWornSet;

        selection.displayHead.SetCosmeticActiveArray([.. outfit.items.Select(item => item.displayName)], outfit.ToOnRightSideArray());
        selection.selectButton.enabled = true;
        selection.selectButton.isOn = index == SelectedOutfit;
        selection.selectButton.UpdateColor();
    }

    public override void SelectCosmetic(int index)
    {
        throw new System.NotImplementedException();
    }

    public override int GetSectionSize()
    {
        throw new System.NotImplementedException();
    }

    public override void OnSectionActivated(bool hasActivated)
    {
        
    }
}