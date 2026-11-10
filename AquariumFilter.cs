using System.IO;
using AquariumFilter.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AquariumFilter
{
	public enum PacketType : byte
	{
		LoadFilter
	}

	public class AquariumFilter : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			PacketType type = (PacketType)reader.ReadByte();
			if (type != PacketType.LoadFilter) {
				return;
			}

			int id = reader.ReadInt32();
			int amount = reader.ReadInt32();
			if (Main.netMode != NetmodeID.Server || amount <= 0) {
				return;
			}

			// A client took `amount` Green Solution from its inventory for this filter. Store what fits and
			// give the rest back to the player.
			int added = 0;
			if (TileEntity.ByID.TryGetValue(id, out TileEntity entity) && entity is AquariumFilterEntity filter) {
				added = filter.AddStored(amount);
				if (added > 0) {
					NetMessage.SendData(MessageID.TileEntitySharing, number: id);
				}
			}

			int left = amount - added;
			if (left > 0) {
				Player player = Main.player[whoAmI];
				player.QuickSpawnItem(player.GetSource_FromThis(), ItemID.GreenSolution, left);
			}
		}
	}
}
