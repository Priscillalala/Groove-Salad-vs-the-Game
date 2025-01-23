using GSvs.Core.Configuration;

namespace GSvs.Core.ContentManipulation
{
    public abstract class StaticContentManipulator<This> where This : StaticContentManipulator<This>
    {
        [Config("Enabled")]
        public static bool enabled;
    }
}