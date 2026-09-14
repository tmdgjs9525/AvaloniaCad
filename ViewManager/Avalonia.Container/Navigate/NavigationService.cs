using Avalonia.Container.Region;
using EyeContainer.Core.Interfaces;
using EyeContainer.Core.Parameter;
using EyeEyeContainer.Core.Interfaces;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Avalonia.Container.Navigate
{
    internal class NavigationService : INavigationService, INavigationRegister, IRegionRegister
    {
        // object는 ViewModel의 Type이 될 수도 있고, ViewModel의 인스턴스가 될 수도 있음
        private readonly Dictionary<string, Tuple<Type, object>> _viewDictionary = new();

        //어태치 프로퍼티로 ContentControl 사용하는 곳에서 등록된다.
        private readonly Dictionary<string, ContentControl> _regionDictionary = new();

        // Region별 네비게이션 히스토리 (Stack구조: LIFO)
        private readonly Dictionary<string, Stack<JournalEntry>> _journal = new();

        // Region별 현재 보여지고 있는 View의 정보 (이동 시 스택에 넣기 위함)
        private readonly Dictionary<string, JournalEntry> _currentViewInfo = new();

        internal NavigationService()
        {

        }

        public void RegisterRegion(string regionName, ContentControl control)
        {
            _regionDictionary[regionName] = control;

            // Region이 등록될 때 히스토리 스택도 초기화
            if (!_journal.ContainsKey(regionName))
            {
                _journal[regionName] = new Stack<JournalEntry>();
            }
        }

        public void NavigateTo(string regionName, string viewName, Parameters? parameters = null, bool recordHistory = false)
        {
            // 1. 현재 보여지고 있는 뷰가 있다면, 히스토리(Stack)에 저장 (Push)
            if (recordHistory && _currentViewInfo.TryGetValue(regionName, out var currentInfo))
            {
                if (!_journal.ContainsKey(regionName))
                {
                    _journal[regionName] = new Stack<JournalEntry>();
                }
                _journal[regionName].Push(currentInfo);
            }

            // 2. 실제 이동 로직 수행
            PerformNavigation(regionName, viewName, parameters);
        }

        public void GoBack(string regionName)
        {
            if (CanGoBack(regionName))
            {
                // 1. 스택에서 이전 뷰 정보를 꺼냄 (Pop)
                var previousEntry = _journal[regionName].Pop();

                // 2. 히스토리에 다시 쌓지 않고 바로 이동 (내부 메서드 호출)
                PerformNavigation(regionName, previousEntry.ViewName, previousEntry.Parameters);
            }
        }

        /// <summary>
        /// 해당 Region에서 뒤로 갈 수 있는지 확인
        /// </summary>
        public bool CanGoBack(string regionName)
        {
            return _journal.ContainsKey(regionName) && _journal[regionName].Count > 0;
        }

        /// <summary>
        /// 히스토리 초기화
        /// </summary>
        public void ClearJournal(string regionName)
        {
            if (_journal.ContainsKey(regionName))
            {
                _journal[regionName].Clear();
            }
            if (_currentViewInfo.ContainsKey(regionName))
            {
                _currentViewInfo.Remove(regionName);
            }
        }

        // 실제 뷰 생성 및 교체 로직 (NavigateTo와 GoBack에서 공통 사용)
        private void PerformNavigation(string regionName, string viewName, Parameters? parameters)
        {
            if (_viewDictionary.ContainsKey(viewName) == false)
            {
                throw new ArgumentNullException($"Can't find '{viewName}' from _viewDictionary ");
            }

            var viewInfo = _viewDictionary[viewName];
            var viewType = viewInfo.Item1;
            var viewModelInfo = viewInfo.Item2;

            if (_regionDictionary.ContainsKey(regionName) == false)
            {
                throw new ArgumentNullException($"Can't find '{regionName}' region");
            }

            var control = Ioc.Default.GetService(viewType) as UserControl;

            if (control == null)
            {
                throw new ArgumentNullException($"Can't find '{viewName}' from Di Container");
            }

            object? viewModelInstance;
            if (viewModelInfo is Type viewModelType)
            {
                viewModelInstance = Ioc.Default.GetService(viewModelType);
                if (viewModelInstance is null)
                {
                    throw new ArgumentNullException($"Can't find '{viewModelType.Name}' ViewModel from Di Container");
                }
            }
            else
            {
                viewModelInstance = viewModelInfo;
            }

            control.DataContext = viewModelInstance;

            if (control.DataContext is INavigateAware navigateAware)
            {
                navigateAware.NavigateTo(parameters ?? new Parameters());
            }

            _regionDictionary[regionName].Content = control;

            // [추가] 현재 뷰 정보를 갱신 (다음 이동 시 이것을 스택에 넣기 위해)
            _currentViewInfo[regionName] = new JournalEntry(viewName, parameters);
        }

        public void AddTransientNavigation<TView, TViewModel>() where TView : Control
                                                                 where TViewModel : IViewModelBase
        {
            _viewDictionary[typeof(TView).Name] =
                new Tuple<Type, object>(typeof(TView), typeof(TViewModel)); 
        }

        public void AddSingletonNavigation<TView, TViewModel>() where TView : Control
                                                                where TViewModel : IViewModelBase
        {
            _viewDictionary[typeof(TView).Name] =
                new Tuple<Type, object>(typeof(TView), typeof(TViewModel));
        }

        public void AddSingletonNavigation<TInterface, TImplementationView, TViewModel>() where TInterface : class               // TInterface는 참조 형식이어야 함
                                                                                         where TImplementationView : Control, TInterface // TImplementation은 Control을 상속하고 TInterface를 구현해야 함
                                                                                         where TViewModel : IViewModelBase
        {
            string viewName = typeof(TInterface).Name.Substring(1);

            _viewDictionary[viewName] = new Tuple<Type, object>(typeof(TImplementationView), typeof(TViewModel));
        }

        public void AddTransientNavigation<TInterface, TImplementationView, TViewModel>() where TInterface : class               // TInterface는 참조 형식이어야 함
                                                                                          where TImplementationView : Control, TInterface // TImplementation은 Control을 상속하고 TInterface를 구현해야 함
                                                                                          where TViewModel : IViewModelBase
        {
            string viewName = typeof(TInterface).Name.Substring(1);

            _viewDictionary[viewName] = new Tuple<Type, object>(typeof(TImplementationView), typeof(TViewModel));
        }

        /// <summary>
        /// View와 미리 생성된 ViewModel 인스턴스를 Transient 라이프타임으로 등록합니다.
        /// View는 탐색 시마다 새로 생성되지만, ViewModel은 제공된 인스턴스를 공유합니다.
        /// </summary>
        public void AddTransientNavigation<TView>(IViewModelBase viewModelInstance) where TView : Control
        {
            _viewDictionary[typeof(TView).Name] =
                new Tuple<Type, object>(typeof(TView), viewModelInstance);
        }

        /// <summary>
        /// View와 미리 생성된 ViewModel 인스턴스를 Singleton 라이프타임으로 등록합니다.
        /// View는 DI 컨테이너에 Singleton으로 등록되어 있어야 하며, ViewModel은 제공된 인스턴스를 공유합니다.
        /// </summary>
        public void AddSingletonNavigation<TView>(IViewModelBase viewModelInstance) where TView : Control
        {
            _viewDictionary[typeof(TView).Name] =
                new Tuple<Type, object>(typeof(TView), viewModelInstance);
        }
    }
}
