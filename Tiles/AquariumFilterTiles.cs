using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AquariumFilter.Tiles
{
	public enum PurifyMode
	{
		All,
		Evil,
		Hallow
	}

	// Shared behaviour for the three filter blocks. The block itself never edits tiles;
	// its tile entity only spawns the vanilla Green Solution spray, and the game converts the blocks.
	public abstract class AquariumFilterTileBase : ModTile
	{
		public abstract PurifyMode Mode { get; }
		protected abstract Color MapColor { get; }
		protected abstract Vector3 LightColor { get; }
		protected abstract int DustKind { get; }

		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			DustType = DustKind;
			HitSound = SoundID.Tink;
			AddMapEntry(MapColor, CreateMapEntryName());
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = LightColor.X;
			g = LightColor.Y;
			b = LightColor.Z;
		}

		public override void PlaceInWorld(int i, int j, Item item) {
			AquariumFilterEntity entity = ModContent.GetInstance<AquariumFilterEntity>();
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 1);
				NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: entity.Type);
			}
			else {
				entity.Place(i, j);
			}
		}

		public override void MouseOver(int i, int j) {
			if (TileEntity.TryGet(i, j, out AquariumFilterEntity filter)) {
				Player player = Main.LocalPlayer;
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = -1;
				player.cursorItemIconText = $"Green Solution: {filter.Stored}";
			}
		}

		// Right-click loads every Green Solution from your inventory into the filter.
		public override bool RightClick(int i, int j) {
			if (!TileEntity.TryGet(i, j, out AquariumFilterEntity filter)) {
				return false;
			}

			int loaded;
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				// Take the Green Solution here and ask the server to store it. The server returns any that don't fit.
				loaded = AquariumFilterEntity.TakeGreenSolution(Main.LocalPlayer, AquariumFilterEntity.MaxStored - filter.Stored);
				if (loaded > 0) {
					ModPacket packet = Mod.GetPacket();
					packet.Write((byte)global::AquariumFilter.PacketType.LoadFilter);
					packet.Write(filter.ID);
					packet.Write(loaded);
					packet.Send();
					SoundEngine.PlaySound(SoundID.Item8, new Vector2(i * 16, j * 16));
					Main.NewText($"Loaded {loaded} Green Solution. The filter now holds {filter.Stored + loaded}.");
				}
				else {
					Main.NewText($"The filter holds {filter.Stored} Green Solution. Right-click with Green Solution in your inventory to load more.");
				}
				return true;
			}

			loaded = filter.LoadFrom(Main.LocalPlayer);
			if (loaded > 0) {
				SoundEngine.PlaySound(SoundID.Item8, new Vector2(i * 16, j * 16));
				Main.NewText($"Loaded {loaded} Green Solution. The filter now holds {filter.Stored}.");
			}
			else {
				Main.NewText($"The filter holds {filter.Stored} Green Solution. Right-click with Green Solution in your inventory to load more.");
			}

			return true;
		}

		// Mining the block returns the stored Green Solution.
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail || effectOnly) {
				return;
			}

			if (TileEntity.TryGet(i, j, out AquariumFilterEntity filter) && filter.Stored > 0 && Main.netMode != NetmodeID.MultiplayerClient) {
				Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemID.GreenSolution, filter.Stored);
			}

			ModContent.GetInstance<AquariumFilterEntity>().Kill(i, j);
		}
	}

	public class AquariumFilterTile : AquariumFilterTileBase
	{
		public override PurifyMode Mode => PurifyMode.All;
		protected override Color MapColor => new Color(60, 210, 120);
		protected override Vector3 LightColor => new Vector3(0.25f, 0.85f, 0.45f);
		protected override int DustKind => DustID.GemEmerald;
	}

	public class EvilAquariumFilterTile : AquariumFilterTileBase
	{
		public override PurifyMode Mode => PurifyMode.Evil;
		protected override Color MapColor => new Color(160, 70, 230);
		protected override Vector3 LightColor => new Vector3(0.65f, 0.3f, 0.9f);
		protected override int DustKind => DustID.GemAmethyst;
	}

	public class HallowAquariumFilterTile : AquariumFilterTileBase
	{
		public override PurifyMode Mode => PurifyMode.Hallow;
		protected override Color MapColor => new Color(126, 202, 255);
		protected override Vector3 LightColor => new Vector3(0.5f, 0.8f, 1f);
		protected override int DustKind => DustID.GemDiamond;
	}
}
