using System.Globalization;

using Spectre.Console.Cli;

namespace Versy;

class Program {

    public static string VersyVersion = "Version: 1.0.0\nMade by M4nd3l";
    
    public static int Main(string[] args) {
        var culture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture   = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        var app = new CommandApp<VVCommand>();
        app.Configure(config => {
            config.SetApplicationName("versy");
            config.AddExample(new[] { "script.vv", "output.exe" });
            config.ValidateExamples();
            config.CaseSensitivity(CaseSensitivity.None);
            config.SetApplicationVersion(VersyVersion);
        });
        return app.Run(args);
    }
}