namespace Covenant.Cli;

internal sealed class CommandLineParser
{
    private readonly IServiceProvider _services;

    public CommandLineParser(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public System.CommandLine.Parsing.Parser Create()
    {
        var root = new RootCommand
        {
            Name = "covenant",
            Description = "SBOM generator for the masses",
        };

        // Create generate command
        var analyzers = _services.GetServices<Analyzer>();
        var generate = CreateGenerateCommand(analyzers);

        // Create convert command
        var convert = new Command("convert");
        foreach (var serializer in _services.GetServices<BomSerializer>())
        {
            convert.Add(CreateConvertCommand(serializer));
        }

        // Add the commands to the root
        root.Add(generate);
        root.Add(convert);
        root.Add(CreateReportCommand());
        root.Add(CreateCheckCommand());

        var builder = new CommandLineBuilder(root);
        return builder.UseDefaults()
            .UseHelp(ctx =>
            {
                ctx.HelpBuilder.CustomizeLayout(
                    _ =>
                        HelpBuilder.Default
                            .GetLayout()
                            .Skip(1)
                            .Prepend(_ =>
                            {
                                AnsiConsole.Write(
                                    new FigletText("Covenant")
                                        .Color(Color.Yellow));
                            }));
            }).Build();
    }

    private Command CreateGenerateCommand(IEnumerable<Analyzer> analyzers)
    {
        // Input
        var pathArgument = new Argument<string?>("PATH", "A file or directory to use as input")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };

        // Output
        var outputOption = new Option<string>(new[] { "-o", "--output" }, "The output path of the SBOM file")
        {
            ArgumentHelpName = "FILE",
        };

        // BOM name
        var nameOption = new Option<string>(new[] { "-n", "--name" }, "The SBOM name")
        {
            ArgumentHelpName = "NAME",
        };

        // BOM version
        var versionOption = new Option<string>(new[] { "-v", "--version" }, "The SBOM version")
        {
            ArgumentHelpName = "VERSION",
        };
        versionOption.SetDefaultValue("0.0.0");

        // Configuration
        var configurationOption = new Option<string?>(new[] { "-c", "--configuration" }, "The Covenant configuration file to use")
        {
            ArgumentHelpName = "FILE",
        };

        // No logo
        var noLogoOption = new Option<bool>("--no-logo", "Do not show the Covenant logo");

        var metadataOption = new Option<Dictionary<string, string>>(
              new[] { "-m", "--metadata" },
              parseArgument: input =>
              {
                  var result = new Dictionary<string, string>();

                  foreach (var token in input.Tokens)
                  {
                      var index = token.Value.IndexOf('=');
                      if (index == -1)
                      {
                          input.ErrorMessage = "Malformed metadata";
                          return result;
                      }

                      var key = token.Value[..index];
                      var value = token.Value.Substring(index + 1, token.Value.Length - index - 1);

                      result[key] = value;
                  }

                  return result;
              },
              isDefault: false,
              description: "Arbitrary metadata in the form 'key=value'")
        {
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        var command = new Command("generate")
        {
            pathArgument,
            outputOption,
            nameOption,
            versionOption,
            metadataOption,
            configurationOption,
            noLogoOption,
        };

        var augmentor = new CommandLineAugmentor(command);
        foreach (var analyzer in analyzers)
        {
            analyzer.Initialize(augmentor);
        }

        command.SetHandler((ctx) =>
        {
            augmentor.ParseResult = ctx.ParseResult;

            var configuration = ctx.ParseResult.GetValueForOption(configurationOption);
            var configurationPath = default(FilePath?);
            if (configuration != null)
            {
                configurationPath = new FilePath(configuration);
            }

            var command = _services.GetRequiredService<GenerateCommand>();
            ctx.ExitCode = command.Analyze(
                new GenerateCommandSettings(augmentor)
                {
                    Input = ctx.ParseResult.GetValueForArgument(pathArgument),
                    Output = ctx.ParseResult.GetValueForOption(outputOption),
                    Name = ctx.ParseResult.GetValueForOption(nameOption),
                    Version = ctx.ParseResult.GetValueForOption(versionOption),
                    Metadata = ctx.ParseResult.GetValueForOption(metadataOption),
                    NoLogo = ctx.ParseResult.GetValueForOption(noLogoOption),
                    Configuration = configurationPath,
                });
        });

        return command;
    }

    private Command CreateConvertCommand(BomSerializer serializer)
    {
        // Input
        var inputArgument = new Argument<string?>("PATH", "The Covenant SBOM file to convert")
        {
            Arity = ArgumentArity.ExactlyOne,
        };

        // Output
        var outputOption = new Option<string>(new[] { "-o", "--output" }, "The output path")
        {
            ArgumentHelpName = "FILE",
        };

        var command = new Command(serializer.Id)
        {
            inputArgument,
            outputOption,
        };

        var augmentor = new CommandLineAugmentor(command);
        serializer.Initialize(augmentor);

        command.SetHandler((ctx) =>
        {
            augmentor.ParseResult = ctx.ParseResult;

            var command = _services.GetRequiredService<ConvertCommand>();
            var inputPath = new FilePath(ctx.ParseResult.GetValueForArgument(inputArgument)!);

            var output = ctx.ParseResult.GetValueForOption(outputOption);
            var outputPath = default(FilePath?);
            if (output != null)
            {
                outputPath = new FilePath(output);
            }

            ctx.ExitCode = command.Convert(
                new ConvertCommandSettings(inputPath, serializer, augmentor)
                {
                    Output = outputPath,
                });
        });
        return command;
    }

    private Command CreateReportCommand()
    {
        // Input
        var inputArgument = new Argument<string?>("PATH", "The Covenant SBOM file to create a HTML report for")
        {
            Arity = ArgumentArity.ExactlyOne,
        };

        // Output
        var outputOption = new Option<string?>(new[] { "-o", "--output" }, "The output path of the HTML report")
        {
            ArgumentHelpName = "FILE",
        };

        var command = new Command("report")
        {
            inputArgument,
            outputOption,
        };

        command.SetHandler((ctx) =>
        {
            var command = _services.GetRequiredService<ReportCommand>();
            var inputPath = new FilePath(ctx.ParseResult.GetValueForArgument(inputArgument)!);

            var output = ctx.ParseResult.GetValueForOption(outputOption);
            var outputPath = default(FilePath?);
            if (output != null)
            {
                outputPath = new FilePath(output);
            }

            ctx.ExitCode = command.Run(
                new ReportCommandSettings(inputPath)
                {
                    Output = outputPath,
                });
        });

        return command;
    }

    private Command CreateCheckCommand()
    {
        // Input
        var inputArgument = new Argument<string?>("PATH", "The Covenant SBOM file to run compliance checks for")
        {
            Arity = ArgumentArity.ExactlyOne,
        };

        // Configuration
        var configurationOption = new Option<string?>(new[] { "-c", "--configuration" }, "The Covenant configuration file to use")
        {
            ArgumentHelpName = "FILE",
        };

        var command = new Command("check")
        {
            inputArgument,
            configurationOption,
        };

        command.SetHandler((ctx) =>
        {
            var command = _services.GetRequiredService<CheckCommand>();
            var inputPath = new FilePath(ctx.ParseResult.GetValueForArgument(inputArgument)!);

            var configuration = ctx.ParseResult.GetValueForOption(configurationOption);
            var configurationPath = default(FilePath?);
            if (configuration != null)
            {
                configurationPath = new FilePath(configuration);
            }

            ctx.ExitCode = command.Run(
                new CheckCommandSettings(inputPath)
                {
                    Configuration = configurationPath,
                });
        });

        return command;
    }
}
