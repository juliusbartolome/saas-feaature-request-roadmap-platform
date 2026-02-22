using FluentAssertions;
using Roadmap.Application.DTOs;
using Xunit;

namespace Roadmap.UnitTests;

public class ValidationTests
{
    [Fact]
    public void CreateFeatureValidator_ShouldFail_WhenTitleMissing()
    {
        var validator = new CreateFeatureValidator();
        var result = validator.Validate(new CreateFeatureDto("", "desc"));
        result.IsValid.Should().BeFalse();
    }
}
