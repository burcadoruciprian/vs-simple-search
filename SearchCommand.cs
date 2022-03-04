using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Web;
using Task = System.Threading.Tasks.Task;

namespace SimpleSearch
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class SearchCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("824b05b4-7a6c-4d0a-bc1f-6914c4b5bed6");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;

    private readonly EnvDTE80.DTE2 dte;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private SearchCommand(AsyncPackage package, OleMenuCommandService commandService, EnvDTE80.DTE2 dte)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      this.dte = dte ?? throw new ArgumentNullException(nameof(dte));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Search, menuCommandID);
      commandService.AddCommand(menuItem);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static SearchCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
    {
      get
      {
        return this.package;
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
      // Switch to the main thread - the call to AddCommand in SearchCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      EnvDTE80.DTE2 dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2; ;
      Instance = new SearchCommand(package, commandService, dte);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void Search(object sender, EventArgs e)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      if (this.dte.ActiveDocument == null || this.dte.ActiveDocument.Selection == null)
        return;

      var textSelection = (EnvDTE.TextSelection)this.dte.ActiveDocument.Selection;
      if (textSelection == null)
        return;

      string text = textSelection.Text.Trim();
      if (string.IsNullOrEmpty(text))
        return;

      string url = Constants.DefaultQueryTemplate;
      url = url.Replace("%SELECTION%", HttpUtility.UrlEncode(text));
      Process.Start(url);
    }
  }
}
