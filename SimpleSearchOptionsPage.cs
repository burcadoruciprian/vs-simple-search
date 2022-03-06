using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace SimpleSearch
{
  internal class SimpleSearchOptionsPage : DialogPage
  {
    private string queryTemplate = SearchTemplates.DefaultQueryTemplate;

    [Category("Options")]
    [DisplayName("Query Template")]
    [Description("You can use this option to change the search engine. " +
      "The %SELECTION% part of the query is replaced by the actual text. Make sure your query string includes it.")]
    public string QueryTemplate
    {
      get { return queryTemplate; }
      set { queryTemplate = value; }
    }
  }
}
