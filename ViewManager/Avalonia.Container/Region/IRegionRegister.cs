using Avalonia.Controls;

namespace Avalonia.Container.Region
{
    public interface IRegionRegister
    {
        public void RegisterRegion(string regionName, ContentControl control);
    }

}
