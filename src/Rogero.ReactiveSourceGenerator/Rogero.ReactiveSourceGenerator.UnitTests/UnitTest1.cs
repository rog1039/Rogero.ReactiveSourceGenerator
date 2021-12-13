using System;
using FluentAssertions;
using Xunit;

namespace Rogero.ReactiveSourceGenerator.UnitTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        PropertyGenerationInfo.GetFieldName("field2").Should().Be("Field2");
        PropertyGenerationInfo.GetFieldName("_field3").Should().Be("Field3");

        void Throw1()
        {
            PropertyGenerationInfo.GetFieldName("Field4");
        }

        void Throw2()
        {
            PropertyGenerationInfo.GetFieldName("__Field5");
        }

        void Throw3()
        {
            PropertyGenerationInfo.GetFieldName("_Field6");
        }

        Action shouldThrow = Throw1;
        shouldThrow.Should().Throw<Exception>();

        shouldThrow = Throw2;
        shouldThrow.Should().Throw<Exception>();
        
        shouldThrow = Throw3;
        shouldThrow.Should().Throw<Exception>();
    }
}