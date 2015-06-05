using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using System.Threading.Tasks;

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
        public async Task<string> Post(string buffer, int line, int column, string newName)
        {
            var document = _project.AddDocument("AdhocDocument", SourceText.From(buffer));
            var sourceText = await document.GetTextAsync();
            var symbolPosition = sourceText.Lines.GetPosition(new LinePosition(line, column));
            var symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, symbolPosition);

            if (symbol != null)
            {
                var newSolution = await Renamer.RenameSymbolAsync(_workspace.CurrentSolution, symbol, newName, _workspace.Options);

                var solutionChanges = newSolution.GetChanges(_workspace.CurrentSolution);

                var projectChanges = solutionChanges.GetProjectChanges();
                var documentChanges = projectChanges.First().GetChangedDocuments();
                var changedDocument = newSolution.GetDocument(documentChanges.First());

                var newSourceText = await changedDocument.GetTextAsync();
                return newSourceText.ToString();
            }

            return null;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
