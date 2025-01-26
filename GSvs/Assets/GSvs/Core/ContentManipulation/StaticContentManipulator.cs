using GSvs.Core.AssetManipulation;
using GSvs.Core.Configuration;

namespace GSvs.Core.ContentManipulation
{
    public abstract class StaticContentManipulator<This> : BaseContentManipulator<This> where This : StaticContentManipulator<This>
    {
        [Config("Installed")]
        public static readonly Config<bool> installed = true;

        protected override bool IsInstalled() => installed;

        protected override void OnInstall()
        {
            base.OnInstall();
            AssetManipulatorAttribute.ProcessAsync(typeof(This));
        }
    }
}