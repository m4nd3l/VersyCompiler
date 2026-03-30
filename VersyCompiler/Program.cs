using System.Globalization;

using Spectre.Console.Cli;
using Versy.LSP;

namespace Versy;

class Program {

    public static string VersyVersion = "Version: 0.0.1\nMade by M4nd3l";
    // TODO : ADD CHAR SUPPORT
    public static async Task Main (string[] args) {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (args.Length > 0 && args[0].Equals("lsp", StringComparison.OrdinalIgnoreCase)) {
            var versyLSP = new VersyLSP();
            await versyLSP.run();
            return;
        }
        
        var culture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture   = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        
        var app = new CommandApp<VVCommand>();
        app.Configure(config => {
            config.SetApplicationName("versy");
            config.AddExample(new[] { "script.vv", "output.exe" });
            config.ValidateExamples();
            config.CaseSensitivity(CaseSensitivity.None);
            config.SetApplicationVersion(VersyVersion);
        });
        app.Run(args);
    }
}