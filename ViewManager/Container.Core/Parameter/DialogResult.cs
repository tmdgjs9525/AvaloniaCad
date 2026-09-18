using Container.Core.Interfaces;
using Container.Core.Parameter;

namespace Container.Core.Parameter
{
    public class DialogResult : IDialogResult
    {
        public bool Success { get; set; }
        public Parameters Parameters { get; set; }

        public DialogResult(bool success = false, Parameters? parameters = null)
        {
            Success = success;
            Parameters = parameters ?? new Parameters();
        }
    }
}
