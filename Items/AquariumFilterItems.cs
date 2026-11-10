using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AquariumFilter.Tiles;

namespace AquariumFilter.Items
{
	public abstract class AquariumFilterItemBase : ModItem
	{
		protected abstract int TileToPlace { get; }

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileToPlace);
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.buyPrice(gold: 5);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.GreenSolution, 10)
				.AddIngredient(ItemID.HallowedBar, 10)
				.AddIngredient(ItemID.SoulofLight, 10)
				.AddIngredient(ItemID.SoulofNight, 10)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class AquariumFilterItem : AquariumFilterItemBase
	{
		protected override int TileToPlace => ModContent.TileType<AquariumFilterTile>();
	}

	public class EvilAquariumFilterItem : AquariumFilterItemBase
	{
		protected override int TileToPlace => ModContent.TileType<EvilAquariumFilterTile>();
	}

	public class HallowAquariumFilterItem : AquariumFilterItemBase
	{
		protected override int TileToPlace => ModContent.TileType<HallowAquariumFilterTile>();
	}
}
