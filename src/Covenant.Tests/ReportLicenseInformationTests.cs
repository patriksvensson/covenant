using Covenant.Core;
using Covenant.Core.Model;
using Covenant.Reporting;
using Shouldly;

namespace Covenant.Tests;

public class ReportLicenseInformationTests
{
    [Fact]
    public void Should_Calculate_Known_Licenses_Correctly()
    {
        // Given, When
        var info = new ReportLicenseInformation();
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Id = "MIT" } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Id = "MIT" } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Id = "APACHE-2.0" } });
        info.AddFile(new BomFile("/foo/bar.txt", new BomHash(BomHashAlgorithm.SHA1, "LOL")) { License = new BomLicense { Id = "MIT" } });

        // Then
        info.Known.Count.ShouldBe(4);
        info.Known.Distinct.ShouldBe(2);
        info.Known["MIT"].ShouldBe(3);
        info.Known["APACHE-2.0"].ShouldBe(1);
    }

    [Fact]
    public void Should_Calculate_Unknown_Licenses_Correctly()
    {
        // Given, When
        var info = new ReportLicenseInformation();
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Name = "Patrik" } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Name = "Patrik" } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Url = "https://example.com" } });
        info.AddFile(new BomFile("/foo/bar.txt", new BomHash(BomHashAlgorithm.SHA1, "LOL")) { License = new BomLicense { Name = "Patrik" } });

        // Then
        info.Unknown.Count.ShouldBe(4);
        info.Unknown.Distinct.ShouldBe(2);
        info.Unknown["Patrik"].ShouldBe(3);
        info.Unknown["https://example.com"].ShouldBe(1);
        info.TotalUnknown.Count.ShouldBe(4);
        info.TotalUnknown.Distinct.ShouldBe(2);
    }

    [Fact]
    public void Should_Calculate_Embedded_Licenses_Correctly()
    {
        // Given, When
        var info = new ReportLicenseInformation();
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Text = Base64EncodedText.Encode("lol") } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Text = Base64EncodedText.Encode("lol") } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Text = Base64EncodedText.Encode("foo") } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Text = Base64EncodedText.Encode("bar") } });
        info.AddFile(new BomFile("/foo/bar.txt", new BomHash(BomHashAlgorithm.SHA1, "LOL")) { License = new BomLicense { Text = Base64EncodedText.Encode("bar") } });

        // Then
        info.Embedded.Count.ShouldBe(5);
        info.Embedded.Distinct.ShouldBe(3);
        info.Embedded["lol"].ShouldBe(2);
        info.Embedded["foo"].ShouldBe(1);
        info.Embedded["bar"].ShouldBe(2);
    }

    [Fact]
    public void Should_Calculate_Unlicensed_Items_Correctly()
    {
        // Given, When
        var info = new ReportLicenseInformation();
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Root) { License = null });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = null });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = null });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = null });
        info.AddFile(new BomFile("/foo/bar.txt", new BomHash(BomHashAlgorithm.SHA1, "LOL")) { License = null });

        // Then
        info.UnlicensedCount.ShouldBe(4);
    }

    [Fact]
    public void Should_Take_Embedded_Licenses_Into_Account_When_Calculating_Total_Unknown()
    {
        // Given, When
        var info = new ReportLicenseInformation();
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Name = "Patrik" } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Name = "Patrik" } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Url = "https://example.com" } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Text = Base64EncodedText.Encode("lol") } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Text = Base64EncodedText.Encode("lol") } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Text = Base64EncodedText.Encode("foo") } });
        info.AddComponent(new BomComponent("purl", "name", "version", BomComponentKind.Library) { License = new BomLicense { Text = Base64EncodedText.Encode("bar") } });
        info.AddFile(new BomFile("/foo/bar.txt", new BomHash(BomHashAlgorithm.SHA1, "LOL")) { License = new BomLicense { Name = "Patrik" } });

        // Then
        info.TotalUnknown.Count.ShouldBe(8);
        info.TotalUnknown.Distinct.ShouldBe(5);
    }
}