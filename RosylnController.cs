using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Recommendations;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace OmnisharpLite.Controllers
{
    [Route("api/[controller]")]
    public class RosylnController : Controller
    {
        private AdhocWorkspace _workspace;
        private Project _project;

        public RosylnController()
        {
            _workspace = new AdhocWorkspace();
            _project = _workspace.AddProject("AdhocProject", LanguageNames.CSharp);
        }

        [Route("rename")]
        [HttpPost]
        public async Task<string> Rename(Request req)
        {
            var document = _workspace.AddDocument(_project.Id, "AdhocDocument", SourceText.From(req.Buffer));
            var sourceText = await document.GetTextAsync();
            var symbolPosition = sourceText.Lines.GetPosition(new LinePosition(req.Line, req.Column));
            var symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, symbolPosition);

            if (symbol != null)
            {
                var newSolution = await Renamer.RenameSymbolAsync(_workspace.CurrentSolution, symbol, req.RenameTo, _workspace.Options);

                var solutionChanges = newSolution.GetChanges(_workspace.CurrentSolution);

                var projectChanges = solutionChanges.GetProjectChanges();
                var documentChanges = projectChanges.First().GetChangedDocuments();
                var changedDocument = newSolution.GetDocument(documentChanges.First());

                var newSourceText = await changedDocument.GetTextAsync();

                return newSourceText.ToString();
            }

            return null;
        }

        [Route("autocomplete")]
        [HttpPost]
        public async Task<List<string>> AutoComplete(Request req)
        {
            var document = _workspace.AddDocument(_project.Id, "AdhocDocument", SourceText.From(req.Buffer));
            var sourceText = await document.GetTextAsync();
            var position = sourceText.Lines.GetPosition(new LinePosition(req.Line, req.Column));
            var model = await document.GetSemanticModelAsync();

            var symbols = Recommender.GetRecommendedSymbolsAtPosition(model, position, _workspace);

            return symbols.Select(s => s.Name).ToList();
        }
    }

    public class Request
    {
        public string Buffer { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string RenameTo { get; set; }
    }
}
