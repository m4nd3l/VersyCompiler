using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Versy.LSP;

public class VersyLSP {
    private Dictionary<string, DocumentSession> sessions;
    private ILanguageServer? _server;
    
    public async Task run() {}
    
    public void onDocumentChanged(string uri, string newCode) {
        if (!sessions.TryGetValue(uri, out var session)) {
            session        = new DocumentSession();
            sessions[uri] = session;
        }

        session.update(newCode);
        publishDiagnostics(uri, session.diagnostics);
    }
    
    private void publishDiagnostics(string uri, List<Diagnostic> diagnostics)
    {
        _server?.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams {
            Uri         = uri,
            Diagnostics = diagnostics
        });
    }
    
    public void setServer(ILanguageServer server) => _server = server;
}
