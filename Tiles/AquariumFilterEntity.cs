using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AquariumFilter.Tiles
{
	// Runs every game tick for each placed filter. Every TicksBetweenSprays ticks it spends stored Green Solution,
	// picks one random matching block anywhere in the world and spawns the vanilla Green Solution projectile on it.
	// That projectile is what calls WorldGen.Convert, so the conversion is the game's own.
	//
	// The world is scanned one vertical slice at a time, so a full sweep never costs more than a few milliseconds.
	public class AquariumFilterEntity : ModTileEntity
	{
		public const int MaxStored = 9999;

		private const int SliceWidth = 120;
		private const int MaxSlicesPerSpray = 2;

		public int Stored { get; private set; }

		// Blocks already paid for but not yet purified. Lets the cost average out to exactly BlocksPerGreenSolution.
		private int credit;
		private int timer;
		private int sweepX = -1;

		public override void SaveData(TagCompound tag) {
			tag[nameof(Stored)] = Stored;
			tag[nameof(credit)] = credit;
		}

		public override void LoadData(TagCompound tag) {
			Stored = tag.GetInt(nameof(Stored));
			credit = tag.GetInt(nameof(credit));
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(Stored);
			writer.Write(credit);
		}

		public override void NetReceive(BinaryReader reader) {
			Stored = reader.ReadInt32();
			credit = reader.ReadInt32();
		}

		public override bool IsTileValidForEntity(int x, int y) {
			Tile tile = Main.tile[x, y];
			return tile.HasTile && TileLoader.GetTile(tile.TileType) is AquariumFilterTileBase;
		}

		// Moves Green Solution from the player's inventory into the filter and returns how many were loaded.
		public int LoadFrom(Player player) {
			return AddStored(TakeGreenSolution(player, MaxStored - Stored));
		}

		// Adds up to `amount` Green Solution and returns how many fit.
		public int AddStored(int amount) {
			int added = Math.Clamp(amount, 0, MaxStored - Stored);
			Stored += added;
			return added;
		}

		// Removes up to `max` Green Solution from the player's inventory and returns how many were removed.
		// In multiplayer the changed slots are sent to the server.
		public static int TakeGreenSolution(Player player, int max) {
			int taken = 0;

			for (int slot = 0; slot < 58 && taken < max; slot++) {
				Item item = player.inventory[slot];
				if (item.IsAir || item.type != ItemID.GreenSolution) {
					continue;
				}

				int take = Math.Min(item.stack, max - taken);
				item.stack -= take;
				taken += take;

				if (item.stack <= 0) {
					item.TurnToAir();
				}

				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncEquipment, number: player.whoAmI, number2: slot, number3: item.prefix);
				}
			}

			return taken;
		}

		public override void Update() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			AquariumFilterConfig config = ModContent.GetInstance<AquariumFilterConfig>();

			timer++;
			if (timer < config.TicksBetweenSprays) {
				return;
			}
			timer = 0;

			// Pay for more purification with a stored Green Solution. With no fuel left, do nothing (and skip the scan).
			bool spent = false;
			while (credit <= 0 && Stored > 0) {
				Stored--;
				credit += config.BlocksPerGreenSolution;
				spent = true;
			}
			if (spent && Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
			}
			if (credit <= 0) {
				return;
			}

			Tile filterTile = Main.tile[Position.X, Position.Y];
			if (!filterTile.HasTile || TileLoader.GetTile(filterTile.TileType) is not AquariumFilterTileBase filter) {
				return;
			}

			if (TryFindAnywhere(filter.Mode, out Point target)) {
				credit -= CountConvertibleTiles(target);

				Vector2 worldPos = new Vector2(target.X * 16 + 8, target.Y * 16 + 8);
				Projectile.NewProjectile(new EntitySource_TileUpdate(Position.X, Position.Y), worldPos, Vector2.Zero, ProjectileID.PureSpray, 0, 0f, Main.myPlayer);
			}
		}

		private bool TryFindAnywhere(PurifyMode mode, out Point target) {
			target = Point.Zero;

			if (sweepX < 0) {
				sweepX = Main.rand.Next(Main.maxTilesX);
			}

			for (int slice = 0; slice < MaxSlicesPerSpray; slice++) {
				int minX = Math.Max(sweepX, 2);
				int maxX = Math.Min(sweepX + SliceWidth - 1, Main.maxTilesX - 3);

				sweepX += SliceWidth;
				if (sweepX >= Main.maxTilesX) {
					sweepX = 0;
				}

				if (ScanArea(minX, maxX, 2, Main.maxTilesY - 3, mode, out target)) {
					return true;
				}
			}

			return false;
		}

		private static bool ScanArea(int minX, int maxX, int minY, int maxY, PurifyMode mode, out Point target) {
			int found = 0;
			target = Point.Zero;

			for (int x = minX; x <= maxX; x++) {
				for (int y = minY; y <= maxY; y++) {
					if (!Matches(Main.tile[x, y], mode)) {
						continue;
					}

					// Reservoir sampling: every matching block has an equal chance of being picked.
					found++;
					if (Main.rand.Next(found) == 0) {
						target = new Point(x, y);
					}
				}
			}

			return found > 0;
		}

		// The spray converts the 5x5 area around its target, so that is what the fuel is charged for.
		private static int CountConvertibleTiles(Point target) {
			int count = 0;

			for (int x = target.X - 2; x <= target.X + 2; x++) {
				for (int y = target.Y - 2; y <= target.Y + 2; y++) {
					if (x < 2 || y < 2 || x > Main.maxTilesX - 3 || y > Main.maxTilesY - 3) {
						continue;
					}

					Tile tile = Main.tile[x, y];
					if (tile.HasTile) {
						ushort type = tile.TileType;
						if (TileID.Sets.Corrupt[type] || TileID.Sets.Crimson[type] || TileID.Sets.Hallow[type]) {
							count++;
						}
					}
				}
			}

			return Math.Max(1, count);
		}

		private static bool Matches(Tile tile, PurifyMode mode) {
			bool evil = false;
			bool hallow = false;

			if (tile.HasTile) {
				ushort type = tile.TileType;
				evil = TileID.Sets.Corrupt[type] || TileID.Sets.Crimson[type];
				hallow = TileID.Sets.Hallow[type];
			}

			if (!evil && !hallow && tile.WallType > 0) {
				ushort wall = tile.WallType;
				evil = WallID.Sets.Corrupt[wall] || WallID.Sets.Crimson[wall];
				hallow = WallID.Sets.Hallow[wall];
			}

			return mode switch {
				PurifyMode.Evil => evil,
				PurifyMode.Hallow => hallow,
				_ => evil || hallow
			};
		}
	}
}
