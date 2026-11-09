using KoDrawing.Core.Rendering;
using Xunit;

namespace KoDrawing.Tests.Rendering;

public class GridRendererTests
{
    private const float MinPixelSpacing = 12f;   // GridRenderer와 같은 값

    [Theory]
    [InlineData(1f, 100f)]     // 12mm 이상 → 100mm
    [InlineData(10f, 10f)]     // 1.2mm 이상 → 10mm
    [InlineData(12f, 1f)]      // 정확히 1mm
    [InlineData(0.1f, 1000f)]  // 120mm 이상 → 1000mm
    public void ChooseSpacing_ReturnsExpectedPowerOfTen(float zoom, float expected)
    {
        Assert.Equal(expected, GridRenderer.ChooseSpacing(zoom), 3);
    }

    [Theory]
    [InlineData(0.01f)]
    [InlineData(0.37f)]
    [InlineData(1f)]
    [InlineData(3.3f)]
    [InlineData(57f)]
    [InlineData(1000f)]
    public void ChooseSpacing_IsSmallestPowerOfTen_AtLeastMinPixels(float zoom)
    {
        var spacing = GridRenderer.ChooseSpacing(zoom);

        // 화면상 간격이 최소 픽셀 이상이고
        Assert.True(spacing * zoom >= MinPixelSpacing * 0.999f, $"spacing={spacing}, zoom={zoom}");

        // 한 단계 작은 간격은 최소 픽셀보다 작아야 한다 (= 가장 작은 10의 거듭제곱)
        Assert.True(spacing / 10f * zoom < MinPixelSpacing, $"spacing={spacing}, zoom={zoom}");

        // 10의 거듭제곱인지
        var exponent = MathF.Log10(spacing);
        Assert.Equal(MathF.Round(exponent), exponent, 3);
    }
}
