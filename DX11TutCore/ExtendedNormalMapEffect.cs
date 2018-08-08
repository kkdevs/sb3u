using SlimDX.Direct3D11;

namespace Core.FX
{
	public class ExtendedNormalMapEffect : NormalMapEffect
	{
		public readonly EffectScalarVariable TweenFactor0;

		public readonly EffectTechnique MorphTech;
		public readonly EffectTechnique SelectedSubmeshMorphTech;
		public readonly EffectTechnique NormalsTech;
		public readonly EffectTechnique BonesTech;
		public readonly EffectTechnique SelectedSubmeshTech;

		public ExtendedNormalMapEffect(Device device, string filename)
			: base(device, filename)
		{
			TweenFactor0 = FX.GetVariableByName("gTweenFactor0").AsScalar();

			MorphTech = FX.GetTechniqueByName("MorphTech");
			SelectedSubmeshMorphTech = FX.GetTechniqueByName("SelectedSubmeshMorphTech");
			NormalsTech = FX.GetTechniqueByName("NormalsTech");
			BonesTech = FX.GetTechniqueByName("BonesTech");
			SelectedSubmeshTech = FX.GetTechniqueByName("SelectedSubmeshTech");
		}
	}
}