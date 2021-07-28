using AssetRipper.Project;
using AssetRipper.Classes.Misc.Serializable;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using AssetRipper.Math;

namespace AssetRipper.Classes.LightProbes
{
	public struct ProbeSetTetrahedralization : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Tetrahedra = reader.ReadAssetArray<Tetrahedron>();
			HullRays = reader.ReadAssetArray<Vector3f>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TetrahedraName, Tetrahedra.ExportYAML(container));
			node.Add(HullRaysName, HullRays.ExportYAML(container));
			return node;
		}

		public Tetrahedron[] Tetrahedra { get; set; }
		public Vector3f[] HullRays { get; set; }

		public const string TetrahedraName = "m_Tetrahedra";
		public const string HullRaysName = "m_HullRays";
	}
}
