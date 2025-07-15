using System;
using System.IO;
using System.Linq;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build;

partial class Program
{
    private const string PackOutput = "./artifacts";
    private const string PackOutputCopy = "../../nuget";
    private const string EnvVarMissing = " environment variable is missing. Aborting.";

    private static class Targets
    {
        public const string CleanBuildOutput = "clean-build-output";
        public const string CleanPackOutput = "clean-pack-output";
        public const string Build = "build";
        public const string Test = "test";
        public const string Pack = "pack";
        public const string SignBinary = "sign-binary";
        public const string SignPackage = "sign-package";
        public const string CopyPackOutput = "copy-pack-output";
    }

    static void Main(string[] args)
    {
        Target(Targets.CleanBuildOutput, () =>
        {
            //Run("dotnet", "clean -c Release -v m --nologo", echoPrefix: Prefix);
        });

        Target(Targets.Build, [Targets.CleanBuildOutput],
            () => { Run("dotnet", "build -c Release --nologo", echoPrefix: Prefix); });

        Target(Targets.SignBinary, [Targets.Build], () => { Sign("./src/bin/Release", "*.dll"); });

        Target(Targets.Test, [Targets.Build],
            () => { Run("dotnet", "test -c Release --no-build", echoPrefix: Prefix); });

        Target(Targets.CleanPackOutput, () =>
        {
            if (Directory.Exists(PackOutput))
            {
                Directory.Delete(PackOutput, true);
            }
        });

        Target(Targets.Pack, [Targets.Build, Targets.CleanPackOutput], () =>
        {
            var project = Directory.GetFiles("./src", "*.csproj", SearchOption.TopDirectoryOnly).OrderBy(s => s)
                .First();

            Run("dotnet",
                $"pack {project} -c Release -o \"{Directory.CreateDirectory(PackOutput).FullName}\" --no-build --nologo",
                echoPrefix: Prefix);
        });

        Target(Targets.SignPackage, [Targets.Pack], () => { Sign(PackOutput, "*.nupkg"); });

        Target(Targets.CopyPackOutput, [Targets.Pack], () =>
        {
            Directory.CreateDirectory(PackOutputCopy);

            foreach (var file in Directory.GetFiles(PackOutput))
            {
                File.Copy(file, Path.Combine(PackOutputCopy, Path.GetFileName(file)), true);
            }
        });

        Target("quick", [Targets.CopyPackOutput]);

        Target("default", [Targets.Test, Targets.CopyPackOutput]);

        Target("sign", [Targets.SignBinary, Targets.Test, Targets.SignPackage, Targets.CopyPackOutput]);

        RunTargetsAndExitAsync(args, ex => ex.Message.EndsWith(EnvVarMissing), () => Prefix).GetAwaiter().GetResult();
    }

    private static void Sign(string path, string searchTerm)
    {
        var signClientSecret = Environment.GetEnvironmentVariable("SignClientSecret");

        if (string.IsNullOrWhiteSpace(signClientSecret))
        {
            throw new Exception($"SignClientSecret{EnvVarMissing}");
        }

        foreach (var file in Directory.GetFiles(path, searchTerm, SearchOption.AllDirectories))
        {
            Console.WriteLine($"  Signing {file}");
            Run("dotnet",
                $"SignClient sign -c ../../signClient.json -i {file} -r sc-ids@dotnetfoundation.org -s \"{signClientSecret}\" -n 'IdentityServer4'",
                noEcho: true);
        }
    }
}