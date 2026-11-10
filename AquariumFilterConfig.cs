using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AquariumFilter
{
	public class AquariumFilterConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// One spray is launched this many game ticks apart (60 ticks = 1 second).
		[DefaultValue(20)]
		[Range(5, 120)]
		[Increment(5)]
		[Slider]
		public int TicksBetweenSprays;

		// How many corrupted or hallowed blocks one Green Solution can purify.
		[DefaultValue(40)]
		[Range(1, 200)]
		[Slider]
		public int BlocksPerGreenSolution;
	}
}
