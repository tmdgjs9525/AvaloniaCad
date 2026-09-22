using Container.Core.Parameter;

namespace Container.Core.Interfaces
{
    //히스토리 관리를 위한 레코드
    public record JournalEntry(string ViewName, Parameters? Parameters);


    public interface INavigationService
    {
        bool CanGoBack(string regionName);
        void ClearJournal(string regionName);
        void GoBack(string regionName);
        void NavigateTo(string regionName, string viewName, Parameters? parameters = null, bool recordHistory = true);
    }

    public interface INavigateAware
    {
        void NavigateTo(Parameters? parameters);
    }
}

// .NET 5 이하 또는 IsExternalInit이 없는 환경에서 record 사용 시 필요
namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit { }
}
