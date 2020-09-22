// How to integrate React server side rendering
// Install React.Core install-package React.Core -version 5.0.0
// Add <add namespace="Codesanook.ReactJS" /> to <system.web.webPages.razor> in Web.config root level
// Add Script.Require("React").AtHead(), ReactDOM and our script in a module cshtml
// Add @Html.ReactInitJavaScript() in bottom of Document.cshtml to make sure our React script load already

using Orchard.Environment;
using React;

namespace Codesanook.EventManagement {
    public class ShellEvent : IOrchardShellEvents {
        public void Activated() {
            ReactSiteConfiguration.Configuration
                // Disable this because we already transformed with Webpack 
                .SetLoadBabel(false)
                .AddScriptWithoutTransform(
                    "~/Modules/Codesanook.EventManagement/Scripts/codesanook-event-management.js"
                );
        }

        public void Terminating() {
        }
    }
}
